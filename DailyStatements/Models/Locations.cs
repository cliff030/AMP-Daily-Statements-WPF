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
    
    public partial class Locations
    {
        public Locations()
        {
            this.Clients = new HashSet<Clients>();
        }
    
        public int LocationID { get; set; }
        public string LocationName { get; set; }
        public Nullable<int> BackColor { get; set; }
        public Nullable<int> ForeColor { get; set; }
        public string LocationAddress1 { get; set; }
        public string LocationAddress2 { get; set; }
        public string LocationCity { get; set; }
        public string LocationState { get; set; }
        public string LocationZip { get; set; }
        public string LocationPhone { get; set; }
        public string LocationFax { get; set; }
        public string Division { get; set; }
        public byte[] upsize_ts { get; set; }
        public Nullable<int> DialingRuleSetID { get; set; }
        public string UserDefinedCode { get; set; }
        public string MarketingCompanyName { get; set; }
        public string Website { get; set; }
        public string APIAffiliateID { get; set; }
        public string APIFeesGroup { get; set; }
        public string Product { get; set; }
        public string Notes { get; set; }
        public Nullable<int> QBCompanyID { get; set; }
    
        public virtual ICollection<Clients> Clients { get; set; }
    }
}
