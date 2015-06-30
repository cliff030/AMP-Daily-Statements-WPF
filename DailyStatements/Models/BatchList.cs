using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DailyStatements.Models
{
    public class BatchList
    {
        private int _BatchGroupID;
        private DateTime _StartDateFilter;
        private DateTime _EndDateFilter;
        private DateTime _CreatedOn;
        private int _NumberOfBatches;
        private decimal _TotalAmount;
        private decimal _TotalDebits;
        private decimal _TotalCredits;
        private DateTime _DatePrinted;
        private string _PrintedBy;

        public int BatchGroupID
        {
            get { return _BatchGroupID; }
            set { _BatchGroupID = value; }
        }

        public DateTime StartDateFilter
        {
            get;
            set;
        }

        public DateTime EndDateFilter
        {
            get;
            set;
        }

        public DateTime CreatedOn
        {
            get;
            set;
        }

        public int NumberOfBatches
        {
            get;
            set;
        }

        public int NumberOfItems
        {
            get;
            set;
        }

        public decimal TotalAmount
        {
            get;
            set;
        }

        public decimal TotalDebits
        {
            get;
            set;
        }

        public decimal TotalCredits
        {
            get;
            set;
        }

        public DateTime DatePrinted
        {
            get;
            set;
        }

        public string PrintedBy
        {
            get;
            set;
        }
    }
}
