using System;

namespace OrderManager
{
    internal class UserInfo
    {
        public int userID;
        public String userName;
        public String surname;
        public String name;
        public String patronymic;
        public String categoryesMachine;
        public String dateOfEmployment;
        public String dateOfBirth;
        public String activeUser;
        public String dateOfDismissal;
        public String note;
        private object v;

        public UserInfo(object v)
        {
            this.v = v;
        }

        public UserInfo(int userID, string userName, string surname, string name, string patronymic, string categoryesMachine, string dateOfEmployment, string dateOfBirth, string activeUser, string dateOfDismissal, string note)
        {
            this.userID = userID;
            this.userName = userName;
            this.surname = surname;
            this.name = name;
            this.patronymic = patronymic;
            this.categoryesMachine = categoryesMachine;
            this.dateOfEmployment = dateOfEmployment;
            this.dateOfBirth = dateOfBirth;
            this.activeUser = activeUser;
            this.dateOfDismissal = dateOfDismissal;
            this.note = note;
        }
    }
}
