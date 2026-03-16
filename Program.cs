using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

#nullable disable 

namespace DeliverySystem
{
    public enum OrderStatus { New, InProgress, Delivered, Canceled }

    public class Cart
    {
        public List<Product> Items { get; private set; } = new List<Product>();
        private Coupon AppliedCoupon = null;

        public void AddProduct(Product product)
        {
            Items.Add(product);
            Console.WriteLine($"[+] Товар '{product.Name}' успішно залетів у кошик.");
        }
        public void RemoveProduct(Product product)
        {
            if (Items.Remove(product))
                Console.WriteLine($"[-] Товар '{product.Name}' Видалено з кошика.");
            else
                Console.WriteLine($"[!] Цього товару немає в кошику.");
        }

        public void ApplyCoupon(Coupon coupon)
        {
            AppliedCoupon = coupon;
            Console.WriteLine($"[%] Купон '{coupon.Code}' активовано! Знижка {coupon.DiscountPercentage}%");
        }

        public void Clear()
        {
            Items.Clear();
            AppliedCoupon = null;
            Console.WriteLine("[!] Кошик повністю очищено.");
        }

        public decimal CalculateTotal()
        {
            decimal sum = Items.Sum(p => p.Price);
            if (AppliedCoupon != null && AppliedCoupon.IsValid())
            {
                sum -= sum * (AppliedCoupon.DiscountPercentage / 100m);
            }
            return sum;
        }

        public void PrintCart()
        {
            Console.WriteLine("\n=== ВАШ КОШИК ===");
            if (Items.Count == 0)
            {
                Console.WriteLine("Кошик порожній, як твій гаманець. Купи щось!");
                return;
            }

            foreach (var item in Items) Console.WriteLine($"- {item.Name} ({item.Price} грн)");
            Console.WriteLine($"\nЗАГАЛЬНА СУМА ДО ОПЛАТИ: {CalculateTotal()} грн");
            Console.WriteLine("=================\n");
        }
    }

    class Program
    {
        static string dbFile = "users.txt"; 
        static string ordersFile = "orders.txt";

        static List<User> LoadUsersFromDatabase()
        {
            var usersDb = new List<User> { new Driver(99, "Максім", "0669876543", "12345", "ВН1234АА") };

            if (File.Exists(dbFile))
            {
                string[] lines = File.ReadAllLines(dbFile);
                int idCounter = 1; 
                foreach (string line in lines)
                {
                    if (!string.IsNullOrWhiteSpace(line))
                    {
                        string[] data = line.Split('|');
                        // Якщо 5 колонок - це Клієнт
                        if (data.Length == 5)
                            usersDb.Add(new Client(idCounter++, data[0], data[2], data[1], data[3], data[4]));
                        // Якщо 4 колонки - це Кур'єр 
                        else if (data.Length == 4)
                            usersDb.Add(new Driver(idCounter++, data[0], data[2], data[1], data[3]));
                    }
                }
            }
            return usersDb;
        }

        static void SaveUserToFile(string name, string password, string phone, string email, string address)
        {
            using (StreamWriter sw = new StreamWriter(dbFile, true))
            {
                sw.WriteLine($"{name}|{password}|{phone}|{email}|{address}");
            }
        }

