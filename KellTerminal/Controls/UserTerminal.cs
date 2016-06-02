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
    public partial class UserTerminal : UserControl
    {
        public UserTerminal()
        {
            InitializeComponent();
        }

        Users use;

        public Users User
        {
            set
            {
                use = value;
                this.panel1.Controls.Clear();
                User user = new User(true);
                user.LoadInfo(use);
                user.Dock = DockStyle.Fill;
                this.panel1.Controls.Add(user);
            }
        }

        public string UserName
        {
            get
            {
                return use.Name;
            }
        }

        List<Terminals> terms;

        public List<Terminals> Terminals
        {
            set
            {
                terms = value;
                this.panel2.Controls.Clear();
                Terminal terminal = new Terminal();
                terminal.LoadNodes(terms);
                terminal.Dock = DockStyle.Fill;
                this.panel2.Controls.Add(terminal);
            }
        }

        public int TerminalCount
        {
            get
            {
                if (terms != null)
                    return terms.Count;
                return 0;
            }
        }
    }
}
