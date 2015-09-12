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

namespace DailyStatements.Views
{
    /// <summary>
    /// Interaction logic for GenericModalView.xaml
    /// </summary>
    public partial class GenericModalView : Window
    {
        public GenericModalView()
        {
            InitializeComponent();
        }

        public GenericModalView(string PromptText)
        {
            InitializeComponent();

            var vm = new DailyStatements.ViewModels.GenericModalViewModel(PromptText);
            vm.ClosingRequest += (sender, e) => this.Close();
            this.DataContext = vm;
        }
    }
}
