using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using AMPStatements.Models;

namespace AMPStatements.ViewModels
{
    public class SelectDatabaseViewModel : ViewModel
    {
        private List<Company> _CompanyList;

        private RelayCommand _SelectCompanyCommand;
        private Company _SelectedCompany;

        public SelectDatabaseViewModel()
        {
            using(var cxt = new CreditsoftCompaniesEntities())
            {
                _CompanyList = (from c in cxt.Companies
                               where c.Active == true
                               orderby c.MenuOption
                               select c).ToList();
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
                try
                {
                    DatabaseConfiguration config = new DatabaseConfiguration(_SelectedCompany.CreditsoftDatabase);

                    MainWindow window = new MainWindow();
                    var vm = new BatchListViewModel(config);
                    vm.ClosingRequest += (sender, e) => window.Close();
                    window.DataContext = vm;
                    window.Show();

                    this.OnClosingRequest();
                }
                catch(Exception ex)
                {
                    System.Windows.MessageBox.Show(ex.ToString());
                }
            }
        }

        public Company SelectedCompany
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

        public ObservableCollection<Company> CompanyList
        {
            get 
            {
                return new ObservableCollection<Company>(_CompanyList); 
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
