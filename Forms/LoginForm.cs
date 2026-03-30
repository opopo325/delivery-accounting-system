using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

#nullable disable

namespace DeliverySystem
{
    public class LoginForm : Form
    {
        private Database db;
        private List<User> usersDb;

        private TextBox txtName, txtPass;

        public LoginForm()
        {
            db = new Database();
            usersDb = db.LoadUsers();
            BuildUI();
        }

        private void BuildUI()
        {
            this.Text = "Система Доставки — Вхід";
            this.ClientSize = new Size(400, 360); // Використовуємо ClientSize, щоб не обрізався низ!
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.BackColor = Color.FromArgb(240, 242, 245); // Сучасний світло-сірий фон

            Panel pnlCard = new Panel
            {
                Size = new Size(340, 300),
                Location = new Point(30, 30),
                BackColor = Color.White
            };

            var lblTitle = new Label { Text = "🚀 Система Доставки", Font = new Font("Segoe UI", 16, FontStyle.Bold), ForeColor = Color.FromArgb(33, 37, 41), AutoSize = true, Location = new Point(20, 20) };
            
            var lblName = new Label { Text = "Ім'я:", Location = new Point(20, 80), AutoSize = true, Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = Color.Gray };
            txtName = new TextBox { Location = new Point(20, 105), Width = 300, Font = new Font("Segoe UI", 12), BorderStyle = BorderStyle.FixedSingle };

            var lblPass = new Label { Text = "Пароль:", Location = new Point(20, 150), AutoSize = true, Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = Color.Gray };
            txtPass = new TextBox { Location = new Point(20, 175), Width = 300, Font = new Font("Segoe UI", 12), PasswordChar = '●', BorderStyle = BorderStyle.FixedSingle };

            Button btnLogin = MakeButton("Увійти", 20, 230, 145, Color.FromArgb(0, 122, 255));
            btnLogin.Click += BtnLogin_Click;

            Button btnRegister = MakeButton("Реєстрація", 175, 230, 145, Color.FromArgb(224, 224, 224));
            btnRegister.ForeColor = Color.Black;
            btnRegister.Click += BtnRegister_Click;

            pnlCard.Controls.AddRange(new Control[] { lblTitle, lblName, txtName, lblPass, txtPass, btnLogin, btnRegister });
            this.Controls.Add(pnlCard);
            this.AcceptButton = btnLogin;
        }

        private Button MakeButton(string text, int x, int y, int w, Color color)
        {
            var btn = new Button { Text = text, Location = new Point(x, y), Width = w, Height = 40, BackColor = color, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10, FontStyle.Bold), Cursor = Cursors.Hand };
            btn.FlatAppearance.BorderSize = 0;
            return btn;
        }

        private void BtnLogin_Click(object sender, EventArgs e)
        {
            usersDb = db.LoadUsers();
            string inputName = txtName.Text.Trim();
            string inputPass = txtPass.Text.Trim();

            if (string.IsNullOrEmpty(inputName) || string.IsNullOrEmpty(inputPass)) { MessageBox.Show("Заповни всі поля!", "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }

            var currentUser = usersDb.FirstOrDefault(u => u.Name.Trim().Equals(inputName, StringComparison.OrdinalIgnoreCase) && u.Password.Trim() == inputPass);

            if (currentUser == null) { MessageBox.Show("Неправильний логін або пароль!", "Помилка входу", MessageBoxButtons.OK, MessageBoxIcon.Error); return; }

            this.Hide();
            if (currentUser is Client client) new ClientForm(client, db).ShowDialog();
            else if (currentUser is Driver driver) new DriverForm(driver, db, usersDb).ShowDialog();
            
            this.Show();
            txtName.Clear(); txtPass.Clear();
        }

        private void BtnRegister_Click(object sender, EventArgs e)
        {
            this.Hide();
            new RegisterForm(db).ShowDialog();
            this.Show();
        }
    }
}