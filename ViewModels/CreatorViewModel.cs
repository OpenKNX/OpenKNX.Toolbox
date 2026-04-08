using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform;
using Avalonia.Platform.Storage;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using OpenKNX.Toolbox.Lib.Data;
using OpenKNX.Toolbox.Lib.Helper;
using OpenKNX.Toolbox.Lib.Models;
using OpenKNX.Toolbox.Lib.Platforms;
using OpenKNX.Toolbox.Localization;
using OpenKNX.Toolbox.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace OpenKNX.Toolbox.ViewModels;

public partial class CreatorViewModel : ViewModelBase, INotifyPropertyChanged
{
    #region Properties
    public ReleaseContentModel? ReleaseContent { get; set;}
    public ObservableCollection<Repository> Repos { get; set; } = new ();
    public ObservableCollection<ReleaseContentModel> LocalReleases { get; set; } = new ();
    public ObservableCollection<PlatformDevice> PlatformDevices { get; set; } = new ();

    public bool CanSelectRepo
    {
        get { return Repos.Count > 0 && !_isDownloading && !_isUpdating; }
    }

    private Repository? _selectedRepository;
    public Repository? SelectedRepository
    {
        get { return _selectedRepository; }
        set { 
            _selectedRepository = value;
            NotifyPropertyChanged("SelectedRepository");
            NotifyPropertyChanged("CanSelectRelease");
            CheckReleases();
            CheckOpenBrowser();
        }
    }

    private Release? _selectedRelease;
    public Release? SelectedRelease
    {
        get { return _selectedRelease; }
        set { 
            _selectedRelease = value;
            NotifyPropertyChanged("SelectedRelease");
            NotifyPropertyChanged("CanDownloadRelease");
            CheckOpenBrowser();
        }
    }

    public bool CanSelectRelease
    {
        get { return _selectedRepository != null && !_isDownloading && !_isUpdating; }
    }

    public bool CanDownloadRelease
    {
        get { return _selectedRelease != null && !_isDownloading && !_isUpdating; }
    }

    public bool CanUpdateRepos
    {
        get { return !_isUpdating && !_isDownloading; }
    }

    private bool _isDownloading = false;
    public bool IsDownloading
    {
        get { return _isDownloading; }
        set {
            _isDownloading = value;
            NotifyPropertyChanged("CanSelectRepo");
            NotifyPropertyChanged("CanSelectRelease");
            NotifyPropertyChanged("CanUpdateRepos");
            NotifyPropertyChanged("CanDownloadRelease");
        }
    }

    private bool _isUpdating = false;
    public bool IsUpdating
    {
        get { return _isUpdating; }
        set {
            _isUpdating = value;
            NotifyPropertyChanged("CanSelectRepo");
            NotifyPropertyChanged("CanSelectRelease");
            NotifyPropertyChanged("CanUpdateRepos");
            NotifyPropertyChanged("CanDownloadRelease");
        }
    }

    private bool _canStep2 = false;
    public bool CanStep2
    {
        get { return _canStep2; }
        set {
            _canStep2 = value;
            NotifyPropertyChanged("CanStep2");
        }
    }

    private bool _showPrereleases = false;
    public bool ShowPrereleases
    {
        get { return _showPrereleases; }
        set {
            _showPrereleases = value;
            NotifyPropertyChanged("ShowPrereleases");
            FilterReleases();
            CheckReleases();
        }
    }

    private Product? _selectedProduct { get; set; }
    public Product? SelectedProduct
    {
        get { return _selectedProduct; }
        set { 
            if(value == null) return;
            _selectedProduct = value;
            NotifyPropertyChanged("SelectedProduct");
            _ = UpdatePlatformDevices();
            CanStep2 = true;
        }
    }

    private PlatformDevice? _selectedPlatformDevice { get; set; }
    public PlatformDevice? SelectedPlatformDevice
    {
        get { return _selectedPlatformDevice; }
        set { 
            _selectedPlatformDevice = value;
            NotifyPropertyChanged("SelectedPlatformDevice");
            CanUploadFirmware = value != null;
        }
    }

    private bool _canUploadFirmware = false;
    public bool CanUploadFirmware
    {
        get { return _canUploadFirmware; }
        set {
            _canUploadFirmware = value;
            NotifyPropertyChanged("CanUploadFirmware");
        }
    }

