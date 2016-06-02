using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using KellTerminal.Model;

namespace KellTerminal
{
    public partial class SearchForm : Form
    {
        public SearchForm()
        {
            InitializeComponent();
        }

        Users select;
        public Users SelectUser { get { return select; } }

        private void comboBox1_TextUpdate(object sender, EventArgs e)
        {
            string s = comboBox1.Text;
            List<Users> users = Common.GetMoreUsers("Name like '" + s + "%'");
            if (users.Count > 0)
            {
                select = users[0];
                comboBox1.Text = select.Name;
                //comboBox1.SelectedText = s;
            }
        }

        private void SearchForm_Load(object sender, EventArgs e)
        {
            List<Users> users = Common.GetMoreUsers();
            users.Sort();
            comboBox1.Tag = users;
            foreach (Users u in users)
            {
                comboBox1.Items.Add(u);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.OK;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            select = null;
            this.DialogResult = System.Windows.Forms.DialogResult.OK;
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox1.SelectedIndex > -1)
            {
                select = comboBox1.SelectedItem as Users;
            }
        }
    }
}
