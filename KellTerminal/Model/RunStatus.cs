using System;
using System.Collections.Generic;
using System.Text;

namespace KellTerminal.Model
{
    public class RunStatus : IComparable
    {
        int id;

        public int ID
        {
            get { return id; }
            set { id = value; }
        }
        string status;

        public string Status
        {
            get { return status; }
            set { status = value; }
        }

        public RunStatus(int id, string status)
        {
            this.id = id;
            this.status = status;
        }

        public override string ToString()
        {
            return this.Status;
        }

        int IComparable.CompareTo(object obj)
        {
            if (obj is RunStatus)
            {
                RunStatus r = obj as RunStatus;
                return this.ID - r.ID;
            }
            throw new Exception("类型不匹配,必须是RunStatus类型的对象!");
        }
    }
}