    private bool _updateProgressIsIndeterminate = false;
    public bool UpdateProgressIsIndeterminate
    {
        get { return _updateProgressIsIndeterminate; }
        set {
            _updateProgressIsIndeterminate = value;
            NotifyPropertyChanged("UpdateProgressIsIndeterminate");
        }
    }

    private int _updateProgress = 0;
    public int UpdateProgress
    {
        get { return _updateProgress; }
        set {
            _updateProgress = value;
            NotifyPropertyChanged("UpdateProgress");
        }
    }

    private bool _uploadProgressIsIndeterminate = false;
    public bool UploadProgressIsIndeterminate
    {
        get { return _uploadProgressIsIndeterminate; }
        set {
            _uploadProgressIsIndeterminate = value;
            NotifyPropertyChanged("UploadProgressIsIndeterminate");
        }
    }

    private int _uploadProgress = 0;
    public int UploadProgress
    {
        get { return _uploadProgress; }
        set {
            _uploadProgress = value;
            NotifyPropertyChanged("UploadProgress");
        }
    }

    private string _releasePlaceHolder = Localizer.Instance.Strings.SelectRelease;
    public string ReleasePlaceHolder
    {
        get { return _releasePlaceHolder; }
        set {
            _releasePlaceHolder = value;
            NotifyPropertyChanged("ReleasePlaceHolder");
        }
    }

    private string _openInBrowserText = Localizer.Instance.Strings.OpenRepoInBrowser;
    public string OpenInBrowserText
    {
        get { return _openInBrowserText; }
        set {
            _openInBrowserText = value;
            NotifyPropertyChanged("OpenInBrowserText");
        }
    }

    #endregion

    public CreatorViewModel()
    {
        Localizer.Instance.LanguageChanged += OnLanguageChanged;
        _ = LoadCache();
    }

    private void OnLanguageChanged()
    {
        if (_selectedRepository != null)
        {
            CheckReleases();
            CheckOpenBrowser();
        }
        else
        {
            ReleasePlaceHolder = Localizer.Instance.Strings.SelectRelease;
            OpenInBrowserText = Localizer.Instance.Strings.OpenRepoInBrowser;
        }
    }

    private async Task LoadCache()
    {
        await Task.Delay(1000);
        
        if(!Directory.Exists(GetStoragePath()))
            Directory.CreateDirectory(GetStoragePath());

        string cache = Path.Combine(GetStoragePath(), "cache.json");
        if(File.Exists(cache))
        {
            try {
                cache = File.ReadAllText(cache);
                var repos = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Repository>>(cache) ?? new List<Repository>();
                Repos.Clear();
                foreach(Repository repo in repos)
                    Repos.Add(repo);
                FilterReleases();
                NotifyPropertyChanged("CanSelectRepo");
            } catch(Exception ex) {
                var box = MessageBoxManager.GetMessageBoxStandard(Localizer.Instance.Strings.Error, Localizer.Instance.Strings.ErrorLoadingReposCache + "\r\n\r\n" + GetExceptionMessages(ex), ButtonEnum.Ok, Icon.Error);
                await box.ShowWindowDialogAsync(MainWindow.Instance);
            }
        }

        foreach(string folder in Directory.GetDirectories(GetStoragePath()))
        {
            try {
                if(!File.Exists(Path.Combine(folder, "cache.json")))
                    continue;
                cache = File.ReadAllText(Path.Combine(folder, "cache.json"));
                var model = Newtonsoft.Json.JsonConvert.DeserializeObject<ReleaseContentModel>(cache);
                if(model == null) continue;
                foreach(Product prod in model.Products)
                    prod.ReleaseContent = model;
                LocalReleases.Add(model);
            } catch(Exception ex) {
                string folderName = folder;
                folderName = folderName.Substring(folderName.LastIndexOf(Path.DirectorySeparatorChar) + 1);
                var box = MessageBoxManager.GetMessageBoxStandard(Localizer.Instance.Strings.Error, string.Format(Localizer.Instance.Strings.ErrorLoadingRepoCache, folderName) + "\r\n\r\n" + GetExceptionMessages(ex), ButtonEnum.Ok, Icon.Error);
                await box.ShowWindowDialogAsync(MainWindow.Instance);
            }
            LocalReleases.Sort((a, b) => string.Compare(a.RepositoryName, b.RepositoryName));
        }
    }

