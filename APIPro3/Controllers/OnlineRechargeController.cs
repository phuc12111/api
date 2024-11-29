using APIPro3.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace APIPro3.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OnlineRechargeController : ControllerBase
    {
        private readonly RechargeOnlineSystemContext _context;

        public OnlineRechargeController(RechargeOnlineSystemContext context)
        {
            _context = context;
        }

        // GET: api/OnlineRecharge
        [HttpGet]
        public IActionResult GetAll()
        {
            var recharges = _context.OnlineRecharges.ToList();
            return Ok(recharges);
        }

        [Authorize]
        [HttpPost]
        public IActionResult Create([FromBody] OnlineRecharge recharge)
        {
            // Kiểm tra dữ liệu đầu vào
            if (recharge == null || recharge.Amount <= 0 || string.IsNullOrEmpty(recharge.Status))
            {
                return BadRequest("Thông tin không hợp lệ.");
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

            // Tạo bản ghi nạp tiền trực tuyến mới
            recharge.UserId = userId; // Gán UserId từ token
            recharge.RechargeDate = DateTime.UtcNow;

            _context.OnlineRecharges.Add(recharge);
            _context.SaveChanges();

            // Trả về thông tin nạp tiền đã tạo
            return CreatedAtAction(nameof(GetAll), new { id = recharge.RechargeId }, recharge);
        }

    }
}
