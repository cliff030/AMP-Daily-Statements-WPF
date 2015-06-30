using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using DailyStatements.Models;

namespace DailyStatements.ViewModels
{
    public class SelectDatabaseViewModel : ViewModel
    {
        private List<DatabaseConfiguration.CompanyList> _CompanyList;

        private RelayCommand _SelectCompanyCommand;
        private DatabaseConfiguration.CompanyList _SelectedCompany;

        public SelectDatabaseViewModel()
        {
            List<DatabaseConfiguration.CompanyList> TempList = DatabaseConfiguration.GetCompanyList();
            _CompanyList = new List<DatabaseConfiguration.CompanyList>();

            // Make sure the list of companies is definitely in the order we want.
            for(int i = 0; i < TempList.Count; i++)
            {
                _CompanyList.Add(TempList.Where(t => t.Order == i).FirstOrDefault());
            }
        }

        public void SelectCompany()
        {
            if(_SelectedCompany == null)
            {
                var e = new Exception("Invalid Company Selected.");
                System.Windows.MessageBox.Show(e.Message);
            }
            else
            {
                DatabaseConfiguration config = new DatabaseConfiguration(_SelectedCompany.DatabaseName);

                MainWindow window = new MainWindow();       
                var vm = new BatchListViewModel(config);
                vm.ClosingRequest += (sender, e) => window.Close();
                window.DataContext = vm;
                window.Show();

                this.OnClosingRequest();
            }
        }

        public DatabaseConfiguration.CompanyList SelectedCompany
        {
            get { return _SelectedCompany; }
            set 
            {
                if (_SelectedCompany != value)
                {
                    _SelectedCompany = value;
                    base.OnPropertyChanged("SelectedCompany");
                }
            }
        }

        public ObservableCollection<DatabaseConfiguration.CompanyList> CompanyList
        {
            get 
            { 
                return new ObservableCollection<DatabaseConfiguration.CompanyList>(_CompanyList); 
            }
        }

        public ICommand SelectCompanyCommand
        {
            get
            {
                if(_SelectCompanyCommand == null)
                {
                    _SelectCompanyCommand = new RelayCommand(
                            param => this.SelectCompany()
                        );
                }

                return _SelectCompanyCommand;
            }
        }
    }
}
