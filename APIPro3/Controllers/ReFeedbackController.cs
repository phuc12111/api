using APIPro3.Entities;
using Microsoft.AspNetCore.Mvc;

namespace APIPro3.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReFeedbackController : ControllerBase
    {
        private readonly RechargeOnlineSystemContext _context;

        public ReFeedbackController(RechargeOnlineSystemContext context)
        {
            _context = context;
        }

        // GET: api/ReFeedback
        [HttpGet]
        public IActionResult GetAll()
        {
            var reFeedbacks = _context.ReFeedbacks.ToList();
            return Ok(reFeedbacks);
        }

        // POST: api/ReFeedback
        [HttpPost]
        public IActionResult Create([FromBody] ReFeedback reFeedback)
        {
            // Kiểm tra tính hợp lệ của dữ liệu đầu vào
            if (string.IsNullOrEmpty(reFeedback.RefeedbackText))
            {
                return BadRequest("Nội dung phản hồi không được để trống.");
            }

            if (string.IsNullOrEmpty(reFeedback.Status))
            {
                return BadRequest("Trạng thái không được để trống.");
            }

            // Kiểm tra feedback_id có tồn tại trong bảng Feedback
            var feedbackExists = _context.Feedbacks.Any(f => f.FeedbackId == reFeedback.FeedbackId);
            if (!feedbackExists)
            {
                return NotFound("Feedback không tồn tại.");
            }

            // Tạo ReFeedback mới
            reFeedback.RefeedbackDate = DateTime.UtcNow;

            _context.ReFeedbacks.Add(reFeedback);
            _context.SaveChanges();

            return CreatedAtAction(nameof(GetAll), new { id = reFeedback.RefeedbackId }, reFeedback);
        }
    }
}
