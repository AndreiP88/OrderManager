using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderManager
{
    internal class GetPercentFromWorkingOut
    {
        public String PercentString(int workOut)
        {
            String result = ((float)(workOut) * 100 / 650).ToString("N1") + "%";

            return result;
        }

        public float Percent(int workOut)
        {
            float result = ((float)(workOut) * 100 / 650);

            return result;
        }

    }
}
