﻿using APIPro3.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace APIPro3.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly RechargeOnlineSystemContext _context;

        public AccountController(RechargeOnlineSystemContext context)
        {
            _context = context;
        }

        // GET: api/Account
        [HttpGet]
        public IActionResult GetAll()
        {
            var accounts = _context.Accounts.ToList();
            return Ok(accounts);
        }

        // POST: api/Account
        [Authorize]
        [HttpPost]
        public IActionResult Create([FromBody] Account account)
        {
            // Kiểm tra tính hợp lệ của dữ liệu đầu vào
            if (account.AccountBalance <= 0)
            {
                return BadRequest("Số dư nạp thêm phải lớn hơn 0.");
            }

            // Lấy UserId từ token (Claim)
            var userId = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("Không tìm thấy UserId trong token.");
            }

            // Chuyển UserId từ string sang int
            if (!int.TryParse(userId, out int parsedUserId))
            {
                return BadRequest("UserId không hợp lệ.");
            }

            // Kiểm tra xem người dùng có tồn tại không
            var user = _context.Users.FirstOrDefault(u => u.UserId == parsedUserId);
            if (user == null)
            {
                return NotFound("Không tìm thấy người dùng với UserId từ token.");
            }

            // Kiểm tra xem tài khoản đã tồn tại hay chưa
            var existingAccount = _context.Accounts.FirstOrDefault(a => a.UserId == parsedUserId);

            if (existingAccount != null)
            {
                // Cập nhật số dư
                existingAccount.AccountBalance += account.AccountBalance;
                existingAccount.LastUpdated = DateTime.UtcNow;

                _context.Accounts.Update(existingAccount);
                _context.SaveChanges();

                return Ok(new
                {
                    Message = "Số dư đã được cập nhật.",
                    UpdatedBalance = existingAccount.AccountBalance
                });
            }

            // Nếu chưa có tài khoản, tạo tài khoản mới
            var newAccount = new Account
            {
                UserId = parsedUserId, // Liên kết đúng với UserId từ token
                AccountBalance = account.AccountBalance,
                LastUpdated = DateTime.UtcNow,
                User = user // Gắn đối tượng User
            };

            _context.Accounts.Add(newAccount);
            _context.SaveChanges();

            return CreatedAtAction(nameof(GetAll), new { id = newAccount.AccountId }, newAccount);
        }

    }
}
