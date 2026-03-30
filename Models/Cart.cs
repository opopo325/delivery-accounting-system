#nullable disable
using System.Collections.Generic;
using System.Linq;

namespace DeliverySystem;

public class Cart
{
    public List<Product> Items { get; private set; } = new List<Product>();
    public Coupon AppliedCoupon { get; private set; } = null;

    public void AddProduct(Product product) { Items.Add(product); }

    public bool RemoveProduct(Product product) { return Items.Remove(product); }

    public void ApplyCoupon(Coupon coupon) { AppliedCoupon = coupon; }

    public void Clear() { Items.Clear(); AppliedCoupon = null; }

    public decimal CalculateTotal()
    {
        decimal sum = Items.Sum(p => p.Price);
        if (AppliedCoupon != null && AppliedCoupon.IsValid())
        {
            // Math.Round виправляє 150 * 20% = 29.9999... → 30.00
            sum -= Math.Round(sum * (AppliedCoupon.DiscountPercentage / 100m), 2);
        }
        return sum;
    }
}
