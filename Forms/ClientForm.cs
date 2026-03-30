using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

#nullable disable

namespace DeliverySystem
{
    public class ClientForm : Form
    {
        private Client client;
        private Database db;
        private Cart cart;
        
        private ListView lvCatalog, lvCart, lvOrders;
        private Label lblTotal, lblCoupon;
        private TextBox txtCoupon;

        public ClientForm(Client c, Database database)
        {
            client = c; db = database; cart = new Cart();
            BuildUI(); LoadCatalog(); LoadOrders();
        }

        private void BuildUI()
        {
            this.Text = $"Система Доставки — {client.Name}";
            this.ClientSize = new Size(720, 580);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.BackColor = Color.FromArgb(240, 242, 245);

            var lblWelcome = new Label { Text = $"👋 Привіт, {client.Name}!   📍 {client.DefaultAddress}", Font = new Font("Segoe UI", 12, FontStyle.Bold), ForeColor = Color.FromArgb(33, 37, 41), AutoSize = true, Location = new Point(15, 15) };
            this.Controls.Add(lblWelcome);

            TabControl tabs = new TabControl { Location = new Point(10, 50), Size = new Size(700, 520), Font = new Font("Segoe UI", 10) };
            TabPage tabCatalog = new TabPage("🍕 Меню"), tabCart = new TabPage("🛒 Кошик"), tabOrders = new TabPage("📦 Мої замовлення");

            // CATALOG
            lvCatalog = MakeListView(10, 10, 670, 400);
            lvCatalog.Columns.Add("ID", 60); lvCatalog.Columns.Add("Назва", 450); lvCatalog.Columns.Add("Ціна (грн)", 150);
            Button btnAddToCart = MakeButton("➕ Додати в кошик", 10, 425, 200, Color.FromArgb(0, 122, 255));
            btnAddToCart.Click += BtnAddToCart_Click;
            tabCatalog.Controls.AddRange(new Control[] { lvCatalog, btnAddToCart });

            // CART
            lvCart = MakeListView(10, 10, 670, 280);
            lvCart.Columns.Add("Назва", 500); lvCart.Columns.Add("Ціна (грн)", 165);
            lblTotal = new Label { Text = "Сума: 0.00 грн", Font = new Font("Segoe UI", 14, FontStyle.Bold), ForeColor = Color.FromArgb(0, 122, 255), AutoSize = true, Location = new Point(10, 305) };
            lblCoupon = new Label { Text = "", Font = new Font("Segoe UI", 9, FontStyle.Italic), ForeColor = Color.FromArgb(52, 199, 89), AutoSize = true, Location = new Point(10, 335) };
            txtCoupon = new TextBox { Location = new Point(10, 360), Width = 180, Font = new Font("Segoe UI", 12), CharacterCasing = CharacterCasing.Upper };
            Button btnApplyCoupon = MakeButton("Застосувати", 200, 359, 140, Color.FromArgb(255, 149, 0));
            btnApplyCoupon.Click += BtnApplyCoupon_Click;
            Button btnRemoveFromCart = MakeButton("🗑 Видалити", 355, 425, 140, Color.FromArgb(255, 59, 48));
            btnRemoveFromCart.Click += BtnRemoveFromCart_Click;
            Button btnClearCart = MakeButton("🧹 Очистити", 505, 425, 133, Color.FromArgb(142, 142, 147));
            btnClearCart.Click += (s, e) => { cart.Clear(); RefreshCartView(); };
            Button btnCheckout = MakeButton("✅ Оформити замовлення", 10, 425, 230, Color.FromArgb(52, 199, 89));
            btnCheckout.Click += BtnCheckout_Click;
            tabCart.Controls.AddRange(new Control[] { lvCart, lblTotal, lblCoupon, txtCoupon, btnApplyCoupon, btnCheckout, btnRemoveFromCart, btnClearCart });

            // ORDERS
            lvOrders = MakeListView(10, 10, 670, 400);
            lvOrders.Columns.Add("ID", 55); lvOrders.Columns.Add("Дата", 120); lvOrders.Columns.Add("Товари", 280); lvOrders.Columns.Add("Сума (грн)", 100); lvOrders.Columns.Add("Статус", 110);
            Button btnRefreshOrders = MakeButton("🔄 Оновити", 10, 425, 140, Color.FromArgb(0, 122, 255));
            btnRefreshOrders.Click += (s, e) => LoadOrders();
            tabOrders.Controls.AddRange(new Control[] { lvOrders, btnRefreshOrders });

            tabs.TabPages.AddRange(new[] { tabCatalog, tabCart, tabOrders });
            tabs.SelectedIndexChanged += (s, e) => { if (tabs.SelectedTab == tabOrders) LoadOrders(); if (tabs.SelectedTab == tabCart) RefreshCartView(); };
            this.Controls.Add(tabs);
        }

