#nullable disable

namespace DeliverySystem;

// Єдине місце де оголошено OrderStatus — прибрано дублікат з Program.cs
public enum OrderStatus { New, InProgress, Delivered, Canceled }

public class Order
{
    public int Id { get; set; }
    public string ClientName { get; set; }
    public string ClientPhone { get; set; }
    public string Address { get; set; }
    public decimal TotalPrice { get; set; }
    public OrderStatus Status { get; set; }
    public List<string> Items { get; set; } = new List<string>();
    public int? DriverId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now; // час оформлення
}
