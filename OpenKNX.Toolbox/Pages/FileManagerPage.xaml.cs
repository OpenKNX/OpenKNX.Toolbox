using Kaenx.Konnect.Addresses;
using OpenKNX.Toolbox.Classes.Actions;
using OpenKNX.Toolbox.Dialogs;
using OpenKNX.Toolbox.Lib.Data;
using OpenKNX.Toolbox.Models;
using OpenKNX.Toolbox.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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
    /// Interaktionslogik für FileManagerPage.xaml
    /// </summary>
    public partial class FileManagerPage : Page
    {
        FileManagerViewModel model = new FileManagerViewModel();
        public FileManagerPage()
        {
            InitializeComponent();
            this.DataContext = model;
        }

        private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {

        }

        private async void PrepareUpload(string[] files)
        {
            bool hasAnyFile = files.Any(f => System.IO.File.Exists(f));
            if(!hasAnyFile)
            {
                return;
            }
            ConnectionModel? _conn = model.GetConnectionModel();
            if (_conn == null)
            {
                return;
            }

            CancellationTokenSource token = new CancellationTokenSource();

            List<string> folderList = new List<string>();
            folderList.Add("/");
            foreach (var item in model.Items)
            {
                if(!item.IsFile)
                {
                    folderList.Add(item.FullPath);
                }
                GetSubFolders(item, folderList);
            }

            StringSelectDialog folderSelectDialog = new("Zielordner wählen", folderList);

            await MainViewModel.Instanz.ContentDialogService.ShowAsync(
                folderSelectDialog,
                token.Token
            );

            if (!string.IsNullOrEmpty(folderSelectDialog.GetSelectedItem()))
            {
                foreach (string file in files)
                {
                    string destination = folderSelectDialog.GetSelectedItem();
                    if(destination.EndsWith("/"))
                    {
                        destination += System.IO.Path.GetFileName(file);
                    } else
                    {
                        destination += "/" + System.IO.Path.GetFileName(file);
                    }

                    ActionsViewModel.Instanz.AddAction(new FileAction(
                        model.RemoteAddressUni,
                        _conn,
                        file,
                        destination,
                        true,
                        Classes.FileActionTypes.Upload));
                }
            }
        }

        private void GetSubFolders(FileModel folder, List<string> folderList)
        {
            foreach(FileModel item in folder.Items)
            {
                if(!item.IsFile)
                {
                    folderList.Add(item.FullPath);
                    GetSubFolders(item, folderList);
                }
            }
        }

        private void TreeView_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                object data = e.Data.GetData(DataFormats.FileDrop);
                string[] files = (string[])data;
                PrepareUpload(files);
                e.Handled = true;
            }
            model.IsDragging = false;
        }

        private void TreeView_DragOver(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            bool hasAnyFile = files.Any(f => System.IO.File.Exists(f));
            if (!e.Data.GetDataPresent(DataFormats.FileDrop) || !hasAnyFile || model.GetConnectionModel() == null)
            {
                e.Effects = DragDropEffects.None;
            } else
            {
                e.Effects = DragDropEffects.Copy;
            }
            e.Handled = true;
        }

        private void TreeView_DragEnter(object sender, DragEventArgs e)
        {
            if (model.GetConnectionModel() == null)
            {
                MainViewModel.Instanz.ShowError("FileManager Error", "Es wurde keine Verbindung zu einem Gateway hergestellt.");
                e.Effects = DragDropEffects.None;
                model.IsDragging = true;
                return;
            }

            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            bool hasAnyFile = files.Any(f => System.IO.File.Exists(f));
            if (!hasAnyFile)
            {
                MainViewModel.Instanz.ShowError("FileManager Error", "Es können nur Dateien übertragen werden.");
                e.Effects = DragDropEffects.None;
                model.IsDragging = true;
                return;
            }

            model.IsDragging = true;
        }

        private void TreeView_DragLeave(object sender, DragEventArgs e)
        {
            model.IsDragging = false;
        }
    }
}
