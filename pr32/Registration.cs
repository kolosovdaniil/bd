using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using System.Configuration;
using System.Threading;


namespace pr32
{
    public partial class Registration : Form
    {
        int count;
        string txtcaptcha;

        public Registration()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            pictureBox2.Visible = false;
            textBox1.Visible = false;

            Configuration currentConfig = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

            string host = Properties.Settings.Default.host;
            string uid = Properties.Settings.Default.uid;
            string pwd = Properties.Settings.Default.pwd;

            string connect_Set = $"host={host};uid={uid};pwd={pwd};database=trade;";

            if (count == 1)
            {
                this.CreateCaptcha(pictureBox2.Width, pictureBox2.Height);
            }
        }
        private void ShowCaptcha()
        {
            pictureBox2.Image = CreateCaptcha(pictureBox2.Width, pictureBox2.Height);
            pictureBox2.Visible = true;
            textBox1.Visible = true;
            textBox1.Clear();
        }

        private async Task CheckAttemptsAsync()
        {
            if (count >= 2)
            {
                MessageBox.Show(
                    "Превышено количество попыток ввода!",
                    "Система заблокирована",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );

                await LockInterfaceAsync();

                count = 0;
            }
        }

        private async Task LockInterfaceAsync()
        {
            this.Enabled = false;

            await Task.Delay(10000);

            this.Enabled = true;

            MessageBox.Show("Доступ разблокирован. Попробуйте снова.", "Система", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private Bitmap CreateCaptcha(int Width, int Height)
        {
            Random random = new Random();
            Bitmap result = new Bitmap(Width, Height);

            int Xpos = Width / 4;
            int Ypos = Height / 4;

            Brush[] color =
            {
                Brushes.Black,
                Brushes.Red,
                Brushes.White
            };

            Pen[] colorLine =
            {
                Pens.Black,
                Pens.Red,
                Pens.White
            };

            FontStyle[] fontStyles =
            {
                FontStyle.Regular,
                FontStyle.Bold,
                FontStyle.Italic
            };

            Int16[] rotate = { 1, -1, 2, -2, 3, -3, 4, -4, 5, -5, 6, -6, 7, -7 };

            Graphics g = Graphics.FromImage((Image)result);

            g.Clear(Color.DarkOrange);

            g.RotateTransform(random.Next(rotate.Length));

            txtcaptcha = String.Empty;
            string bukv = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";

            for (int i = 0; i < 4; i++)
            {
                txtcaptcha += bukv[random.Next(bukv.Length)];
            }

            g.DrawString(txtcaptcha,
                        new Font("Comic Sans MS", 30, fontStyles[random.Next(fontStyles.Length)]),
                        color[random.Next(color.Length)],
                        new PointF(Xpos, Ypos));


            g.DrawString(txtcaptcha,
                new Font("Comic Sans MS", 30, fontStyles[random.Next(fontStyles.Length)]),
                color[random.Next(color.Length)],
                new PointF(Xpos, Ypos));

          
            g.DrawLine(colorLine[random.Next(colorLine.Length)],
                       new Point(0, 0),
                       new Point(Width -1, Height - 1));

            g.DrawLine(colorLine[random.Next(colorLine.Length)],
                       new Point(0, Height - 1),
                       new Point(Width - 1, 0));


            for (int i = 0; i < Width; ++i)
                for (int j = 0; j < Height; ++j)
                    if (random.Next() % 20 == 0)
                        result.SetPixel(i, j, Color.Black);

            return result;

        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            string login = txtLogin.Text.Trim();
            string password = txtPassword.Text.Trim();

            if (string.IsNullOrEmpty(login) || login == "Логин" || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Введите логин и пароль", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);

                count++;
                CheckAttemptsAsync();
                return;
            }

            if (pictureBox2.Visible)
            {
                string captchaInput = textBox1.Text.Trim();

                if (txtcaptcha != captchaInput)
                {
                    MessageBox.Show("Неверная капча", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);

                    count++;
                    CheckAttemptsAsync();
                    textBox1.Clear();
                    ShowCaptcha();
                    return;
                }
            }

            string host = Properties.Settings.Default.host;
            string uid = Properties.Settings.Default.uid;
            string pwd = Properties.Settings.Default.pwd;

            string connect = $"server={host};uid={uid};pwd={pwd};database=trade;";

            try
            {
                using (MySqlConnection connection = new MySqlConnection(connect))
                {
                    connection.Open();
                    string query = @"SELECT UserID, UserRole 
                             FROM user 
                             WHERE UserLogin = @login AND UserPassword = @password;";

                    using (MySqlCommand cmd = new MySqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@login", login);
                        cmd.Parameters.AddWithValue("@password", password);

                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                count = 0;

                                int userId = reader.GetInt32("UserID");
                                int roleId = reader.GetInt32("UserRole");

                                MessageBox.Show($"Вход выполнен!",
                                    "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);

                                this.Hide();
                                Client import = new Client();
                                import.ShowDialog();
                                this.Close();
                            }
                            else
                            {
                                MessageBox.Show("Неверный логин или пароль", "Ошибка входа",
                                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                                count++;
                                CheckAttemptsAsync();

                                txtPassword.Clear();
                                txtPassword.Focus();

                                if (count >= 1)
                                {
                                    ShowCaptcha();
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка подключения:\n{ex.Message}",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);

                this.Hide();

                Connect settings = new Connect();
                if (settings.ShowDialog() == DialogResult.OK)
                {
                    this.Show();
                }
                else
                {
                    this.Close();
                }
            }
        }
    }
}

