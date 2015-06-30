using DailyStatements.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using System.Threading;

namespace DailyStatements.ViewModels
{
    public class BatchListViewModel : ViewModel
    {

        #region Fields

        private ObservableCollection<ACHBatchGroup> _ACHBatchGroups = new ObservableCollection<ACHBatchGroup>();

        private ObservableCollection<ACHBatchList> _ACHBatchLists = new ObservableCollection<ACHBatchList>();

        private DateTime _StartDate = DateTime.Now.AddDays(-30).Date;
        private DateTime _EndDate = DateTime.Now.AddDays(1).Date;

        private DatabaseConfiguration _config;

        #endregion // Fields

        #region Threads

        private Thread _BatchListThread;

        #endregion // Threads

        #region Constructors

        public BatchListViewModel(DatabaseConfiguration config) 
        {
            _config = config;

            _BatchListThread = new Thread(new ThreadStart(_GetACHBatchGroups));
            _BatchListThread.IsBackground = true;

            _BatchListThread.Start();
        }

        #endregion // Constructors

        #region Methods

        private void _GetACHBatchGroups()
        {
            using (var cxt = new Entities(_config))
            {
                _ACHBatchGroups = new ObservableCollection<ACHBatchGroup>(
                        (from ag in cxt.ACHBatchGroup
                         join a in cxt.ACHBatch on ag.ACHBatchGroupID equals a.ACHBatchGroupID
                         where DbFunctions.TruncateTime(ag.StartDateFilter) >= _StartDate.Date && DbFunctions.TruncateTime(ag.EndDateFilter) <= _EndDate.Date
                         orderby ag.ACHBatchGroupID descending
                         select ag).Distinct().ToList().Take(20).ToList()
                    );


                _ACHBatchLists = new ObservableCollection<ACHBatchList>();
                foreach (ACHBatchGroup BatchGroup in _ACHBatchGroups)
                {
                    cxt.Entry(BatchGroup).Collection(a => a.ACHBatch).Load();

                    _ACHBatchLists.Add(new ACHBatchList(_config, BatchGroup));
                }

                _ACHBatchLists = new ObservableCollection<ACHBatchList>(_ACHBatchLists.OrderByDescending(a => a.ACHBatchGroupID).ToList());
            }

            App.Current.Dispatcher.Invoke((Action)delegate
                {
                    base.OnPropertyChanged("ACHBatchLists");
                });
        }

        #endregion // Methods

        #region Properties

        public ObservableCollection<ACHBatchList> ACHBatchLists
        {
            get { return _ACHBatchLists; }
        }

        #endregion // Properties
    }
}
