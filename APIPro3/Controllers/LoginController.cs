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



    }
}
