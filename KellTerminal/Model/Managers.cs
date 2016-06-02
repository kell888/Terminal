using System;
using System.Collections.Generic;
using System.Text;

namespace KellTerminal.Model
{
    public class Managers
    {
        int id;

        public int ID
        {
            get { return id; }
            set { id = value; }
        }
        string _username;

        public string username
        {
            get { return _username; }
            set { _username = value; }
        }
        string _pwd;

        public string pwd
        {
            get { return _pwd; }
            set { _pwd = value; }
        }
    }
}
