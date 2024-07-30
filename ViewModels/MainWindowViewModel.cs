﻿using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OpenKNX.Toolbox.Lib.Data;
using OpenKNX.Toolbox.Lib.Helper;
using OpenKNX.Toolbox.Lib.Models;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace OpenKNX.Toolkit.ViewModels;

public partial class MainWindowViewModel : ViewModelBase, INotifyPropertyChanged
{
    #region Properties
    public ReleaseContentModel ReleaseContent { get; set;}
    public ObservableCollection<Repository> Repos { get; set; } = new ();
    public ObservableCollection<ReleaseContentModel> LocalReleases { get; set; } = new ();

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
        }
    }

    private Release? _selectedRelease;
    public Release? SelectedRelease
    {
        get { return _selectedRelease; }
        set { 
            _selectedRelease = value;
            NotifyPropertyChanged("SelectedRelease");
            NotifyPropertyChanged("CanDownload");
        }
    }

    public bool CanSelectRelease
    {
        get { return _selectedRepository != null && !_isDownloading && !_isUpdating; }
    }

    public bool CanDownload
    {
        get { return _selectedRelease != null && !_isDownloading && !_isUpdating; }
    }

    public bool CanUpdate
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
            NotifyPropertyChanged("CanDownload");
            NotifyPropertyChanged("CanUpdate");
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
            NotifyPropertyChanged("CanDownload");
            NotifyPropertyChanged("CanUpdate");
        }
    }

    private bool _showPrereleases = false;
    public bool ShowPrereleases
    {
        get { return _showPrereleases; }
        set {
            _showPrereleases = value;
            NotifyPropertyChanged("ShowPrereleases");
        }
    }

    #endregion



    public MainWindowViewModel()
    {
        string cache = Path.Combine(GetStoragePath(), "cache.json");
        if(File.Exists(cache))
        {
            cache = File.ReadAllText(cache);
            var repos = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Repository>>(cache);
            Repos.Clear();
            foreach(Repository repo in repos)
                Repos.Add(repo);
            NotifyPropertyChanged("CanSelectRepo");
        }

        if(Directory.Exists(GetStoragePath()))
        {
            foreach(string folder in Directory.GetDirectories(GetStoragePath()))
            {
                if(!File.Exists(Path.Combine(folder, "cache.json")))
                    continue;
                cache = File.ReadAllText(Path.Combine(folder, "cache.json"));
                var model = Newtonsoft.Json.JsonConvert.DeserializeObject<ReleaseContentModel>(cache);
                LocalReleases.Add(model);
                //LocalReleases.Sort((a, b) => a.RepositoryName.ComareTo(b.RepositoryName));
            }
        }
    }

    [RelayCommand]
    public async Task DownloadRelease()
    {
        System.Console.WriteLine("Downloading Release: " + SelectedRelease.Name);
        IsDownloading = true;
        string targetPath = "";
        string targetFolder = "";

        try {
            string current = GetStoragePath();
            if(!Directory.Exists(current))
                Directory.CreateDirectory(current);

            targetPath = Path.Combine(current, "download.zip");
            if(File.Exists(targetPath))
                File.Delete(targetPath);
            Console.WriteLine("Save Release in " + targetPath);
            
            await GitHubAccess.DownloadRepo(SelectedRelease.Url, targetPath);
            
            targetFolder = Path.Combine(current, SelectedRelease.Name.Substring(0, SelectedRelease.Name.LastIndexOf('.')));
            if(Directory.Exists(targetFolder))
            {
                Directory.Delete(targetFolder, true);
                Directory.CreateDirectory(targetFolder);
            }
            System.IO.Compression.ZipFile.ExtractToDirectory(targetPath, targetFolder);

            if(File.Exists(targetPath))
                File.Delete(targetPath);

            ReleaseContentModel content = ReleaseContentHelper.GetReleaseContent(Path.Combine(targetFolder, "data"));
            content.RepositoryName = SelectedRepository.Name;
            content.ReleaseName = SelectedRelease.Name;
            content.IsPrerelease = SelectedRelease.IsPrerelease;
            content.Published = SelectedRelease.Published;
            File.WriteAllText(Path.Combine(targetFolder, "cache.json"), Newtonsoft.Json.JsonConvert.SerializeObject(content));
            LocalReleases.Add(content);
            //LocalReleases.Sort((a, b) => a.RepositoryName.ComareTo(b.RepositoryName));
        } catch(Exception ex)
        {
            // TODO handle exception and notify user
        }

        IsDownloading = false;
    }

    [RelayCommand]
    public async Task UpdateRepos()
    {
        System.Console.WriteLine("Updating Repos");
        IsUpdating = true;
        try {
            var x = await GitHubAccess.GetOpenKnxRepositoriesAsync(ShowPrereleases);
            Repos.Clear();
            foreach(Repository repo in x)
                Repos.Add(repo);
            NotifyPropertyChanged("CanSelectRepo");
            File.WriteAllText(Path.Combine(GetStoragePath(), "cache.json"), Newtonsoft.Json.JsonConvert.SerializeObject(Repos));
        } catch(Exception ex)
        {
            System.Console.WriteLine("Failed to update Repos: " + ex.Message);
        }
        IsUpdating = false;
    }

    private string GetStoragePath()
    {
        //System.AppContext.BaseDirectory
        return Path.Combine(Directory.GetCurrentDirectory(), "Storage");
    }


    public event PropertyChangedEventHandler? PropertyChanged; 
    private void NotifyPropertyChanged(string propertyName = "")  
    {  
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }  
}
