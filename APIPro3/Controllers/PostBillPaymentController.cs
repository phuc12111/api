using APIPro3.Entities;
using APIPro3.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace APIPro3.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PostBillPaymentController : ControllerBase
    {
        private readonly RechargeOnlineSystemContext _context;
        private readonly IVnPayService _vnPayService;


        public PostBillPaymentController(RechargeOnlineSystemContext context, IVnPayService vnPayService)
        {
            _context = context;
            _vnPayService = vnPayService;
        }

        // GET: api/PostBillPayment
        [HttpGet]
        public IActionResult GetAll()
        {
            var payments = _context.PostBillPayments.ToList();
            return Ok(payments);
        }

        // GET: api/PostBillPayment/{id}
        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var payment = _context.PostBillPayments.FirstOrDefault(p => p.PaymentId == id);
            if (payment == null)
            {
                return NotFound($"Không tìm thấy thanh toán hóa đơn với ID: {id}");
            }
            return Ok(payment);
        }

        [Authorize]
        [HttpPost]
        public IActionResult Create([FromBody] PostBillPayment payment)
        {
            // Kiểm tra dữ liệu đầu vào
            if (payment == null)
            {
                return BadRequest("Dữ liệu thanh toán không hợp lệ.");
            }

            if (payment.Amount <= 0)
            {
                return BadRequest("Số tiền thanh toán phải lớn hơn 0.");
            }

            if (string.IsNullOrEmpty(payment.BillNumber) || string.IsNullOrEmpty(payment.Status))
            {
                return BadRequest("Thông tin hóa đơn và trạng thái không được để trống.");
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

            // Tạo thanh toán hóa đơn mới
            payment.UserId = userId; // Gán UserId từ token
            payment.PaymentDate = DateTime.UtcNow;

            _context.PostBillPayments.Add(payment);
            _context.SaveChanges();

            // Trả về thông tin thanh toán đã tạo
            return CreatedAtAction(nameof(GetById), new { id = payment.PaymentId }, payment);
        }




        [HttpPost("CreateVNPay")]
        public IActionResult CreateVNPay([FromBody] PostBillPayment billpay)
        {
            // Kiểm tra dữ liệu đầu vào
            if (billpay == null || billpay.Amount <= 0 || string.IsNullOrEmpty(billpay.Status))
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
            billpay.UserId = userId;
            billpay.PaymentDate = DateTime.UtcNow;

            // Lưu bản ghi vào cơ sở dữ liệu
            _context.PostBillPayments.Add(billpay);
            _context.SaveChanges();

            // Tạo link thanh toán
            var paymentUrl = _vnPayService.CreatePostBillPaymentUrl(billpay, HttpContext);

            // Trả về thông tin bản ghi đã lưu và link thanh toán
            return Ok(new
            {
                rechargeId = billpay.PaymentId,
                paymentUrl
            });
        }



    }
}
