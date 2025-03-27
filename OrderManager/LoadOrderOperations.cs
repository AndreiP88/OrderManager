namespace OrderManager
{
    public class LoadOrderOperations
    {
        private int orderOperationID;
        private string makereadyStart = "";
        private string makereadyStop = "";
        private string workStart = "";
        private string workStop = "";
        private int makereadyComplete;
        private int oldValueMakereadyComplete;
        private int done;
        private int oldValueDone;

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

        public int OrderOperationID
        {
            get => orderOperationID;
            set => orderOperationID = value;
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
        public int OLDValueMakereadyComplete
        {
            get => oldValueMakereadyComplete;
            set => oldValueMakereadyComplete = value;
        }
        public int Done
        {
            get => done;
            set => done = value;
        }
        public int OLDValueDone
        {
            get => oldValueDone;
            set => oldValueDone = value;
        }
    }
}
