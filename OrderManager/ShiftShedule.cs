using System;
using System.Collections.Generic;
using System.Drawing;

namespace OrderManager
{
    internal class ShiftShedule
    {
        private int _id;
        private string _shiftStartDate;
        private List<ShiftBlank> _shiftBlanks = new List<ShiftBlank>();
        private Color[] _shiftColors = new Color[7];

        public ShiftShedule()
        {
        }

        public ShiftShedule(int UserID, string ShiftStartDate, List<ShiftBlank> Shifts, Color[] Colors)
        {
            _id = UserID;
            _shiftStartDate = ShiftStartDate;
            _shiftBlanks = Shifts;
            _shiftColors = Colors;
        }

        public int UserID
        {
            get => _id;
            set => _id = value;
        }
        public string ShiftStartDate
        {
            get => _shiftStartDate;
            set => _shiftStartDate = value;
        }
        public List<ShiftBlank> ShiftBlanks
        {
            get => _shiftBlanks;
            set => _shiftBlanks = value;
        }

        public Color[] ShiftColors
        {
            get => _shiftColors;
            set => _shiftColors = value;
        }

        public string GetCurrentShiftFromShedule(string currentDate)
        {
            string result = "";

            if (_shiftStartDate != "" && currentDate != "")
            {
                DateTime current = Convert.ToDateTime(currentDate);
                DateTime shiftStart = Convert.ToDateTime(_shiftStartDate);
                int countBlanks = _shiftBlanks.Count;

                if (countBlanks > 0)
                {
                    int dateDifferentDay = current.Subtract(shiftStart).Days;

                    int index = Math.Abs(dateDifferentDay) % countBlanks;

                    if (dateDifferentDay < 0 && index != 0)
                    {
                        index = countBlanks - index;
                    }

                    result = _shiftBlanks[index].Shift;
                }
            }

            return result;
        }
    }
}
