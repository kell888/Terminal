using System;
using System.Collections.Generic;
using System.Text;
using System.Configuration;
using System.Data;
using System.Windows.Forms;
using System.IO;
using System.Data.SqlClient;
using KellTerminal.SQL;
using KellTerminal.Model;
using System.Xml;
using System.Net;

namespace KellTerminal
{
    public static class Common
    {
        public static void GetServerSocket(out IPAddress ip, out int port)
        {
            ip = IPAddress.Loopback;
            port = 8888;
            string ServerSocket = ConfigurationManager.AppSettings["ServerSocket"];
            if (!string.IsNullOrEmpty(ServerSocket))
            {
                string[] ipport = ServerSocket.Split(':');
                if (ipport.Length == 2)
                {
                    IPAddress i;
                    if (IPAddress.TryParse(ipport[0], out i))
                        ip = i;
                    int p;
                    if (int.TryParse(ipport[1], out p))
                        port = p;
                }
            }
        }

        public static Exception SetServerSocket(string endPoint)
        {
            try
            {
                XmlDocument xDoc = new XmlDocument();
                xDoc.Load(Application.ExecutablePath + ".config");
                XmlNode xNode;
                XmlElement xElem;
                xNode = xDoc.SelectSingleNode("//appSettings");
                xElem = (XmlElement)xNode.SelectSingleNode("//add[@key='ServerSocket']");
                xElem.SetAttribute("value", endPoint);
                xDoc.Save(Application.ExecutablePath + ".config");
            }
            catch (Exception e)
            {
                return e;
            }
            return null;
        }
        /// <summary>
        /// 用于充值
        /// </summary>
        /// <param name="user"></param>
        /// <param name="money"></param>
        /// <returns></returns>
        public static bool UpdateDeadline(Users user, decimal money)
        {
            List<Terminals> terms = GetMoreTerminals("UserID=" + user.ID + " and IsEnable=1");
            if (terms.Count > 0)
            {
                decimal sum = 0;
                foreach (Terminals t in terms)
                {
                    decimal price = GetOneTerminalTypes(t.TypeID).Price;
                    sum += price;
                }
                double minutes = (double)(money / sum);
                user.Deadline = user.Deadline.AddMinutes(minutes);
                return UpdateUsers(user);
            }
            return false;
        }
        /// <summary>
        /// 用于禁用和启用终端，以及修改总段类型的价格之后，对用户的改动
        /// </summary>
        /// <param name="user"></param>
        /// <param name="money"></param>
        /// <returns></returns>
        public static bool UpdateDeadline(Users user)
        {
            decimal money = GetRestMoney(user);
            List<Terminals> terms = GetMoreTerminals("UserID=" + user.ID + " and IsEnable=1");
            if (terms.Count > 0)
            {
                decimal sum = 0;
                foreach (Terminals t in terms)
                {
                    decimal price = GetOneTerminalTypes(t.TypeID).Price;
                    sum += price;
                }
                double minutes = (double)(money / sum);
                user.Deadline = user.Deadline.AddMinutes(minutes);
                return UpdateUsers(user);
            }
            return false;
        }
        /// <summary>
        /// 返回实时的用户余额
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public static decimal GetRestMoney(Users user)
        {
            decimal money = 0;
            List<Terminals> terms = GetMoreTerminals("UserID=" + user.ID + " and IsEnable=1");
            if (terms.Count > 0)
            {
                decimal sum = 0;
                foreach (Terminals t in terms)
                {
                    decimal price = GetOneTerminalTypes(t.TypeID).Price;
                    sum += price;
                }
                double minutes = user.Deadline.Subtract(DateTime.Now).TotalMinutes;
                money = sum * (decimal)minutes;
            }
            return money;
        }

        public static string GetNow()
        {
            return DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        }

