namespace OrderManager
{
    internal class ShiftBlank
    {
        private string shift;
        private string name;

        public ShiftBlank()
        {

        }

        public ShiftBlank(string Shift, string Name)
        {
            shift = Shift;
            name = Name;
        }

        public string Shift
        {
            get => shift;
            set => shift = value;
        }
        public string Name
        {
            get => name;
            set => name = value;
        }
    }
}
