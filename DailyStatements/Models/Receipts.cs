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
    
    public partial class Receipts
    {
        public int ReceiptID { get; set; }
        public int ClientID { get; set; }
        public System.DateTime DateReceived { get; set; }
        public decimal ReceiptAmount { get; set; }
        public int BankAccountID { get; set; }
        public string PaymentType { get; set; }
        public bool Deposited { get; set; }
        public bool Marked { get; set; }
        public Nullable<int> BankID { get; set; }
        public bool NSF { get; set; }
        public Nullable<System.DateTime> NSFDate { get; set; }
        public Nullable<int> FileID { get; set; }
        public string Memo { get; set; }
        public string OperatorStamp { get; set; }
        public Nullable<System.DateTime> DateTimeStamp { get; set; }
        public Nullable<int> ACHBatchID { get; set; }
        public string NSFReason { get; set; }
        public Nullable<int> RefundDetailID { get; set; }
        public Nullable<System.DateTime> DateEntered { get; set; }
        public Nullable<int> TransferedToClientID { get; set; }
        public byte[] upsize_ts { get; set; }
        public Nullable<int> ACHBankAccountID { get; set; }
        public Nullable<int> StatementID { get; set; }
        public Nullable<int> ReceiptBatchID { get; set; }
        public Nullable<int> CommissionBatchID { get; set; }
        public Nullable<int> FileLogDetailsID { get; set; }
        public Nullable<int> TransferredFromClientID { get; set; }
        public Nullable<int> ACHBatchDetailID { get; set; }
        public Nullable<int> OriginatingFromNSFReceiptID { get; set; }
        public Nullable<int> NSFReversalBankID { get; set; }
        public Nullable<int> NSFFileID { get; set; }
        public string CreatedUserID { get; set; }
        public Nullable<int> OriginatingFromVoidCheckID { get; set; }
        public Nullable<int> OriginatingFromVoidBankAccountID { get; set; }
        public bool NSFRecovered { get; set; }
        public Nullable<int> AgencyID { get; set; }
        public Nullable<int> CreditorID { get; set; }
        public Nullable<int> CoAppID { get; set; }
        public bool AllowZeroAmountReceipt { get; set; }
        public Nullable<decimal> TempCreditAmount { get; set; }
        public string TempNetGross { get; set; }
        public Nullable<int> SalesOrderID { get; set; }
        public Nullable<int> OnlineTransactionID { get; set; }
        public bool M2_TransferFromAgency { get; set; }
        public string M2_UniqueID { get; set; }
        public string M2_TransactionID { get; set; }
        public bool PaymentProcessor_TransferFromAgency { get; set; }
        public string PaymentProcessor_TransactionRefID { get; set; }
        public string ReferenceReceiptNumber { get; set; }
        public string ReferenceDepositNumber { get; set; }
        public bool IsLateReturn { get; set; }
        public Nullable<int> CCBatchID { get; set; }
        public Nullable<int> CCBatchDetailsID { get; set; }
    
        public virtual ACHBatchDetails ACHBatchDetails { get; set; }
        public virtual Bank Bank { get; set; }
        public virtual Clients Clients { get; set; }
        public virtual Creditors Creditors { get; set; }
        public virtual Receipts Receipts1 { get; set; }
        public virtual Receipts Receipts2 { get; set; }
    }
}
