//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace AMPStatements.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class Custom_DailyStatementsPrintLog
    {
        public int ID { get; set; }
        public int ACHBatchID { get; set; }
        public System.DateTime DatePrinted { get; set; }
        public string PrintedBy { get; set; }
        public bool Success { get; set; }
    }
}