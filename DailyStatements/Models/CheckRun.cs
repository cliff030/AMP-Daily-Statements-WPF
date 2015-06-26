//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace DailyStatements.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class CheckRun
    {
        public CheckRun()
        {
            this.Checks = new HashSet<Checks>();
        }
    
        public int CheckRunID { get; set; }
        public System.DateTime CheckRunDate { get; set; }
        public System.DateTime CheckDate { get; set; }
        public Nullable<int> StartCheck { get; set; }
        public Nullable<int> EndCheck { get; set; }
        public short CheckRunType { get; set; }
        public Nullable<System.DateTime> CheckRunStartDate { get; set; }
        public Nullable<System.DateTime> CheckRunEndDate { get; set; }
        public Nullable<int> BankAccountID { get; set; }
        public byte[] upsize_ts { get; set; }
        public string CheckRunUserID { get; set; }
    
        public virtual ICollection<Checks> Checks { get; set; }
    }
}