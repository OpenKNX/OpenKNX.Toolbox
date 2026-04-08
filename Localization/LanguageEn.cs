namespace OpenKNX.Toolbox.Localization;

public class LanguageEn : LanguageBase
{
    public override string Code => "en";
    public override string DisplayName => "English";

    // MainWindow
    public override string SoftFirmware => "Soft-/Firmware";
    public override string Language => "Language";

    // Creator.axaml
    public override string AddRelease => "Add Release";
    public override string Projects => "Projects";
    public override string SelectProject => "Select project";
    public override string Release => "Release";
    public override string Refresh => "Refresh";
    public override string Download => "Download";
    public override string ImportZip => "Import Zip";
    public override string ShowPrereleases => "Show pre-releases in projects";
    public override string Selected => "Selected";
    public override string CreateKnxProd => "Create KnxProd";
    public override string FlashRelease => "Flash Release";
    public override string RefreshList => "Refresh List";
    public override string FlashFirmware => "Flash Firmware";
    public override string LocalReleases => "Local Releases";

    // Filetransfer + Terminal
    public override string OpenPutty => "Open Putty";

    // CreatorViewModel
    public override string SelectRelease => "Select release";
    public override string OpenRepoInBrowser => "Open Repo in Browser";
    public override string Error => "Error";
    public override string Warning => "Warning";
    public override string ErrorLoadingReposCache => "Could not load the local file for repos:";
    public override string ErrorLoadingRepoCache => "Could not load the local file for repo '{0}':";
    public override string ReleaseExistsOverwrite => "The release '{0}' already exists locally.\r\nDo you want to overwrite it?";
    public override string ErrorDownloadingRepo => "Could not download the repository:";
    public override string OnlyPrereleasesAvailable => "Only pre-releases available";
    public override string NoReleasesAvailable => "No releases available";
    public override string OpenReleaseNotes => "Open Release Notes";
    public override string OpenRepository => "Open Repository";
    public override string RateLimitExceeded => "GitHub rate limit exceeded.\r\nIt will reset in one hour.\r\nPlease try again later.";
    public override string ErrorUpdatingRepos => "Could not update the repositories:";
    public override string NoReleaseOrRepoSelected => "No release or repository selected.";
    public override string SaveKnxProd => "Save KnxProd";
    public override string KnxProductDatabase => "KNX Product Database";
    public override string Success => "Success";
    public override string KnxProdCreatedSuccessfully => "KnxProd created successfully.";
    public override string ErrorCreatingKnxProd => "Could not create the KnxProd:";
    public override string OpenReleaseFile => "Open Release File";
    public override string NoProductSelected => "No product selected.";
    public override string NoProductOrReleaseContent => "No product selected or ReleaseContent is null.";
    public override string NoPlatformForArchitecture => "Could not find a platform for architecture {0}";
    public override string ErrorUpdatingList => "Could not update the list:";
    public override string NoProductOrDeviceSelected => "No product or device selected.";
    public override string FirmwareTransferredSuccessfully => "Firmware transferred successfully.";
    public override string ErrorTransferringFirmware => "Could not transfer the firmware:";
    public override string SelectProjectFirst => "Please select a project first.";

    // TerminalViewModel
    public override string ErrorStartingPutty => "Could not start Putty:";
}
