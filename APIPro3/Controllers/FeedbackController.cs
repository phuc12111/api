using APIPro3.Entities;
using Microsoft.AspNetCore.Mvc;

namespace APIPro3.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FeedbackController : ControllerBase
    {
        private readonly RechargeOnlineSystemContext _context;

        public FeedbackController(RechargeOnlineSystemContext context)
        {
            _context = context;
        }

        // GET: api/Feedback
        [HttpGet]
        public IActionResult GetAll()
        {
            var feedbacks = _context.Feedbacks.ToList();
            return Ok(feedbacks);
        }

        // POST: api/Feedback
        [HttpPost]
        public IActionResult Create([FromBody] Feedback feedback)
        {
            // Kiểm tra tính hợp lệ của dữ liệu đầu vào
            if (string.IsNullOrEmpty(feedback.FeedbackText))
            {
                return BadRequest("Nội dung phản hồi không được để trống.");
            }

            if (string.IsNullOrEmpty(feedback.Status))
            {
                return BadRequest("Trạng thái phản hồi không được để trống.");
            }

            // Lấy UserId từ token
            var userIdClaim = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "UserId");
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return Unauthorized("Không tìm thấy UserId trong token.");
            }

            // Kiểm tra UserId có tồn tại trong bảng Users
            var userExists = _context.Users.Any(u => u.UserId == userId);
            if (!userExists)
            {
                return NotFound("Người dùng không tồn tại.");
            }

            // Tạo phản hồi mới
            feedback.UserId = userId; // Gán UserId từ token
            feedback.FeedbackDate = DateTime.UtcNow;

            _context.Feedbacks.Add(feedback);
            _context.SaveChanges();

            return CreatedAtAction(nameof(GetAll), new { id = feedback.FeedbackId }, feedback);
        }
    }
}
