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

        private bool _OkButtonEnabled = false;
        private ACHBatchList _SelectedACHBatchList;
        private System.Windows.Visibility _ReportCreationProgressVisibility = System.Windows.Visibility.Hidden;
        private double _CurrentProgress = 0;

#if DEBUG
        private bool _SaveReports = true;
#else
        private bool _SaveReports = false;
#endif

        #endregion // Fields

        #region Threads

        private Thread _BatchListThread;
        private Thread _CreateReportsThread;

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
                        cxt.Entry(BatchGroup).Collection(a => a.ACHBatch).Load();

                        _ACHBatchLists.Add(new ACHBatchList(_config, BatchGroup));
                    }

                    _ACHBatchLists = new ObservableCollection<ACHBatchList>(_ACHBatchLists.OrderByDescending(a => a.ACHBatchGroupID).ToList());
                    _OkButtonEnabled = true;
                }
            }
            catch (Exception e)
            {
                System.Windows.MessageBox.Show(e.ToString());
            }
            finally
            {
                App.Current.Dispatcher.Invoke((Action)delegate
                    {
                        base.OnPropertyChanged("OkButtonEnabled");
                        base.OnPropertyChanged("ACHBatchLists");
                    });
            }
        }

        private void _ToggleOkButton()
        {
            App.Current.Dispatcher.Invoke((Action)delegate
                   {
                       if(_SelectedACHBatchList != null)
                       {
                           OkButtonEnabled = true;
                       }
                   });
        }

        private void _CreateReports()
        {
            App.Current.Dispatcher.Invoke((Action)delegate
            {
                ReportCreationProgressVisibility = System.Windows.Visibility.Visible;
            });

            double i = 0;
            double TotalClients = _SelectedACHBatchList.FilteredACHClientIDs.Count;

            foreach (int ClientID in _SelectedACHBatchList.FilteredACHClientIDs)
            {
                ParameterValue[] Parameters = new ParameterValue[1];
                Parameters[0] = new ReportExecutionService.ParameterValue();
                Parameters[0].Name = "ClientID";
                Parameters[0].Value = ClientID.ToString();

                Report ClientStatement = new Report(ClientID, _config, _SelectedACHBatchList.ACHBatchGroupID, _SaveReports);
                ClientStatement.PrintReport(Parameters, -1, -1);

                i++;

                App.Current.Dispatcher.Invoke((Action)delegate
                {
                    CurrentProgress = (i / TotalClients) * 100;
                });
            }

            App.Current.Dispatcher.Invoke((Action)delegate
            {
                System.Windows.MessageBox.Show("Reports printed!");
            });
        }

        public void CreateReports()
        {
            _CreateReportsThread = new Thread(new ThreadStart(_CreateReports));
            _CreateReportsThread.IsBackground = true;

            _CreateReportsThread.Start();           
        }

        public void SelectACHBatchLists()
        {

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

        public double CurrentProgress
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

        #endregion // Properties
    }
}
