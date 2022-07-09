using System;

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
