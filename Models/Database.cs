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
        try
        {
            if (!File.Exists(menuFile))
            {
               var defaultCatalog = new List<Product> {
                        new Product(1, "🍕 Піца Маргарита", 250m),
                        new Product(2, "🍕 Піца Пепероні", 300m),
                        new Product(3, "🍕 Піца 4 Сири", 350m),
                        new Product(4, "🍕 Піца Гавайська", 280m),
                        new Product(5, "🍕 Піца М'ясна", 380m),
                        new Product(6, "🍣 Суші Філадельфія", 400m),
                        new Product(7, "🍣 Суші Каліфорнія", 350m),
                        new Product(8, "🍣 Дракон з вугром", 450m),
                        new Product(9, "🍣 Макі з лососем", 200m),
                        new Product(10, "🍔 Чизбургер", 180m),
                        new Product(11, "🍔 Дабл Чизбургер", 250m),
                        new Product(12, "🍔 Бургер BBQ", 220m),
                        new Product(13, "🍟 Картопля Фрі", 80m),
                        new Product(14, "🍟 Картопля по-селянськи", 90m),
                        new Product(15, "🍗 Нагетси (9 шт)", 150m),
                        new Product(16, "🥤 Кока-Кола 0.5л", 40m),
                        new Product(17, "🥤 Пепсі 0.5л", 40m),
                        new Product(18, "🥤 Сік Апельсиновий", 50m),
                        new Product(19, "🍰 Чизкейк", 120m),
                        new Product(20, "🍰 Тірамісу", 140m)
                    };
                File.WriteAllText(menuFile, JsonSerializer.Serialize(defaultCatalog, options));
                return defaultCatalog;
            }
            return JsonSerializer.Deserialize<List<Product>>(File.ReadAllText(menuFile), options);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[!] Не вдалося завантажити menu.json: {ex.Message}");
            return new List<Product>();
        }
    }

    public List<User> LoadUsers()
    {
        try
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
        catch (Exception ex)
        {
            Console.WriteLine($"[!] Не вдалося завантажити users.json: {ex.Message}");
            return new List<User>();
        }
    }

    public void SaveUsers(List<User> users)
    {
        try
        {
            string json = JsonSerializer.Serialize(users, options);
            File.WriteAllText(usersFile, json);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[!] Помилка збереження users.json: {ex.Message}");
        }
    }

    public List<Order> LoadOrders()
    {
        try
        {
            if (!File.Exists(ordersFile)) return new List<Order>();
            string json = File.ReadAllText(ordersFile);
            return JsonSerializer.Deserialize<List<Order>>(json, options) ?? new List<Order>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[!] Не вдалося завантажити orders.json: {ex.Message}");
            return new List<Order>();
        }
    }

    public void SaveOrders(List<Order> orders)
    {
        try
        {
            string json = JsonSerializer.Serialize(orders, options);
            File.WriteAllText(ordersFile, json);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[!] Помилка збереження orders.json: {ex.Message}");
        }
    }
}
