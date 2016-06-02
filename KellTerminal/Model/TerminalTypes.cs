using System;
using System.Collections.Generic;
using System.Text;

namespace KellTerminal.Model
{
    public class TerminalTypes
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
        decimal price;

        public decimal Price
        {
            get { return price; }
            set { price = value; }
        }
        string remark;

        public string Remark
        {
            get { return remark; }
            set { remark = value; }
        }
    }
}