    private void FilterReleases()
    {
        foreach(Repository repo in Repos)
        {
            if(repo.Releases.Count == 0)
            {
                foreach(Release rel in repo.ReleasesAll)
                    repo.Releases.Add(rel);
            }

            if(ShowPrereleases)
            {
                foreach(Release rel in repo.ReleasesAll.Where(r => r.IsPrerelease))
                {
                    if(!repo.Releases.Contains(rel))
                        repo.Releases.Insert(repo.ReleasesAll.IndexOf(rel), rel);
                }
            } else {
                foreach(Release rel in repo.ReleasesAll.Where(r => r.IsPrerelease))
                {
                    if(repo.Releases.Contains(rel))
                        repo.Releases.Remove(rel);
                }
            }
        }
    }

    public async Task DownloadRelease()
    {
        System.Console.WriteLine("Downloading Release: " + SelectedRelease?.Name);
        IsDownloading = true;
        string targetPath = "";
        string targetFolder = "";

        try {
            if(SelectedRelease == null || SelectedRepository == null)
                throw new Exception(Localizer.Instance.Strings.NoReleaseOrRepoSelected);

            string current = GetStoragePath();
            if(!Directory.Exists(current))
                Directory.CreateDirectory(current);

            targetPath = Path.Combine(current, "download.zip");
            if(File.Exists(targetPath))
                File.Delete(targetPath);
            Console.WriteLine("Save Release in " + targetPath);
            
            var progress = new Progress<KeyValuePair<long, long>>();
            progress.ProgressChanged += ProgressChanged_UpdateRepos;
            await GitHubAccess.DownloadRepo(SelectedRelease.Url, targetPath, progress);
            
            targetFolder = Path.Combine(current, SelectedRelease.Name.Substring(0, SelectedRelease.Name.LastIndexOf('.')));
            if(Directory.Exists(targetFolder))
            {
                var box = MessageBoxManager.GetMessageBoxStandard(Localizer.Instance.Strings.Warning, string.Format(Localizer.Instance.Strings.ReleaseExistsOverwrite, SelectedRelease.Name), ButtonEnum.YesNo, Icon.Warning);
                var result = await box.ShowWindowDialogAsync(MainWindow.Instance);
                if(result == ButtonResult.No) {
                    IsDownloading = false;
                    return;
                }
                Directory.Delete(targetFolder, true);
                Directory.CreateDirectory(targetFolder);
            }
            System.IO.Compression.ZipFile.ExtractToDirectory(targetPath, targetFolder);

            if(File.Exists(targetPath))
                File.Delete(targetPath);

            ReleaseContentModel content = ReleaseContentHelper.GetReleaseContent(targetFolder);
            content.RepositoryName = SelectedRepository.Name;
            content.ReleaseName = SelectedRelease?.Name ?? "";
            content.IsPrerelease = SelectedRelease?.IsPrerelease ?? false;
            content.Published = SelectedRelease?.Published ?? DateTime.Now;
            content.Version = $"v{SelectedRelease?.Version}";
            
            File.WriteAllText(Path.Combine(targetFolder, "cache.json"), Newtonsoft.Json.JsonConvert.SerializeObject(content));
            LocalReleases.Add(content);
            //LocalReleases.Sort((a, b) => a.RepositoryName.ComareTo(b.RepositoryName));
        } catch(Exception ex)
        {
            var box = MessageBoxManager.GetMessageBoxStandard(Localizer.Instance.Strings.Error, Localizer.Instance.Strings.ErrorDownloadingRepo + "\r\n\r\n" + GetExceptionMessages(ex), ButtonEnum.Ok, Icon.Error);
            await box.ShowWindowDialogAsync(MainWindow.Instance);
        }

        IsDownloading = false;
    }

    public void CheckReleases()
    {
        if(_selectedRepository != null)
        {
            if(_selectedRepository.Releases.Count == 0 && _selectedRepository.ReleasesAll.Count > 0)
            {
                ReleasePlaceHolder = Localizer.Instance.Strings.OnlyPrereleasesAvailable;
            }
            else if(_selectedRepository.ReleasesAll.Count == 0)
            {
                ReleasePlaceHolder = Localizer.Instance.Strings.NoReleasesAvailable;
            }
            else
            {
                ReleasePlaceHolder = Localizer.Instance.Strings.SelectRelease;
            }
        }
    }

