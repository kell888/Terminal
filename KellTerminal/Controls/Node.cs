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
    public partial class Node : UserControl
    {
        public Node(bool readOnly = false)
        {
            InitializeComponent();
            textBox1.ReadOnly = textBox2.ReadOnly = textBox3.ReadOnly = textBox4.ReadOnly = textBox5.ReadOnly = textBox6.ReadOnly = textBox7.ReadOnly = readOnly;
        }

        public void LoadInfo(Terminals term)
        {
            textBox1.Text = Common.GetTypeNameByID(term.TypeID);
            textBox2.Text = term.Name;
            textBox3.Text = term.IP;
            textBox4.Text = term.SIM;
            textBox5.Text = Common.GetRunStatus(term.RunStatus);
            textBox6.Text = term.IsEnable ? "是" : "否";
            textBox7.Text = term.Remark;
        }
        public void SetNo(string no)
        {
            label8.Text = no;
        }
    }
}
