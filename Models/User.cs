namespace DeliverySystem;
public abstract class User
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Phone { get; set; }
        public string Password { get; set; }

        public User(int id, string name, string phone, string password)
        {
            Id = id; Name = name; Phone = phone; Password = password;
        }
    }