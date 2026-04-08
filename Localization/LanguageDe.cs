namespace OpenKNX.Toolbox.Localization;

public class LanguageDe : LanguageBase
{
    public override string Code => "de";
    public override string DisplayName => "Deutsch";

    // MainWindow
    public override string SoftFirmware => "Soft-/Firmware";
    public override string Language => "Sprache";

    // Creator.axaml
    public override string AddRelease => "Release hinzufügen";
    public override string Projects => "Projekte";
    public override string SelectProject => "Projekt auswählen";
    public override string Release => "Release";
    public override string Refresh => "Aktualisieren";
    public override string Download => "Herunterladen";
    public override string ImportZip => "Zip importieren";
    public override string ShowPrereleases => "Pre-Releases in Projekten anzeigen";
    public override string Selected => "Ausgewählt";
    public override string CreateKnxProd => "KnxProd erstellen";
    public override string FlashRelease => "Release aufspielen";
    public override string RefreshList => "Liste aktualisieren";
    public override string FlashFirmware => "Firmware aufspielen";
    public override string LocalReleases => "Lokale Releases";

    // Filetransfer + Terminal
    public override string OpenPutty => "Putty öffnen";

    // CreatorViewModel
    public override string SelectRelease => "Release auswählen";
    public override string OpenRepoInBrowser => "Repo in Browser öffnen";
    public override string Error => "Fehler";
    public override string Warning => "Warnung";
    public override string ErrorLoadingReposCache => "Die lokale Datei für die Repos konnte nicht geladen werden:";
    public override string ErrorLoadingRepoCache => "Die lokale Datei für das Repo '{0}' konnte nicht geladen werden:";
    public override string ReleaseExistsOverwrite => "Das Release '{0}' existiert bereits lokal.\r\nSoll es überschrieben werden?";
    public override string ErrorDownloadingRepo => "Das Repository konnte nicht heruntergeladen werden:";
    public override string OnlyPrereleasesAvailable => "Nur Prereleases verfügbar";
    public override string NoReleasesAvailable => "Keine Releases verfügbar";
    public override string OpenReleaseNotes => "Release-Notes öffnen";
    public override string OpenRepository => "Repository öffnen";
    public override string RateLimitExceeded => "Das Ratelimit für Github wurde überschritten.\r\nDieser wird in einer Stunde zurückgesetzt.\r\nVersuchen Sie es dann erneut.";
    public override string ErrorUpdatingRepos => "Die Repositories konnten nicht aktualisiert werden:";
    public override string NoReleaseOrRepoSelected => "Es wurde kein Release oder Repository ausgewählt.";
    public override string SaveKnxProd => "Speichere KnxProd";
    public override string KnxProductDatabase => "Knx Produkt Datenbank";
    public override string Success => "Erfolgreich";
    public override string KnxProdCreatedSuccessfully => "Die KnxProd wurde erfolgreich erstellt.";
    public override string ErrorCreatingKnxProd => "Die KnxProd konnte nicht erstellt werden:";
    public override string OpenReleaseFile => "Öffne Release Datei";
    public override string NoProductSelected => "Es wurde kein Produkt ausgewählt.";
    public override string NoProductOrReleaseContent => "Es wurde kein Produkt ausgewählt oder ReleaseContent ist null.";
    public override string NoPlatformForArchitecture => "Es konnte keine Platform für Architectur {0} gefunden werden";
    public override string ErrorUpdatingList => "Die Liste konnte nicht aktualisiert werden:";
    public override string NoProductOrDeviceSelected => "Es wurde kein Produkt oder Gerät ausgewählt.";
    public override string FirmwareTransferredSuccessfully => "Die Firmware wurde erfolgreich übertragen.";
    public override string ErrorTransferringFirmware => "Die Firmware konnte nicht übertragen werden:";
    public override string SelectProjectFirst => "Bitte wählen Sie zuerst ein Projekt aus.";

    // TerminalViewModel
    public override string ErrorStartingPutty => "Putty konnte nicht gestartet werden:";
}
