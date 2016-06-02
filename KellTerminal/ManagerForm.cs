using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace KellTerminal
{
    public partial class ManagerForm : Form
    {
        public ManagerForm()
        {
            InitializeComponent();
        }

        private void ManagerForm_Load(object sender, EventArgs e)
        {
            LoadEditor("Managers");
            LoadTableToSetting("Managers");
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
                bool flag = EditCommon.InsertRecordDB(record, "Managers", out id);
                MessageBox.Show("添加数据" + (flag ? "成功" : "失败"));
                if (flag) LoadTableToSetting("Managers");
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Dictionary<string, string> record = settingEditor1.Result;
            if (record != null && record.Count > 0)
            {
                int id = Convert.ToInt32(record["ID"]);
                record.Remove("ID");
                bool flag = EditCommon.UpdateRecordDB(record, "Managers", id);
                MessageBox.Show("修改数据" + (flag ? "成功" : "失败"));
                if (flag) LoadTableToSetting("Managers");
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
                    bool flag = EditCommon.DeleteRecordDB("Managers", id);
                    MessageBox.Show("删除数据" + (flag ? "成功" : "失败"));
                    if (flag) LoadTableToSetting("Managers");
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
    }
}
