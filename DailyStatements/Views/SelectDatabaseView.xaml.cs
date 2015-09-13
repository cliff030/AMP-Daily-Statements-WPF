using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using AMPStatements.ViewModels;

namespace AMPStatements.Views
{
    /// <summary>
    /// Interaction logic for SelectDatabaseView.xaml
    /// </summary>
    public partial class SelectDatabaseView : Window
    {
        public SelectDatabaseView()
        {
            InitializeComponent();

            var vm = new SelectDatabaseViewModel();
            this.DataContext = vm;
            vm.ClosingRequest += (sender, e) => this.Close();
        }
    }
}
