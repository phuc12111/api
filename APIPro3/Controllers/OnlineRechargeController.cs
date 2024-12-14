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

        [Authorize]
        [HttpPost]
        public IActionResult Create([FromBody] OnlineRecharge recharge)
        {
            // Kiểm tra dữ liệu đầu vào
            if (recharge == null || recharge.Amount <= 0 || string.IsNullOrEmpty(recharge.Status))
            {
                return BadRequest("Thông tin thanh toán không hợp lệ.");
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

            // Lấy thông tin tài khoản của người dùng
            var account = _context.Accounts.FirstOrDefault(a => a.UserId == userId);
            if (account == null)
            {
                return NotFound("Tài khoản không tồn tại.");
            }

            // Kiểm tra số dư tài khoản
            if (account.AccountBalance == null || account.AccountBalance < recharge.Amount)
            {
                return BadRequest("Số dư tài khoản không đủ để thực hiện giao dịch.");
            }

            // Trừ số tiền từ tài khoản
            account.AccountBalance -= recharge.Amount;
            account.LastUpdated = DateTime.UtcNow;

            // Gán các thông tin cần thiết cho bản ghi nạp tiền
            recharge.UserId = userId;
            recharge.RechargeDate = DateTime.UtcNow;

            // Thêm bản ghi nạp tiền và cập nhật tài khoản
            _context.OnlineRecharges.Add(recharge);
            _context.Accounts.Update(account);
            _context.SaveChanges();

            // Trả về thông tin giao dịch đã tạo
            return CreatedAtAction(nameof(GetById), new { id = recharge.RechargeId }, recharge);
        }

        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            // Tìm giao dịch theo ID
            var recharge = _context.OnlineRecharges.FirstOrDefault(r => r.RechargeId == id);
            if (recharge == null)
            {
                return NotFound($"Không tìm thấy giao dịch với ID: {id}");
            }

            // Trả về thông tin giao dịch
            return Ok(recharge);
        }




        [HttpPost("CreateVNPay")]
        public IActionResult CreateVNPay([FromBody] OnlineRecharge Onlbillpay)
        {
            // Kiểm tra dữ liệu đầu vào
            if (Onlbillpay == null || Onlbillpay.Amount <= 0 || string.IsNullOrEmpty(Onlbillpay.Status))
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
            Onlbillpay.UserId = userId;
            Onlbillpay.RechargeDate = DateTime.UtcNow;

            // Lưu bản ghi vào cơ sở dữ liệu
            _context.OnlineRecharges.Add(Onlbillpay);
            _context.SaveChanges();

            // Tạo link thanh toán
            var paymentUrl = _vnPayService.CreateOnlineRechargetUrl(Onlbillpay, HttpContext);

            // Trả về thông tin bản ghi đã lưu và link thanh toán
            return Ok(new
            {
                rechargeId = Onlbillpay.RechargeId,
                paymentUrl
            });
        }




        [HttpGet("GetRevenueReport")]
        public IActionResult GetRevenueReport()
        {
            var revenueReport = _context.OnlineRecharges
                .Where(r => r.RechargeDate.HasValue) // Lọc giao dịch có RechargeDate
                .GroupBy(r => r.RechargeDate.Value.Date) // Nhóm theo ngày
                .Select(g => new
                {
                    Date = g.Key, // Sử dụng kiểu DateTime
                    TotalRevenue = g.Sum(r => r.Amount) // Tính tổng doanh thu
                })
                .AsEnumerable() // Chuyển sang xử lý phía client
                .Select(r => new
                {
                    Date = r.Date.ToString("yyyy-MM-dd"), // Chuyển đổi sang chuỗi phía client
                    r.TotalRevenue
                })
                .OrderBy(r => r.Date) // Sắp xếp theo ngày
                .ToList();

            return Ok(revenueReport);
        }




        [HttpGet("GetFullOnlineRechargeRevenueReport")]
        public IActionResult GetFullOnlineRechargeRevenueReport()
        {
            // Lấy ngày đầu tiên và ngày cuối cùng trong bảng dữ liệu
            var firstDate = _context.OnlineRecharges
                .Where(r => r.RechargeDate.HasValue)
                .OrderBy(r => r.RechargeDate)
                .Select(r => r.RechargeDate.Value.Date)
                .FirstOrDefault();

            var lastDate = _context.OnlineRecharges
                .Where(r => r.RechargeDate.HasValue)
                .OrderByDescending(r => r.RechargeDate)
                .Select(r => r.RechargeDate.Value.Date)
                .FirstOrDefault();

            if (firstDate == default || lastDate == default)
            {
                return NotFound("Không có dữ liệu giao dịch.");
            }

            // Tạo danh sách tất cả các ngày từ ngày đầu tiên đến ngày cuối cùng
            var allDates = Enumerable.Range(0, (lastDate - firstDate).Days + 1)
                .Select(offset => firstDate.AddDays(offset))
                .ToList();

            // Lấy doanh thu theo ngày
            var revenueData = _context.OnlineRecharges
                .Where(r => r.RechargeDate.HasValue)
                .GroupBy(r => r.RechargeDate.Value.Date)
                .Select(g => new
                {
                    Date = g.Key,
                    TotalRevenue = g.Sum(r => r.Amount)
                })
                .ToDictionary(r => r.Date, r => r.TotalRevenue);

            // Tạo kết quả cuối cùng bao gồm các ngày không có doanh thu
            var fullRevenueReport = allDates.Select(date => new
            {
                Date = date.ToString("yyyy-MM-dd"),
                TotalRevenue = revenueData.ContainsKey(date) ? revenueData[date] : 0
            })
            .OrderBy(r => r.Date)
            .ToList();

            return Ok(fullRevenueReport);
        }




    }
}
