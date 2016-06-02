using System;
using System.Collections.Generic;
using System.Text;

namespace KellTerminal.Model
{
    public class Users : IComparable
    {
        int id;

        public int ID
        {
            get { return id; }
            set { id = value; }
        }

        string name;

        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        DateTime deadline;

        public DateTime Deadline
        {
            get { return deadline; }
            set { deadline = value; }
        }

        string contactTel;

        public string ContactTel
        {
            get { return contactTel; }
            set { contactTel = value; }
        }

        string contactAddress;

        public string ContactAddress
        {
            get { return contactAddress; }
            set { contactAddress = value; }
        }

        string contacter;

        public string Contacter
        {
            get { return contacter; }
            set { contacter = value; }
        }

        string remark;

        public string Remark
        {
            get { return remark; }
            set { remark = value; }
        }

        public override string ToString()
        {
            return this.Name;
        }

        int IComparable.CompareTo(object obj)
        {
            if (obj is Users)
            {
                Users u = obj as Users;
                return String.Compare(this.Name, u.Name);
            }
            throw new Exception("类型不匹配,必须是Users类型的对象!");
        }
    }
}
