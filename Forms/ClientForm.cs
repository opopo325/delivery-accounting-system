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
        private TextBox txtCoupon, txtSearch;

        // Наші нові "вкладки" (ніякого глючного TabControl)
        private Panel pnlCatalog, pnlCart, pnlOrders;
        private Button btnTabCatalog, btnTabCart, btnTabOrders;

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

            Button btnTheme = MakeButton("🌙 Тема", 590, 10, 110, Color.FromArgb(50, 50, 50));
            btnTheme.Click += (s, e) => ToggleTheme();
            this.Controls.Add(btnTheme);

            btnTabCatalog = MakeButton("🍕 Меню", 15, 55, 150, Color.FromArgb(0, 122, 255));
            btnTabCart = MakeButton("🛒 Кошик", 170, 55, 150, Color.FromArgb(200, 200, 200));
            btnTabOrders = MakeButton("📦 Мої замовлення", 325, 55, 180, Color.FromArgb(200, 200, 200));
            
            btnTabCart.ForeColor = btnTabOrders.ForeColor = Color.Black;

            pnlCatalog = new Panel { Location = new Point(15, 100), Size = new Size(685, 470) };
            pnlCart = new Panel { Location = new Point(15, 100), Size = new Size(685, 470), Visible = false };
            pnlOrders = new Panel { Location = new Point(15, 100), Size = new Size(685, 470), Visible = false };

            // Логіка перемикання екранів
            btnTabCatalog.Click += (s, e) => SwitchTab(pnlCatalog, btnTabCatalog);
            btnTabCart.Click += (s, e) => SwitchTab(pnlCart, btnTabCart);
            btnTabOrders.Click += (s, e) => SwitchTab(pnlOrders, btnTabOrders);

            this.Controls.AddRange(new Control[] { btnTabCatalog, btnTabCart, btnTabOrders, pnlCatalog, pnlCart, pnlOrders });

            //ЕКРАН 1: КАТАЛОГ
            txtSearch = new TextBox { Location = new Point(0, 0), Width = 685, Font = new Font("Segoe UI", 12), PlaceholderText = "Пошук товару..." };
            txtSearch.TextChanged += (s, e) => FilterCatalog(txtSearch.Text);

            lvCatalog = MakeListView(0, 35, 685, 375);
            lvCatalog.Columns.Add("ID", 60); lvCatalog.Columns.Add("Назва", 450); lvCatalog.Columns.Add("Ціна (грн)", 150);
            Button btnAddToCart = MakeButton("➕ Додати в кошик", 0, 420, 200, Color.FromArgb(0, 122, 255));
            btnAddToCart.Click += BtnAddToCart_Click;
            pnlCatalog.Controls.AddRange(new Control[] { txtSearch, lvCatalog, btnAddToCart });

            //ЕКРАН 2: КОШИК 
            lvCart = MakeListView(0, 0, 685, 280);
            lvCart.Columns.Add("Назва", 500); lvCart.Columns.Add("Ціна (грн)", 165);
            lblTotal = new Label { Text = "Сума: 0.00 грн", Font = new Font("Segoe UI", 14, FontStyle.Bold), ForeColor = Color.FromArgb(0, 122, 255), AutoSize = true, Location = new Point(0, 295) };
            lblCoupon = new Label { Text = "", Font = new Font("Segoe UI", 9, FontStyle.Italic), ForeColor = Color.FromArgb(52, 199, 89), AutoSize = true, Location = new Point(0, 325) };
            txtCoupon = new TextBox { Location = new Point(0, 350), Width = 180, Font = new Font("Segoe UI", 12), CharacterCasing = CharacterCasing.Upper };
            Button btnApplyCoupon = MakeButton("Застосувати", 190, 349, 140, Color.FromArgb(255, 149, 0));
            btnApplyCoupon.Click += BtnApplyCoupon_Click;
            Button btnRemoveFromCart = MakeButton("🗑 Видалити", 380, 420, 140, Color.FromArgb(255, 59, 48));
            btnRemoveFromCart.Click += BtnRemoveFromCart_Click;
            Button btnClearCart = MakeButton("🧹 Очистити", 530, 420, 155, Color.FromArgb(142, 142, 147));
            btnClearCart.Click += (s, e) => { cart.Clear(); RefreshCartView(); };
            Button btnCheckout = MakeButton("✅ Оформити замовлення", 0, 420, 230, Color.FromArgb(52, 199, 89));
            btnCheckout.Click += BtnCheckout_Click;
            pnlCart.Controls.AddRange(new Control[] { lvCart, lblTotal, lblCoupon, txtCoupon, btnApplyCoupon, btnCheckout, btnRemoveFromCart, btnClearCart });

            //ЕКРАН 3: ЗАМОВЛЕННЯ 
            lvOrders = MakeListView(0, 0, 685, 410);
            lvOrders.Columns.Add("ID", 55); lvOrders.Columns.Add("Дата", 120); lvOrders.Columns.Add("Товари", 280); lvOrders.Columns.Add("Сума (грн)", 100); lvOrders.Columns.Add("Статус", 110);
            Button btnRefreshOrders = MakeButton("🔄 Оновити", 0, 420, 140, Color.FromArgb(0, 122, 255));
            btnRefreshOrders.Click += (s, e) => LoadOrders();
            pnlOrders.Controls.AddRange(new Control[] { lvOrders, btnRefreshOrders });
        }

        private void SwitchTab(Panel activePanel, Button activeButton)
        {
            pnlCatalog.Visible = pnlCart.Visible = pnlOrders.Visible = false;
            
            // Скидаємо кольори неактивних кнопок
            Color inactiveBg = isDark ? Color.FromArgb(45, 45, 48) : Color.FromArgb(200, 200, 200);
            Color inactiveFg = isDark ? Color.White : Color.Black;
            
            btnTabCatalog.BackColor = btnTabCart.BackColor = btnTabOrders.BackColor = inactiveBg;
            btnTabCatalog.ForeColor = btnTabCart.ForeColor = btnTabOrders.ForeColor = inactiveFg;

            // Фарбуємо активну
            activePanel.Visible = true;
            activeButton.BackColor = Color.FromArgb(0, 122, 255);
            activeButton.ForeColor = Color.White;

            if (activePanel == pnlOrders) LoadOrders();
            if (activePanel == pnlCart) RefreshCartView();
        }

        private ListView MakeListView(int x, int y, int w, int h) => new ListView { Location = new Point(x, y), Size = new Size(w, h), View = View.Details, FullRowSelect = true, GridLines = true, Font = new Font("Segoe UI", 10), BorderStyle = BorderStyle.None };
        private Button MakeButton(string text, int x, int y, int w, Color color) { var btn = new Button { Text = text, Location = new Point(x, y), Width = w, Height = 40, BackColor = color, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10, FontStyle.Bold), Cursor = Cursors.Hand }; btn.FlatAppearance.BorderSize = 0; return btn; }

        private void LoadCatalog()
        {
            lvCatalog.Items.Clear();
            foreach (var p in db.LoadCatalog()) lvCatalog.Items.Add(new ListViewItem(p.Id.ToString()) { SubItems = { p.Name, p.Price.ToString("F2") }, Tag = p });
        }

        private void FilterCatalog(string query)
        {
            lvCatalog.Items.Clear();
            var filtered = db.LoadCatalog().Where(p => p.Name.Contains(query, StringComparison.OrdinalIgnoreCase));
            foreach (var p in filtered) lvCatalog.Items.Add(new ListViewItem(p.Id.ToString()) { SubItems = { p.Name, p.Price.ToString("F2") }, Tag = p });
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

        // --- ПОВНОЦІННА ТЕМНА ТЕМА (БЕЗ TABCONTROL) ---
        private bool isDark = false;
        
        private void ToggleTheme()
        {
            isDark = !isDark;
            Color bgMain = isDark ? Color.FromArgb(30, 30, 30) : Color.FromArgb(240, 242, 245);
            Color bgSec = isDark ? Color.FromArgb(45, 45, 48) : Color.White;
            Color fg = isDark ? Color.White : Color.FromArgb(33, 37, 41);

            this.BackColor = bgMain;
            UpdateThemeRecursive(this, bgMain, bgSec, fg);
            
            // Оновлюємо кнопки вкладок під нову тему
            SwitchTab(pnlCatalog.Visible ? pnlCatalog : (pnlCart.Visible ? pnlCart : pnlOrders), 
                      pnlCatalog.Visible ? btnTabCatalog : (pnlCart.Visible ? btnTabCart : btnTabOrders));
        }

        private void UpdateThemeRecursive(Control parent, Color bgMain, Color bgSec, Color fg)
        {
            foreach (Control c in parent.Controls)
            {
                if (c is Panel)
                {
                    c.BackColor = bgMain;
                }
                else if (c is ListView || c is TextBox)
                {
                    c.BackColor = bgSec;
                    c.ForeColor = fg;
                }
                else if (c is Label)
                {
                    c.ForeColor = fg;
                }

                if (c.HasChildren)
                {
                    UpdateThemeRecursive(c, bgMain, bgSec, fg);
                }
            }
        }
    }
}