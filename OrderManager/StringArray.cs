using System;

namespace OrderManager
{
    internal class StringArray
    {
        char separator = ';';
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
        public String[] ArrayFromTheString(String str)
        {
            String[] result = str.Split(separator);

            return result;
        }
        /// <summary>
        /// Преобразует массив згачений в строку со значениями разделенных специальным символом
        /// </summary>
        /// <param name="arr"></param>
        /// <returns>Строку с разделителем</returns>
        public String StringFromTheArray(String[] arr)
        {
            String result = "";

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
