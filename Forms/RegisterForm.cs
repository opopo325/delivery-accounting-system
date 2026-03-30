using System;
using System.Linq;
using System.Drawing;
using System.Windows.Forms;

#nullable disable

namespace DeliverySystem
{
    public class RegisterForm : Form
    {
        private Database db;
        private RadioButton rbClient, rbDriver;
        private Label lblExtra1, lblExtra2;
        private TextBox txtName, txtPass, txtPhone, txtExtra1, txtExtra2;

        public RegisterForm(Database database)
        {
            db = database;
            BuildUI();
        }

        private void BuildUI()
        {
            this.Text = "Система Доставки — Реєстрація";
            this.ClientSize = new Size(420, 560); // Більша висота для гарантії
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.BackColor = Color.FromArgb(240, 242, 245);

            Panel pnlCard = new Panel { Size = new Size(380, 520), Location = new Point(20, 20), BackColor = Color.White };

            var lblTitle = new Label { Text = "📝 Нова реєстрація", Font = new Font("Segoe UI", 16, FontStyle.Bold), ForeColor = Color.FromArgb(33, 37, 41), AutoSize = true, Location = new Point(20, 15) };

            rbClient = new RadioButton { Text = "Клієнт", Location = new Point(25, 60), Font = new Font("Segoe UI", 10, FontStyle.Bold), Checked = true, AutoSize = true, ForeColor = Color.FromArgb(0, 122, 255) };
            rbDriver = new RadioButton { Text = "Кур'єр", Location = new Point(130, 60), Font = new Font("Segoe UI", 10, FontStyle.Bold), AutoSize = true, ForeColor = Color.FromArgb(255, 149, 0) };
            rbClient.CheckedChanged += Role_Changed; rbDriver.CheckedChanged += Role_Changed;

            int y = 95, w = 330;
            var lblName = MakeLabel("Ім'я:", 20, y); txtName = MakeTextBox(20, y += 25, w); y += 40;
            var lblPass = MakeLabel("Пароль (мін. 4 символи):", 20, y); txtPass = MakeTextBox(20, y += 25, w, true); y += 40;
            var lblPhone = MakeLabel("Телефон:", 20, y); txtPhone = MakeTextBox(20, y += 25, w); y += 40;
            
            lblExtra1 = MakeLabel("Email:", 20, y); txtExtra1 = MakeTextBox(20, y += 25, w); y += 40;
            lblExtra2 = MakeLabel("Адреса доставки:", 20, y); txtExtra2 = MakeTextBox(20, y += 25, w); y += 50;

            Button btnRegister = MakeButton("Зареєструватись", 20, y, 200, Color.FromArgb(52, 199, 89));
            btnRegister.Click += BtnRegister_Click;
            Button btnBack = MakeButton("Назад", 230, y, 120, Color.FromArgb(224, 224, 224));
            btnBack.ForeColor = Color.Black;
            btnBack.Click += (s, e) => this.Close();

            pnlCard.Controls.AddRange(new Control[] { lblTitle, rbClient, rbDriver, lblName, txtName, lblPass, txtPass, lblPhone, txtPhone, lblExtra1, txtExtra1, lblExtra2, txtExtra2, btnRegister, btnBack });
            this.Controls.Add(pnlCard);
            this.AcceptButton = btnRegister;
        }

        private Label MakeLabel(string text, int x, int y) => new Label { Text = text, Location = new Point(x, y), AutoSize = true, Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = Color.Gray };
        private TextBox MakeTextBox(int x, int y, int w, bool isPass = false) => new TextBox { Location = new Point(x, y), Width = w, Font = new Font("Segoe UI", 11), BorderStyle = BorderStyle.FixedSingle, PasswordChar = isPass ? '●' : '\0' };
        private Button MakeButton(string text, int x, int y, int w, Color color)
        {
            var btn = new Button { Text = text, Location = new Point(x, y), Width = w, Height = 40, BackColor = color, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10, FontStyle.Bold), Cursor = Cursors.Hand };
            btn.FlatAppearance.BorderSize = 0; return btn;
        }

        private void Role_Changed(object sender, EventArgs e)
        {
            lblExtra1.Text = rbClient.Checked ? "Email:" : "Номер машини:";
            lblExtra2.Visible = txtExtra2.Visible = rbClient.Checked;
        }

        private void BtnRegister_Click(object sender, EventArgs e)
        {
            var usersDb = db.LoadUsers();
            string name = txtName.Text.Trim(), pass = txtPass.Text.Trim(), phone = txtPhone.Text.Trim();

            if (string.IsNullOrWhiteSpace(name)) { Warn("Ім'я не може бути порожнім."); return; }
            if (usersDb.Any(u => u.Name.Equals(name, StringComparison.OrdinalIgnoreCase))) { Warn($"Користувач '{name}' вже існує."); return; }
            if (string.IsNullOrWhiteSpace(pass) || pass.Length < 4) { Warn("Пароль занадто короткий."); return; }
            if (string.IsNullOrWhiteSpace(phone)) { Warn("Телефон не може бути порожнім."); return; }

            int newId = usersDb.Count > 0 ? usersDb.Max(u => u.Id) + 1 : 1;

            if (rbClient.Checked)
            {
                if (string.IsNullOrWhiteSpace(txtExtra1.Text)) { Warn("Email порожній."); return; }
                if (string.IsNullOrWhiteSpace(txtExtra2.Text)) { Warn("Адреса порожня."); return; }
                usersDb.Add(new Client(newId, name, phone, pass, txtExtra1.Text.Trim(), txtExtra2.Text.Trim()));
            }
            else
            {
                if (string.IsNullOrWhiteSpace(txtExtra1.Text)) { Warn("Номер машини порожній."); return; }
                usersDb.Add(new Driver(newId, name, phone, pass, txtExtra1.Text.Trim()));
            }

            db.SaveUsers(usersDb);
            MessageBox.Show("Чотко! Тебе зареєстровано.", "Успіх!", MessageBoxButtons.OK, MessageBoxIcon.Information);
            this.Close();
        }

        private void Warn(string msg) => MessageBox.Show(msg, "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
    }
}