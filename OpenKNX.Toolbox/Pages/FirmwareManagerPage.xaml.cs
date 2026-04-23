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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace OpenKNX.Toolbox.Pages
{
    /// <summary>
    /// Interaktionslogik für Firmware_Manager.xaml
    /// </summary>
    public partial class FirmwareManagerPage : Page
    {
        public FirmwareManagerPage()
        {
            InitializeComponent();
            this.DataContext = ViewModels.FirmwareManagerViewModel.Instanz;
        }
    }
}
