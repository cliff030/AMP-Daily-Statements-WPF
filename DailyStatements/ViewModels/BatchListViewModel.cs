using AMPStatements.Models;
using AMPStatements.Models.Reports;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using System.Threading;
using System.Windows.Input;
using AMPStatements.ReportExecutionService;
using System.ComponentModel;
using System.Windows;

namespace AMPStatements.ViewModels
{
    public class BatchListViewModel : ViewModel
    {

        #region Fields

        private ObservableCollection<ACHBatchList> _ACHBatchLists = new ObservableCollection<ACHBatchList>();

        private DateTime _StartDate = DateTime.Now.AddDays(-30).Date;
        private DateTime _EndDate = DateTime.Now.AddDays(1).Date;

        private DatabaseConfiguration _config;

        private bool _Reprint = false;

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
                System.Windows.MessageBox.Show("Report creation cancelled.");
            }
            else if(e.Error != null)
            {
                System.Windows.MessageBox.Show(e.Error.ToString());
            }
            else
            {
               _SelectedACHBatchList.UpdatePrintLog(System.Security.Principal.WindowsIdentity.GetCurrent());

               List<ACHBatchList> tmpACHBatchLists = _ACHBatchLists.ToList();
               var tmpACHBatchList = tmpACHBatchLists.SingleOrDefault(a => a.ACHBatchGroupID == _SelectedACHBatchList.ACHBatchGroupID);
               tmpACHBatchList = _SelectedACHBatchList;

               ACHBatchLists = new ObservableCollection<ACHBatchList>(tmpACHBatchLists);

               string prompt = String.Format("Reports for Batch Group ID {0} have been successfully printed.", _SelectedACHBatchList.ACHBatchGroupID);

                System.Windows.MessageBox.Show(prompt);
            }

            CurrentProgress = 0;
            ReportCreationProgressVisibility = System.Windows.Visibility.Hidden;
        }

        private void _GetACHBatchGroups(object sender, DoWorkEventArgs e)
        {
            LoadingPromptVisibility = System.Windows.Visibility.Visible;

            try
            {
                List<ACHBatchGroup> ACHBatchGroups = ACHBatchList.GetACHBatchGroups(_config, _StartDate, _EndDate, 25);

                _ACHBatchLists = new ObservableCollection<ACHBatchList>();
                foreach (ACHBatchGroup BatchGroup in ACHBatchGroups)
                {
                    if(_BatchListWorker.CancellationPending)
                    {
                        e.Cancel = true;
                        break;
                    }
              
                    _ACHBatchLists.Add(new ACHBatchList(_config, BatchGroup));
                }
            }
            catch (Exception ex)
            {
                e.Cancel = true;
                System.Windows.MessageBox.Show(ex.ToString());
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
            _Reprint = false;

            Application.Current.Dispatcher.Invoke((Action)delegate
            {

                if (_SelectedACHBatchList.PrintLog != null)
                {
                    string PromptText = String.Format("Batch Group ID {0} was printed on {1:d} by {2}. Would you like to print it again?",
                        _SelectedACHBatchList.ACHBatchGroupID, _SelectedACHBatchList.PrintLog.DatePrinted, _SelectedACHBatchList.PrintLog.PrintedBy);

                    var vw = new AMPStatements.Views.GenericModalView(PromptText);
                    var vm = (GenericModalViewModel)vw.DataContext;
                    
                    Nullable<bool> DialogResult = vw.ShowDialog();

                    _Reprint = vm.IsYes;
                }

            });

            if (_Reprint == false && _SelectedACHBatchList.PrintLog != null)
            {
                e.Cancel = true;
                return;
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
            var vw = new AMPStatements.Views.SelectDatabaseView();
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
