using APIPro3.Entities;
using APIPro3.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace APIPro3.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        private readonly RechargeOnlineSystemContext _context;

        public LoginController(RechargeOnlineSystemContext ctx)
        {
            _context = ctx;
        }
        [HttpGet]
        public IActionResult GetAll()
        {
            return Ok(_context.Users.ToList());
        }


        [HttpPost("login")]
        public IActionResult Login([FromBody] UseRequestModel useRequestModel)
        {
            if (string.IsNullOrEmpty(useRequestModel.Email) || string.IsNullOrEmpty(useRequestModel.Password))
            {
                return BadRequest("Username hoặc Password không thể để trống.");
            }

            // Kiểm tra thông tin đăng nhập từ cơ sở dữ liệu
            var user = _context.Users
                .FirstOrDefault(u => u.Email == useRequestModel.Email && u.Password == useRequestModel.Password);

            if (user == null)
            {
                return BadRequest("Đăng nhập không thành công. Vui lòng kiểm tra lại thông tin đăng nhập.");
            }

            // Tạo Claims từ thông tin đăng nhập
            var claims = new List<Claim>
    {
        new Claim(ClaimTypes.Email, user.Email),
        new Claim(ClaimTypes.Name, user.UserName),
        new Claim(ClaimTypes.Role, user.Role ?? "Guest"), // Giả sử Role lưu trong cơ sở dữ liệu
        new Claim("UserId", user.UserId.ToString()) // Mã hóa UserId
    };

            // Tạo Token Key và Signing Credentials
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("y9A4mF@hKD#EPLt!ZrxVQ9J%2XvNcTfYWkj87uF3eM$1opBLG^dR")); // Key mạnh
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // Tạo JWT token
            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(30), // Token hết hạn sau 30 phút
                signingCredentials: creds
            );

            // Chuyển đổi token thành chuỗi và trả về
            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            // Trả về token và thông tin người dùng
            return Ok(new
            {
                Token = tokenString,
                Expiration = token.ValidTo
            });
        }

        [Authorize]
        [HttpGet("get-user-details")]
        public IActionResult GetUserDetails()
        {
            // Lấy thông tin từ Claims trong token
            var userId = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("Không tìm thấy UserId trong token.");
            }

            // Tìm thông tin user từ cơ sở dữ liệu
            var user = _context.Users.FirstOrDefault(u => u.UserId.ToString() == userId);
            if (user == null)
            {
                return NotFound("Người dùng không tồn tại.");
            }

            // Trả về thông tin user
            return Ok(new
            {
                UserId = user.UserId,
                UsUserName = user.UserName,
                Email = user.Email,
                Role = user.Role
            });
        }


        [HttpPost("register")]
        public IActionResult CreateUser([FromBody] User userRequest)
        {
            // Kiểm tra tính hợp lệ của dữ liệu đầu vào
            if (string.IsNullOrEmpty(userRequest.UserName) ||
                string.IsNullOrEmpty(userRequest.Password) ||
                string.IsNullOrEmpty(userRequest.Email) ||
                string.IsNullOrEmpty(userRequest.Phone))
            {
                return BadRequest("Thông tin người dùng không được để trống.");
            }

            // Kiểm tra email hoặc số điện thoại đã tồn tại
            var existingUser = _context.Users.FirstOrDefault(u => u.Email == userRequest.Email || u.Phone == userRequest.Phone);
            if (existingUser != null)
            {
                return Conflict("Email hoặc số điện thoại đã tồn tại.");
            }

            // Tạo đối tượng User
            var newUser = new User
            {
                UserName = userRequest.UserName,
                Password = userRequest.Password, // Nên mã hóa mật khẩu trước khi lưu
                Email = userRequest.Email,
                Phone = userRequest.Phone,
                Role = userRequest.Role ?? "Customer", // Gán vai trò mặc định là Customer
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            // Lưu vào cơ sở dữ liệu
            _context.Users.Add(newUser);
            _context.SaveChanges();

            // Trả về thông tin người dùng vừa tạo
            return Ok(newUser);

        }








        [HttpGet("UserRegistrationStats")]
        public IActionResult GetUserRegistrationStats()
        {
            var stats = _context.Users
                .Where(u => u.CreatedAt.HasValue) // Loại bỏ các bản ghi có CreatedAt null
                .GroupBy(u => u.CreatedAt.Value.Date) // Nhóm theo ngày
                .Select(g => new
                {
                    Date = g.Key, // Lấy trực tiếp DateTime, xử lý định dạng sau
                    UserCount = g.Count() // Đếm số lượng user
                })
                .AsEnumerable() // Chuyển sang xử lý phía client
                .Select(g => new
                {
                    Date = g.Date.ToString("yyyy-MM-dd"), // Định dạng ngày thành chuỗi
                    g.UserCount
                })
                .OrderBy(stat => stat.Date) // Sắp xếp theo ngày
                .ToList();

            return Ok(stats);
        }






        [HttpGet("GetUserRegistrationReport")]
        public IActionResult GetUserRegistrationReport()
        {
            // Lấy ngày đầu tiên và ngày cuối cùng trong bảng dữ liệu
            var firstDate = _context.Users
                .OrderBy(u => u.CreatedAt)
                .Select(u => u.CreatedAt.Value.Date)
                .FirstOrDefault();

            var lastDate = _context.Users
                .OrderByDescending(u => u.CreatedAt)
                .Select(u => u.CreatedAt.Value.Date)
                .FirstOrDefault();

            if (firstDate == default || lastDate == default)
            {
                return NotFound("Không có dữ liệu người dùng.");
            }

            // Tạo danh sách tất cả các ngày từ ngày đầu tiên đến ngày cuối cùng
            var allDates = Enumerable.Range(0, (lastDate - firstDate).Days + 1)
                .Select(offset => firstDate.AddDays(offset))
                .ToList();

            // Nhóm và đếm số lượng người dùng đăng ký mỗi ngày
            var userRegistrations = _context.Users
                .GroupBy(u => u.CreatedAt.Value.Date)
                .Select(g => new
                {
                    Date = g.Key,
                    UserCount = g.Count()
                })
                .ToDictionary(r => r.Date, r => r.UserCount);

            // Tạo kết quả cuối cùng bao gồm các ngày không có đăng ký
            var registrationReport = allDates.Select(date => new
            {
                Date = date.ToString("yyyy-MM-dd"),
                UserCount = userRegistrations.ContainsKey(date) ? userRegistrations[date] : 0
            })
            .OrderBy(r => r.Date)
            .ToList();

            return Ok(registrationReport);
        }




        [Authorize]
        [HttpPost("UpdateProfile")]
        public IActionResult UpdateUserProfile([FromBody] UpdateUserProfileModel model)
        {
            // Lấy UserId từ token
            var userIdClaim = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "UserId");
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return Unauthorized("Không tìm thấy UserId trong token.");
            }

            // Tìm người dùng dựa trên UserId
            var user = _context.Users.FirstOrDefault(u => u.UserId == userId);
            if (user == null)
            {
                return NotFound("Người dùng không tồn tại.");
            }

            // Cập nhật thông tin cá nhân
            if (!string.IsNullOrEmpty(model.UserName))
                user.UserName = model.UserName;

            if (!string.IsNullOrEmpty(model.Email))
                user.Email = model.Email;

            if (!string.IsNullOrEmpty(model.Phone))
                user.Phone = model.Phone;

            // Cập nhật ngày chỉnh sửa cuối cùng
            user.UpdatedAt = DateTime.UtcNow;

            // Lưu thay đổi
            _context.Users.Update(user);
            _context.SaveChanges();

            return Ok(new
            {
                Message = "Cập nhật thông tin cá nhân thành công.",
                User = new
                {
                    user.UserId,
                    user.UserName,
                    user.Email,
                    user.Phone,
                    user.UpdatedAt
                }
            });
        }


    }
}
