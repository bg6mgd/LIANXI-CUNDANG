using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using mydll;
namespace testdll
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        Class1 obj = new Class1();

        private void button1_Click(object sender, EventArgs e)
        {
            string a = textBox1.Text.ToString();
            int ss = strlength(a);
          MessageBox.Show(ss.ToString())  ;

        }
        private int strlength(string a)
        {
            return obj.strlength(a);
        }
        private string cancet(string c,string d )
        {
            return obj.concat(c, d);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            string a = textBox1.Text.ToString();
            string b = textBox2.Text.ToString();
            string x = cancet(a, b);
            textBox3.Text = x;

        }
    }
}
