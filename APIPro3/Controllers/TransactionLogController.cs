using APIPro3.Entities;
using Microsoft.AspNetCore.Mvc;

namespace APIPro3.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TransactionLogController : ControllerBase
    {
        private readonly RechargeOnlineSystemContext _context;

        public TransactionLogController(RechargeOnlineSystemContext context)
        {
            _context = context;
        }

        // GET: api/TransactionLog
        [HttpGet]
        public IActionResult GetAll()
        {
            var transactionLogs = _context.TransactionLogs.ToList();
            return Ok(transactionLogs);
        }

        [HttpPost]
        public IActionResult Create([FromBody] TransactionLog transactionLog)
        {
            // Kiểm tra tính hợp lệ của dữ liệu đầu vào
            if (string.IsNullOrEmpty(transactionLog.TransactionType) ||
                transactionLog.Amount <= 0 ||
                string.IsNullOrEmpty(transactionLog.Status))
            {
                return BadRequest("TransactionType, Amount và Status không được để trống hoặc không hợp lệ.");
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

            // Tạo bản ghi TransactionLog mới
            transactionLog.UserId = userId; // Gán UserId từ token
            transactionLog.TransactionDate = DateTime.UtcNow; // Gán thời gian giao dịch

            _context.TransactionLogs.Add(transactionLog);
            _context.SaveChanges();

            return CreatedAtAction(nameof(GetAll), new { id = transactionLog.TransactionId }, transactionLog);
        }
    }
}