    public void CheckOpenBrowser()
    {
        if(SelectedRepository == null)
            return;

        if(SelectedRelease != null)
        {
            OpenInBrowserText = Localizer.Instance.Strings.OpenReleaseNotes;
        }
        else
        {
            OpenInBrowserText = Localizer.Instance.Strings.OpenRepository;
        }
    }

    public async Task UpdateRepos()
    {
        System.Console.WriteLine("Updating Repos");
        IsUpdating = true;
        UpdateProgressIsIndeterminate = true;
        try {
            var progress = new Progress<KeyValuePair<long, long>>();
            progress.ProgressChanged += ProgressChanged_UpdateRepos;
            var x = await GitHubAccess.GetOpenKnxRepositoriesAsync(progress);
            Repos.Clear();
            foreach(Repository repo in x)
                Repos.Add(repo);
            NotifyPropertyChanged("CanSelectRepo");
            File.WriteAllText(Path.Combine(GetStoragePath(), "cache.json"), Newtonsoft.Json.JsonConvert.SerializeObject(Repos));
        } catch(Octokit.RateLimitExceededException ex) {
            System.Console.WriteLine("Raitlimit Exceeded " + ex.Message);
            var box = MessageBoxManager.GetMessageBoxStandard(Localizer.Instance.Strings.Error, Localizer.Instance.Strings.RateLimitExceeded, ButtonEnum.Ok, Icon.Error);
            await box.ShowWindowDialogAsync(MainWindow.Instance);
        } catch(Exception ex) {
            System.Console.WriteLine("Failed to update Repos: " + ex.Message);
            var box = MessageBoxManager.GetMessageBoxStandard(Localizer.Instance.Strings.Error, Localizer.Instance.Strings.ErrorUpdatingRepos + "\r\n\r\n" + GetExceptionMessages(ex), ButtonEnum.Ok, Icon.Error);
            await box.ShowWindowDialogAsync(MainWindow.Instance);
        }
        FilterReleases();
        IsUpdating = false;
        UpdateProgressIsIndeterminate = false;
    }

    private void ProgressChanged_UpdateRepos(object? sender, KeyValuePair<long, long> e)
    {
        if(e.Value == 0) {
            UpdateProgress = 0;
            UpdateProgressIsIndeterminate = true;
        } else {
            UpdateProgressIsIndeterminate = false;
            UpdateProgress = (int)(e.Key * 100.0 / e.Value);
        }
        Console.WriteLine($"Progress: {e.Key} von {e.Value} ({UpdateProgress}%)");
    }

    private void ProgressChanged_UpdateUpload(object? sender, KeyValuePair<long, long> e)
    {
        if(e.Value == 0) {
            UploadProgress = 0;
            UploadProgressIsIndeterminate = true;
        } else {
            UploadProgressIsIndeterminate = false;
            int newI = (int)(e.Key * 100.0 / e.Value);
            if(newI != UploadProgress)
            {
                Console.WriteLine($"Progress: {e.Key} von {e.Value} ({UploadProgress}%)");
                UploadProgress = newI;
            }
        }
    }

    public void DeleteRelease(ReleaseContentModel model)
    {
        System.Console.WriteLine($"Delete Release {model.RepositoryName} {model.ReleaseName}");

        string name = model.ReleaseName;
        name = name.Substring(0, name.LastIndexOf('.'));
        string path = Path.Combine(GetStoragePath(), name);
        Directory.Delete(path, true);
        LocalReleases.Remove(model);
    }

