using APIPro3.Entities;
using Microsoft.AspNetCore.Mvc;

namespace APIPro3.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RingtoneController : ControllerBase
    {
        private readonly RechargeOnlineSystemContext _context;

        public RingtoneController(RechargeOnlineSystemContext context)
        {
            _context = context;
        }

        // GET: api/Ringtone
        [HttpGet]
        public IActionResult GetAll()
        {
            var ringtones = _context.Ringtones.ToList();
            return Ok(ringtones);
        }

        // POST: api/Ringtone
        [HttpPost]
        public IActionResult Create([FromBody] Ringtone ringtone)
        {
            // Kiểm tra tính hợp lệ của dữ liệu đầu vào
            if (string.IsNullOrEmpty(ringtone.RingtoneName) || string.IsNullOrEmpty(ringtone.FileUrl))
            {
                return BadRequest("Tên nhạc chuông và đường dẫn file không được để trống.");
            }

            // Tạo nhạc chuông mới
            var newRingtone = new Ringtone
            {
                RingtoneName = ringtone.RingtoneName,
                FileUrl = ringtone.FileUrl,
                RingtoneDate = DateTime.UtcNow
            };

            _context.Ringtones.Add(newRingtone);
            _context.SaveChanges();

            return CreatedAtAction(nameof(GetAll), new { id = newRingtone.RingtoneId }, newRingtone);
        }
    }
}