        static void SaveOrderToFile(Client client, Cart cart)
        {
            string jsonOrders = "orders.json";
            List<Order> orders = new List<Order>();
            
            if (File.Exists(jsonOrders))
            {
                orders = JsonSerializer.Deserialize<List<Order>>(File.ReadAllText(jsonOrders)) ?? new List<Order>();
            }

            orders.Add(new Order {
                Id = orders.Count > 0 ? orders.Max(o => o.Id) + 1 : 1,
                ClientName = client.Name,
                ClientPhone = client.Phone,
                Address = client.DefaultAddress,
                TotalPrice = cart.CalculateTotal(),
                Status = OrderStatus.New,
                Items = cart.Items.Select(i => i.Name).ToList()
            });

            File.WriteAllText(jsonOrders, JsonSerializer.Serialize(orders, new JsonSerializerOptions { WriteIndented = true }));
        }
            static List<Product> LoadCatalog()
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
                File.WriteAllText(menuFile, JsonSerializer.Serialize(defaultCatalog, new JsonSerializerOptions { WriteIndented = true }));
                return defaultCatalog;
            }
            return JsonSerializer.Deserialize<List<Product>>(File.ReadAllText(menuFile));
        }
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.InputEncoding = System.Text.Encoding.UTF8;
            List<User> usersDb = LoadUsersFromDatabase();

            List<Product> catalog = LoadCatalog();

            Console.WriteLine("=== СИСТЕМА ДОСТАВКИ ===");
            Console.WriteLine("1. Вхід (Авторизація)");
            Console.WriteLine("2. Реєстрація нового клієнта");
            Console.Write("Обери дію (1 або 2): ");
            
            string choice = Console.ReadLine();
            User currentUser = null;
            if (choice == "2")
            {
                Console.WriteLine("\n--- РЕЄСТРАЦІЯ ---");
                Console.WriteLine("Хто ти по життю?");
                Console.WriteLine("1. Клієнт (замовляти їжу)");
                Console.WriteLine("2. Кур'єр (розвозити замовлення)");
                Console.Write("Обери (1 або 2): ");
                string role = Console.ReadLine();

                if (role != "1" && role != "2")
                {
                    Console.WriteLine("[-] Ти ввів якусь діч. Реєстрація скасована.");
                    return;
                }

                Console.Write("Придумай ім'я: "); string newName = Console.ReadLine();
                Console.Write("Придумай пароль: "); string newPass = Console.ReadLine();
                Console.Write("Твій телефон: "); string newPhone = Console.ReadLine();

                int newId = usersDb.Count > 0 ? usersDb.Max(u => u.Id) + 1 : 1;

                if (role == "1")
                {
                    Console.Write("Твоя пошта: "); string newEmail = Console.ReadLine();
                    Console.Write("Адреса доставки: "); string newAddress = Console.ReadLine();
                    
                    using (StreamWriter sw = new StreamWriter(dbFile, true))
                        sw.WriteLine($"{newName}|{newPass}|{newPhone}|{newEmail}|{newAddress}");
                    
                    usersDb.Add(new Client(newId, newName, newPhone, newPass, newEmail, newAddress));
                }
                else if (role == "2")
                {
                    Console.Write("Номер твоєї тачки: "); string newCar = Console.ReadLine();
                    
                    using (StreamWriter sw = new StreamWriter(dbFile, true))
                        sw.WriteLine($"{newName}|{newPass}|{newPhone}|{newCar}");
                    
                    usersDb.Add(new Driver(newId, newName, newPhone, newPass, newCar));
                }

                Console.WriteLine("[+] Чотко! Тебе зареєстровано. Перезапусти програму і увійди (цифра 1).");
                return; 
            }
            else if (choice == "1")
            {
                Console.WriteLine("\n--- ВХІД ---");
                Console.Write("Введіть ім'я: "); string inputName = Console.ReadLine();
                Console.Write("Введіть пароль: "); string inputPass = Console.ReadLine();

                currentUser = usersDb.FirstOrDefault(u => u.Name.Trim() == inputName.Trim() && u.Password.Trim() == inputPass.Trim());  

                if (currentUser == null)
                {
                    Console.WriteLine("Помилка! Не обманюй, неправильний логін/пароль.");
                    return;
                }

                Console.WriteLine($"\nАвторизація успішна! Вітаємо, {currentUser.Name}.");

                if (currentUser is Client client)
                {
                    Cart cart = new Cart();
                    bool shopping = true;

                    while (shopping)
                    {
                        Console.WriteLine("\n--- ГОЛОВНЕ МЕНЮ ---");
                        Console.WriteLine("1. Подивитися меню (Каталог)");
                        Console.WriteLine("2. Додати товар у кошик (за ID)");
                        Console.WriteLine("3. Видалити товар з кошика (за ID)");
                        Console.WriteLine("4. Показати мій кошик");
                        Console.WriteLine("5. Очистити кошик");
                        Console.WriteLine("6. ОФОРМИТИ ЗАМОВЛЕННЯ");
                        Console.WriteLine("7. Ввести промокод");
                        Console.WriteLine("0. Вийти з програми");
                        Console.Write("Твій вибір: ");

                        string action = Console.ReadLine();

                        switch (action)
                        {
                            case "1":
                                Console.WriteLine("\n--- КАТАЛОГ ТОВАРІВ ---");
                                foreach (var p in catalog) Console.WriteLine($"[ID: {p.Id}] {p.Name} - {p.Price} грн");
                                break;
                            case "2":
                                Console.Write("Введи ID товару, який хочеш додати: ");
                                if (int.TryParse(Console.ReadLine(), out int addId))
                                {
                                    Product productToAdd = catalog.FirstOrDefault(p => p.Id == addId);
                                    if (productToAdd != null) cart.AddProduct(productToAdd);
                                    else Console.WriteLine("Блядь, немає товару з таким ID!");
                                }
                                break;
                            case "3":
                                Console.Write("Введи ID товару, який хочеш викинути: ");
                                if (int.TryParse(Console.ReadLine(), out int removeId))
                                {
                                    Product productToRemove = cart.Items.FirstOrDefault(p => p.Id == removeId);
                                    if (productToRemove != null) cart.RemoveProduct(productToRemove);
                                    else Console.WriteLine("В кошику немає такої хуйні.");
                                }
                                break;
                            case "4":
                                cart.PrintCart();
                                break;
                            case "5":
                                cart.Clear();
                                break;
                               case "6":
                                if (cart.Items.Count == 0)
                                {
                                    Console.WriteLine("Куди ти оформлюєш, йолопе? Кошик порожній!");
                                }
                                else
                                {
                                    SaveOrderToFile(client, cart);
                                    Console.WriteLine("\n[✅] ЗАМОВЛЕННЯ УСПІШНО ОФОРМЛЕНО!");
                                    Console.WriteLine("Дані збережено у файл 'orders.txt'. Чекай на кур'єра.");
                                    cart.Clear(); 
                                }
                                break;

                            case "7":
                                Console.Write("Введи промокод: ");
                                string code = Console.ReadLine();
                                if (code == "HALYAVA") 
                                {
                                    cart.ApplyCoupon(new Coupon(code, 20m)); 
                                }
                                else 
                                {
                                    Console.WriteLine("Такого промокоду не існує, не видумуй.");
                                }
                                break;

                            case "0":
                                shopping = false;
                                Console.WriteLine("Давай, бувай здоровий.");
                                break;

                            default:
                                Console.WriteLine("Ти ввів якусь діч, спробуй ще раз.");
                                break;
                        }
                        if (shopping)
                        {
                            Console.WriteLine("\nНатисни Enter, щоб продовжити...");
                            Console.ReadLine();
                            Console.Clear();
                        }
                    }
                }
                    else if (currentUser is Driver driver)
                {
                    bool working = true;
                    string jsonOrders = "orders.json";
                    
                    while (working)
                    {
                        Console.WriteLine($"\n--- РОБОЧЕ МІСЦЕ КУР'ЄРА ---");
                        Console.WriteLine($"Водій: {driver.Name} | Статус: {(driver.IsAvailable ? "🟢 Вільний" : "🔴 Зайнятий")}");
                        Console.WriteLine("1. Подивитися всі замовлення");
                        Console.WriteLine("2. Взяти замовлення в роботу (за ID)");
                        Console.WriteLine("3. Відмітити як доставлене (за ID)");
                        Console.WriteLine("0. Звалити додому");
                        Console.Write("Вибір: ");

                        string dAction = Console.ReadLine();
                        List<Order> currentOrders = File.Exists(jsonOrders) ? JsonSerializer.Deserialize<List<Order>>(File.ReadAllText(jsonOrders)) : new List<Order>();

                        if (dAction == "1")
                        {
                            if (currentOrders.Count == 0) Console.WriteLine("Глухо, замовлень нема.");
                            else foreach (var o in currentOrders) Console.WriteLine($"[{o.Id}] {o.ClientName} ({o.Address}) - {o.TotalPrice}грн - Статус: {o.Status}");
                        }
                        else if (dAction == "2" || dAction == "3")
                        {
                            Console.Write("Введи ID замовлення: ");
                            if (int.TryParse(Console.ReadLine(), out int oId))
                            {
                                var order = currentOrders.FirstOrDefault(o => o.Id == oId);
                                if (order != null)
                                {
                                    order.Status = dAction == "2" ? OrderStatus.InProgress : OrderStatus.Delivered;
                                    File.WriteAllText(jsonOrders, JsonSerializer.Serialize(currentOrders, new JsonSerializerOptions { WriteIndented = true }));
                                    Console.WriteLine($"[+] Статус замовлення {oId} оновлено на {order.Status}!");
                                }
                                else Console.WriteLine("Бля, немає такого ID в базі.");
                            }
                        }
                        else if (dAction == "0") working = false;

                        if (working) { Console.ReadLine(); Console.Clear(); }
                    }
                }
            }
        }
    }
}