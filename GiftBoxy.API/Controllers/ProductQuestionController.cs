using GiftBoxy.Application.DTOs.QA;
using GiftBoxy.Domain.Entities;
using GiftBoxy.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace GiftBoxy.API.Controllers
{
    [Route("api/questions")]
    [ApiController]
    public class ProductQuestionController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ProductQuestionController(AppDbContext context)
        {
            _context = context;
        }

        // -----------------------------------------------
        // PUBLIC ENDPOINTS
        // -----------------------------------------------

        [HttpGet("product/{productId}")]
        public async Task<IActionResult> GetByProduct(int productId)
        {
            var questions = await _context.ProductQuestions
                .Include(q => q.User)
                .Where(q => q.ProductId == productId)
                .OrderByDescending(q => q.CreatedAt)
                .Select(q => new QuestionResponseDto
                {
                    Id = q.Id,
                    QuestionText = q.QuestionText,
                    AnswerText = q.AnswerText,
                    AskedAt = q.CreatedAt,
                    AnsweredAt = q.AnsweredAt,
                    BuyerName = q.User.Name
                })
                .ToListAsync();

            return Ok(questions);
        }

        // BUYER ENDPOINTS

        [Authorize(Roles = "Buyer")]
        [HttpPost]
        public async Task<IActionResult> Ask([FromBody] AskQuestionDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.Id == dto.ProductId);

            if (product == null)
                return NotFound("Product not found");

            // Seller-in userId-si
            if (product.UserId == null)
                return BadRequest("This product has no seller");

            var question = new ProductQuestion
            {
                QuestionText = dto.QuestionText,
                ProductId = dto.ProductId,
                UserId = userId,
                SellerId = product.UserId
            };

            _context.ProductQuestions.Add(question);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Question submitted" });
        }

        [Authorize(Roles = "Buyer")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var question = await _context.ProductQuestions
                .FirstOrDefaultAsync(q => q.Id == id && q.UserId == userId);

            if (question == null)
                return NotFound();

            // Cavablanmış sualı silmək olmaz
            if (question.AnswerText != null)
                return BadRequest("Answered questions cannot be deleted");

            _context.ProductQuestions.Remove(question);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Question deleted" });
        }

        // SELLER ENDPOINTS

        [Authorize(Roles = "Seller")]
        [HttpGet("my-questions")]
        public async Task<IActionResult> GetMyQuestions([FromQuery] bool? unanswered = null)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var query = _context.ProductQuestions
                .Include(q => q.User)
                .Include(q => q.Product)
                .Where(q => q.SellerId == userId)
                .AsQueryable();

            if (unanswered.HasValue)
            {
                query = unanswered.Value
                    ? query.Where(q => q.AnswerText == null)
                    : query.Where(q => q.AnswerText != null);
            }

            var questions = await query
                .OrderByDescending(q => q.CreatedAt)
                .Select(q => new
                {
                    id = q.Id,
                    questionText = q.QuestionText,
                    answerText = q.AnswerText,
                    askedAt = q.CreatedAt,
                    answeredAt = q.AnsweredAt,
                    buyerName = q.User.Name,
                    productTitle = q.Product.Title,
                    productId = q.ProductId,
                    isAnswered = q.AnswerText != null
                })
                .ToListAsync();

            return Ok(questions);
        }

        [Authorize(Roles = "Seller")]
        [HttpPatch("{id}/answer")]
        public async Task<IActionResult> Answer(int id, [FromBody] AnswerQuestionDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var question = await _context.ProductQuestions
                .FirstOrDefaultAsync(q => q.Id == id && q.SellerId == userId);

            if (question == null)
                return NotFound();

            if (question.AnswerText != null)
                return BadRequest("Question already answered");

            question.AnswerText = dto.AnswerText;
            question.AnsweredAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Answer submitted" });
        }

        [Authorize(Roles = "Seller")]
        [HttpPatch("{id}/update-answer")]
        public async Task<IActionResult> UpdateAnswer(int id, [FromBody] AnswerQuestionDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var question = await _context.ProductQuestions
                .FirstOrDefaultAsync(q => q.Id == id && q.SellerId == userId);

            if (question == null)
                return NotFound();

            if (question.AnswerText == null)
                return BadRequest("Question has not been answered yet");

            question.AnswerText = dto.AnswerText;
            question.AnsweredAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Answer updated" });
        }
    }
}
