using System;
using System.Collections.Generic;
using System.Text;

namespace KellTerminal.Model
{
    public class Terminals
    {
        int id;

        public int ID
        {
            get { return id; }
            set { id = value; }
        }

        int typeID;

        public int TypeID
        {
            get { return typeID; }
            set { typeID = value; }
        }

        string name;

        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        string ip;

        public string IP
        {
            get { return ip; }
            set { ip = value; }
        }

        string sim;

        public string SIM
        {
            get { return sim; }
            set { sim = value; }
        }

        int userId;

        public int UserID
        {
            get { return userId; }
            set { userId = value; }
        }

        string remark;

        public string Remark
        {
            get { return remark; }
            set { remark = value; }
        }

        int runStatus;

        public int RunStatus
        {
            get { return runStatus; }
            set { runStatus = value; }
        }

        bool isEnable = true;

        public bool IsEnable
        {
            get { return isEnable; }
            set { isEnable = value; }
        }
    }
}
