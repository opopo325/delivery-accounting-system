#nullable disable
using System.Collections.Generic;
using System.Linq;

namespace DeliverySystem; 
public class Client : User
    {
        public string Email { get; set; }
        public string DefaultAddress { get; set; }

        public Client(int id, string name, string phone, string pass, string email, string address) 
            : base(id, name, phone, pass)
        {
            Email = email; DefaultAddress = address;
        }
    }
