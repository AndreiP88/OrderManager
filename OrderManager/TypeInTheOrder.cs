using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderManager
{
    internal class TypeInTheOrder
    {
        public int id;
        public int indexTypeList;
        public string name;
        public int count;
        public int done;

        public TypeInTheOrder(int typeOrder, int doneOrder)
        {
            indexTypeList = typeOrder;
            done = doneOrder;
        }

        public TypeInTheOrder(int index, string nameItem, int countOrder)
        {
            indexTypeList = index;
            name = nameItem;
            count = countOrder;
        }
        public TypeInTheOrder(int index, int idTypeList, int doneOrder)
        {
            id = index;
            indexTypeList = idTypeList;
            done = doneOrder;
        }
    }
}
