using OpenKNX.Toolbox.Lib.Data;
using OpenKNX.Toolbox.Lib.Helper;
using OpenKNX.Toolbox.Lib.Platforms;
using OpenKNX.Toolbox.Models;
using OpenKNX.Toolbox.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
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
    /// Interaktionslogik für NewConnectionDialog.xaml
    /// </summary>
    public partial class NewConnectionDialog : ContentDialog
    {
        NewConnectionDialogViewModel ViewModel = new NewConnectionDialogViewModel();
        private ConnectionModel? connection;

        public NewConnectionDialog()
        {
            InitializeComponent();
            this.DataContext = ViewModel;

            base.Closing += NewConnectionDialog_Closing;
        }

        private void NewConnectionDialog_Closing(ContentDialog sender, ContentDialogClosingEventArgs args)
        {
            if (args.Result != ContentDialogResult.Primary)
            {
                connection = null;
                return;
            }

            connection = new ConnectionModel()
            {
                FriendlyName = ViewModel.Name,
                EndPoint = new IPEndPoint(IPAddress.Parse(ViewModel.IpAddress), ViewModel.IpPort),
                IsManuallyAdded = true,
                IsTunnel = true
            };

            if (args.Result != ContentDialogResult.Primary)
                connection = null;
        }

        public ConnectionModel? GetAddedItem()
        {
            return connection;
        }
    }
}
