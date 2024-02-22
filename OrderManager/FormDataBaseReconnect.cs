using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OrderManager
{
    public partial class FormDataBaseReconnect : Form
    {
        string ExceptionString = "";

        public FormDataBaseReconnect()
        {
            InitializeComponent();
        }

        public FormDataBaseReconnect(string exceptionString)
        {
            InitializeComponent();

            ExceptionString = exceptionString;
        }

        CancellationTokenSource cancelTokenSource;

        int countDown = 60;

        private bool reconnect = false;

        public bool Reconnect
        {
            get
            {
                return reconnect;
            }
            set
            {
                reconnect = value;
            }
        }

        public string ExceptionStr
        {
            get
            {
                return "";
            }
            set
            {
                ExceptionString = value;
            }
        }

        private void FormDataBaseReconnect_Load(object sender, EventArgs e)
        {
            timer1.Enabled = true;

            textBox1.Text = ExceptionString;
        }

        private void StartCountDown()
        {
            cancelTokenSource?.Cancel();

            int countDown = 60;

            Task task = new Task(() => CountDown(cancelTokenSource.Token, countDown), cancelTokenSource.Token);
            //LoadUsersFromBase(cancelTokenSource.Token, date, selectLoadedBase, typeValueLoad);

            task.Start();
        }

        private void CountDown(CancellationToken token, int countDown)
        {
            for (int i = 0; i <= countDown; i++)
            {

            }

        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            button1.Text = "Повтор (" + countDown + ")";

            countDown--;

            if (countDown == 0)
            {
                timer1.Enabled = false;

                HideThisForm(true);
            }
        }

        private void HideThisForm(bool reconnect)
        {
            Reconnect = reconnect;

            this.Hide();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            HideThisForm(true);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}
