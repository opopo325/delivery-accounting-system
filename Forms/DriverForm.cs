using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

#nullable disable

namespace DeliverySystem
{
    public class DriverForm : Form
    {
        private Driver driver;
        private Database db;
        private List<User> usersDb;
        private ListView lvOrders;
        private Label lblStatus;

        public DriverForm(Driver d, Database database, List<User> users)
        {
            driver = d; db = database; usersDb = users;
            BuildUI(); LoadOrders();
        }

        private void BuildUI()
        {
            this.Text = $"Система Доставки — Кур'єр {driver.Name}";
            this.ClientSize = new Size(740, 560);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.BackColor = Color.FromArgb(240, 242, 245);

            var lblTitle = new Label { Text = $"🚚 Робоче місце кур'єра: {driver.Name}", Font = new Font("Segoe UI", 14, FontStyle.Bold), ForeColor = Color.FromArgb(33, 37, 41), AutoSize = true, Location = new Point(15, 15) };
            lblStatus = new Label { Text = StatusText(), Font = new Font("Segoe UI", 11, FontStyle.Bold), AutoSize = true, Location = new Point(15, 45) };

            lvOrders = new ListView { Location = new Point(15, 80), Size = new Size(710, 400), View = View.Details, FullRowSelect = true, GridLines = true, Font = new Font("Segoe UI", 10) };
            lvOrders.Columns.Add("ID", 55); lvOrders.Columns.Add("Клієнт", 120); lvOrders.Columns.Add("Адреса", 180); lvOrders.Columns.Add("Товари", 165); lvOrders.Columns.Add("Сума", 85); lvOrders.Columns.Add("Статус", 100);

            Button btnTakeOrder = MakeButton("▶ Взяти", 15, 500, 155, Color.FromArgb(0, 122, 255));
            btnTakeOrder.Click += (s, e) => UpdateOrderStatus(OrderStatus.InProgress);
            
            Button btnDeliverOrder = MakeButton("✅ Доставлено", 185, 500, 155, Color.FromArgb(52, 199, 89));
            btnDeliverOrder.Click += (s, e) => UpdateOrderStatus(OrderStatus.Delivered);
            
            Button btnCancelOrder = MakeButton("❌ Скасувати", 355, 500, 155, Color.FromArgb(255, 59, 48));
            btnCancelOrder.Click += (s, e) => UpdateOrderStatus(OrderStatus.Canceled);
            
            Button btnRefresh = MakeButton("🔄 Оновити", 525, 500, 200, Color.FromArgb(142, 142, 147));
            btnRefresh.Click += (s, e) => LoadOrders();

            this.Controls.AddRange(new Control[] { lblTitle, lblStatus, lvOrders, btnTakeOrder, btnDeliverOrder, btnCancelOrder, btnRefresh });
        }

        private Button MakeButton(string text, int x, int y, int w, Color color) { var btn = new Button { Text = text, Location = new Point(x, y), Width = w, Height = 40, BackColor = color, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10, FontStyle.Bold), Cursor = Cursors.Hand }; btn.FlatAppearance.BorderSize = 0; return btn; }

        private void LoadOrders()
        {
            lvOrders.Items.Clear();
            var orders = db.LoadOrders();
            if (orders.Count == 0) { lvOrders.Items.Add(new ListViewItem("Замовлень немає.")); return; }

            foreach (var o in orders.OrderByDescending(x => x.CreatedAt))
            {
                var item = new ListViewItem(o.Id.ToString()) { SubItems = { o.ClientName, o.Address, string.Join(", ", o.Items), o.TotalPrice.ToString("F0") + " грн", StatusLabel(o.Status) }, Tag = o };
                item.ForeColor = o.Status == OrderStatus.Delivered ? Color.Green : (o.Status == OrderStatus.Canceled ? Color.Gray : (o.Status == OrderStatus.InProgress ? Color.DarkOrange : Color.Black));
                lvOrders.Items.Add(item);
            }
            lblStatus.Text = StatusText();
            lblStatus.ForeColor = driver.IsAvailable ? Color.Green : Color.Red;
        }

        private void UpdateOrderStatus(OrderStatus newStatus)
        {
            if (lvOrders.SelectedItems.Count == 0) return;
            var order = (Order)lvOrders.SelectedItems[0].Tag;
            if (order == null || order.Status == OrderStatus.Delivered || order.Status == OrderStatus.Canceled) return;

            var currentOrders = db.LoadOrders();
            var target = currentOrders.FirstOrDefault(o => o.Id == order.Id);
            if (target == null) return;

            target.Status = newStatus; target.DriverId = driver.Id;
            driver.IsAvailable = newStatus != OrderStatus.InProgress;

            db.SaveOrders(currentOrders); db.SaveUsers(usersDb); LoadOrders();
        }

        private string StatusText() => driver.IsAvailable ? "🟢 Статус: Вільний" : "🔴 Статус: Зайнятий";
        private string StatusLabel(OrderStatus s) => s switch { OrderStatus.New => "Нове", OrderStatus.InProgress => "В дорозі", OrderStatus.Delivered => "Доставлено", OrderStatus.Canceled => "Скасовано", _ => s.ToString() };
    }
}