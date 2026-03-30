using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows.Forms;
#nullable disable 

namespace DeliverySystem
{
    static class AvailableCoupons
    {
        public static readonly Dictionary<string, decimal> List = new()
        {
            { "HALYAVA", 20m },
            { "WELCOME", 10m },
            { "VIP30",   30m }
        };
    }

    static class Program
    {
        [STAThread]
        static void Main(string[] args)
        
        {
             ApplicationConfiguration.Initialize();
            Application.Run(new LoginForm());
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.InputEncoding = System.Text.Encoding.UTF8;
            Database db = new Database(); 
            List<User> usersDb = db.LoadUsers(); 
            List<Product> catalog = db.LoadCatalog();
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
                // Валідація: ім'я не може бути порожнім
                if (string.IsNullOrWhiteSpace(newName)) { Console.WriteLine("[-] Ім'я не може бути порожнім."); return; }
                // Перевірка унікальності імені
                if (usersDb.Any(u => u.Name.Equals(newName.Trim(), StringComparison.OrdinalIgnoreCase)))
                {
                    Console.WriteLine($"[-] Користувач '{newName}' вже існує. Обери інше ім'я.");
                    return;
                }

                Console.Write("Придумай пароль: "); string newPass = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(newPass) || newPass.Length < 4) { Console.WriteLine("[-] Пароль занадто короткий (мінімум 4 символи)."); return; }

                Console.Write("Твій телефон: "); string newPhone = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(newPhone)) { Console.WriteLine("[-] Телефон не може бути порожнім."); return; }

                int newId = usersDb.Count > 0 ? usersDb.Max(u => u.Id) + 1 : 1;

                if (role == "1")
                {
                    Console.Write("Твоя пошта: "); string newEmail = Console.ReadLine();
                    if (string.IsNullOrWhiteSpace(newEmail)) { Console.WriteLine("[-] Email не може бути порожнім."); return; }

                    Console.Write("Адреса доставки: "); string newAddress = Console.ReadLine();
                    if (string.IsNullOrWhiteSpace(newAddress)) { Console.WriteLine("[-] Адреса не може бути порожньою."); return; }

                    usersDb.Add(new Client(newId, newName, newPhone, newPass, newEmail, newAddress));
                    db.SaveUsers(usersDb); 
                }
                else if (role == "2")
                    {
                        Console.Write("Номер твоєї тачки: "); string newCar = Console.ReadLine();
                        if (string.IsNullOrWhiteSpace(newCar)) { Console.WriteLine("[-] Номер машини не може бути порожнім."); return; }

                        usersDb.Add(new Driver(newId, newName, newPhone, newPass, newCar));
                        db.SaveUsers(usersDb); 
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
                        Console.WriteLine("8. Мої замовлення");
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
                                    if (productToAdd != null) 
                                    {
                                        cart.AddProduct(productToAdd);
                                        Console.WriteLine($"[+] Товар '{productToAdd.Name}' успішно залетів у кошик.");
                                    }
                                    else Console.WriteLine("[-] Немає товару з таким ID!");
                                }
                                else Console.WriteLine("[-] Введи числовий ID.");
                                break;
                            case "3":
                                Console.Write("Введи ID товару, який хочеш викинути: ");
                                if (int.TryParse(Console.ReadLine(), out int removeId))
                                {
                                    Product productToRemove = cart.Items.FirstOrDefault(p => p.Id == removeId);
                                    if (productToRemove != null) 
                                    {
                                        bool isRemoved = cart.RemoveProduct(productToRemove);
                                        if (isRemoved)
                                            Console.WriteLine($"[-] Товар '{productToRemove.Name}' видалено з кошика.");
                                        else
                                            Console.WriteLine($"[!] Цього товару і так немає в кошику.");
                                    }
                                    else Console.WriteLine("[-] В кошику немає товару з таким ID.");
                                }
                                break;
                           case "4":
                            Console.WriteLine("\n=== ВАШ КОШИК ===");
                            if (cart.Items.Count == 0)
                            {
                                Console.WriteLine("Кошик порожній, як твій гаманець. Купи щось!");
                            }
                            else
                            {
                                foreach (var item in cart.Items) 
                                {
                                    Console.WriteLine($"- {item.Name} ({item.Price} грн)");
                                }
                                
                                if (cart.AppliedCoupon != null)
                                {
                                    Console.WriteLine($"[%] Активовано знижку: {cart.AppliedCoupon.DiscountPercentage}%");
                                }
                                
                                Console.WriteLine($"\nЗАГАЛЬНА СУМА ДО ОПЛАТИ: {cart.CalculateTotal()} грн");
                            }
                            Console.WriteLine("=================\n");
                            break;
                            case "5":
                                cart.Clear();
                                Console.WriteLine("[-] Кошик очищено.");
                                break;
                              case "6":
                                if (cart.Items.Count == 0)
                                {
                                    Console.WriteLine("Кошик порожній!");
                                }
                                else
                                {
                                    var orders = db.LoadOrders();
                                    orders.Add(new Order {
                                        Id = orders.Count > 0 ? orders.Max(o => o.Id) + 1 : 1,
                                        ClientName = client.Name,
                                        ClientPhone = client.Phone,
                                        Address = client.DefaultAddress,
                                        TotalPrice = cart.CalculateTotal(),
                                        Status = OrderStatus.New,
                                        Items = cart.Items.Select(i => i.Name).ToList(),
                                        CreatedAt = DateTime.Now
                                    });
                                    db.SaveOrders(orders);
                                    Console.WriteLine("\n[✅] ЗАМОВЛЕННЯ ОФОРМЛЕНО!");
                                    cart.Clear(); 
                                }
                                break;

