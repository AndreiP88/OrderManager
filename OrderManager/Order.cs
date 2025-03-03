using System;

namespace OrderManager
{
    internal class Order
    {
        public int TypeJob;
        public int id;
        public int orderIndex;
        public string machineOfOrder;
        public string numberOfOrder;
        public string modificationOfOrder;
        public string nameOfOrder;
        public int amountOfOrder;
        public int lastCount;
        public int plannedTimeMakeready;
        public int plannedTimeWork;
        public int Status;
        public int facticalTimeMakeready;
        public int facticalTimeWork;
        public int done;
        public int norm;
        public int workingOut;
        public float workingOutDB;
        public int mkDeviation;
        public int wkDeviation;
        public int counterRepeat;
        public string note;
        public string notePrivate;
        public Order(int typeJob, int indexOrderInProgress, int orderID, string machine, string number, string modification, string orderName, int orderAmount, int lastCountOfOrder, int plannedMakeready,
            int plannedWork, int status, int facticalMakeready, int facticalWork, int countDone, int cNorm,
            int orderWorkingOut, int mkDeviationOrder, int wkDeviationOrder, int orderCounterRepeat, string orderNote, string orderPrivateNote, float workingOutDB)
        {
            this.TypeJob = typeJob;
            this.id = indexOrderInProgress;
            this.orderIndex = orderID;
            this.machineOfOrder = machine;
            this.numberOfOrder = number;
            this.modificationOfOrder = modification;
            this.nameOfOrder = orderName;
            this.amountOfOrder = orderAmount;
            this.lastCount = lastCountOfOrder;
            this.plannedTimeMakeready = plannedMakeready;
            this.plannedTimeWork = plannedWork;
            this.Status = status;
            this.facticalTimeMakeready = facticalMakeready;
            this.facticalTimeWork = facticalWork;
            this.done = countDone;
            this.norm = cNorm;
            this.workingOut = orderWorkingOut;
            this.mkDeviation = mkDeviationOrder;
            this.wkDeviation = wkDeviationOrder;
            this.counterRepeat = orderCounterRepeat;
            this.note = orderNote;
            this.notePrivate = orderPrivateNote;
            this.workingOutDB = workingOutDB;
        }







        /*public String machineOfOrder;
        public String numberOfOrder;
        public String modificationOfOrder;
        public String nameOfOrder;
        public int amountOfOrder;
        public int lastCount;
        public String plannedTimeMakeready;
        public String plannedTimeWork;
        public String facticalTimeMakeready;
        public String facticalTimeWork;
        public int done;
        public int norm;
        public int workingOut;
        public String deviation;
        public String counterRepeat;
        public String note;
        public String notePrivate;
        public Order(String machine, String number, string modification, String orderName, int orderAmount, int lastCountOfOrder, String plannedMakeready,
            String plannedWork, String facticalMakeready, String facticalWork, int countDone, int cNorm,
            int orderWorkingOut, String deviationOrder, String orderCounterRepeat, String orderNote, String orderPrivateNote)
        {
            this.machineOfOrder = machine;
            this.numberOfOrder = number;
            this.modificationOfOrder = modification;
            this.nameOfOrder = orderName;
            this.amountOfOrder = orderAmount;
            this.lastCount = lastCountOfOrder;
            this.plannedTimeMakeready = plannedMakeready;
            this.plannedTimeWork = plannedWork;
            this.facticalTimeMakeready = facticalMakeready;
            this.facticalTimeWork = facticalWork;
            this.done = countDone;
            this.norm = cNorm;
            this.workingOut = orderWorkingOut;
            this.deviation = deviationOrder;
            this.counterRepeat = orderCounterRepeat;
            this.note = orderNote;
            this.notePrivate = orderPrivateNote;
        }*/

    }
}
