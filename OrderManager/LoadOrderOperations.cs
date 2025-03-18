namespace OrderManager
{
    public class LoadOrderOperations
    {
        private string makereadyStart;
        private string makereadyStop;
        private string workStart;
        private string workStop;
        private int makereadyComplete;
        private int done;

        public LoadOrderOperations()
        {

        }
        public LoadOrderOperations(string makereadyStart, string makereadyStop, string workStart, string workStop, int makereadyComplete, int done)
        {
            MakereadyStart = makereadyStart;
            MakereadyStop = makereadyStop;
            WorkStart = workStart;
            WorkStop = workStop;
            MakereadyComplete = makereadyComplete;
            Done = done;
        }

        public string MakereadyStart
        {
            get => makereadyStart;
            set => makereadyStart = value;
        }

        public string MakereadyStop
        {
            get => makereadyStop;
            set => makereadyStop = value;
        }

        public string WorkStart
        {
            get => workStart;
            set => workStart = value;
        }

        public string WorkStop
        {
            get => workStop;
            set => workStop = value;
        }

        public int MakereadyComplete
        {
            get => makereadyComplete;
            set => makereadyComplete = value;
        }

        public int Done
        {
            get => done;
            set => done = value;
        }
    }
}
