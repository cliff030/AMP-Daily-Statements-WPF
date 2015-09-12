using DailyStatements.Models;
using DailyStatements.Models.Reports;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using System.Threading;
using System.Windows.Input;
using DailyStatements.ReportExecutionService;
using System.ComponentModel;

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

        private RelayCommand _SelectACHBatchListsCommand;
        private RelayCommand _ToggleOkButtonCommand;
        private RelayCommand _CreateReportsCommand;
        private RelayCommand _CancelCommand;
        private RelayCommand<System.Windows.Window> _ChangeDatabaseCommand;
        
        private bool _OkButtonEnabled = false;
        private ACHBatchList _SelectedACHBatchList;
        private System.Windows.Visibility _ReportCreationProgressVisibility = System.Windows.Visibility.Hidden;
        private int _CurrentProgress = 0;
        private System.Windows.Visibility _LoadingPromptVisibility = System.Windows.Visibility.Visible;

#if DEBUG
        private bool _SaveReports = true;
#else
        private bool _SaveReports = false;
#endif

        #endregion // Fields

        #region Workers

        private BackgroundWorker _BatchListWorker = new BackgroundWorker();
        private BackgroundWorker _CreateReportsWorker = new BackgroundWorker();

        #endregion // Workers

        #region Constructors

        public BatchListViewModel(DatabaseConfiguration config) 
        {
            _config = config;

            _BatchListWorker.WorkerSupportsCancellation = true;
            _BatchListWorker.DoWork += _GetACHBatchGroups;
            _BatchListWorker.RunWorkerCompleted += _BatchListWorkerComplete;

            _CreateReportsWorker.WorkerSupportsCancellation = true;
            _CreateReportsWorker.WorkerReportsProgress = true;
            _CreateReportsWorker.DoWork += _CreateReports;
            _CreateReportsWorker.RunWorkerCompleted += _CreateReportsWorkerComplete;
            _CreateReportsWorker.ProgressChanged += _CreateReportsWorker_ProgressChanged;

            if (! _BatchListWorker.IsBusy)
            {
                _BatchListWorker.RunWorkerAsync();
            }
        }


        #endregion // Constructors

        #region Methods

        private void _CreateReportsWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            CurrentProgress = e.ProgressPercentage;
        }

        private void _BatchListWorkerComplete(object sender, RunWorkerCompletedEventArgs e)
        {
            LoadingPromptVisibility = System.Windows.Visibility.Hidden;

            if(e.Cancelled)
            {
                System.Windows.MessageBox.Show("The operation has been cancelled.");
            }
            else if(e.Error != null)
            {
                System.Windows.MessageBox.Show(e.Error.ToString());
            }
            else
            {
                ACHBatchLists = new ObservableCollection<ACHBatchList>(_ACHBatchLists.OrderByDescending(a => a.ACHBatchGroupID).ToList());
                OkButtonEnabled = true;
            }
        }

        private void _CreateReportsWorkerComplete(object sender, RunWorkerCompletedEventArgs e)
        {
            if(e.Cancelled)
            {
                CurrentProgress = 0;
                ReportCreationProgressVisibility = System.Windows.Visibility.Hidden;

                System.Windows.MessageBox.Show("Report creation cancelled.");
            }
            else if(e.Error != null)
            {
                System.Windows.MessageBox.Show(e.Error.ToString());
            }
            else
            {
                System.Windows.MessageBox.Show("Reports printed!");
            }
        }

        private void _GetACHBatchGroups(object sender, DoWorkEventArgs e)
        {
            LoadingPromptVisibility = System.Windows.Visibility.Visible;

            try
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
                        if(_BatchListWorker.CancellationPending)
                        {
                            e.Cancel = true;
                            break;
                        }

                        cxt.Entry(BatchGroup).Collection(a => a.ACHBatch).Load();

                        _ACHBatchLists.Add(new ACHBatchList(_config, BatchGroup));
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void _ToggleOkButton()
        {
            if(_SelectedACHBatchList != null)
            {
                OkButtonEnabled = true;
            }
        }

        private void _Cancel()
        {
            if (_BatchListWorker.IsBusy == false && _CreateReportsWorker.IsBusy == false)
            {
                System.Windows.Application.Current.Shutdown();
            }
            else
            {
                if (_BatchListWorker.WorkerSupportsCancellation)
                {
                    _BatchListWorker.CancelAsync();
                }

                if (_CreateReportsWorker.WorkerSupportsCancellation)
                {
                    _CreateReportsWorker.CancelAsync();
                }
            }
        }

        private void _CreateReports(object sender, DoWorkEventArgs e)
        {
            if(_SelectedACHBatchList.CreatedOn != null || _SelectedACHBatchList.PrintedBy != null)
            {

            }

            ReportCreationProgressVisibility = System.Windows.Visibility.Visible;

            double i = 0;
            double TotalClients = _SelectedACHBatchList.FilteredACHClientIDs.Count;

            foreach (int ClientID in _SelectedACHBatchList.FilteredACHClientIDs)
            {
                if(_CreateReportsWorker.CancellationPending)
                {
                    e.Cancel = true;
                    break;
                }

                ParameterValue[] Parameters = new ParameterValue[1];
                Parameters[0] = new ReportExecutionService.ParameterValue();
                Parameters[0].Name = "ClientID";
                Parameters[0].Value = ClientID.ToString();

                Report ClientStatement = new Report(ClientID, _config, _SelectedACHBatchList.ACHBatchGroupID, _SaveReports);
                ClientStatement.PrintReport(Parameters, -1, -1);

                i++;

                int Progress = Convert.ToInt32((i / TotalClients) * 100);
                _CreateReportsWorker.ReportProgress(Progress);
            }
        }

        public void CreateReports()
        {
            if(! _CreateReportsWorker.IsBusy)
            {
                _CreateReportsWorker.RunWorkerAsync();
            }
        }

        public void SelectACHBatchLists()
        {

        }

        public void ChangeDatabase(System.Windows.Window window)
        {            
            var vw = new DailyStatements.Views.SelectDatabaseView();
            vw.Show();
            
            window.Close();
        }

        #endregion // Methods

        #region Properties

        public System.Windows.Visibility ReportCreationProgressVisibility
        {
            get { return _ReportCreationProgressVisibility; }
            set
            {
                if(_ReportCreationProgressVisibility != value)
                {
                    _ReportCreationProgressVisibility = value;
                    base.OnPropertyChanged("ReportCreationProgressVisibility");
                }
            }
        }

        public ACHBatchList SelectedACHBatchList
        {
            get { return _SelectedACHBatchList; }
            set
            {
                if(_SelectedACHBatchList != value)
                {
                    _SelectedACHBatchList = value;
                    base.OnPropertyChanged("SelectedACHBatchList");
                }
            }
        }

        public bool OkButtonEnabled
        {
            get { return _OkButtonEnabled; }
            set 
            {
                if (_OkButtonEnabled != value)
                {
                    _OkButtonEnabled = value;
                    base.OnPropertyChanged("OkButtonEnabled");
                }
            }
        }

        public System.Windows.Visibility LoadingPromptVisibility
        {
            get { return _LoadingPromptVisibility; }
            set
            {
                if(_LoadingPromptVisibility != value)
                {
                    _LoadingPromptVisibility = value;
                    base.OnPropertyChanged("LoadingPromptVisibility");
                }
            }
        }

        public int CurrentProgress
        {
            get { return _CurrentProgress; }
            set
            {
                if(_CurrentProgress != value)
                {
                    _CurrentProgress = value;
                    base.OnPropertyChanged("CurrentProgress");
                }
            }
        }

        public ObservableCollection<ACHBatchList> ACHBatchLists
        {
            get { return _ACHBatchLists; }
            set
            {
                if(_ACHBatchLists != value)
                {
                    _ACHBatchLists = value;
                    base.OnPropertyChanged("ACHBatchLists");
                }
            }
        }

        public ICommand SelectACHBatchListsCommand
        {
            get
            {
                if(_SelectACHBatchListsCommand == null)
                {
                    _SelectACHBatchListsCommand = new RelayCommand(
                            param => this.SelectACHBatchLists()
                        );
                }

                return _SelectACHBatchListsCommand;
            }
        }

        public ICommand ToggleOkButtonCommand
        {
            get
            {
                if (_ToggleOkButtonCommand == null)
                {
                    _ToggleOkButtonCommand = new RelayCommand(
                            param => _ToggleOkButton()
                        );
                }

                return _ToggleOkButtonCommand;
            }
        }

        public ICommand CancelCommand
        {
            get
            {
                if (_CancelCommand == null)
                {
                    _CancelCommand = new RelayCommand(
                            param => _Cancel()
                        );
                }

                return _CancelCommand;
            }
        }

        public ICommand CreateReportsCommand
        {
            get
            {
                if(_CreateReportsCommand == null)
                {
                    _CreateReportsCommand = new RelayCommand(
                            param => CreateReports()
                        );
                }

                return _CreateReportsCommand;
            }
        }

        public ICommand ChangeDatabaseCommand
        {
            get
            {
                if(_ChangeDatabaseCommand == null)
                {
                    _ChangeDatabaseCommand = new RelayCommand<System.Windows.Window>(ChangeDatabase);
                }

                return _ChangeDatabaseCommand;
            }
        }

        #endregion // Properties
    }
}
