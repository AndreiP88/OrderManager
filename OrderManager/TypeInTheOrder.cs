using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderManager
{
    internal class TypeInTheOrder
    {
        public string id;
        public string indexTypeList;
        public string type;
        public int done;

        public TypeInTheOrder(string typeOrder, int doneOrder)
        {
            type = typeOrder;
            done = doneOrder;
        }
        public TypeInTheOrder(string index, string idTypeList, string typeOrder, int doneOrder)
        {
            id = index;
            indexTypeList = idTypeList;
            type = typeOrder;
            done = doneOrder;
        }
    }
}
