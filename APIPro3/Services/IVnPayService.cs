using APIPro3.Entities;
using APIPro3.Models;

namespace APIPro3.Services;
public interface IVnPayService
{
    string CreatePaymentUrl(PaymentInformationModel model, HttpContext context);
    public string CreateOnlineRechargetUrl(OnlineRecharge model, HttpContext context);
    public string CreatePostBillPaymentUrl(PostBillPayment model, HttpContext context);
    PaymentResponseModel PaymentExecute(IQueryCollection collections);
}