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
    public partial class UserForm : Form
    {
        public UserForm()
        {
            InitializeComponent();
        }

        StringBuilder where;
        string name, deadline;
        bool all;

        private void UserForm_Load(object sender, EventArgs e)
        {
            LoadEditor("Users");
            LoadTableToSetting("Users");
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
                bool flag = EditCommon.InsertRecordDB(record, "Users", out id);
                MessageBox.Show("添加数据" + (flag ? "成功" : "失败"));
                if (flag) LoadTableToSetting("Users");
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Dictionary<string, string> record = settingEditor1.Result;
            if (record != null && record.Count > 0)
            {
                int id = Convert.ToInt32(record["ID"]);
                record.Remove("ID");
                DateTime deadline = Common.GetOneUsers(id).Deadline;
                if (Convert.ToDateTime(record["Deadline"]) != deadline)
                {
                    record["Deadline"] = deadline.ToString("yyyy-MM-dd HH:mm:ss");
                    MessageBox.Show("不允许手动修改用户的截止时间！");
                }
                bool flag = EditCommon.UpdateRecordDB(record, "Users", id);
                MessageBox.Show("修改数据" + (flag ? "成功" : "失败"));
                if (flag) LoadTableToSetting("Users");
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
                    bool flag = EditCommon.DeleteRecordDB("Users", id);
                    MessageBox.Show("删除数据" + (flag ? "成功" : "失败"));
                    if (flag) LoadTableToSetting("Users");
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

        private void LoadTableToSetting(string table, string where = "(1=1)")
        {
            button2.Enabled = button3.Enabled = false;
            DataTable dt = EditCommon.LoadTableFromDB(table, where);
            settingList.DataSource = dt;
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
                where.Append(deadline);
                LoadTableToSetting("Users", where.ToString());
            }
        }

        private void dateTimePicker1_ValueChanged(object sender, EventArgs e)
        {
            deadline = " and Deadline <='" + dateTimePicker1.Value + "'";
            if (!all)
            {
                where = new StringBuilder("(1=1)");
                where.Append(name);
                where.Append(deadline);
                LoadTableToSetting("Users", where.ToString());
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            all = true;
            textBox1.Text = "";
            dateTimePicker1.Value = DateTime.Now;
            LoadTableToSetting("Users");
            all = false;
        }

        private void button5_Click(object sender, EventArgs e)
        {
            if (settingList.SelectedRows != null && settingList.SelectedRows.Count > 0)
            {
                int index = settingList.SelectedRows[settingList.SelectedRows.Count - 1].Index;
                if (settingList.DataSource != null)
                {
                    DataTable dt = settingList.DataSource as DataTable;
                    if (dt != null && dt.Rows.Count > index)
                    {
                        decimal money = numericUpDown1.Value;

                        Users u = new Users();
                        u.ID = Convert.ToInt32(dt.Rows[index]["ID"]);
                        u.Name = dt.Rows[index]["Name"].ToString();
                        u.Deadline = Convert.ToDateTime(dt.Rows[index]["Deadline"]);
                        u.ContactTel = dt.Rows[index]["ContactTel"].ToString();
                        u.Contacter = dt.Rows[index]["Contacter"].ToString();
                        u.ContactAddress = dt.Rows[index]["ContactAddress"].ToString();
                        u.Remark = dt.Rows[index]["Remark"].ToString();

                        if (!Common.UpdateDeadline(u, money))
                        {
                            MessageBox.Show("充值失败！" + Environment.NewLine + "该用户可能没有启用的终端，无法为其充值，请为其指定终端或启用终端！");
                        }
                        else
                        {
                            MessageBox.Show("为用户[" + u.Name + "]充值成功！");
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show("请先选定一个用户！");
            }
        }
    }
}