using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace OpenKNX.Toolbox.Localization;

public abstract class LanguageBase
{
    public abstract string Code { get; }
    public abstract string DisplayName { get; }

    // MainWindow
    public abstract string SoftFirmware { get; }
    public abstract string Language { get; }

    // Creator.axaml
    public abstract string AddRelease { get; }
    public abstract string Projects { get; }
    public abstract string SelectProject { get; }
    public abstract string Release { get; }
    public abstract string Refresh { get; }
    public abstract string Download { get; }
    public abstract string ImportZip { get; }
    public abstract string ShowPrereleases { get; }
    public abstract string Selected { get; }
    public abstract string CreateKnxProd { get; }
    public abstract string FlashRelease { get; }
    public abstract string RefreshList { get; }
    public abstract string FlashFirmware { get; }
    public abstract string LocalReleases { get; }

    // Filetransfer + Terminal
    public abstract string OpenPutty { get; }

    // CreatorViewModel
    public abstract string SelectRelease { get; }
    public abstract string OpenRepoInBrowser { get; }
    public abstract string Error { get; }
    public abstract string Warning { get; }
    public abstract string ErrorLoadingReposCache { get; }
    public abstract string ErrorLoadingRepoCache { get; }
    public abstract string ReleaseExistsOverwrite { get; }
    public abstract string ErrorDownloadingRepo { get; }
    public abstract string OnlyPrereleasesAvailable { get; }
    public abstract string NoReleasesAvailable { get; }
    public abstract string OpenReleaseNotes { get; }
    public abstract string OpenRepository { get; }
    public abstract string RateLimitExceeded { get; }
    public abstract string ErrorUpdatingRepos { get; }
    public abstract string NoReleaseOrRepoSelected { get; }
    public abstract string SaveKnxProd { get; }
    public abstract string KnxProductDatabase { get; }
    public abstract string Success { get; }
    public abstract string KnxProdCreatedSuccessfully { get; }
    public abstract string ErrorCreatingKnxProd { get; }
    public abstract string OpenReleaseFile { get; }
    public abstract string NoProductSelected { get; }
    public abstract string NoProductOrReleaseContent { get; }
    public abstract string NoPlatformForArchitecture { get; }
    public abstract string ErrorUpdatingList { get; }
    public abstract string NoProductOrDeviceSelected { get; }
    public abstract string FirmwareTransferredSuccessfully { get; }
    public abstract string ErrorTransferringFirmware { get; }
    public abstract string SelectProjectFirst { get; }

    // TerminalViewModel
    public abstract string ErrorStartingPutty { get; }

    private Dictionary<string, string>? _cache;

    public Dictionary<string, string> ToDictionary()
    {
        if (_cache != null) return _cache;

        _cache = GetType()
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.PropertyType == typeof(string) && p.Name != nameof(Code) && p.Name != nameof(DisplayName))
            .ToDictionary(p => p.Name, p => (string)(p.GetValue(this) ?? ""));

        return _cache;
    }
}
