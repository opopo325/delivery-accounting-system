    namespace DeliverySystem;
    public class Coupon
    {
        public string Code { get; set; }
        public decimal DiscountPercentage { get; set; }

        public Coupon(string code, decimal discount)
        {
            Code = code; DiscountPercentage = discount;
        }
        public bool IsValid() => true; 
    }