                            case "7":
                                Console.Write("Введи промокод: ");
                                string code = Console.ReadLine()?.Trim().ToUpper();
                                // Перевірка по словнику промокодів
                                if (AvailableCoupons.List.TryGetValue(code, out decimal discount))
                                {
                                    cart.ApplyCoupon(new Coupon(code, discount));
                                    Console.WriteLine($"[+] Промокод активовано! Знижка {discount}%.");
                                }
                                else 
                                {
                                    Console.WriteLine("Такого промокоду не існує, не видумуй.");
                                }
                                break;

                            case "8":
                                Console.WriteLine("\n--- МОЇ ЗАМОВЛЕННЯ ---");
                                var myOrders = db.LoadOrders()
                                    .Where(o => o.ClientName == client.Name)
                                    .OrderByDescending(o => o.CreatedAt)
                                    .ToList();
                                if (myOrders.Count == 0) Console.WriteLine("Замовлень ще не було.");
                                else foreach (var o in myOrders)
                                    Console.WriteLine($"[{o.Id}] {o.CreatedAt:dd.MM HH:mm} | {o.TotalPrice} грн | Статус: {o.Status}");
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
                    
                    while (working)
                    {
                        Console.WriteLine($"\n--- РОБОЧЕ МІСЦЕ КУР'ЄРА ---");
                        Console.WriteLine($"Водій: {driver.Name} | Статус: {(driver.IsAvailable ? "🟢 Вільний" : "🔴 Зайнятий")}");
                        Console.WriteLine("1. Подивитися всі замовлення");
                        Console.WriteLine("2. Взяти замовлення в роботу (за ID)");
                        Console.WriteLine("3. Відмітити як доставлене (за ID)");
                        Console.WriteLine("4. Скасувати замовлення (за ID)");
                        Console.WriteLine("0. Звалити додому");
                        Console.Write("Вибір: ");

                        string dAction = Console.ReadLine();
                        List<Order> currentOrders = db.LoadOrders();

                        if (dAction == "1")
                        {
                            if (currentOrders.Count == 0) Console.WriteLine("Глухо, замовлень нема.");
                            else foreach (var o in currentOrders) Console.WriteLine($"[{o.Id}] {o.ClientName} ({o.Address}) - {o.TotalPrice}грн - Статус: {o.Status}");
                        }
                        else if (dAction == "2" || dAction == "3" || dAction == "4")
                        {
                            Console.Write("Введи ID замовлення: ");
                            if (int.TryParse(Console.ReadLine(), out int oId))
                            {
                                var order = currentOrders.FirstOrDefault(o => o.Id == oId);
                                if (order != null)
                                {
                                    // Не можна чіпати вже завершені замовлення
                                    if (order.Status == OrderStatus.Delivered || order.Status == OrderStatus.Canceled)
                                    {
                                        Console.WriteLine($"[!] Замовлення вже має статус '{order.Status}', змінити не можна.");
                                    }
                                    else
                                    {
                                        if (dAction == "2") { order.Status = OrderStatus.InProgress; driver.IsAvailable = false; }
                                        else if (dAction == "3") { order.Status = OrderStatus.Delivered; driver.IsAvailable = true; }
                                        else if (dAction == "4") { order.Status = OrderStatus.Canceled; driver.IsAvailable = true; }

                                        order.DriverId = driver.Id;
                                        db.SaveOrders(currentOrders);
                                        db.SaveUsers(usersDb); // зберігаємо IsAvailable водія
                                        Console.WriteLine($"[+] Статус замовлення {oId} оновлено на {order.Status}!");
                                    }
                                }
                                else Console.WriteLine("[-] Немає такого ID в базі.");
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
