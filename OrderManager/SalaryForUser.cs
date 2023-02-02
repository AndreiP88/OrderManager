using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderManager
{
    internal class SalaryForUser
    {
        public string id;
        public string period;
        public decimal basicSalary;
        public decimal bonusSalary;
        public decimal tax;
        public decimal pension;

        public SalaryForUser(string lPeriod, decimal lBasicSalary, decimal lBonusSalary, decimal lTax, decimal lPension)
        {
            this.period = lPeriod;
            this.basicSalary = lBasicSalary;
            this.bonusSalary = lBonusSalary;
            this.tax = lTax;
            this.pension = lPension;
        }

        public SalaryForUser(string lID, string lPeriod, decimal lBasicSalary, decimal lBonusSalary, decimal lTax, decimal lPension)
        {
            this.id = lID;
            this.period = lPeriod;
            this.basicSalary = lBasicSalary;
            this.bonusSalary = lBonusSalary;
            this.tax = lTax;
            this.pension = lPension;
        }
    }
}
