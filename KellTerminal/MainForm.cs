using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using KellTerminal.SQL;
using KellTerminal.Model;
using KellTerminal.Controls;
using DataTransfer;
using System.Net;

namespace KellTerminal
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
            server = new Server();
            server.RefreshStatus += new EventHandler<StatusArgs>(server_RefreshStatus);
        }

        void server_RefreshStatus(object sender, StatusArgs e)
        {
            Common.UpdateStatus(e.IP, e.Status);
        }

        Server server;
        bool sure;

        /// <summary>
        /// 启动指定一个终端(会以同步阻塞的方式进行)
        /// </summary>
        /// <param name="ip"></param>
        public void RunClient(string ip)
        {
            if (server != null)
            {
                server.Run(ip);
            }
        }

        /// <summary>
        /// 停止指定一个终端(会以同步阻塞的方式进行)
        /// </summary>
        /// <param name="ip"></param>
        public void StopClient(string ip)
        {
            if (server != null)
            {
                server.Stop(ip);
            }
        }

        private void Exit()
        {
            sure = true;
            this.Close();
        }

        private void 退出程序ToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            Exit();
        }

        private void 退出程序ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Exit();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (sure)
            {
                if (MessageBox.Show("确定要退出程序吗？", "退出提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == System.Windows.Forms.DialogResult.OK)
                {
                    StopTerminalServer();
                    notifyIcon1.Dispose();
                    Environment.Exit(0);
                }
                else
                {
                    e.Cancel = true;
                    sure = false;
                }
            }
            else
            {
                e.Cancel = true;
                this.Hide();
            }
        }

        private void StopTerminalServer()
        {
            server.End();
        }

        private void ShowUI()
        {
            if (this.WindowState == FormWindowState.Minimized)
                this.WindowState = FormWindowState.Normal;
            this.Show();
            this.BringToFront();
        }

        private void 显示界面ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ShowUI();
        }

        private void notifyIcon1_DoubleClick(object sender, EventArgs e)
        {
            ShowUI();
        }

        private void 用户管理ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            UserForm f2 = new UserForm();
            f2.ShowDialog();
            RefreshTerminals();
        }

        private void 系统设置ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SettingForm f3 = new SettingForm();
            f3.ShowDialog();
            RefreshTerminals();
        }

        private void 数据库管理ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DBForm f4 = new DBForm();
            f4.ShowDialog();
        }

        private void 管理员设置ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ManagerForm f5 = new ManagerForm();
            f5.ShowDialog();
        }

        private void 查询终端ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SearchForm f6 = new SearchForm();
            if (f6.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                Users user = f6.SelectUser;
                RefreshTerminals(user);
            }
        }

        private void 终端管理ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TerminalForm f7 = new TerminalForm(this);
            f7.ShowDialog();
            RefreshTerminals();
        }

        private void RefreshTerminals(Users user = null)
        {
            this.panel1.Controls.Clear();
            string sql = "select * from Terminals";
            if (user != null)
                sql += " where UserID=" + user.ID;
            else
                sql += " order by UserID desc, TypeID desc";
            DataTable dt = null;
            try
            {
                SQLDBHelper sqlHelper = new SQLDBHelper();
                dt = sqlHelper.Query(sql);
            }
            catch (Exception e)
            {
                MessageBox.Show("数据库操作失败：" + e.Message);
                info.Text = "数据库操作失败：" + e.Message + " - " + Common.GetNow();
            }
            if (dt != null && dt.Rows.Count > 0)
            {
                info.Text = "终端载入中，请稍候... - " + Common.GetNow();
                List<Terminals> terms = new List<Terminals>();
                UserTerminal ut = null;
                int userid = Convert.ToInt32(dt.Rows[0]["UserID"]);
                Terminals term = null;
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    if (Convert.ToInt32(dt.Rows[i]["UserID"]) != userid)
                    {
                        ut = new UserTerminal();
                        ut.Dock = DockStyle.Top;
                        if (user != null)
                            ut.User = user;
                        else
                            ut.User = GetUserById(userid);
                        Terminals[] ts = new Terminals[terms.Count];
                        terms.CopyTo(ts);
                        ut.Terminals = new List<Terminals>(ts);
                        this.panel1.Controls.Add(ut);
                        terms.Clear();
                        userid = Convert.ToInt32(dt.Rows[i]["UserID"]);
                    }
                    term = new Terminals();
                    term.ID = Convert.ToInt32(dt.Rows[i]["ID"]);
                    term.TypeID = Convert.ToInt32(dt.Rows[i]["TypeID"]);
                    term.Name = dt.Rows[i]["Name"].ToString();
                    term.IP = dt.Rows[i]["IP"].ToString();
                    term.UserID = Convert.ToInt32(dt.Rows[i]["UserID"]);
                    if (dt.Rows[i]["SIM"] != null && dt.Rows[i]["SIM"] != DBNull.Value)
                        term.SIM = dt.Rows[i]["SIM"].ToString();
                    else
                        term.SIM = string.Empty;
                    if (dt.Rows[i]["Remark"] != null && dt.Rows[i]["Remark"] != DBNull.Value)
                        term.Remark = dt.Rows[i]["Remark"].ToString();
                    else
                        term.Remark = string.Empty;
                    term.RunStatus = Convert.ToInt32(dt.Rows[i]["RunStatus"]);
                    term.IsEnable = Convert.ToBoolean(dt.Rows[i]["IsEnable"]);
                    terms.Add(term);
                }
                ut = new UserTerminal();
                ut.Dock = DockStyle.Top;
                if (user != null)
                    ut.User = user;
                else
                    ut.User = GetUserById(userid);
                ut.Terminals = terms;
                this.panel1.Controls.Add(ut);
                if (user != null)
                    info.Text = "载入完毕：用户[" + ut.UserName + "]共有" + ut.TerminalCount + "个终端" + " - " + Common.GetNow();
                else
                    info.Text = "载入完毕：系统中共有" + this.panel1.Controls.Count + "个用户" + " - " + Common.GetNow();
            }
            else
            {
                info.Text = "系统中尚未存在任何用户" + " - " + Common.GetNow();
            }
        }

        private Users GetUserById(int id)
        {
            try
            {
                return Common.GetOneUsers(id);
            }
            catch (Exception e)
            {
                info.Text = "数据库操作失败：" + e.Message + " - " + Common.GetNow();
            }
            return null;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            LoginForm f8 = new LoginForm();
            if (f8.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                this.panel2.SendToBack();
                this.panel2.Hide();
                RefreshTerminals();
                StartTerminalServer();
            }
            else
            {
                notifyIcon1.Dispose();
                Environment.Exit(0);
            }
        }

        private void StartTerminalServer()
        {
            server.Start();
        }

        private void 锁定系统ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LockSystem();
        }

        private void 锁定系统ToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            LockSystem();
        }

        private void LockSystem()
        {
            this.textBox1.Clear();
            this.textBox2.Clear();
            this.WindowState = FormWindowState.Normal;
            this.BringToFront();
            this.Show();
            this.panel2.Show();
            this.panel2.BringToFront();
            DisableMenus();
        }

        private void UnlockSystem()
        {
            this.panel2.SendToBack();
            this.panel2.Hide();
            EnableMenus();
        }

        private void DisableMenus()
        {
            foreach (ToolStripMenuItem c in menuStrip1.Items)
            {
                if (c.Name != "锁定系统ToolStripMenuItem")
                    c.Enabled = false;
            }
            foreach (ToolStripItem c in contextMenuStrip1.Items)
            {
                if (c.Name != "锁定系统ToolStripMenuItem1")
                    c.Enabled = false;
            }
        }

        private void EnableMenus()
        {
            foreach (ToolStripMenuItem c in menuStrip1.Items)
            {
                if (c.Name != "锁定系统ToolStripMenuItem")
                    c.Enabled = true;
            }
            foreach (ToolStripItem c in contextMenuStrip1.Items)
            {
                if (c.Name != "锁定系统ToolStripMenuItem1")
                    c.Enabled = true;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {//解锁
            string username = textBox1.Text.Trim();
            string pwd = textBox2.Text;
            SQLDBHelper sqlHelper = new SQLDBHelper();
            object o = sqlHelper.GetSingle("select pwd from Managers where username='" + username + "'");
            if (o != null && o != DBNull.Value)
            {
                if (o.ToString() == pwd)
                {
                    UnlockSystem();
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
