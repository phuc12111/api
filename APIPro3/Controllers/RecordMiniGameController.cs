using APIPro3.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace APIPro3.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RecordMiniGameController : ControllerBase
    {
        private readonly RechargeOnlineSystemContext _context;

        public RecordMiniGameController(RechargeOnlineSystemContext context)
        {
            _context = context;
        }

        // GET: api/RecordMiniGame
        [HttpGet]
        public IActionResult GetAll()
        {
            var records = _context.RecordMiniGames.ToList();
            return Ok(records);
        }

        [Authorize]
        [HttpPost]
        public IActionResult Create([FromBody] RecordMiniGame record)
        {
            // Lấy UserId từ token (Claim)
            var userId = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("Không tìm thấy UserId trong token.");
            }

            // Kiểm tra SpinTurn có hợp lệ hay không
            if (record.SpinTurn < 0)
            {
                return BadRequest("SpinTurn phải lớn hơn hoặc bằng 0.");
            }

            // Kiểm tra xem UserId có tồn tại trong bảng Users
            var userExists = _context.Users.Any(u => u.UserId.ToString() == userId);
            if (!userExists)
            {
                return NotFound("Người dùng không tồn tại.");
            }

            // Tạo RecordMiniGame mới
            var newRecord = new RecordMiniGame
            {
                UserId = int.Parse(userId), // Lấy UserId từ token
                SpinTurn = record.SpinTurn,
            };

            // Lưu bản ghi vào cơ sở dữ liệu
            _context.RecordMiniGames.Add(newRecord);
            _context.SaveChanges();

            // Trả về bản ghi vừa tạo
            return CreatedAtAction(nameof(GetAll), new { id = newRecord.RecordId }, newRecord);
        }

    }
}
