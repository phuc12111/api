using Microsoft.AspNetCore.Mvc;
using APIPro3.Models;
using APIPro3.Services;

namespace APIPro3.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentController : ControllerBase
    {
        private readonly IVnPayService _vnPayService;

        // Constructor nhận IVnPayService qua dependency injection
        public PaymentController(IVnPayService vnPayService)
        {
            _vnPayService = vnPayService;
        }

        // Phương thức POST tạo link thanh toán
        [HttpPost("create-payment-url")]
        public IActionResult CreatePaymentUrl(PaymentInformationModel model)
        {
            var url = _vnPayService.CreatePaymentUrl(model, HttpContext);

            return Ok(url);
        }

    }
}
