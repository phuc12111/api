using APIPro3.Entities;
using Microsoft.AspNetCore.Mvc;

namespace APIPro3.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DateManageController : ControllerBase
    {
        private readonly RechargeOnlineSystemContext _context;

        public DateManageController(RechargeOnlineSystemContext context)
        {
            _context = context;
        }

        // GET: api/DateManage
        [HttpGet]
        public IActionResult GetAll()
        {
            var dates = _context.DateManages.ToList();
            return Ok(dates);
        }

        // POST: api/DateManage
        [HttpPost]
        public IActionResult Create([FromBody] DateManage dateManage)
        {
            // Kiểm tra tính hợp lệ của dữ liệu đầu vào
            if (dateManage.DateSignin == null)
            {
                return BadRequest("DateSignIn không được để trống.");
            }

            // Tạo bản ghi DateManage mới
            _context.DateManages.Add(dateManage);
            _context.SaveChanges();

            return CreatedAtAction(nameof(GetAll), new { id = dateManage.DateId }, dateManage);
        }
    }
}
