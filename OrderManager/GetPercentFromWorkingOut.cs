namespace OrderManager
{
    internal class GetPercentFromWorkingOut
    {
        public string PercentString(int workOut)
        {
            return Percent(workOut).ToString("P1");
        }

        public float Percent(int workOut)
        {
            float result = 0;

            if (workOut > 0)
            {
                result = (float)workOut / 650;
            }

            return result;
        }

        public string GetBonusWorkingOut(int wOut)
        {
            return GetBonusWorkingOutF(wOut).ToString("P0");
        }

        public float GetBonusWorkingOutF(int wOut)
        {
            float result = 0;

            if (wOut < 600)
            {
                result = 0f;
            }
            if (wOut >= 600 && wOut < 630)
            {
                result = 0.1f;
            }
            else if (wOut >= 630 && wOut < 660)
            {
                result = 0.12f;
            }
            else if (wOut >= 660 && wOut < 720)
            {
                result = 0.15f;
            }
            else if (wOut >= 720)
            {
                result = 0.2f;
            }

            return result;
        }
    }
}
