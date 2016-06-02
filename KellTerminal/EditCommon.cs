using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Windows.Forms;
using System.Configuration;
using System.Data.SqlClient;
using System.Text.RegularExpressions;
using KellTerminal.SQL;

namespace KellTerminal
{
    public class PrimaryKeyShowField
    {
        string primaryKey;

        public string PrimaryKey
        {
            get { return primaryKey; }
            set { primaryKey = value; }
        }
        string showField;

        public string ShowField
        {
            get { return showField; }
            set { showField = value; }
        }

        public PrimaryKeyShowField(string primaryKey, string showField)
        {
            this.PrimaryKey = primaryKey;
            this.ShowField = showField;
        }
    }

    public static class EditCommon
    {
        private static readonly object syncObj = new object();

        public static bool IsNumber(string s)
        {
            string pattern = @"^\d+(\.)?\d*$";
            if (!string.IsNullOrEmpty(s))
            {
                if (!Regex.IsMatch(s, pattern))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            return false;
        }

        public static bool IsNumericType(Type type)
        {
            if (type.Equals(typeof(Int16)) || type.Equals(typeof(Int32)) || type.Equals(typeof(Int64)) || type.Equals(typeof(UInt16)) || type.Equals(typeof(UInt32)) || type.Equals(typeof(UInt64)) || type.Equals(typeof(Byte)) || type.Equals(typeof(SByte)) || type.Equals(typeof(Decimal)) || type.Equals(typeof(Single)) || type.Equals(typeof(Double)))
            {
                return true;
            }
            return false;
        }

        public static bool SaveDataSetToFile(DataSet dataSet, string filename)
        {
            if (dataSet == null)
                return false;
            try
            {
                dataSet.WriteXml(filename, XmlWriteMode.WriteSchema);
                return true;
            }
            catch (Exception e)
            {
                MessageBox.Show("写入文件出错：" + e.Message);
                return false;
            }
        }

        public static bool SaveTableToFile(DataTable data, string filename)
        {
            if (data == null || data.Columns.Count == 0)
                return false;
            try
            {
                data.WriteXml(filename, XmlWriteMode.WriteSchema);
                return true;
            }
            catch (Exception e)
            {
                MessageBox.Show("写入文件出错：" + e.Message);
                return false;
            }
        }

        public static DataSet LoadDataSetFromFile(string filename)
        {
            DataSet ds = new DataSet();
            try
            {
                XmlReadMode xrm = ds.ReadXml(filename);
            }
            catch (Exception e)
            {
                MessageBox.Show("读取文件出错：" + e.Message);
            }
            return ds;
        }

        public static DataTable LoadTableFromFile(string filename)
        {
            DataTable data = new DataTable();
            try
            {
                XmlReadMode xrm = data.ReadXml(filename);
            }
            catch (Exception e)
            {
                MessageBox.Show("读取文件出错：" + e.Message);
            }
            return data;
        }

        public static DataTable LoadTableFromDB(string table)
        {
            DataTable dt = new DataTable(table);
            try
            {
                SqlHelper SqlHelper = new SqlHelper();
                string sql = "select * from " + table;
                dt = SqlHelper.ExecuteQueryDataTable(sql);
                dt.TableName = table;
            }
            catch (Exception e)
            {
                MessageBox.Show("读取数据库出错：" + e.Message);
            }
            return dt;
        }

        public static DataTable LoadTableFromDB(string table, string where)
        {
            DataTable dt = new DataTable(table);
            try
            {
                SqlHelper SqlHelper = new SqlHelper();
                string sql = "select * from " + table + " where " + where;
                dt = SqlHelper.ExecuteQueryDataTable(sql);
                dt.TableName = table;
            }
            catch (Exception e)
            {
                MessageBox.Show("读取数据库出错：" + e.Message);
            }
            return dt;
        }

        public static Dictionary<string, string> LoadRecordFromDB(string table, object id)
        {
            Dictionary<string, string> record = new Dictionary<string, string>();
            try
            {
                StringBuilder sb = new StringBuilder();
                List<SqlParameter> param = new List<SqlParameter>();
                param.Add(new SqlParameter("@id", id));
                SqlHelper SqlHelper = new SqlHelper();
                string sql = "select * from " + table + " where ID=@id";
                DataTable dt = SqlHelper.ExecuteQueryDataTable(sql, CommandType.Text, param.ToArray());
                if (dt.Rows.Count > 0)
                {
                    for (int i = 0; i < dt.Columns.Count; i++)
                    {
                        record.Add(dt.Columns[i].ColumnName, dt.Rows[0][i].ToString());
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("打开数据出错：" + e.Message);
            }
            return record;
        }

        public static bool InsertRecordDB(Dictionary<string, string> fields, string table, out int id)
        {
            lock (syncObj)
            {
                id = 0;
                if (fields.Count > 0)
                {
                    try
                    {
                        Dictionary<string, string> record = fields;
                        StringBuilder sb = new StringBuilder();
                        StringBuilder sb2 = new StringBuilder();
                        List<SqlParameter> param = new List<SqlParameter>();
                        sb.Append("(");
                        sb2.Append("(");
                        foreach (string f in record.Keys)
                        {
                            sb.Append(f + ",");
                            sb2.Append("@" + f + ",");
                            param.Add(new SqlParameter("@" + f, record[f]));
                        }
                        string s = sb.ToString().Substring(0, sb.Length - 1);
                        string s2 = sb2.ToString().Substring(0, sb2.Length - 1);
                        s += ")";
                        s2 += ")";
                        SqlHelper SqlHelper = new SqlHelper();
                        string sql = "insert into " + table + " " + s + " values " + s2;
                        int i = SqlHelper.ExecuteNonQuery(sql, CommandType.Text, param.ToArray());
                        id = Convert.ToInt32(SqlHelper.ExecuteScalar("SELECT IDENT_CURRENT('" + table + "')"));
                        return i > 0;
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show("插入数据出错：" + e.Message);
                    }
                }
                return false;
            }
        }

        public static bool DeleteRecordDB(string table, int id)
        {
            lock (syncObj)
            {
                try
                {
                    List<SqlParameter> param = new List<SqlParameter>();
                    param.Add(new SqlParameter("@id", id));
                    SqlHelper SqlHelper = new SqlHelper();
                    string sql = "delete from " + table + " where ID=@id";
                    int i = SqlHelper.ExecuteNonQuery(sql, CommandType.Text, param.ToArray());
                    return i > 0;
                }
                catch (Exception e)
                {
                    MessageBox.Show("删除数据出错：" + e.Message);
                }
                return false;
            }
        }

        public static bool DeleteRecordDB(string table, string where)
        {
            try
            {
                SqlHelper SqlHelper = new SqlHelper();
                string w = "";
                if (!string.IsNullOrEmpty(where))
                    w = " where (1=1) " + where;
                string sql = "delete from " + table + w;
                int i = SqlHelper.ExecuteNonQuery(sql);
                return i > 0;
            }
            catch (Exception e)
            {
                MessageBox.Show("删除数据出错：" + e.Message);
            }
            return false;
        }

        public static bool UpdateRecordDB(Dictionary<string, string> record, string table, string where)
        {
            if (record.Count > 0)
            {
                try
                {
                    StringBuilder sb = new StringBuilder();
                    List<SqlParameter> param = new List<SqlParameter>();
                    foreach (string f in record.Keys)
                    {
                        sb.Append(f + "=@" + f + ",");
                        param.Add(new SqlParameter("@" + f, record[f]));
                    }
                    string s = sb.ToString().Substring(0, sb.Length - 1);
                    SqlHelper SqlHelper = new SqlHelper();
                    string w = "";
                    if (!string.IsNullOrEmpty(where))
                        w = " where (1=1) " + where;
                    string sql = "update " + table + " set " + s + w;
                    int i = SqlHelper.ExecuteNonQuery(sql, CommandType.Text, param.ToArray());
                    return i > 0;
                }
                catch (Exception e)
                {
                    MessageBox.Show("更新数据出错：" + e.Message);
                }
            }
            return false;
        }

        public static bool UpdateRecordDB(Dictionary<string, string> fields, string table, int id)
        {
            lock (syncObj)
            {
                if (fields.Count > 0)
                {
                    try
                    {
                        Dictionary<string, string> record = fields;
                        StringBuilder sb = new StringBuilder();
                        List<SqlParameter> param = new List<SqlParameter>();
                        foreach (string f in record.Keys)
                        {
                            sb.Append(f + "=@" + f + ",");
                            param.Add(new SqlParameter("@" + f, record[f]));
                        }
                            param.Add(new SqlParameter("@id", id));
                        string s = sb.ToString().Substring(0, sb.Length - 1) + " where ID=@id";
                        SqlHelper SqlHelper = new SqlHelper();
                        string sql = "update " + table + " set " + s;
                        int i = SqlHelper.ExecuteNonQuery(sql, CommandType.Text, param.ToArray());
                        return i > 0;
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show("更新数据出错：" + e.Message);
                    }
                }
                return false;
            }
        }

        public static object GetFirstIdByWhere(string table, string where)
        {
            SqlHelper SqlHelper = new SqlHelper();
            string w = "";
            if (!string.IsNullOrEmpty(where))
                w = " where (1=1) " + where;
            string sql = "select ID from " + table + w;
            return SqlHelper.ExecuteScalar(sql);
        }

        public static List<string> GetDbNullFields()
        {
            List<string> DbNulls = new List<string>();
            string DBNullFields = ConfigurationManager.AppSettings["DBNullFields"];
            if (!string.IsNullOrEmpty(DBNullFields))
            {
                string[] dbNulls = DBNullFields.Split(',');
                foreach (string dbn in dbNulls)
                {
                    DbNulls.Add(dbn.Trim().ToLower());
                }
            }
            return DbNulls;
        }

        public static Dictionary<string, string> GetNewRecord(string table)
        {
            Dictionary<string, string> record = new Dictionary<string, string>();
            SqlHelper sqlHelper = new SqlHelper();
            DataTable dt = sqlHelper.ExecuteQueryDataTable("select * from " + table + " where 1>2");
            if (dt != null)
            {
                for (int i = 0; i < dt.Columns.Count; i++)
                {
                    record.Add(dt.Columns[i].ColumnName, dt.Columns[i].DefaultValue.ToString());
                }
            }
            return record;
        }
    }
    public static class Extensions
    {
        public static string ConvertToString(Dictionary<string, string> me)
        {
            StringBuilder sb = new StringBuilder();
            foreach (string key in me.Keys)
            {
                sb.Append("{");
                sb.Append(key);
                sb.Append(",");
                sb.Append(me[key]);
                sb.Append("}");
            }
            return sb.ToString();
        }

        public static Dictionary<string, string> ConvertToDictionary(string me)
        {
            Dictionary<string, string> dict = new Dictionary<string, string>();
            try
            {
                string[] mes = me.Split(new string[] { "}{" }, StringSplitOptions.None);
                string first = mes[0].Substring(1);//去掉{
                string[] ele = first.Split(',');
                dict.Add(ele[0], GetValue(ele[1]));
                for (int i = 1; i < mes.Length - 1; i++)
                {
                    string item = mes[i];
                    string[] its = item.Split(',');
                    dict.Add(its[0], GetValue(its[1]));
                }
                if (mes.Length > 1)
                {
                    string last = mes[mes.Length - 1].Substring(0, mes[mes.Length - 1].Length - 1);//去掉}
                    string[] els = last.Split(',');
                    dict.Add(els[0], GetValue(els[1]));
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("ConvertToDictionary出错：" + e.Message);
            }
            return dict;
        }

        public static string GetValue(string rawValue)
        {
            string ret = "";
            if (rawValue != null)
            {
                ret = rawValue;
                int index = ret.IndexOf("[");
                if (index > -1)
                {
                    ret = ret.Substring(0, index);
                }
            }
            return ret;
        }
    }
}