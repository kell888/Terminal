using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Configuration;
using KellTerminal.Model;

namespace KellTerminal
{
    public partial class TerminalForm : Form
    {
        public TerminalForm(MainForm owner)
        {
            InitializeComponent();
            this.owner = owner;
        }

        MainForm owner;
        StringBuilder where;
        string name, user, status, enable;
        bool all;

        private void TerminalForm_Load(object sender, EventArgs e)
        {
            LoadEditor("Terminals");
            LoadTableToSetting("Terminals");
            List<Users> users = Common.GetMoreUsers();
            foreach (Users user in users)
            {
                comboBox1.Items.Add(user);
            }
            List<RunStatus> status = Common.GetAllStatus();
            status.Sort();
            foreach (RunStatus s in status)
            {
                 comboBox2.Items.Add(s);
            }
        }

        private void LoadEditor(string table)
        {
            Dictionary<string, string> record = EditCommon.GetNewRecord(table);
            settingEditor1.NewSetting(record);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Dictionary<string, string> record = settingEditor1.Result;
            if (record != null && record.Count > 0)
            {
                int id;
                record.Remove("ID");
                bool flag = EditCommon.InsertRecordDB(record, "Terminals", out id);
                Users u = Common.GetOneUsers(Convert.ToInt32(record["UserID"]));
                Common.UpdateDeadline(u);
                MessageBox.Show("添加数据" + (flag ? "成功" : "失败"));
                if (flag) LoadTableToSetting("Terminals");
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Dictionary<string, string> record = settingEditor1.Result;
            if (record != null && record.Count > 0)
            {
                bool flag = false;
                int id = Convert.ToInt32(record["ID"]);
                record.Remove("ID");
                bool isEnable = Common.GetOneTerminals(id).IsEnable;
                if (record["IsEnable"] != (isEnable ? "1" : "0"))
                {
                    Users u = Common.GetOneUsers(Convert.ToInt32(record["UserID"]));
                    flag = Common.UpdateDeadline(u);
                    Common.UpdateKellControl(record["IP"], isEnable);
                }
                flag = EditCommon.UpdateRecordDB(record, "Terminals", id);
                MessageBox.Show("修改数据" + (flag ? "成功" : "失败"));
                if (flag) LoadTableToSetting("Terminals");
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("确定要删除该记录吗？", "删除提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == System.Windows.Forms.DialogResult.OK)
            {
                Dictionary<string, string> record = settingEditor1.Result;
                if (record != null && record.Count > 0)
                {
                    int id = Convert.ToInt32(record["ID"]);
                    bool flag = EditCommon.DeleteRecordDB("Terminals", id);
                    if (flag) Common.DeleteKellControl(record["IP"]);
                    MessageBox.Show("删除数据" + (flag ? "成功" : "失败"));
                    if (flag) LoadTableToSetting("Terminals");
                }
            }
        }

        private void settingList_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            button2.Enabled = button3.Enabled = true;
            if (e.RowIndex > -1)
            {
                DataTable dt = settingList.DataSource as DataTable;
                if (dt != null && dt.Rows.Count > 0)
                {
                    Dictionary<string, string> record = new Dictionary<string, string>();
                    DataRow row = dt.Rows[e.RowIndex];
                    for (int i = 0; i < row.Table.Columns.Count; i++)
                    {
                        record.Add(row.Table.Columns[i].ColumnName, row[i].ToString());
                    }
                    EditSetting(record);
                }
            }
        }

        private void EditSetting(Dictionary<string, string> record)
        {
            settingEditor1.LoadSetting(record);
            if (record != null && record.Count > 0)
            {
                string status = record["RunStatus"];
                if (status == "1")
                {
                    button4.Enabled = false;
                    button5.Enabled = true;
                }
                else
                {
                    button4.Enabled = true;
                    button5.Enabled = false;
                }
            }
        }

        private void LoadTableToSetting(string table, string where = "(1=1)")
        {
            toolStripStatusLabel1.Text = "终端载入中，请稍候 - " + Common.GetNow();
            try
            {
                button2.Enabled = button3.Enabled = false;
                DataTable dt = EditCommon.LoadTableFromDB(table, where);
                settingList.DataSource = dt;
            }
            catch { }
            finally
            {
                toolStripStatusLabel1.Text = "终端载入完毕 - " + Common.GetNow();
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            if (textBox1.Text.Trim() != "")
            {
                name = " and Name like '%" + textBox1.Text.Trim() + "%'";
            }
            else
            {
                name = "";
            }
            if (!all)
            {
                where = new StringBuilder("(1=1)");
                where.Append(name);
                where.Append(user);
                where.Append(status);
                where.Append(enable);
                LoadTableToSetting("Terminals", where.ToString());
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox1.SelectedIndex > -1)
            {
                int userId = (comboBox1.Items[comboBox1.SelectedIndex] as Users).ID;
                user = " and UserID=" + userId;
            }
            else
            {
                user = "";
            }
            if (!all)
            {
                where = new StringBuilder("(1=1)");
                where.Append(name);
                where.Append(user);
                where.Append(status);
                where.Append(enable);
                LoadTableToSetting("Terminals", where.ToString());
            }
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox2.SelectedIndex > -1)
            {
                status = " and RunStatus=" + (comboBox2.Items[comboBox2.SelectedIndex] as RunStatus).ID;
            }
            else
            {
                status = "";
            }
            if (!all)
            {
                where = new StringBuilder("(1=1)");
                where.Append(name);
                where.Append(user);
                where.Append(status);
                where.Append(enable);
                LoadTableToSetting("Terminals", where.ToString());
            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            enable = " and IsEnable=" + (checkBox1.Checked ? "0" : "1");
            if (!all)
            {
                where = new StringBuilder("(1=1)");
                where.Append(name);
                where.Append(user);
                where.Append(status);
                where.Append(enable);
                LoadTableToSetting("Terminals", where.ToString());
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            all = true;
            textBox1.Text = "";
            comboBox1.SelectedIndex = -1;
            comboBox2.SelectedIndex = -1;
            checkBox1.Checked = false;
            LoadTableToSetting("Terminals");
            all = false;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            Dictionary<string, string> record = settingEditor1.Result;
            if (record != null && record.Count > 0)
            {
                string ip = record["IP"];
                if (!string.IsNullOrEmpty(ip))
                {
                    owner.RunClient(ip);
                    button4.Enabled = false;
                    button5.Enabled = true;
                }
            }
            else
            {
                MessageBox.Show("请先选定一个终端再启动！");
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            Dictionary<string, string> record = settingEditor1.Result;
            if (record != null && record.Count > 0)
            {
                string ip = record["IP"];
                if (!string.IsNullOrEmpty(ip))
                {
                    owner.StopClient(ip);
                    button4.Enabled = true;
                    button5.Enabled = false;
                }
            }
            else
            {
                MessageBox.Show("请先选定一个终端再停止！");
            }
        }
    }
}
