using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Regent
{
    public class Grammar
    {
        public string _id;
        public List<string> Items = new List<string>();

        public string GetItem(Random rand)
        {
            var item = Items[rand.Next(Items.Count)];
            return item;
        }
    }
}
