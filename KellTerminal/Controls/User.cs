using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using KellTerminal.Model;

namespace KellTerminal.Controls
{
    public partial class User : UserControl
    {
        public User(bool readOnly = false)
        {
            InitializeComponent();
            textBox1.ReadOnly = textBox2.ReadOnly = textBox3.ReadOnly = textBox4.ReadOnly = textBox5.ReadOnly = textBox6.ReadOnly = readOnly;
        }

        public void LoadInfo(Users user)
        {
            textBox1.Text = user.Name;
            textBox2.Text = user.Deadline.ToString("yyyy-MM-dd HH:mm:ss");
            textBox3.Text = user.ContactTel;
            textBox4.Text = user.ContactAddress;
            textBox5.Text = user.Contacter;
            textBox6.Text = user.Remark;
            label7.Text = Math.Round(Common.GetRestMoney(user), 2).ToString();
            toolTip1.SetToolTip(label7, label7.Text);
        }
    }
}
