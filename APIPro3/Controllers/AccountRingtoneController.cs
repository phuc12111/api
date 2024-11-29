using APIPro3.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace APIPro3.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountRingtoneController : ControllerBase
    {
        private readonly RechargeOnlineSystemContext _context;

        public AccountRingtoneController(RechargeOnlineSystemContext context)
        {
            _context = context;
        }

        // GET: api/AccountRingtone
        [HttpGet]
        public IActionResult GetAll()
        {
            var accountRingtones = _context.AccountRingtones
                .Include(ar => ar.Account)
                .Include(ar => ar.Ringtone)
                .ToList();

            return Ok(accountRingtones);
        }

        // POST: api/AccountRingtone
        [HttpPost]
        public IActionResult Create([FromBody] AccountRingtone accountRingtone)
        {
            // Kiểm tra tính hợp lệ của dữ liệu đầu vào
            if (accountRingtone.AccountId <= 0 || accountRingtone.RingtoneId <= 0)
            {
                return BadRequest("AccountId và RingtoneId không hợp lệ.");
            }

            // Tạo bản ghi Account_Ringtone mới
            var newAccountRingtone = new AccountRingtone
            {
                AccountId = accountRingtone.AccountId,
                RingtoneId = accountRingtone.RingtoneId,
                AccountRingtoneDate = DateTime.UtcNow
            };

            _context.AccountRingtones.Add(newAccountRingtone);
            _context.SaveChanges();

            return CreatedAtAction(nameof(GetAll), new { accountId = newAccountRingtone.AccountId, ringtoneId = newAccountRingtone.RingtoneId }, newAccountRingtone);
        }
    }
}
