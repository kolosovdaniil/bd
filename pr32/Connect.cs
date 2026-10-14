using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace pr32
{
    public partial class Connect : Form
    {
        public Connect()
        {
            InitializeComponent();
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            txtHost.Text = Properties.Settings.Default.host;
            txtUsername.Text = Properties.Settings.Default.uid;
            txtPassword.Text = Properties.Settings.Default.pwd;
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            string connect = $"host={txtHost.Text}; uid={txtUsername.Text};pwd={txtPassword.Text};database=trade;";

            MySqlConnection con = new MySqlConnection(connect);

            try
            {
                con.Open();

                MessageBox.Show($"Успешное подключение к БД", "Ок", MessageBoxButtons.OK, MessageBoxIcon.Information);
                Properties.Settings.Default.host = txtHost.Text;
                Properties.Settings.Default.uid = txtUsername.Text;
                Properties.Settings.Default.pwd = txtPassword.Text;
                Properties.Settings.Default.Save();

                this.DialogResult = DialogResult.OK;
                this.Close();
                con.Close();

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка подключения к {connect}\n{ex.Message}", "ОШИБКА", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
