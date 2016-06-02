using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace KellTerminal.Controls
{
    public partial class SettingEditor : UserControl
    {
        public SettingEditor()
        {
            InitializeComponent();
        }

        public void ClearSettings()
        {
            this.Controls.Clear();
        }

        public void LoadSetting(Dictionary<string, string> setting)
        {
            if (setting != null)
            {
                int x = 4;
                int x2 = 110;
                int y = 4;
                this.Controls.Clear();
                foreach (string key in setting.Keys)
                {
                    string val = "";
                    if (setting[key] != null)
                        val = setting[key];
                    Label l = new Label();
                    l.Name = "L_" + key;
                    if (key.ToUpper() == "ID")
                        l.Text = key + "[主键]";
                    else
                        l.Text = key;
                    l.AutoSize = false;
                    l.AutoEllipsis = true;
                    l.Size = new Size(100, 21);
                    l.TextAlign = ContentAlignment.MiddleRight;
                    l.Location = new Point(x, y);
                    this.Controls.Add(l);
                    TextBox t = new TextBox();
                    t.Name = key;
                    t.Text = val;
                    t.Size = new Size(150, 21);
                    t.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right;
                    t.Location = new Point(x2, y);
                    if (key.ToUpper() == "ID")
                        t.Enabled = false;
                    this.Controls.Add(t);
                    y += 26;
                }
            }
        }

        public Dictionary<string, string> Result
        {
            get
            {
                Dictionary<string, string> current = new Dictionary<string, string>();
                foreach (Control c in this.Controls)
                {
                    if (c is TextBox)
                    {
                        TextBox t = c as TextBox;
                        current.Add(t.Name, t.Text);
                    }
                }
                return current;
            }
        }

        public void NewSetting(Dictionary<string, string> setting)
        {
            if (setting != null)
            {
                int x = 4;
                int x2 = 110;
                int y = 4;
                this.Controls.Clear();
                foreach (string key in setting.Keys)
                {
                    string val = "";
                    if (setting[key] != null)
                        val = setting[key];
                    Label l = new Label();
                    l.Name = "L_" + key;
                    if (key.ToUpper() == "ID")
                        l.Text = key + "[主键]";
                    else
                        l.Text = key;
                    l.AutoSize = false;
                    l.AutoEllipsis = true;
                    l.Size = new Size(100, 21);
                    l.TextAlign = ContentAlignment.MiddleRight;
                    l.Location = new Point(x, y);
                    this.Controls.Add(l);
                    TextBox t = new TextBox();
                    t.Name = key;
                    t.Text = val;//"";
                    t.Size = new Size(150, 21);
                    t.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right;
                    t.Location = new Point(x2, y);
                    if (key.ToUpper() == "ID")
                        t.Enabled = false;
                    this.Controls.Add(t);
                    y += 26;
                }
            }
        }

        public void EditSetting(Dictionary<string, string> setting)
        {
            if (setting != null)
            {
                int x = 4;
                int x2 = 110;
                int y = 4;
                this.Controls.Clear();
                foreach (string key in setting.Keys)
                {
                    string val = "";
                    if (setting[key] != null)
                        val = setting[key];
                    Label l = new Label();
                    l.Name = "L_" + key;
                    if (key.ToUpper() == "ID")
                        l.Text = key + "[主键]";
                    else
                        l.Text = key;
                    l.AutoSize = false;
                    l.AutoEllipsis = true;
                    l.Size = new Size(100, 21);
                    l.TextAlign = ContentAlignment.MiddleRight;
                    l.Location = new Point(x, y);
                    this.Controls.Add(l);
                    TextBox t = new TextBox();
                    t.Name = key;
                    t.Text = val;
                    t.Size = new Size(150, 21);
                    t.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right;
                    t.Location = new Point(x2, y);
                    if (key.ToUpper() == "ID")
                        t.Enabled = false;
                    this.Controls.Add(t);
                    y += 26;
                }
            }
        }
    }
}