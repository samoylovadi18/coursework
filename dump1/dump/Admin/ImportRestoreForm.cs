using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace dump
{
    public partial class ImportRestoreForm : Form
    {
        public ImportRestoreForm()
        {
            InitializeComponent();
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            
        }

        private void pictureBox2_Click_1(object sender, EventArgs e)
        {
            this.Visible = false;
            AdminForm admin = new AdminForm();
            admin.Show();
        }

        private void button1_Click(object sender, EventArgs e)
        {

        }
    }
}
