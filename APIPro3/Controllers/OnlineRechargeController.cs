using APIPro3.Entities;
using APIPro3.Models;
using APIPro3.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace APIPro3.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OnlineRechargeController : ControllerBase
    {
        private readonly RechargeOnlineSystemContext _context;
        private readonly IVnPayService _vnPayService;

        public OnlineRechargeController(RechargeOnlineSystemContext context, IVnPayService vnPayService)
        {
            _context = context;
            _vnPayService = vnPayService;
        }

        // GET: api/OnlineRecharge
        [HttpGet]
        public IActionResult GetAll()
        {
            var recharges = _context.OnlineRecharges.ToList();
            return Ok(recharges);
        }

        // POST: api/OnlineRecharge/create
        [HttpPost("create")]
        public IActionResult Create([FromBody] OnlineRecharge recharge)
        {
            // Kiểm tra dữ liệu đầu vào
            if (recharge == null || recharge.Amount <= 0 || string.IsNullOrEmpty(recharge.Status))
            {
                return BadRequest("Thông tin không hợp lệ.");
            }

            // Mặc định UserId là null
            int? userId = null;

            // Lấy UserId từ token (nếu có)
            var userIdClaim = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "UserId");
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int parsedUserId))
            {
                userId = parsedUserId; // Gán giá trị từ token nếu có
            }

            // Gán giá trị UserId (hoặc null nếu không có token)
            recharge.UserId = userId;
            recharge.RechargeDate = DateTime.UtcNow;

            // Lưu bản ghi vào cơ sở dữ liệu
            _context.OnlineRecharges.Add(recharge);
            _context.SaveChanges();

            // Tạo link thanh toán
            var paymentUrl = _vnPayService.CreateOnlineRechargetUrl(recharge, HttpContext);

            // Trả về thông tin bản ghi đã lưu và link thanh toán
            return Ok(new
            {
                rechargeId = recharge.RechargeId,
                paymentUrl
            });
        }


    }
}
