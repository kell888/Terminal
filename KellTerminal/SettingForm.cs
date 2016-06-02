using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using KellTerminal.Model;
using System.Net;

namespace KellTerminal
{
    public partial class SettingForm : Form
    {
        public SettingForm()
        {
            InitializeComponent();
        }

        private void SettingForm_Load(object sender, EventArgs e)
        {
            LoadEditor("TerminalTypes");
            LoadTableToSetting("TerminalTypes");
            IPAddress ip;
            int port;
            Common.GetServerSocket(out ip, out port);
            textBox1.Text = ip.ToString();
            textBox2.Text = port.ToString();
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
                bool flag = EditCommon.InsertRecordDB(record, "TerminalTypes", out id);
                MessageBox.Show("添加数据" + (flag ? "成功" : "失败"));
                if (flag) LoadTableToSetting("TerminalTypes");
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Dictionary<string, string> record = settingEditor1.Result;
            if (record != null && record.Count > 0)
            {
                int id = Convert.ToInt32(record["ID"]);
                record.Remove("ID");
                decimal price = Common.GetOneTerminalTypes(id).Price;
                if (Convert.ToDecimal(record["Price"]) != price)
                {
                    List<Terminals> terms = Common.GetMoreTerminals("TypeID=" + id);
                    foreach (Terminals t in terms)
                    {
                        Users u = Common.GetOneUsers(t.UserID);
                        Common.UpdateDeadline(u);
                    }
                }
                bool flag = EditCommon.UpdateRecordDB(record, "TerminalTypes", id);
                MessageBox.Show("修改数据" + (flag ? "成功" : "失败"));
                if (flag) LoadTableToSetting("TerminalTypes");
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
                    bool flag = EditCommon.DeleteRecordDB("TerminalTypes", id);
                    MessageBox.Show("删除数据" + (flag ? "成功" : "失败"));
                    if (flag) LoadTableToSetting("TerminalTypes");
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
        }

        private void LoadTableToSetting(string table)
        {
            button2.Enabled = button3.Enabled = false;
            DataTable dt = EditCommon.LoadTableFromDB(table);
            settingList.DataSource = dt;
        }

        private void textBox1_Leave(object sender, EventArgs e)
        {
            IPAddress ip;
            if (!IPAddress.TryParse(textBox1.Text.Trim(), out ip))
            {
                MessageBox.Show("请输入合法的IP地址！");
                textBox1.Focus();
                textBox1.SelectAll();
            }
        }

        private void textBox2_Leave(object sender, EventArgs e)
        {
            ushort port;
            if (!ushort.TryParse(textBox2.Text.Trim(), out port))
            {
                MessageBox.Show("请输入合法的端口号！");
                textBox2.Focus();
                textBox2.SelectAll();
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            Exception ex = Common.SetServerSocket(IPAddress.Parse(textBox1.Text.Trim())+ ":" + int.Parse(textBox2.Text.Trim()));
            if (ex != null)
            {
                MessageBox.Show("保存失败：" + ex.Message);
            }
            else
            {
                MessageBox.Show("保存成功！");
            }
        }
    }
}
