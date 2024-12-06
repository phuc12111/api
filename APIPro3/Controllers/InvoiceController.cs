using APIPro3.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace APIPro3.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InvoiceController : ControllerBase
    {

        private readonly RechargeOnlineSystemContext _context;

        public InvoiceController(RechargeOnlineSystemContext context)
        {
            _context = context;
        }
        // GET: api/Invoice
        [HttpGet]
        public IActionResult GetAll()
        {
            return Ok(_context.Invoices.ToList());
        }


        [HttpGet("{id}")]
        public IActionResult GetById(string id) // id kiểu string do InvoiceId là string
        {
            var invoice = _context.Invoices.FirstOrDefault(i => i.InvoiceId == id);

            if (invoice == null)
            {
                return NotFound(new { message = $"Invoice with ID {id} not found." });
            }

            return Ok(invoice);
        }

    }
}
