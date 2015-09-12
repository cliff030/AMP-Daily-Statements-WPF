using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;

namespace DailyStatements.ViewModels
{
    public class GenericModalViewModel : ViewModel
    {
        #region Fields

        private string _PromptText;
        private bool _IsYes = false;

        private RelayCommand<Button> _UserPrompt;

        #endregion // Fields

        public GenericModalViewModel(string PromptText)
        {
            this.PromptText = PromptText;
        }

        private void _UserSelection(Button UserSelection)
        {
            if (UserSelection.Name == "YesButton")
                IsYes = true;
            else
                IsYes = false;

            this.OnClosingRequest();
        }

        #region Properties

        public string PromptText
        {
            get { return _PromptText; }
            set
            {
                if(_PromptText != value)
                {
                    _PromptText = value;
                    base.OnPropertyChanged("PromptText");
                }
            }
        }

        public bool IsYes
        {
            get { return _IsYes; }
            set { _IsYes = value; }
        }

        #endregion // Properties

        #region Commands

        public ICommand UserPrompt
        {
            get
            {
                if (_UserPrompt == null)
                {
                    _UserPrompt = new RelayCommand<Button>(_UserSelection);
                }

                return _UserPrompt;
            }
        }

        #endregion // Commands
    }
}
