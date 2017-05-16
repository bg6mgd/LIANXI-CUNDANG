using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace SQWRZS
{
    public partial class frmPic : Form
    {
        public frmPic()
        {
            InitializeComponent();
        }

        private void pictureBox1_DoubleClick(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Normal)
            {
                this.WindowState = FormWindowState.Maximized;
            }
            else
            {
                this.Close();
            }
        }
    }
}
