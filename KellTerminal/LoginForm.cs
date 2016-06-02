using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using KellTerminal.SQL;

namespace KellTerminal
{
    public partial class LoginForm : Form
    {
        public LoginForm()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string username = textBox1.Text.Trim();
            string pwd = textBox2.Text;
            SQLDBHelper sqlHelper = new SQLDBHelper();
            object o = sqlHelper.GetSingle("select pwd from Managers where username='" + username + "'");
            if (o != null && o != DBNull.Value)
            {
                if (o.ToString() == pwd)
                {
                    this.DialogResult = System.Windows.Forms.DialogResult.OK;
                }
                else
                {
                    MessageBox.Show("密码错误！");
                    textBox2.SelectAll();
                    textBox2.Focus();
                }
            }
            else
            {
                MessageBox.Show("该账号不存在！");
                textBox1.SelectAll();
                textBox1.Focus();
            }
        }
    }
}
