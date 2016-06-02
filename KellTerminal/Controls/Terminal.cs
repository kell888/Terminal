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
    public partial class Terminal : UserControl
    {
        public Terminal()
        {
            InitializeComponent();
        }

        public void LoadNodes(List<Terminals> terms)
        {
            this.panel1.Controls.Clear();
            if (terms != null && terms.Count > 0)
            {
                int i = 1;
                foreach (Terminals term in terms)
                {
                    Node node = new Node(true);
                    node.LoadInfo(term);
                    node.SetNo(i.ToString());
                    node.Dock = DockStyle.Left;
                    this.panel1.Controls.Add(node);
                    i++;
                }
            }
        }
    }
}
