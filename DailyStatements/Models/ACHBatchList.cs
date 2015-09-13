﻿using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AMPStatements.Models
{
    public class ACHBatchList
    {
        private DatabaseConfiguration _config;

        private ACHBatchGroup _ACHBatchGroup;
        private List<int> _FilteredACHClientIDs;

        private decimal _TotalCredits = 0;
        private decimal _TotalDebits = 0;

        private Custom_DailyStatementsPrintLog _PrintLog;

        public ACHBatchList(DatabaseConfiguration config, ACHBatchGroup ACHBatchGroup)
        {
            _ACHBatchGroup = ACHBatchGroup;
            _config = config;

            using(var cxt = new Entities(_config))
            {
                _FilteredACHClientIDs = (from ab in cxt.ACHBatch
                        join ad in cxt.ACHBatchDetails on ab.ACHBatchID equals ad.ACHBatchID
                        join c in cxt.Clients on ad.ClientID equals c.ClientID
                        where ab.ACHBatchGroupID == _ACHBatchGroup.ACHBatchGroupID
                        &&
                        (
                           (c.PreferredCommunicationMethod == "Mail" || (c.PreferredCommunicationMethod == "Fax" && c.Email == null))
                        || (c.PreferredCommunicationMethod == null && c.Email == null)
                        )
                        select ad.ClientID)
                        .Distinct()
                        .ToList();
            }

            _GetPrintLog();
            _GetTotalCredits();
            _GetTotalDebits();
        }

        public static List<ACHBatchGroup> GetACHBatchGroups(DatabaseConfiguration config, DateTime StartDate, DateTime EndDate, int limit)
        {
            List<ACHBatchGroup> ACHBatchGroups = new List<ACHBatchGroup>();

            using(var cxt = new Entities(config))
            {
                ACHBatchGroups = (from ag in cxt.ACHBatchGroup
                                  join a in cxt.ACHBatch on ag.ACHBatchGroupID equals a.ACHBatchGroupID
                                  where DbFunctions.TruncateTime(ag.StartDateFilter) >= StartDate.Date && DbFunctions.TruncateTime(ag.EndDateFilter) <= EndDate.Date
                                  orderby ag.ACHBatchGroupID descending
                                  select ag).Distinct().ToList().Take(limit).ToList();

                ACHBatchGroups.ForEach(a => cxt.Entry(a).Collection(ach => ach.ACHBatch).Load());
            }

            return ACHBatchGroups;
        }

        private void _GetPrintLog()
        {
            using(var cxt = new Entities(_config))
            {
                _PrintLog = (
                        from cds in cxt.Custom_DailyStatementsPrintLog
                        where cds.ACHBatchID == _ACHBatchGroup.ACHBatchGroupID
                        orderby cds.DatePrinted descending
                        select cds
                        ).FirstOrDefault();
            }
        }

        private void _GetTotalCredits()
        {
            using(var cxt = new Entities(_config))
            {
                _TotalCredits = (
                        from ad in cxt.ACHBatchDetails
                        join a in cxt.ACHBatch on ad.ACHBatchID equals a.ACHBatchID
                        where _FilteredACHClientIDs.Contains(ad.ClientID) && a.ACHBatchGroupID == _ACHBatchGroup.ACHBatchGroupID && ad.Amount <= 0
                        select (decimal?)ad.Amount
                    ).Sum() ?? 0;
            }
        }

        private void _GetTotalDebits()
        {
            using (var cxt = new Entities(_config))
            {
                _TotalDebits = (
                        from ad in cxt.ACHBatchDetails
                        join a in cxt.ACHBatch on ad.ACHBatchID equals a.ACHBatchID
                        where _FilteredACHClientIDs.Contains(ad.ClientID) && a.ACHBatchGroupID == _ACHBatchGroup.ACHBatchGroupID && ad.Amount >= 0
                        select (decimal?)ad.Amount
                    ).Sum() ?? 0;
            }
        }

        public void UpdatePrintLog(System.Security.Principal.WindowsIdentity User)
        {
            DateTime CurDate = DateTime.Now;

            using(var cxt = new Entities(_config))
            {                
                _PrintLog = new Custom_DailyStatementsPrintLog() { ACHBatchID = _ACHBatchGroup.ACHBatchGroupID, DatePrinted = CurDate, PrintedBy = User.Name, Success = true };

                cxt.Custom_DailyStatementsPrintLog.Add(_PrintLog);
                cxt.SaveChanges();                                
            }
        }

        public List<int> FilteredACHClientIDs
        {
            get { return _FilteredACHClientIDs; }
        }

        public int ACHBatchGroupID
        {
            get { return _ACHBatchGroup.ACHBatchGroupID; }
        }

        public DateTime? StartDateFilter
        {
            get { return _ACHBatchGroup.StartDateFilter; }
        }

        public DateTime? EndDateFilter
        {
            get { return _ACHBatchGroup.EndDateFilter; }
        }

        public DateTime CreatedOn
        {
            get { return _ACHBatchGroup.CreatedOn; }
        }

        public int NumberOfBatches
        {
            get { return _ACHBatchGroup.ACHBatch.Count; }
        }

        public int NumberOfItems
        {
            get { return _FilteredACHClientIDs.Count; }
        }

        public decimal Total
        {
            get { return _TotalDebits - _TotalCredits; }
        }

        public decimal TotalDebits
        {
            get { return _TotalDebits; }
        }

        public decimal TotalCredits
        {
            get { return _TotalCredits; }
        }        

        public Custom_DailyStatementsPrintLog PrintLog
        {
            get { return _PrintLog; }
        }
    }
}