    public async Task CreateKnxProd()
    {
        Console.WriteLine("creating knxprod");
        UploadProgressIsIndeterminate = true;
        try {
            if(SelectedProduct == null || SelectedProduct.ReleaseContent == null)
                throw new Exception(Localizer.Instance.Strings.NoProductOrReleaseContent);

            if (Application.Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop ||
                desktop.MainWindow?.StorageProvider is not { } provider)
                throw new NullReferenceException("Missing StorageProvider instance.");

            string defaultName = SelectedProduct.ReleaseContent?.ReleaseName ?? "default";
            defaultName = defaultName.Substring(0, defaultName.LastIndexOf('.'));

            var file = await provider.SaveFilePickerAsync(new FilePickerSaveOptions
            {
                Title = Localizer.Instance.Strings.SaveKnxProd,
                SuggestedFileName = defaultName,
                FileTypeChoices = new[] { new FilePickerFileType(Localizer.Instance.Strings.KnxProductDatabase)
                {
                    Patterns = new[] { "*.knxprod" }
                }}
            });

            if (file is not null)
            {
                string outpuFolder = Path.Combine(GetStoragePath(), "Temp");
                string xmlFile = SelectedProduct.ReleaseContent?.XmlFile ?? "";
                string outFile = file.Path.AbsolutePath.Replace("%20", " ");
                if(!outFile.EndsWith(".knxprod"))
                    outFile += ".knxprod";
                if(File.Exists(outFile))
                    File.Delete(outFile);
                string workingDir = GetAbsWorkingDir(xmlFile);
                await Toolbox.Sign.SignHelper.ExportKnxprodAsync(workingDir, outFile, xmlFile, "", false, false);
                var box = MessageBoxManager.GetMessageBoxStandard(Localizer.Instance.Strings.Success, Localizer.Instance.Strings.KnxProdCreatedSuccessfully, ButtonEnum.Ok, Icon.Info);
                await box.ShowWindowDialogAsync(MainWindow.Instance);
            }
        } catch(Exception ex) {
            var box = MessageBoxManager.GetMessageBoxStandard(Localizer.Instance.Strings.Error, Localizer.Instance.Strings.ErrorCreatingKnxProd + "\r\n\r\n" + GetExceptionMessages(ex), ButtonEnum.Ok, Icon.Error);
            await box.ShowWindowDialogAsync(MainWindow.Instance);
        }
        UploadProgressIsIndeterminate = false;
    }

