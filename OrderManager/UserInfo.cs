using System;

namespace OrderManager
{
    internal class UserInfo
    {
        public int userID;
        public String surname;
        public String name;
        public String patronymic;
        public String categoryesMachine;
        public String dateOfEmployment;
        public String dateOfBirth;
        public String activeUser;
        public string indexUser;
        public String dateOfDismissal;
        public String note;
        private object v;

        public UserInfo(object v)
        {
            this.v = v;
        }

        public UserInfo(int userID, string surname, string name, string patronymic, string categoryesMachine, string dateOfEmployment, string dateOfBirth, string activeUser, string indexUser, string dateOfDismissal, string note)
        {
            this.userID = userID;
            this.surname = surname;
            this.name = name;
            this.patronymic = patronymic;
            this.categoryesMachine = categoryesMachine;
            this.dateOfEmployment = dateOfEmployment;
            this.dateOfBirth = dateOfBirth;
            this.activeUser = activeUser;
            this.indexUser = indexUser;
            this.dateOfDismissal = dateOfDismissal;
            this.note = note;
        }
    }
}
