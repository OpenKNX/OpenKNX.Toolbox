using OpenKNX.Toolbox.Lib.Data;
using OpenKNX.Toolbox.Lib.Helper;
using OpenKNX.Toolbox.Lib.Models;
using OpenKNX.Toolbox.Lib.Platforms;
using OpenKNX.Toolbox.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Security.RightsManagement;
using System.Text;
using System.Threading.Tasks;

namespace OpenKNX.Toolbox.ViewModels
{
    public class ObjectSelectDialogViewModel : INotifyPropertyChanged
    {
        private List<object> _items = new List<object>();
        public List<object> Items
        {
            get { return _items; }
            set
            {
                _items = value;
                Changed("Items");
            }
        }

        private object? _selectedItem;
        public object? SelectedItem
        {
            get { return _selectedItem; }
            set
            {
                _selectedItem = value;
                Changed("SelectedItem");
                CanContinue = _selectedItem != null;
            }
        }

        private bool _canContinue = false;
        public bool CanContinue
        {
            get { return _canContinue; }
            set
            {
                _canContinue = value;
                Changed("CanContinue");
            }
        }

        private string _title = string.Empty;
        public string Title
        {
            get { return _title; }
            set
            {
                _title = value;
                Changed("Title");
            }
        }


        public event PropertyChangedEventHandler? PropertyChanged;

        private void Changed(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
