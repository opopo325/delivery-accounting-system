  #nullable disable
  using System.Collections.Generic;
using System.Linq;

namespace DeliverySystem; 
    public class Order
        {
        public int Id { get; set; }
        public string ClientName { get; set; }
        public string ClientPhone { get; set; }
        public string Address { get; set; }
        public decimal TotalPrice { get; set; }
        public OrderStatus Status { get; set; }
            public List<string> Items { get; set; }
            }