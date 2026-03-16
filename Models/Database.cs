using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace DeliverySystem;

public class Database
{
    private readonly string usersFile = "users.json";
    private readonly string ordersFile = "orders.json";
    private readonly JsonSerializerOptions options = new JsonSerializerOptions { WriteIndented = true };
        public List<Product> LoadCatalog()
        {
            string menuFile = "menu.json";
            if (!File.Exists(menuFile))
            {
                var defaultCatalog = new List<Product>
                {
                    new Product(1, "Піца Маргарита", 250m), new Product(2, "Піца Пепероні", 300m),
                    new Product(3, "Суші Філадельфія", 400m), new Product(4, "Бургер", 180m),
                    new Product(5, "Кока-Кола 0.5л", 40m) 
                };
                File.WriteAllText(menuFile, JsonSerializer.Serialize(defaultCatalog, options));
                return defaultCatalog;
            }
            return JsonSerializer.Deserialize<List<Product>>(File.ReadAllText(menuFile), options);
        }
    public List<User> LoadUsers()
    {
        if (!File.Exists(usersFile))
        {
            var defaultUsers = new List<User> { new Driver(99, "Максім", "0669876543", "12345", "ВН1234АА") };
            SaveUsers(defaultUsers);
            return defaultUsers;
        }
        string json = File.ReadAllText(usersFile);
        return JsonSerializer.Deserialize<List<User>>(json, options) ?? new List<User>();
    }

    public void SaveUsers(List<User> users)
    {
        string json = JsonSerializer.Serialize(users, options);
        File.WriteAllText(usersFile, json);
    }

    public List<Order> LoadOrders()
    {
        if (!File.Exists(ordersFile)) return new List<Order>();
        string json = File.ReadAllText(ordersFile);
        return JsonSerializer.Deserialize<List<Order>>(json, options) ?? new List<Order>();
    }

    public void SaveOrders(List<Order> orders)
    {
        string json = JsonSerializer.Serialize(orders, options);
        File.WriteAllText(ordersFile, json);
    }
}