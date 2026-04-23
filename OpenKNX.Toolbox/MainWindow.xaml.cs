using System.Text;
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

namespace OpenKNX.Toolbox;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : FluentWindow
{
    public MainWindow()
    {
        InitializeComponent();

        this.DataContext = ViewModels.MainViewModel.Instanz;
        _ = ViewModels.MainViewModel.Instanz.Init();

        ViewModels.MainViewModel.Instanz.SetContentPresenterDialog(MainContentPresenter);
        ViewModels.MainViewModel.Instanz.SetContentPresenterSnackbar(MainSnackbarPresenter);
    }
}