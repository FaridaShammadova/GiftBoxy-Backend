using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using GiftBoxy.Application.DTOs.BuyerProfile;
using GiftBoxy.Domain.Entities;
using GiftBoxy.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace GiftBoxy.API.Controllers
{
    [Route("api/buyer-profiles")]
    [ApiController]
    public class BuyerProfileController : ControllerBase
    {
        private readonly AppDbContext _context;

        public BuyerProfileController(AppDbContext context)
        {
            _context = context;
        }

        [Authorize(Roles = "Buyer")]
        [HttpGet("me")]
        public async Task<IActionResult> GetMyProfile()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var profile = await _context.BuyerProfiles
                .Include(b => b.User)
                .FirstOrDefaultAsync(b => b.UserId == userId);

            if (profile == null)
                return NotFound();

            return Ok(MapToDto(profile));
        }

        [Authorize(Roles = "Buyer")]
        [HttpPut("me")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateBuyerProfileDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var profile = await _context.BuyerProfiles
                .FirstOrDefaultAsync(b => b.UserId == userId);

            if (profile == null)
                return NotFound();

            if (dto.Location != null) profile.Location = dto.Location;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Profile updated" });
        }

        private static object MapToDto(BuyerProfile b) => new
        {
            b.Id,
            b.Location,
            b.UserId,
            Name = b.User.Name,
            Email = b.User.Email
        };
    }
}
