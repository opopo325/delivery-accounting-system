#nullable disable
using System.Collections.Generic;
using System.Linq;
namespace DeliverySystem; 
public class Cart
{
    public List<Product> Items { get; private set; } = new List<Product>();
    public Coupon AppliedCoupon { get; private set; } = null;

    public void AddProduct(Product product)
    {
        Items.Add(product);
    }

   public void RemoveProduct(Product product)
        {
            if (Items.Remove(product))
                Console.WriteLine($"[-] Товар '{product.Name}' успішно видалено з кошика.");
            else
                Console.WriteLine($"[!] Цього товару і так немає в кошику.");
        }

    public void ApplyCoupon(Coupon coupon)
    {
        AppliedCoupon = coupon;
    }

    public void Clear()
    {
        Items.Clear();
        AppliedCoupon = null;
    }
    public decimal CalculateTotal()
        {
            decimal sum = Items.Sum(p => p.Price);
            if (AppliedCoupon != null && AppliedCoupon.IsValid())
            {
                sum -= sum * (AppliedCoupon.DiscountPercentage / 100m);
            }
            return Math.Round(sum, 2); 
        }

}