        private ListView MakeListView(int x, int y, int w, int h) => new ListView { Location = new Point(x, y), Size = new Size(w, h), View = View.Details, FullRowSelect = true, GridLines = true, Font = new Font("Segoe UI", 10) };
        private Button MakeButton(string text, int x, int y, int w, Color color) { var btn = new Button { Text = text, Location = new Point(x, y), Width = w, Height = 40, BackColor = color, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10, FontStyle.Bold), Cursor = Cursors.Hand }; btn.FlatAppearance.BorderSize = 0; return btn; }

        private void LoadCatalog()
        {
            lvCatalog.Items.Clear();
            foreach (var p in db.LoadCatalog()) lvCatalog.Items.Add(new ListViewItem(p.Id.ToString()) { SubItems = { p.Name, p.Price.ToString("F2") }, Tag = p });
        }

        private void BtnAddToCart_Click(object sender, EventArgs e)
        {
            if (lvCatalog.SelectedItems.Count == 0) return;
            cart.AddProduct((Product)lvCatalog.SelectedItems[0].Tag);
            RefreshCartView();
        }

        private void RefreshCartView()
        {
            lvCart.Items.Clear();
            foreach (var p in cart.Items) lvCart.Items.Add(new ListViewItem(p.Name) { SubItems = { p.Price.ToString("F2") }, Tag = p });
            lblTotal.Text = $"Сума: {cart.CalculateTotal():F2} грн";
            lblCoupon.Text = cart.AppliedCoupon != null ? $"🎟 Знижка {cart.AppliedCoupon.DiscountPercentage}%" : "";
        }

        private void BtnApplyCoupon_Click(object sender, EventArgs e)
        {
            string code = txtCoupon.Text.Trim().ToUpper();
            if (AvailableCoupons.List.TryGetValue(code, out decimal discount)) { cart.ApplyCoupon(new Coupon(code, discount)); RefreshCartView(); }
            else MessageBox.Show("Такого промокоду не існує!", "Увага", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        private void BtnRemoveFromCart_Click(object sender, EventArgs e)
        {
            if (lvCart.SelectedItems.Count > 0) { cart.RemoveProduct((Product)lvCart.SelectedItems[0].Tag); RefreshCartView(); }
        }

        private void BtnCheckout_Click(object sender, EventArgs e)
        {
            if (cart.Items.Count == 0) return;
            var orders = db.LoadOrders();
            orders.Add(new Order { Id = orders.Count > 0 ? orders.Max(o => o.Id) + 1 : 1, ClientName = client.Name, ClientPhone = client.Phone, Address = client.DefaultAddress, TotalPrice = cart.CalculateTotal(), Status = OrderStatus.New, Items = cart.Items.Select(i => i.Name).ToList(), CreatedAt = DateTime.Now });
            db.SaveOrders(orders); cart.Clear(); RefreshCartView();
            MessageBox.Show("✅ ЗАМОВЛЕННЯ ОФОРМЛЕНО!", "Інфо", MessageBoxButtons.OK, MessageBoxIcon.Information);
            LoadOrders();
        }

        private void LoadOrders()
        {
            lvOrders.Items.Clear();
            var myOrders = db.LoadOrders().Where(o => o.ClientName == client.Name).OrderByDescending(o => o.CreatedAt).ToList();
            foreach (var o in myOrders)
            {
                var item = new ListViewItem(o.Id.ToString()) { SubItems = { o.CreatedAt.ToString("dd.MM HH:mm"), string.Join(", ", o.Items), o.TotalPrice.ToString("F2"), StatusLabel(o.Status) } };
                item.ForeColor = o.Status == OrderStatus.Delivered ? Color.Green : (o.Status == OrderStatus.Canceled ? Color.Red : Color.DarkOrange);
                lvOrders.Items.Add(item);
            }
        }

        private string StatusLabel(OrderStatus s) => s switch { OrderStatus.New => "🆕 Нове", OrderStatus.InProgress => "🚚 В дорозі", OrderStatus.Delivered => "✅ Доставлено", OrderStatus.Canceled => "❌ Скасовано", _ => s.ToString() };
    }
}