        public static string GetRunStatus(int flag)
        {
            string rsConfig = ConfigurationManager.AppSettings["RunStatus"];
            if (!string.IsNullOrEmpty(rsConfig))
            {
                string[] runs = rsConfig.Split("|".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                foreach (string run in runs)
                {
                    string[] stats = run.Split(":".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                    if (stats.Length == 2)
                    {
                        int RET;
                        if (int.TryParse(stats[0], out RET))
                        {
                            if (RET == flag)
                            {
                                return stats[1];
                            }
                        }
                    }
                }
            }
            return "未知状态";
        }

        public static List<RunStatus> GetAllStatus()
        {
            List<RunStatus> status = new List<RunStatus>();
            string rsConfig = ConfigurationManager.AppSettings["RunStatus"];
            if (!string.IsNullOrEmpty(rsConfig))
            {                
                string[] runs = rsConfig.Split("|".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                foreach (string run in runs)
                {
                    string[] stats = run.Split(":".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                    if (stats.Length == 2)
                    {
                        int RET;
                        if (int.TryParse(stats[0], out RET))
                        {
                            status.Add(new RunStatus(RET, stats[1]));
                        }
                    }
                }
            }
            return status;
        }

        public static bool EnableNode(Terminals term)
        {
            term.IsEnable = true;
            return Common.UpdateTerminals(term);
        }

        public static bool DisableNode(Terminals term)
        {
            term.IsEnable = false;
            return Common.UpdateTerminals(term);
        }

        public static bool UserRecharge(Users user, decimal money)
        {
            return Common.UpdateDeadline(user, money);
        }

        public static int GetNodeStatus(string ip)
        {
            List<Terminals> ts = Common.GetMoreTerminals("IP='" + ip + "'");
            if (ts.Count > 0)
            {
                return Common.GetOneTerminals(ts[0].ID).RunStatus;
            }
            return 0;
        }

        public static string GetTypeNameByID(int typeId)
        {
            Users user = new Users();
            string sql = "select Name from TerminalTypes where ID=" + typeId;
            try
            {
                SQLDBHelper sqlHelper = new SQLDBHelper();
                object o = sqlHelper.GetSingle(sql);
                if (o != null && o != DBNull.Value)
                {
                    return o.ToString();
                }
            }
            catch (Exception e)
            {
                return "[" + e.Message + "]";
            }
            return "未知类型";
        }

        public static bool InsertUsers(Users user)
        {
            string sql = "insert into Users(Name, Deadline, Contacter, ContactTel, ContactAddress, Remark) values ('" + user.Name + "', '" + user.Deadline + "', '" + user.Contacter + "', '" + user.ContactTel + "', '" + user.ContactAddress + "', '" + user.Remark + "')";
            try
            {
                SQLDBHelper sqlHelper = new SQLDBHelper();
                int r = sqlHelper.ExecuteSql(sql);
                return r > 0;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public static bool DeleteUsers(int id)
        {
            string sql = "delete from Users where ID=" + id;
            try
            {
                SQLDBHelper sqlHelper = new SQLDBHelper();
                int r = sqlHelper.ExecuteSql(sql);
                return r > 0;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public static bool UpdateUsers(Users user)
        {
            string sql = "update Users set Name='" + user.Name + "', Deadline='" + user.Deadline.ToString("yyyy-MM-dd HH:mm:ss") + "', Contacter='" + user.Contacter + "', ContactTel='" + user.ContactTel + "', ContactAddress='" + user.ContactAddress + "', Remark='" + user.Remark + "' where ID=" + user.ID;
            try
            {
                SQLDBHelper sqlHelper = new SQLDBHelper();
                int r = sqlHelper.ExecuteSql(sql);
                return r > 0;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public static Users GetOneUsers(int id)
        {
            Users user = new Users();
            string sql = "select * from Users where ID=" + id.ToString();
            DataTable dt = null;
            try
            {
                SQLDBHelper sqlHelper = new SQLDBHelper();
                dt = sqlHelper.Query(sql);
            }
            catch (Exception e)
            {
                throw e;
            }
            if (dt != null && dt.Rows.Count > 0)
            {
                user.ID = Convert.ToInt32(dt.Rows[0]["ID"]);
                user.Name = dt.Rows[0]["Name"].ToString();
                user.Deadline = Convert.ToDateTime(dt.Rows[0]["Deadline"]);
                user.ContactTel = dt.Rows[0]["ContactTel"].ToString();
                if (dt.Rows[0]["Contacter"] != null && dt.Rows[0]["Contacter"] != DBNull.Value)
                    user.Contacter = dt.Rows[0]["Contacter"].ToString();
                else
                    user.Contacter = string.Empty;
                if (dt.Rows[0]["ContactAddress"] != null && dt.Rows[0]["ContactAddress"] != DBNull.Value)
                    user.ContactAddress = dt.Rows[0]["ContactAddress"].ToString();
                else
                    user.ContactAddress = string.Empty;
                if (dt.Rows[0]["Remark"] != null && dt.Rows[0]["Remark"] != DBNull.Value)
                    user.Remark = dt.Rows[0]["Remark"].ToString();
                else
                    user.Remark = string.Empty;
            }
            return user;
        }

        public static List<Users> GetMoreUsers(string where = "(1=1)")
        {
            List<Users> users = new List<Users>();
            string sql = "select * from Users where " + where;
            DataTable dt = null;
            try
            {
                SQLDBHelper sqlHelper = new SQLDBHelper();
                dt = sqlHelper.Query(sql);
            }
            catch (Exception e)
            {
                throw e;
            }
            if (dt != null && dt.Rows.Count > 0)
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    Users user = new Users();
                    user.ID = Convert.ToInt32(dt.Rows[i]["ID"]);
                    user.Name = dt.Rows[i]["Name"].ToString();
                    user.Deadline = Convert.ToDateTime(dt.Rows[i]["Deadline"]);
                    user.ContactTel = dt.Rows[i]["ContactTel"].ToString();
                    if (dt.Rows[i]["ContactAddress"] != null && dt.Rows[i]["ContactAddress"] != DBNull.Value)
                        user.ContactAddress = dt.Rows[i]["ContactAddress"].ToString();
                    else
                        user.ContactAddress = string.Empty;
                    if (dt.Rows[i]["Contacter"] != null && dt.Rows[i]["Contacter"] != DBNull.Value)
                        user.Contacter = dt.Rows[i]["Contacter"].ToString();
                    else
                        user.Contacter = string.Empty;
                    if (dt.Rows[i]["Remark"] != null && dt.Rows[i]["Remark"] != DBNull.Value)
                        user.Remark = dt.Rows[i]["Remark"].ToString();
                    else
                        user.Remark = string.Empty;
                    users.Add(user);
                }
            }
            return users;
        }

        public static bool InsertManagers(Managers manager)
        {
            string sql = "insert into Managers(username, pwd) values ('" + manager.username + "', '" + manager.pwd + "')";
            try
            {
                SQLDBHelper sqlHelper = new SQLDBHelper();
                int r = sqlHelper.ExecuteSql(sql);
                return r > 0;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public static bool DeleteManagers(int id)
        {
            string sql = "delete from Managers where ID=" + id;
            try
            {
                SQLDBHelper sqlHelper = new SQLDBHelper();
                int r = sqlHelper.ExecuteSql(sql);
                return r > 0;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public static bool UpdateManagers(Managers manager)
        {
            string sql = "update Managers set username='" + manager.username + "', pwd='" + manager.pwd + "' where ID=" + manager.ID;
            try
            {
                SQLDBHelper sqlHelper = new SQLDBHelper();
                int r = sqlHelper.ExecuteSql(sql);
                return r > 0;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public static Managers GetOneManagers(int id)
        {
            Managers manager = new Managers();
            string sql = "select * from Managers where ID=" + id.ToString();
            DataTable dt = null;
            try
            {
                SQLDBHelper sqlHelper = new SQLDBHelper();
                dt = sqlHelper.Query(sql);
            }
            catch (Exception e)
            {
                throw e;
            }
            if (dt != null && dt.Rows.Count > 0)
            {
                manager.ID = Convert.ToInt32(dt.Rows[0]["ID"]);
                manager.username = dt.Rows[0]["username"].ToString();
                manager.pwd = dt.Rows[0]["pwd"].ToString();
            }
            return manager;
        }

        public static List<Managers> GetMoreManagers(string where = "(1=1)")
        {
            List<Managers> managers = new List<Managers>();
            string sql = "select * from Managers where " + where;
            DataTable dt = null;
            try
            {
                SQLDBHelper sqlHelper = new SQLDBHelper();
                dt = sqlHelper.Query(sql);
            }
            catch (Exception e)
            {
                throw e;
            }
            if (dt != null && dt.Rows.Count > 0)
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    Managers manager = new Managers();
                    manager.ID = Convert.ToInt32(dt.Rows[i]["ID"]);
                    manager.username = dt.Rows[i]["username"].ToString();
                    manager.pwd = dt.Rows[i]["pwd"].ToString();
                    managers.Add(manager);
                }
            }
            return managers;
        }

        public static bool InsertTerminals(Terminals term)
        {
            string sql = "insert into Terminals(TypeID, Name, IP, SIM, RunStatus, UserID, Remark, IsEnable) values (" + term.TypeID + ", '" + term.Name + "', '" + term.IP + "', '" + term.SIM + "', " + term.RunStatus + ", " + term.UserID + ", '" + term.Remark + "', " + (term.IsEnable ? "1" : "0") + ")";
            try
            {
                SQLDBHelper sqlHelper = new SQLDBHelper();
                int r = sqlHelper.ExecuteSql(sql);
                return r > 0;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public static bool DeleteTerminals(int id)
        {
            string sql = "delete from Terminals where ID=" + id;
            try
            {
                SQLDBHelper sqlHelper = new SQLDBHelper();
                int r = sqlHelper.ExecuteSql(sql);
                return r > 0;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public static bool UpdateTerminals(Terminals term)
        {
            string sql = "update Terminals set TypeID=" + term.TypeID + ", Name='" + term.Name + "', IP='" + term.IP + "', SIM='" + term.SIM + "', RunStatus=" + term.RunStatus + ", UserID=" + term.UserID + ", Remark='" + term.Remark + "', IsEnable=" + term.IsEnable + " where ID=" + term.ID;
            try
            {
                SQLDBHelper sqlHelper = new SQLDBHelper();
                int r = sqlHelper.ExecuteSql(sql);
                return r > 0;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public static Terminals GetOneTerminals(int id)
        {
            Terminals term = new Terminals();
            string sql = "select * from Terminals where ID=" + id.ToString();
            DataTable dt = null;
            try
            {
                SQLDBHelper sqlHelper = new SQLDBHelper();
                dt = sqlHelper.Query(sql);
            }
            catch (Exception e)
            {
                throw e;
            }
            if (dt != null && dt.Rows.Count > 0)
            {
                term.ID = Convert.ToInt32(dt.Rows[0]["ID"]);
                term.Name = dt.Rows[0]["Name"].ToString();
                term.IP = dt.Rows[0]["IP"].ToString();
                term.TypeID = Convert.ToInt32(dt.Rows[0]["TypeID"]);
                term.UserID = Convert.ToInt32(dt.Rows[0]["UserID"]);
                term.RunStatus = Convert.ToInt32(dt.Rows[0]["RunStatus"]);
                term.IsEnable = Convert.ToBoolean(dt.Rows[0]["IsEnable"]);
                if (dt.Rows[0]["SIM"] != null && dt.Rows[0]["SIM"] != DBNull.Value)
                    term.SIM = dt.Rows[0]["SIM"].ToString();
                else
                    term.SIM = string.Empty;
                if (dt.Rows[0]["Remark"] != null && dt.Rows[0]["Remark"] != DBNull.Value)
                    term.Remark = dt.Rows[0]["Remark"].ToString();
                else
                    term.Remark = string.Empty;
            }
            return term;
        }

        public static List<Terminals> GetMoreTerminals(string where = "(1=1)")
        {
            List<Terminals> terms = new List<Terminals>();
            string sql = "select * from Terminals where " + where;
            DataTable dt = null;
            try
            {
                SQLDBHelper sqlHelper = new SQLDBHelper();
                dt = sqlHelper.Query(sql);
            }
            catch (Exception e)
            {
                throw e;
            }
            if (dt != null && dt.Rows.Count > 0)
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    Terminals term = new Terminals();
                    term.ID = Convert.ToInt32(dt.Rows[i]["ID"]);
                    term.Name = dt.Rows[i]["Name"].ToString();
                    term.IP = dt.Rows[i]["IP"].ToString();
                    term.TypeID = Convert.ToInt32(dt.Rows[i]["TypeID"]);
                    term.UserID = Convert.ToInt32(dt.Rows[i]["UserID"]);
                    term.RunStatus = Convert.ToInt32(dt.Rows[i]["RunStatus"]);
                    term.IsEnable = Convert.ToBoolean(dt.Rows[i]["IsEnable"]);
                    if (dt.Rows[i]["SIM"] != null && dt.Rows[i]["SIM"] != DBNull.Value)
                        term.SIM = dt.Rows[i]["SIM"].ToString();
                    else
                        term.SIM = string.Empty;
                    if (dt.Rows[i]["Remark"] != null && dt.Rows[i]["Remark"] != DBNull.Value)
                        term.Remark = dt.Rows[i]["Remark"].ToString();
                    else
                        term.Remark = string.Empty;
                    terms.Add(term);
                }
            }
            return terms;
        }

        public static bool InsertTerminalTypes(TerminalTypes termType)
        {
            string sql = "insert into TerminalTypes(Name, Price, Remark) values ('" + termType.Name + "', " + termType.Price + ", '" + termType.Remark + "')";
            try
            {
                SQLDBHelper sqlHelper = new SQLDBHelper();
                int r = sqlHelper.ExecuteSql(sql);
                return r > 0;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public static bool DeleteTerminalTypes(int id)
        {
            string sql = "delete from TerminalTypes where ID=" + id;
            try
            {
                SQLDBHelper sqlHelper = new SQLDBHelper();
                int r = sqlHelper.ExecuteSql(sql);
                return r > 0;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public static bool UpdateTerminalTypes(TerminalTypes termType)
        {
            string sql = "update TerminalTypes set Name='" + termType.Name + "', Price=" + termType.Price + ", Remark='" + termType.Remark + "' where ID=" + termType.ID;
            try
            {
                SQLDBHelper sqlHelper = new SQLDBHelper();
                int r = sqlHelper.ExecuteSql(sql);
                return r > 0;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public static TerminalTypes GetOneTerminalTypes(int id)
        {
            TerminalTypes termType = new TerminalTypes();
            string sql = "select * from TerminalTypes where ID=" + id.ToString();
            DataTable dt = null;
            try
            {
                SQLDBHelper sqlHelper = new SQLDBHelper();
                dt = sqlHelper.Query(sql);
            }
            catch (Exception e)
            {
                throw e;
            }
            if (dt != null && dt.Rows.Count > 0)
            {
                termType.ID = Convert.ToInt32(dt.Rows[0]["ID"]);
                termType.Name = dt.Rows[0]["Name"].ToString();
                termType.Price = Convert.ToDecimal(dt.Rows[0]["Price"]);
                if (dt.Rows[0]["Remark"] != null && dt.Rows[0]["Remark"] != DBNull.Value)
                    termType.Remark = dt.Rows[0]["Remark"].ToString();
                else
                    termType.Remark = string.Empty;
            }
            return termType;
        }

        public static List<TerminalTypes> GetMoreTerminalTypes(string where = "(1=1)")
        {
            List<TerminalTypes> termTypes = new List<TerminalTypes>();
            string sql = "select * from TerminalTypes where " + where;
            DataTable dt = null;
            try
            {
                SQLDBHelper sqlHelper = new SQLDBHelper();
                dt = sqlHelper.Query(sql);
            }
            catch (Exception e)
            {
                throw e;
            }
            if (dt != null && dt.Rows.Count > 0)
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    TerminalTypes termType = new TerminalTypes();
                    termType.ID = Convert.ToInt32(dt.Rows[i]["ID"]);
                    termType.Name = dt.Rows[i]["Name"].ToString();
                    termType.Price = Convert.ToDecimal(dt.Rows[i]["Price"]);
                    if (dt.Rows[i]["Remark"] != null && dt.Rows[i]["Remark"] != DBNull.Value)
                        termType.Remark = dt.Rows[i]["Remark"].ToString();
                    else
                        termType.Remark = string.Empty;
                    termTypes.Add(termType);
                }
            }
            return termTypes;
        }




        /// <summary>
        /// SQL数据库备份
        /// </summary>
        /// <param name="path">备份到的路径< /param>
        /// <param name="Backup_PercentComplete">进度</param>
        /// <param name="oBackup">数据库备份服务对象</param>
        /// <param name="remark">备份备注</param>
        public static bool SQLDbBackup(string path, SQLDMO.BackupSink_PercentCompleteEventHandler Backup_PercentComplete, out SQLDMO.Backup oBackup, string remark = null)
        {
            SqlConnectionStringBuilder connStrBuilder = new SqlConnectionStringBuilder(ConfigurationManager.ConnectionStrings["connString"].ConnectionString);
            string ServerIP = connStrBuilder.DataSource;
            string LoginUserName = connStrBuilder.UserID;
            string LoginPass = connStrBuilder.Password;
            string DBName = connStrBuilder.InitialCatalog;
            string dir = path + "\\" + DBName;
            dir = dir.Replace("\\\\", "\\");

            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            string locale = string.Empty;
            string username = string.Empty;
            string password = string.Empty;
            string authority = string.Empty;
            string RemoteAuth = ConfigurationManager.AppSettings["RemoteAuth"];
            if (!string.IsNullOrEmpty(RemoteAuth))
            {
                string[] ra = RemoteAuth.Split("|".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                if (ra.Length == 4)
                {
                    locale = ra[0];
                    username = ra[1];
                    password = ra[2];
                    authority = ra[3];
                }
            }
            System.Management.ManagementScope scope = new System.Management.ManagementScope("\\\\" + ServerIP + "\\root\\cimv2", new System.Management.ConnectionOptions(locale, username, password, authority, System.Management.ImpersonationLevel.Impersonate, System.Management.AuthenticationLevel.Default, true, null, TimeSpan.MaxValue));
            if (ServerIP == "." || ServerIP == "(local)" || ServerIP == "127.0.0.1")
            {

            }
            else
            {
                int i = WmiShareRemote.CreateRemoteDirectory(scope, Path.GetDirectoryName(dir), Directory.GetParent(dir).FullName);
            }
            string DBFile = DBName + DateTime.Now.ToString("yyyyMMddHHmm");
            string filename = dir + "\\" + DBFile + ".bak";
            if (!File.Exists(filename))
            {
                try
                {
                    FileStream fs = File.Create(filename);
                    fs.Close();
                }
                catch (Exception e)
                {
                    MessageBox.Show("无法创建 [" + filename + "] 数据库备份文件！" + Environment.NewLine + e.Message);
                }
            }
            if (ServerIP == "." || ServerIP == "(local)" || ServerIP == "127.0.0.1")
            {

            }
            else
            {
                bool flag = WmiShareRemote.WmiFileCopyToRemote(ServerIP, username, password, dir, "DatabaseBackup", "数据库备份集", null, new string[] { filename }, 0);
            }
            oBackup = new SQLDMO.BackupClass();
            SQLDMO.SQLServer oSQLServer = new SQLDMO.SQLServerClass();
            try
            {
                oSQLServer.LoginSecure = false;
                oSQLServer.Connect(ServerIP, LoginUserName, LoginPass);
                oBackup.Action = SQLDMO.SQLDMO_BACKUP_TYPE.SQLDMOBackup_Database;
                oBackup.PercentComplete += Backup_PercentComplete;
                oBackup.Database = DBName;
                oBackup.Files = @"" + string.Format("[{0}]", filename) + "";
                oBackup.BackupSetName = DBName;
                oBackup.BackupSetDescription = "备份集" + DBName;
                oBackup.Initialize = true;
                oBackup.SQLBackup(oSQLServer);//这里可能存在问题：比如备份远程数据库的时候，选的路径path却是本地的，恰好是远程服务器上不存在的目录
                SqlHelper SqlHelper = new SqlHelper();
                SqlHelper.SetConnConfig("connStringBackup");
                SqlHelper.ExecuteNonQuery("insert into Record (DB, Path, Remark) values ('" + DBName + "', '" + filename + "', '" + remark + "')");
                return true;
            }
            catch (Exception e)
            {
                MessageBox.Show("备份时出错：" + e.Message);
            }
            finally
            {
                oSQLServer.DisConnect();
            }
            return false;
        }

        public static void CancelDbBackup(SQLDMO.Backup oBackup, SQLDMO.BackupSink_PercentCompleteEventHandler Backup_PercentComplete)
        {
            try
            {
                oBackup.Abort();
                oBackup.PercentComplete -= Backup_PercentComplete;
            }
            catch (Exception e)
            {
                MessageBox.Show("取消数据库备份失败：" + e.Message);
            }
        }

        public static void CancelDbRestore(SQLDMO.Restore oRestore, SQLDMO.RestoreSink_PercentCompleteEventHandler Restore_PercentComplete)
        {
            try
            {
                oRestore.Abort();
                oRestore.PercentComplete -= Restore_PercentComplete;
            }
            catch (Exception e)
            {
                MessageBox.Show("取消数据库还原失败：" + e.Message);
            }
        }

        /// < summary>
        /// SQL恢复数据库
        /// < /summary>
        /// <param name="id">备份集ID</param>
        /// <param name="Restore_PercentComplete">进度</param>
        /// <param name="oRestore">数据库还原服务对象</param>
        public static bool SQLDbRestore(int id, SQLDMO.RestoreSink_PercentCompleteEventHandler Restore_PercentComplete, out SQLDMO.Restore oRestore)
        {
            oRestore = null;
            SqlHelper SqlHelper = new SqlHelper();
            SqlHelper.SetConnConfig("connStringBackup");
            object obj = SqlHelper.ExecuteScalar("select Path from Record where ID=" + id);
            if (obj != null && obj != DBNull.Value)
            {
                string filename = obj.ToString();
                SqlConnectionStringBuilder connStrBuilder = new SqlConnectionStringBuilder(ConfigurationManager.ConnectionStrings["connString"].ConnectionString);
                string ServerIP = connStrBuilder.DataSource;
                if (ServerIP == "." || ServerIP == "(local)" || ServerIP == "127.0.0.1")
                {

                }
                else
                {
                    StringBuilder TargetDir = new StringBuilder();
                    TargetDir.Append(@"\\");
                    TargetDir.Append(ServerIP);
                    TargetDir.Append("\\");
                    TargetDir.Append("DatabaseBackup");
                    TargetDir.Append("\\");
                    filename = TargetDir + Path.GetFileName(filename);
                }
                if (File.Exists(filename))
                {
                    string LoginUserName = connStrBuilder.UserID;
                    string LoginPass = connStrBuilder.Password;
                    string DBName = connStrBuilder.InitialCatalog;
                    oRestore = new SQLDMO.RestoreClass();
                    SQLDMO.SQLServer oSQLServer = new SQLDMO.SQLServerClass();
                    try
                    {
                        oSQLServer.LoginSecure = false;
                        oSQLServer.Connect(ServerIP, LoginUserName, LoginPass);
                        //因为数据库正在使用，所以无法获得对数据库的独占访问权。不一定是由于其他进程的占用，还有其他的原因，所以要脱机再联机...
                        //KillProcess(DBName);
                        //KillSqlProcess(oSQLServer, DBName);
                        //OffAndOnLine(DBName);
                        OffLine(DBName);
                        oRestore.Action = SQLDMO.SQLDMO_RESTORE_TYPE.SQLDMORestore_Database;
                        oRestore.PercentComplete += Restore_PercentComplete;
                        oRestore.Database = DBName;
                        oRestore.Files = @"" + string.Format("[{0}]", filename) + "";
                        oRestore.FileNumber = 1;
                        oRestore.ReplaceDatabase = true;
                        oRestore.SQLRestore(oSQLServer);//这里可能存在问题！
                        OnLine(DBName);
                        return true;
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show("恢复时出错：" + e.Message);
                    }
                    finally
                    {
                        oSQLServer.DisConnect();
                    }
                }
                else
                {
                    MessageBox.Show("找不到要还原的备份数据库文件 [" + filename + "]");
                }
            }
            return false;
        }

        private static void OnLine(string DBName)
        {
            string sql = "use master;alter database [" + DBName + "] set online with ROLLBACK IMMEDIATE";
            SqlHelper SqlHelper = new SqlHelper();
            SqlHelper.ExecuteNonQuery(sql);
        }

        private static void OffLine(string DBName)
        {
            string sql = "use master;alter database [" + DBName + "] set offline with ROLLBACK IMMEDIATE";
            SqlHelper SqlHelper = new SqlHelper();
            SqlHelper.ExecuteNonQuery(sql);
        }

        private static void OffAndOnLine(string DBName)
        {
            string sql = "use master;alter database [" + DBName + "] set offline with ROLLBACK IMMEDIATE;alter database " + DBName + " set online with ROLLBACK IMMEDIATE";
            SqlHelper SqlHelper = new SqlHelper();
            SqlHelper.ExecuteNonQuery(sql);
        }

        private static void KillProcess(string DbName)
        {
            string sql = "select spid from sys.sysprocesses where dbid in (select dbid from master..sysdatabases where name = '" + DbName + "')";
            SqlHelper SqlHelper = new SqlHelper();
            DataTable dt = SqlHelper.ExecuteQueryDataTable(sql);
            if (dt.Rows.Count > 0)
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    try
                    {
                        SqlHelper.ExecuteNonQuery("kill " + dt.Rows[i][0].ToString());
                    }
                    catch { }//避免KILL自身进程造成错误！
                }
            }
        }

        private static void KillSqlProcess(SQLDMO.SQLServer svr, string DbName)
        {
            SQLDMO.QueryResults qr = svr.EnumProcesses(-1);
            int iColPIDNum = -1;
            int iColDbName = -1;
            for (int i = 1; i <= qr.Columns; i++)
            {
                string strName = qr.get_ColumnName(i);
                if (strName.ToUpper().Trim() == "SPID")
                {
                    iColPIDNum = i;
                }
                else if (strName.ToUpper().Trim() == "DBNAME")
                {
                    iColDbName = i;
                }
                if (iColPIDNum != -1 && iColDbName != -1)
                    break;
            }
            //杀死使用DbName数据库的进程
            for (int i = 1; i <= qr.Rows; i++)
            {
                int lPID = qr.GetColumnLong(i, iColPIDNum);
                string strDBName = qr.GetColumnString(i, iColDbName);
                if (strDBName.ToUpper() == DbName.ToUpper())
                {
                    svr.KillProcess(lPID);
                }
            }
        }

        public static string GetBackupPathById(int id)
        {
            SqlHelper SqlHelper = new SqlHelper();
            SqlHelper.SetConnConfig("connStringBackup");
            object obj = SqlHelper.ExecuteScalar("select Path from Record where ID=" + id);
            if (obj != null && obj != DBNull.Value)
            {
                string filename = obj.ToString();
                FileInfo fi = new FileInfo(filename);
                if (fi.Directory != null)
                    return fi.Directory.Parent.FullName;
            }
            return "";
        }

        public static void CopyDirectory(string srcDir, string dstDir)
        {
            DirectoryInfo source = new DirectoryInfo(srcDir);
            DirectoryInfo target = new DirectoryInfo(dstDir);

            if (!source.Exists)
            {
                return;
            }

            if (target.FullName.StartsWith(source.FullName, StringComparison.CurrentCultureIgnoreCase))
            {
                throw new Exception("父目录不能拷贝到子目录！");
            }

            if (!target.Exists)
            {
                target.Create();
            }

            FileInfo[] files = source.GetFiles();

            for (int i = 0; i < files.Length; i++)
            {
                File.Copy(files[i].FullName, target.FullName + @"\" + files[i].Name, true);
            }

            DirectoryInfo[] dirs = source.GetDirectories();

            for (int j = 0; j < dirs.Length; j++)
            {
                CopyDirectory(dirs[j].FullName, target.FullName + @"\" + dirs[j].Name);
            }
        }

        public static bool DeleteDB(int year, int month, int year2, int month2, List<string> tables)
        {
            int count = 0;
            DateTime start = new DateTime(year, month, 1);
            DateTime end = new DateTime(year2, month2, 1);
            if (end >= start)
            {
                int months = 0;
                if (end.Year > start.Year)//2013.10-2014.8
                {
                    months += 12 - start.Month + 1;
                    months += 12 * (end.Year - start.Year - 1);
                    months += end.Month;
                }
                else//2013.8-2013.10
                {
                    months = end.Month - start.Month + 1;
                }
                for (int i = 0; i < months; i++)
                {
                    DateTime current = start.AddMonths(i);
                    string YYYYMM = current.Year.ToString().PadLeft(4, '0') + current.Month.ToString().PadLeft(2, '0');
                    SqlHelper SqlHelper = new SqlHelper();
                    foreach (string table in tables)
                    {
                        string sql = "DROP TABLE " + table + YYYYMM;
                        try
                        {
                            SqlHelper.ExecuteNonQuery(sql);
                            count++;
                        }
                        catch { }//不存在的表出错时略过...
                    }
                }
            }
            return count > 0;
        }
        /// <summary>
        /// 删除指定时间范围内的目录，时间的范围的粒度是天(连同跟时间格式匹配的所有子目录均被删除！)
        /// </summary>
        /// <param name="path">指定的图片或者视频根目录</param>
        /// <param name="start">删除的开始时间</param>
        /// <param name="end">删除的结束时间</param>
        /// <returns></returns>
        public static bool DeleteDirectory(string path, DateTime start, DateTime end)
        {
            int count = 0;
            int days = end.Subtract(start).Days;
            DateTime curr = start;
            for (int i = 0; i < days; i++)
            {
                string yyyyMMdd = curr.Year.ToString().PadLeft(4, '0') + curr.Month.ToString().PadLeft(2, '0') + curr.Day.ToString().PadLeft(2, '0');
                string[] dirs = Directory.GetDirectories(path, yyyyMMdd + "*");
                foreach (string dir in dirs)
                {
                    DirectoryInfo di = new DirectoryInfo(dir);
                    string thePath = path + "\\" + di.Name;
                    try
                    {
                        Directory.Delete(thePath, true);
                        count++;
                    }
                    catch (Exception e) { MessageBox.Show("DeleteDirectory[" + thePath + "]：" + e.Message); }
                }
                curr.AddDays(1);
            }
            return count > 0;
        }

        public static bool ShrinkDB()
        {
            try
            {
                SqlHelper SqlHelper = new SqlHelper();
                string sql = "DBCC SHRINKDATABASE('" + SqlHelper.Conn.Database + "')";
                SqlHelper.ExecuteNonQuery(sql);
                return true;
            }
            catch (Exception e)
            {
                MessageBox.Show("ShrinkDB：" + e.Message);
                return false;
            }
        }

        public static DataTable GetBackupHistory(string DB)
        {
            try
            {
            SqlHelper SqlHelper = new SqlHelper();
            SqlHelper.SetConnConfig("connStringBackup");
            string sql = "select * from Record where DB='" + DB + "' order by ID desc";
            DataTable dt = SqlHelper.ExecuteQueryDataTable(sql);
            return dt;
            }
            catch (Exception e)
            {
                MessageBox.Show("GetBackupHistory：" + e.Message);
                return null;
            }
        }

        public static void DeleteKellControl(string ip)
        {
            if (!string.IsNullOrEmpty(ip))
            {
            try
            {
                SqlHelper SqlHelper = new SqlHelper();
                SqlHelper.SetConnConfig("connStringControl");
                string sql = "delete from MyControl where Host='" + ip + "'";
                int i = SqlHelper.ExecuteNonQuery(sql);
            }
            catch (Exception e)
            {
                MessageBox.Show("DeleteKellControl：" + e.Message);
            }
            }
        }

        public static void UpdateKellControl(string ip, bool isEnable)
        {
            if (!string.IsNullOrEmpty(ip))
            {
                try
                {
                SqlHelper SqlHelper = new SqlHelper();
                SqlHelper.SetConnConfig("connStringControl");
                string sql = "update MyControl set IsEnable=" + (isEnable ? "1" : "0") + " where Host='" + ip + "'";
                int i = SqlHelper.ExecuteNonQuery(sql);
            }
            catch (Exception e)
            {
                MessageBox.Show("UpdateKellControl：" + e.Message);
            }
            }
        }

        public static void UpdateStatus(string ip, int status)
        {
            try
            {
                SqlHelper SqlHelper = new SqlHelper();
                string sql = "update Terminals set RunStatus=" + status + " where IP='" + ip + "'";
                int i = SqlHelper.ExecuteNonQuery(sql);
            }
            catch (Exception e)
            {
                MessageBox.Show("UpdateStatus：" + e.Message);
            }
        }
    }
}
