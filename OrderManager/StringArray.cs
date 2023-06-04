using System;
using System.Windows.Forms;

namespace OrderManager
{
    internal class StringArray
    {
        readonly char separator = ';';
        /// <summary>
        /// С указанием разделителя
        /// </summary>
        /// <param name="separator"></param>
        public StringArray(char separator)
        {
            this.separator = separator;
        }
        /// <summary>
        /// Без указания разделителя
        /// </summary>
        public StringArray()
        {
        }
        /// <summary>
        /// Преобразует строку в массив значений разделенных специальным символом
        /// </summary>
        /// <param name="str"></param>
        /// <returns>Массив значений</returns>
        public string[] ArrayFromTheString(string str)
        {
            string[] result = str.Split(separator);

            return result;
        }
        /// <summary>
        /// Преобразует массив згачений в строку со значениями разделенных специальным символом
        /// </summary>
        /// <param name="arr"></param>
        /// <returns>Строку с разделителем</returns>
        public string StringFromTheArray(string[] arr)
        {
            string result = "";

            for (int i = 0; i < arr.Length; i++)
            {
                if (i != arr.Length - 1)
                    result += arr[i] + separator;
                else
                    result += arr[i];
            }

            return result;
        }
    }
}
