using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace pr32
{
    public partial class Client : Form
    {
        public Client()//(*string fullName*)
        {
            InitializeComponent();
            //lblUserInfo.Text = "Добро пожаловать, " + fullName + "!";
        }

        private void btnLogout_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}
