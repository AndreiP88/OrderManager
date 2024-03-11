using System;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace OrderManager
{
    static class DataBaseReconnect
    {
        public static DialogResult DataBaseReconnectionRequest(string exception)
        {

            Form1._viewDatabaseRequestForm = true;
            //Form1._pauseEvent.Reset();

            //FormDataBaseReconnect form = new FormDataBaseReconnect(exception);
            //form.ShowDialog();

            //DialogResult dialog = form.DialogResult;

            if (Form1.formSQLException == null || Form1.formSQLException.IsDisposed)
            {
                Form1.formSQLException = new FormDataBaseReconnect(exception);
                Form1.formSQLException.ShowDialog();
            }
            else
            {
                //Form1.formSQLException.Activate();
            }

            DialogResult dialog = Form1.formSQLException.DialogResult;

            //Form1.formSQLException?.Dispose();

            Form1._viewDatabaseRequestForm = false;
            //
            //Thread.Sleep(5000);

            //form.Close();
            //

            /*Form1.formSQLException.ExceptionStr = exception;
            Form1.formSQLException.ShowDialog();

            result = Form1.formSQLException.Reconnect;*/

            //Form1._pauseEvent.Set();

            return dialog;
        }

        //---------------------------------------------------------
        // Статический метод записи строки в файл лога с переносом
        //---------------------------------------------------------
        public static void WriteLine(string message)
        {
            using (StreamWriter sw = new StreamWriter(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\log.txt", true))
            {
                sw.WriteLine(String.Format("{0,-23} {1}", DateTime.Now.ToString() + ": ", message));
            }
        }
    }
}
