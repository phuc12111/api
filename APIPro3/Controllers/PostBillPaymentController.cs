﻿using APIPro3.Entities;
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

            if (string.IsNullOrEmpty(payment.BillNumber) || string.IsNullOrEmpty(payment.Status))
            {
                return BadRequest("Thông tin hóa đơn và trạng thái không được để trống.");
            }

            // Lấy hóa đơn từ BillNumber (InvoiceId)
            var invoice = _context.Invoices.FirstOrDefault(i => i.InvoiceId == payment.BillNumber);
            if (invoice == null)
            {
                return NotFound("Hóa đơn không tồn tại.");
            }

            // Lấy số tiền từ hóa đơn
            payment.Amount = invoice.Amount;

            // Kiểm tra trạng thái hóa đơn
            if (invoice.Status)
            {
                return BadRequest("Hóa đơn đã được thanh toán.");
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
            if (account.AccountBalance == null || account.AccountBalance < payment.Amount)
            {
                return BadRequest("Số dư tài khoản không đủ để thanh toán.");
            }

            // Trừ số tiền thanh toán từ số dư tài khoản
            account.AccountBalance -= payment.Amount;
            account.LastUpdated = DateTime.UtcNow;

            // Cập nhật trạng thái hóa đơn thành "đã thanh toán"
            invoice.Status = true;

            // Tạo thanh toán hóa đơn mới
            payment.UserId = userId; // Gán UserId từ token
            payment.PaymentDate = DateTime.UtcNow;

            _context.PostBillPayments.Add(payment);

            // Cập nhật thông tin tài khoản và hóa đơn vào cơ sở dữ liệu
            _context.Accounts.Update(account);
            _context.Invoices.Update(invoice);
            _context.SaveChanges();

            // Trả về thông tin thanh toán đã tạo
            return CreatedAtAction(nameof(GetById), new { id = payment.PaymentId }, payment);
        }


        [HttpPost("CreateVNPay")]
        public IActionResult CreateVNPay([FromBody] PostBillPayment billpay)
        {
            // Kiểm tra dữ liệu đầu vào
            if (billpay == null || string.IsNullOrEmpty(billpay.BillNumber) || string.IsNullOrEmpty(billpay.Status))
            {
                return BadRequest("Thông tin không hợp lệ.");
            }

            // Tìm hóa đơn (Invoice) dựa trên BillNumber
            var invoice = _context.Invoices.FirstOrDefault(i => i.InvoiceId == billpay.BillNumber);
            if (invoice == null)
            {
                return NotFound("Hóa đơn không tồn tại.");
            }

            // Lấy số tiền từ hóa đơn
            billpay.Amount = invoice.Amount;

            // Kiểm tra trạng thái hóa đơn
            if (invoice.Status)
            {
                return BadRequest("Hóa đơn đã được thanh toán.");
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

            // Kiểm tra tài khoản người dùng
            var account = _context.Accounts.FirstOrDefault(a => a.UserId == userId);
            if (account == null)
            {
                return NotFound("Tài khoản không tồn tại.");
            }

            // Kiểm tra số dư tài khoản trước khi trừ tiền
            if (account.AccountBalance == null || account.AccountBalance < billpay.Amount)
            {
                return BadRequest("Số dư tài khoản không đủ để thanh toán.");
            }

            // Trừ số tiền thanh toán từ số dư tài khoản
            account.AccountBalance -= billpay.Amount;
            account.LastUpdated = DateTime.UtcNow;

            // Đổi trạng thái hóa đơn sang "Đã thanh toán"
            invoice.Status = true;

            // Lưu bản ghi thanh toán
            _context.PostBillPayments.Add(billpay);

            // Cập nhật thông tin tài khoản và hóa đơn vào cơ sở dữ liệu
            _context.Accounts.Update(account);
            _context.Invoices.Update(invoice);

            // Lưu tất cả các thay đổi vào cơ sở dữ liệu
            _context.SaveChanges();

            // Tạo link thanh toán (nếu cần)
            var paymentUrl = _vnPayService.CreatePostBillPaymentUrl(billpay, HttpContext);

            // Trả về thông tin bản ghi đã lưu và link thanh toán
            return Ok(new
            {
                rechargeId = billpay.PaymentId,
                billNumber = billpay.BillNumber,
                amount = billpay.Amount,
                status = invoice.Status ? "Paid" : "Pending",
                paymentUrl
            });
        }


        [HttpGet("GetPaymentRevenueReport")]
        public IActionResult GetPaymentRevenueReport()
        {
            var paymentRevenueReport = _context.PostBillPayments
                .Where(p => p.PaymentDate.HasValue) // Lọc giao dịch có PaymentDate
                .GroupBy(p => p.PaymentDate.Value.Date) // Nhóm theo ngày
                .Select(g => new
                {
                    Date = g.Key, // Sử dụng kiểu DateTime
                    TotalRevenue = g.Sum(p => p.Amount) // Tính tổng doanh thu
                })
                .AsEnumerable() // Chuyển sang xử lý phía client
                .Select(r => new
                {
                    Date = r.Date.ToString("yyyy-MM-dd"), // Chuyển đổi sang chuỗi phía client
                    r.TotalRevenue
                })
                .OrderBy(r => r.Date) // Sắp xếp theo ngày
                .ToList();

            return Ok(paymentRevenueReport);
        }






        [HttpGet("GetFullPaymentRevenueReport")]
        public IActionResult GetFullPaymentRevenueReport()
        {
            // Lấy ngày đầu tiên và ngày cuối cùng trong bảng dữ liệu
            var firstDate = _context.PostBillPayments
                .Where(p => p.PaymentDate.HasValue)
                .OrderBy(p => p.PaymentDate)
                .Select(p => p.PaymentDate.Value.Date)
                .FirstOrDefault();

            var lastDate = _context.PostBillPayments
                .Where(p => p.PaymentDate.HasValue)
                .OrderByDescending(p => p.PaymentDate)
                .Select(p => p.PaymentDate.Value.Date)
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
            var revenueData = _context.PostBillPayments
                .Where(p => p.PaymentDate.HasValue)
                .GroupBy(p => p.PaymentDate.Value.Date)
                .Select(g => new
                {
                    Date = g.Key,
                    TotalRevenue = g.Sum(p => p.Amount)
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
