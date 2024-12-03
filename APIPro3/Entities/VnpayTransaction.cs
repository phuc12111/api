namespace APIPro3.Entities
{
    public class VnpayRequest
    {
        public string OrderId { get; set; }
        public string OrderInfo { get; set; }
        public long Amount { get; set; }
        public string ReturnUrl { get; set; }
    }



}
