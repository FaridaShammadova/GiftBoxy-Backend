using GiftBoxy.Application.DTOs.Auth;
using GiftBoxy.Application.Services.Interfaces;
using GiftBoxy.Domain.Entities;
using GiftBoxy.Domain.Enums;
using GiftBoxy.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace GiftBoxy.API.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ITokenService _tokenService;
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly IConfiguration _configuration;
        private readonly AppDbContext _context;

        public AuthController(
            UserManager<AppUser> userManager,
            SignInManager<AppUser> signInManager,
            IConfiguration configuration,
            AppDbContext context,
            ITokenService tokenService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
            _context = context;
            _tokenService = tokenService;
        }

        [HttpPost("buyer-register")]
        public async Task<IActionResult> BuyerRegister(RegisterDto dto)
        {
            var existingUser = await _userManager.FindByEmailAsync(dto.Email);
            if (existingUser != null)
                return BadRequest("User already exists");

            var user = new AppUser
            {
                Name = dto.Name,
                Email = dto.Email,
                UserName = dto.Email,
                Role = UserRole.Buyer
            };

            var result = await _userManager.CreateAsync(user, dto.Password);
            if (!result.Succeeded)
                return BadRequest(result.Errors);

            return Ok(new { message = "Buyer account created" });
        }

        [HttpPost("seller-register")]
        public async Task<IActionResult> SellerRegister(SellerRegisterDto dto)
        {
            var existingUser = await _userManager.FindByEmailAsync(dto.Email);

            if (existingUser != null)
                return BadRequest("User already exists");

            var user = new AppUser
            {
                Name = dto.FullName,
                Email = dto.Email,
                UserName = dto.Email,
                Role = UserRole.Seller
            };

            var result = await _userManager.CreateAsync(user, dto.Password);

            if (!result.Succeeded)
                return BadRequest(result.Errors);

            var sellerProfile = new SellerProfile
            {
                UserId = user.Id,
                StoreName = dto.StoreName,
                Bio = dto.Bio,
                Location = dto.Location,
                ShopUrl = dto.ShopUrl
            };

            _context.SellerProfiles.Add(sellerProfile);

            await _context.SaveChangesAsync();

            foreach (var categoryName in dto.Categories)
            {
                var category = await _context.Categories
                    .FirstOrDefaultAsync(x => x.Name == categoryName);

                if (category == null) continue;

                _context.SellerCategories.Add(new SellerCategory
                {
                    SellerProfile = sellerProfile,
                    CategoryId = category.Id
                });
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Seller account created",
                sellerProfileId = sellerProfile.Id
            });
        }

        [HttpPost("buyer-login")]
        public async Task<IActionResult> BuyerLogin(LoginDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);

            if (user == null || user.Role != UserRole.Buyer)
                return Unauthorized("Invalid credentials");

            var result = await _signInManager.CheckPasswordSignInAsync(user, dto.Password, false);
            if (!result.Succeeded)
                return Unauthorized("Invalid credentials");

            var (accessToken, refreshToken) = await GenerateTokens(user);

            return Ok(new AuthResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                Email = user.Email!,
                Name = user.Name,
                Role = user.Role
            });
        }

        [HttpPost("seller-login")]
        public async Task<IActionResult> SellerLogin(LoginDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);

            if (user == null || user.Role != UserRole.Seller)
                return Unauthorized("Invalid credentials");

            var result = await _signInManager.CheckPasswordSignInAsync(user, dto.Password, false);
            if (!result.Succeeded)
                return Unauthorized("Invalid credentials");

            var (accessToken, refreshToken) = await GenerateTokens(user);

            return Ok(new AuthResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                Email = user.Email!,
                Name = user.Name,
                Role = user.Role
            });
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken(RefreshTokenDto dto)
        {
            var user = _userManager.Users
                .FirstOrDefault(u => u.RefreshToken == dto.RefreshToken);

            if (user == null)
                return Unauthorized("Invalid refresh token");

            if (user.RefreshTokenExpireDate < DateTime.UtcNow)
                return Unauthorized("Refresh token expired");

            var (accessToken, refreshToken) = await GenerateTokens(user);

            return Ok(new AuthResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                Email = user.Email!,
                Name = user.Name,
                Role = user.Role
            });
        }

        [Authorize]
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword(ChangePasswordDto dto)
        {
            if (dto.NewPassword != dto.ConfirmNewPassword)
                return BadRequest("Passwords do not match");

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId!);

            if (user == null)
                return NotFound();

            var result = await _userManager.ChangePasswordAsync(
                user, dto.CurrentPassword, dto.NewPassword);

            if (!result.Succeeded)
                return BadRequest(result.Errors);

            return Ok(new { message = "Password changed successfully" });
        }

        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId!);

            if (user == null)
                return NotFound();

            // Refresh token-i sil
            user.RefreshToken = null;
            user.RefreshTokenExpireDate = null;
            await _userManager.UpdateAsync(user);

            return Ok(new { message = "Logged out successfully" });
        }

        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> Me()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId!);

            if (user == null)
                return NotFound();

            return Ok(new AuthResponseDto
            {
                Email = user.Email!,
                Name = user.Name,
                Role = user.Role
            });
        }

        // --- Private metodlar ---

        private async Task<(string accessToken, string refreshToken)> GenerateTokens(AppUser user)
        {
            var accessToken = _tokenService.CreateToken(user);
            var refreshToken = GenerateRefreshToken();

            user.RefreshToken = refreshToken;
            user.RefreshTokenExpireDate = DateTime.UtcNow.AddDays(30);
            await _userManager.UpdateAsync(user);

            return (accessToken, refreshToken);
        }

        private string GenerateRefreshToken()
        {
            var randomBytes = new byte[64];
            using var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);
            return Convert.ToBase64String(randomBytes);
        }
    }
}
