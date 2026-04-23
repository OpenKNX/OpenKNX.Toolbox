using OpenKNX.Toolbox.Lib.Data;
using OpenKNX.Toolbox.Lib.Helper;
using OpenKNX.Toolbox.Lib.Platforms;
using OpenKNX.Toolbox.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Wpf.Ui.Controls;

namespace OpenKNX.Toolbox.Dialogs
{
    /// <summary>
    /// Interaktionslogik für FlashSelectDialog.xaml
    /// </summary>
    public partial class ObjectSelectDialog : ContentDialog
    {
        ObjectSelectDialogViewModel ViewModel = new ObjectSelectDialogViewModel();

        public ObjectSelectDialog(string title, List<object> list)
        {
            InitializeComponent();
            ViewModel.Items = list;
            ViewModel.Title = title;
            this.DataContext = ViewModel;

            base.Closing += FlashSelectDialog_Closing;
        }

        private void FlashSelectDialog_Closing(ContentDialog sender, ContentDialogClosingEventArgs args)
        {
            if (args.Result != ContentDialogResult.Primary)
                ViewModel.SelectedItem = string.Empty;
        }

        public object? GetSelectedItem()
        {
            return ViewModel.SelectedItem;
        }
    }
}
