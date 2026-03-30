#nullable disable
using System.Collections.Generic;
using System.Linq;

namespace DeliverySystem; 
  
 public class Driver : User
    {
        public string CarNumber { get; set; }
        public bool IsAvailable { get; set; }
            public Driver() { }
        public Driver(int id, string name, string phone, string pass, string carNum) 
            : base(id, name, phone, pass)
        {
            CarNumber = carNum; IsAvailable = true;
        }
    }