    public async Task ImportZip()
    {
        if (Application.Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop ||
            desktop.MainWindow?.StorageProvider is not { } provider)
            throw new NullReferenceException("Missing StorageProvider instance.");

        var files = await provider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = Localizer.Instance.Strings.OpenReleaseFile,
            AllowMultiple = false
        });

        if (files.Count >= 1)
        {
            // Open reading stream from the first file.
            string outputPath = files[0].Path.AbsolutePath;
            string fileName = Path.GetFileName(outputPath);
            fileName = fileName.Substring(0, fileName.LastIndexOf('.'));
            outputPath = Path.Combine(GetStoragePath(), fileName);
            if(!Directory.Exists(outputPath))
                Directory.CreateDirectory(outputPath);
            System.IO.Compression.ZipFile.ExtractToDirectory(files[0].Path.AbsolutePath.Replace("%20", " "), outputPath);

            Regex regex = new Regex("([0-9]+).([0-9]+).([0-9]+)");
            Match m = regex.Match(fileName);
            int major = 0, minor = 0, build = 0;
            if(m.Success) 
            {
                major = int.Parse(m.Groups[1].Value);
                minor = int.Parse(m.Groups[2].Value);
                build = int.Parse(m.Groups[3].Value);
            } else 
            {
                regex = new Regex("([0-9]+).([0-9]+)");
                m = regex.Match(fileName);
                if(m.Success)
                {
                    major = int.Parse(m.Groups[1].Value);
                    minor = int.Parse(m.Groups[2].Value);
                } else {

                }
            }

            ReleaseContentModel content = ReleaseContentHelper.GetReleaseContent(outputPath);
            content.RepositoryName = fileName.Substring(0, fileName.LastIndexOf('-'));;
            content.ReleaseName = Path.GetFileName(files[0].Path.AbsolutePath);
            // content.IsPrerelease = SelectedRelease.IsPrerelease;
            // content.Published = SelectedRelease.Published;
            content.Version = $"v{major}.{minor}.{build}";
            
            File.WriteAllText(Path.Combine(outputPath, "cache.json"), Newtonsoft.Json.JsonConvert.SerializeObject(content));
            LocalReleases.Add(content);
        }
    }
    
    public async Task UpdatePlatformDevices()
    {
        UploadProgressIsIndeterminate = true;
        try
        {
            if(SelectedProduct == null)
                throw new Exception(Localizer.Instance.Strings.NoProductSelected);

            PlatformDevices.Clear();
            IPlatform? platform = null;
            foreach(IPlatform plat in OpenKNX.Toolbox.Lib.Helper.PlatformHelper.GetPlatforms())
                if(plat.Architecture == SelectedProduct.Architecture)
                    platform = plat;
            
            if(platform == null) throw new Exception(string.Format(Localizer.Instance.Strings.NoPlatformForArchitecture, SelectedProduct.Architecture));
            var devices = await platform.GetDevices();
            foreach(var y in devices)
                PlatformDevices.Add(y);
        } catch(Exception ex) {
            var box = MessageBoxManager.GetMessageBoxStandard(Localizer.Instance.Strings.Error, Localizer.Instance.Strings.ErrorUpdatingList + "\r\n\r\n" + GetExceptionMessages(ex), ButtonEnum.Ok, Icon.Error);
            await box.ShowWindowDialogAsync(MainWindow.Instance);
        }
        UploadProgressIsIndeterminate = false;
    }

    public async Task UploadFirmware()
    {
        Console.WriteLine("uploading");
        UploadProgressIsIndeterminate = true;
        try
        {
            if(SelectedProduct == null || SelectedPlatformDevice == null)
                throw new Exception(Localizer.Instance.Strings.NoProductOrDeviceSelected);

            IPlatform? platform = null;
            foreach(IPlatform plat in OpenKNX.Toolbox.Lib.Helper.PlatformHelper.GetPlatforms())
                if(plat.Architecture == SelectedProduct.Architecture)
                    platform = plat;
            
            if(platform == null) throw new Exception(string.Format(Localizer.Instance.Strings.NoPlatformForArchitecture, SelectedProduct.Architecture));
            
            var progress = new Progress<KeyValuePair<long, long>>();
            progress.ProgressChanged += ProgressChanged_UpdateUpload;
            Console.WriteLine("uploading2");
            await platform.DoUpload(SelectedPlatformDevice, SelectedProduct.FirmwareFile, progress);
            var box = MessageBoxManager.GetMessageBoxStandard(Localizer.Instance.Strings.Success, Localizer.Instance.Strings.FirmwareTransferredSuccessfully, ButtonEnum.Ok, Icon.Success);
            await box.ShowWindowDialogAsync(MainWindow.Instance);
        } catch(Exception ex) {
            var box = MessageBoxManager.GetMessageBoxStandard(Localizer.Instance.Strings.Error, Localizer.Instance.Strings.ErrorTransferringFirmware + "\r\n\r\n" + GetExceptionMessages(ex), ButtonEnum.Ok, Icon.Error);
            await box.ShowWindowDialogAsync(MainWindow.Instance);
        }
        UploadProgressIsIndeterminate = false;
    }

    public async Task OpenInBrowser()
    {
        if(SelectedRepository == null)
        {
            var box = MessageBoxManager.GetMessageBoxStandard(Localizer.Instance.Strings.Error, Localizer.Instance.Strings.SelectProjectFirst, ButtonEnum.Ok, Icon.Error);
            await box.ShowWindowDialogAsync(MainWindow.Instance);
            return;
        }

        string url = $"https://github.com/OpenKNX/{SelectedRepository.Name}";
        if(SelectedRelease != null)
        {
            url = SelectedRelease.UrlRelease;
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            using var proc = new Process { StartInfo = { UseShellExecute = true, FileName = url } };
            proc.Start();
            return;
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            Process.Start("x-www-browser", url);
            return;
        }
    }

    private static string GetAbsWorkingDir(string iFilename)
    {
        string lResult = Path.GetFullPath(iFilename);
        lResult = Path.GetDirectoryName(lResult) ?? "";
        return lResult;
    }

    private string GetExceptionMessages(Exception ex)
    {
        string message = "";
        Exception? current = ex;
        while(current != null)
        {
            message += ex.GetType().ToString() + "\r\n";
            if(!string.IsNullOrEmpty(current.Message))
                message += current.Message +  "\r\n";
            current = current.InnerException;
        }

        message += ex.StackTrace;

        return message;
    }

    private string GetStoragePath()
    {
        return Path.Combine(Directory.GetCurrentDirectory(), "Storage");
    }

    public new event PropertyChangedEventHandler? PropertyChanged; 
    private void NotifyPropertyChanged(string propertyName = "")  
    {  
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }  
}