namespace Winmizer.Core.Localization;

/// <summary>
/// Polish translations, keyed by the English source text. Strings that are intentionally identical in
/// both languages (product names etc.) are listed too, so `Winmizer.exe --lang-check` can verify coverage.
/// </summary>
internal static class PolishStrings
{
    public static readonly Dictionary<string, string> Map = Build();

    private static Dictionary<string, string> Build()
    {
        var m = new Dictionary<string, string>(StringComparer.Ordinal);
        void A(string en, string pl) => m[en] = pl;

        // ---- Labels ----
        A("Safe", "Bezpieczny");
        A("Moderate", "Umiarkowany");
        A("Advanced", "Zaawansowany");
        A("Gaming", "Gaming");
        A("Power", "Zasilanie");
        A("Privacy & telemetry", "Prywatność i telemetria");
        A("Services", "Usługi");
        A("Network", "Sieć");
        A("Appearance & UI", "Wygląd i interfejs");
        A("Storage", "Dysk");
        A("Cleanup", "Czyszczenie");
        A("App debloat", "Odśmiecanie aplikacji");
        A("OneDrive", "OneDrive");

        // ---- Core messages ----
        A("Detected {0}. Winmizer supports Windows 10 1809 (build {1}) and newer - on older versions some tweaks may not work. Use at your own risk.",
          "Wykryto {0}. Winmizer wspiera Windows 10 1809 (build {1}) i nowsze - na starszych wersjach część tweaków może nie działać. Używasz na własną odpowiedzialność.");
        A("N/A on this system (Windows 11 only)", "N/D w tym systemie (tylko Windows 11)");
        A("N/A on this system (Windows 10 only)", "N/D w tym systemie (tylko Windows 10)");
        A("N/A on this system (needs build {0}+, you have {1})", "N/D w tym systemie (wymaga builda {0}+, masz {1})");
        A("Cleanup actions cannot be undone.", "Akcji czyszczenia nie można cofnąć.");
        A("Would create a system restore point (Checkpoint-Computer).", "Utworzyłbym punkt przywracania systemu (Checkpoint-Computer).");
        A("System restore point created.", "Utworzono punkt przywracania systemu.");
        A("Already applied", "Już zastosowane");
        A("Startup: {0}", "Autostart: {0}");
        A("Cannot read backup.json: {0}", "Nie można wczytać backup.json: {0}");
        A("Writing backup.json failed: {0}", "Zapis backup.json nie powiódł się: {0}");
        A("Cannot save the backup (backup.json) - change aborted: {0}", "Nie można zapisać kopii zapasowej (backup.json) - zmiana wstrzymana: {0}");
        A("Would run: {0} {1}", "Uruchomiłbym: {0} {1}");
        A("Failed to start {0}", "Nie udało się uruchomić {0}");
        A("Timed out: {0}", "Przekroczono limit czasu: {0}");
        A("Would run PowerShell: {0}", "Uruchomiłbym PowerShell: {0}");
        A("Would set registry: {0} -> {1} = {2}", "Ustawiłbym rejestr: {0} -> {1} = {2}");
        A("Cannot open registry key {0}", "Nie można otworzyć klucza {0}");
        A("Would delete registry value: {0} -> {1}", "Usunąłbym wartość rejestru: {0} -> {1}");
        A("Service {0} is protected and cannot be changed.", "Usługa {0} jest chroniona i nie może być zmieniana.");
        A("Unsupported start type: {0}", "Nieobsługiwany typ startu: {0}");
        A("Would set service {0} to mode: {1}", "Ustawiłbym usługę {0} na tryb: {1}");
        A("sc config {0} failed: {1}", "sc config {0} nie powiodło się: {1}");
        A("Service {0} is protected.", "Usługa {0} jest chroniona.");
        A("Would stop service {0}", "Zatrzymałbym usługę {0}");
        A("Would start service {0}", "Uruchomiłbym usługę {0}");
        A("Would set powercfg {0} {1}/{2} = {3}", "Ustawiłbym powercfg {0} {1}/{2} = {3}");
        A("Would activate power plan {0}", "Aktywowałbym plan zasilania {0}");
        A("Would delete power plan {0}", "Usunąłbym plan zasilania {0}");
        A("Would set DNS on adapter '{0}': {1}", "Ustawiłbym DNS na karcie '{0}': {1}");
        A("automatic", "automatyczny");
        A("No backup - nothing to restore.", "Brak kopii zapasowej - nic do przywrócenia.");
        A("Would enable task {0}{1}", "Włączyłbym zadanie {0}{1}");
        A("Would restore fsutil {0} = {1}", "Przywróciłbym fsutil {0} = {1}");
        A("Would run powercfg -h {0}", "powercfg -h {0}");
        A("The Winmizer Ultimate plan is still active - not deleting it.", "Plan Winmizer Ultimate jest nadal aktywny - nie usuwam go.");
        A("Unknown backup entry kind: {0}", "Nieznany rodzaj kopii zapasowej: {0}");
        A("Startup folder (current user)", "Folder Autostart (użytkownik)");
        A("Startup folder (all users)", "Folder Autostart (wszyscy)");
        A("Would move {0} -> {1}", "Przeniósłbym {0} -> {1}");
        A("Moved {0} -> {1}", "Przeniesiono {0} -> {1}");
        A("Would restore {0} -> {1}", "Przywróciłbym {0} -> {1}");
        A("File to restore not found: {0}", "Brak pliku do przywrócenia: {0}");

        // ---- Tweak messages ----
        A("Would duplicate the Ultimate Performance plan, name it \"{0}\" and activate it (current: {1}).",
          "Zduplikowałbym plan Ultimate Performance, nazwał go \"{0}\" i aktywował (obecny: {1}).");
        A("This system does not offer the Ultimate Performance plan. Use the High performance plan instead.",
          "Ten system nie udostępnia planu Ultimate Performance. Użyj planu Wysoka wydajność.");
        A("Would disable task {0}{1}", "Wyłączyłbym zadanie {0}{1}");
        A("Task {0}: {1}", "Zadanie {0}: {1}");
        A("Service {0} is protected - cannot create a tweak for it.", "Usługa {0} jest chroniona - nie można utworzyć tweaku.");
        A("Service does not exist.", "Usługa nie istnieje.");
        A("Could not stop {0}: {1}", "Nie zatrzymano {0}: {1}");
        A("No active network adapters.", "Brak aktywnych kart sieciowych.");
        A("Would run fsutil behavior set {0} {1}", "fsutil behavior set {0} {1}");
        A("Skipped dangerous cleanup folder: {0}", "Pominięto niebezpieczny folder czyszczenia: {0}");
        A("Would delete up to {0} from: {1}", "Usunąłbym do {0} z: {1}");
        A("~{0} to free", "do zwolnienia ~{0}");
        A("freed {0}", "zwolniono {0}");
        A("Would empty the Recycle Bin", "Opróżniłbym Kosz");
        A("Would run DISM /Online /Cleanup-Image /StartComponentCleanup", "Uruchomiłbym DISM /Online /Cleanup-Image /StartComponentCleanup");
        A("DISM: exit code {0}", "DISM: kod {0}");
        A("Would close the OneDrive.exe process", "Zamknąłbym proces OneDrive.exe");
        A("Would run {0} /uninstall", "Uruchomiłbym {0} /uninstall");
        A("OneDriveSetup.exe not found - trying winget.", "Nie znaleziono OneDriveSetup.exe - próbuję winget.");
        A("OneDrive uninstall failed: {0}", "Deinstalacja OneDrive nie powiodła się: {0}");
        A("Would run {0} (reinstall OneDrive)", "Uruchomiłbym {0} (ponowna instalacja OneDrive)");
        A("Would run winget install Microsoft.OneDrive", "Uruchomiłbym winget install Microsoft.OneDrive");
        A("Could not reinstall OneDrive. Install it manually from the Microsoft website.", "Nie udało się zainstalować OneDrive ponownie. Zainstaluj ręcznie ze strony Microsoft.");
        A("Package {0} is protected - cannot create a tweak for it.", "Pakiet {0} jest chroniony - nie można utworzyć tweaku.");
        A("Would remove Appx package: {0} (Remove-AppxPackage -AllUsers + Remove-AppxProvisionedPackage)",
          "Usunąłbym pakiet Appx: {0} (Remove-AppxPackage -AllUsers + Remove-AppxProvisionedPackage)");
        A("The removed app must be reinstalled from the Microsoft Store (or with winget install).",
          "Usunięta aplikacja wymaga ponownej instalacji ze sklepu Microsoft Store (lub: winget install).");

        // ---- Tweaks: Gaming ----
        A("Enable Game Mode", "Tryb gry (Game Mode) włączony");
        A("Windows prioritizes the game and limits background tasks (Windows Update won't install drivers while you play).",
          "Windows priorytetyzuje grę i ogranicza zadania w tle (Windows Update nie instaluje sterowników w trakcie gry).");
        A("Hardware-accelerated GPU scheduling (HAGS)", "Planowanie GPU przyspieszane sprzętowo (HAGS)");
        A("Sets HwSchMode=2. Can reduce latency and CPU load; needs a supported GPU and driver. Requires a restart.",
          "Ustawia HwSchMode=2. Może zmniejszyć opóźnienia i obciążenie CPU; wymaga wspieranej karty i sterownika. Wymaga restartu.");
        A("On older GPUs/drivers it can cause artifacts - use Revert if you run into problems.",
          "Na starszych GPU/sterownikach może powodować artefakty - w razie problemów użyj Cofnij.");
        A("Needs Windows 10 2004+ (build 19041), a WDDM 2.7+ GPU and a recent driver; otherwise the setting is ignored.",
          "Wymaga Windows 10 2004+ (build 19041) oraz karty z WDDM 2.7+ i nowego sterownika; bez tego ustawienie jest ignorowane.");
        A("Disable Game DVR / background recording", "Wyłącz Game DVR / nagrywanie w tle");
        A("Turns off Xbox Game Bar background recording (GameDVR_Enabled=0, AllowGameDVR=0). Game Bar itself and the Xbox services stay.",
          "Wyłącza nagrywanie w tle Xbox Game Bar (GameDVR_Enabled=0, AllowGameDVR=0). Sam Game Bar i usługi Xbox pozostają.");
        A("Disable fullscreen optimizations globally", "Wyłącz optymalizacje pełnoekranowe globalnie");
        A("Sets the global FSE behavior (GameDVR_FSEBehaviorMode=2). Some games run smoother in true fullscreen, others worse (alt-tab).",
          "Ustawia globalnie zachowanie FSE (GameDVR_FSEBehaviorMode=2). Część gier działa płynniej w prawdziwym trybie pełnoekranowym, inne gorzej (alt-tab).");
        A("MMCSS: system responsiveness and network", "MMCSS: responsywność systemu i sieć");
        A("SystemResponsiveness=10 (not 0 - leaves headroom for audio/system) and NetworkThrottlingIndex=0xFFFFFFFF (disables multimedia network throttling).",
          "SystemResponsiveness=10 (nie 0 - zostawia zapas dla audio/systemu) oraz NetworkThrottlingIndex=0xFFFFFFFF (wyłącza dławienie sieci multimediów).");
        A("MMCSS: \"Games\" task profile", "MMCSS: profil zadań \"Games\"");
        A("GPU Priority=8, Priority=6, Scheduling Category=High, SFIO Priority=High for game processes.",
          "Priorytet GPU=8, Priority=6, Scheduling Category=High, SFIO Priority=High dla procesów gier.");
        A("Win32PrioritySeparation = 0x26", "Win32PrioritySeparation = 0x26");
        A("Gives the foreground application (the game) a larger share of CPU time compared to background processes.",
          "Zwiększa udział czasu CPU dla aplikacji pierwszoplanowej (gry) względem procesów w tle.");
        A("Disable mouse acceleration", "Wyłącz przyspieszenie myszy");
        A("Turns off \"Enhance pointer precision\" (MouseSpeed/Threshold = 0) - constant, predictable sensitivity in FPS games.",
          "Wyłącza \"Zwiększ precyzję wskaźnika\" (MouseSpeed/Threshold = 0) - stała, przewidywalna czułość w grach FPS.");
        A("Disable Sticky/Filter/Toggle Keys prompts", "Wyłącz okna Sticky/Filter/Toggle Keys");
        A("Turns off the shortcuts that open accessibility dialogs (5x Shift etc.), which can minimize a game.",
          "Wyłącza skróty uruchamiające okna dialogowe (5x Shift itd.), które potrafią zminimalizować grę.");
        A("Windowed-game optimizations + VRR", "Optymalizacje gier w oknie + VRR");
        A("Enables SwapEffectUpgradeEnable and VRROptimizeEnable (DirectX) - lower latency in windowed games and variable refresh rate.",
          "Włącza SwapEffectUpgradeEnable i VRROptimizeEnable (DirectX) - mniejsze opóźnienie w grach okienkowych i zmienne odświeżanie.");
        A("Windows 10 has only partial support (VRR since 1903, windowed-game optimizations are limited).",
          "Windows 10 ma tylko częściowe wsparcie (VRR od 1903, optymalizacje okienkowe działają w ograniczonym zakresie).");

        // ---- Tweaks: Power ----
        A("Disable USB selective suspend", "Wyłącz selektywne zawieszanie USB");
        A("Stops USB ports from going to sleep (fewer mouse/keyboard/gamepad stutters). On laptops, AC power only.",
          "Zapobiega usypianiu portów USB (mniej zacięć myszy/klawiatury/padów). Na laptopach tylko zasilanie sieciowe (AC).");
        A("Disable PCIe link power saving", "Wyłącz oszczędzanie energii łącza PCIe");
        A("Link State Power Management = Off - lower GPU/NVMe latency. On laptops, AC only.",
          "Link State Power Management = Wyłączone - mniejsze opóźnienia GPU/NVMe. Na laptopach tylko AC.");
        A("Minimum processor state 100% (AC)", "Minimalny stan procesora 100% (AC)");
        A("The CPU never drops its clock speed on AC power - steadier FPS, higher idle power draw and temperature.",
          "Procesor nie obniża taktowania na zasilaniu sieciowym - stabilniejszy FPS, wyższy pobór prądu i temperatura w spoczynku.");
        A("Disable Fast Startup", "Wyłącz szybkie uruchamianie (Fast Startup)");
        A("Full shutdown on \"Shut down\" - clean driver state and fewer problems after updates. Boot may be slightly slower.",
          "Pełne zamknięcie systemu przy \"Zamknij\" - czysty stan sterowników i mniej problemów po aktualizacjach. Start może być nieco wolniejszy.");
        A("Power plan: Winmizer Ultimate (Ultimate Performance)", "Plan zasilania: Winmizer Ultimate (Ultimate Performance)");
        A("Creates and activates the Ultimate Performance plan (lowest latency). Your original plan is remembered, so you can switch back.",
          "Tworzy i aktywuje plan Ultimate Performance (najniższe opóźnienia). Oryginalny plan jest zapamiętany i można do niego wrócić.");
        A("Laptops: this plan drains the battery much faster - recommended on AC power only.",
          "Laptopy: plan zużywa baterię znacznie szybciej - zalecany tylko przy zasilaniu z sieci.");
        A("Power plan: High performance", "Plan zasilania: Wysoka wydajność");
        A("Activates the built-in High performance plan (a lighter alternative to Ultimate). You can switch back to Balanced.",
          "Aktywuje wbudowany plan Wysoka wydajność (lżejsza alternatywa dla Ultimate). Można wrócić do Zrównoważonego.");
        A("Laptops: shorter battery life.", "Laptopy: krótszy czas pracy na baterii.");
        A("Disable hibernation (frees disk space)", "Wyłącz hibernację (zwalnia miejsce)");
        A("powercfg -h off deletes hiberfil.sys (several to a dozen or so GB). Also disables Fast Startup and hibernation.",
          "powercfg -h off usuwa hiberfil.sys (kilka-kilkanaście GB). Wyłącza też Fast Startup i hibernację.");
        A("Laptops: you lose the ability to hibernate.", "Laptopy: utrata możliwości hibernacji.");

        // ---- Tweaks: Privacy ----
        A("Telemetry: basic level", "Telemetria: poziom podstawowy");
        A("AllowTelemetry=1 (Basic/Required). Value 0 only works on Enterprise, so it isn't used. Turn off the DiagTrack services in the Services category.",
          "AllowTelemetry=1 (Podstawowe/Wymagane). Wartość 0 działa tylko w Enterprise, dlatego jej nie używamy. Usługi DiagTrack wyłączasz w kategorii Usługi.");
        A("Disable advertising ID", "Wyłącz identyfikator reklamowy");
        A("Apps can no longer use the Advertising ID to profile you for ads.",
          "Aplikacje nie mogą używać Advertising ID do profilowania reklam.");
        A("Disable tailored experiences", "Wyłącz spersonalizowane środowiska");
        A("Microsoft doesn't use diagnostic data to personalize tips and ads.",
          "Microsoft nie używa danych diagnostycznych do dopasowywania porad i reklam.");
        A("Disable activity history", "Wyłącz historię aktywności");
        A("Stops collecting and uploading activity history (Timeline).",
          "Wyłącza zbieranie i wysyłanie historii aktywności (Oś czasu).");
        A("Disable consumer features and suggested apps", "Wyłącz funkcje konsumenckie i sugerowane aplikacje");
        A("DisableWindowsConsumerFeatures=1 - Windows no longer installs promoted apps automatically (Candy Crush etc.).",
          "DisableWindowsConsumerFeatures=1 - Windows nie instaluje automatycznie promowanych aplikacji (Candy Crush itp.).");
        A("Disable ads and suggestions in the Start menu", "Wyłącz reklamy i sugestie w Menu Start");
        A("Removes suggested apps/ads in Start and Settings, plus recommended items.",
          "Usuwa sugerowane aplikacje/reklamy w Start i Ustawieniach oraz polecane elementy.");
        A("Disable tips and ads on the lock screen", "Wyłącz wskazówki i reklamy na ekranie blokady");
        A("A lock screen without fun facts, Spotlight ads and tips.",
          "Ekran blokady bez \"ciekawostek\", Spotlight-reklam i wskazówek.");
        A("Disable telemetry tasks", "Wyłącz zadania telemetryczne");
        A("Disables the scheduled tasks Compatibility Appraiser, ProgramDataUpdater, Consolidator, UsbCeip and DmClient. Windows Update is not affected.",
          "Wyłącza harmonogramowane zadania: Compatibility Appraiser, ProgramDataUpdater, Consolidator, UsbCeip, DmClient. Windows Update nie jest naruszony.");
        A("Disable and remove Cortana (policy + app)", "Wyłącz i usuń Cortanę (zasada + aplikacja)");
        A("Turns Cortana off via the AllowCortana=0 policy and uninstalls the Cortana app (Microsoft.549981C3F5F10). Windows Search keeps working normally. Counts as applied only when the app is gone too.",
          "Wyłącza Cortanę zasadą AllowCortana=0 i odinstalowuje aplikację Cortana (Microsoft.549981C3F5F10). Wyszukiwanie Windows działa normalnie. Uznawane za zastosowane dopiero, gdy aplikacja zniknie.");
        A("Disable and remove Windows Copilot (policy, taskbar button, app)", "Wyłącz i usuń Windows Copilot (zasada, przycisk paska zadań, aplikacja)");
        A("TurnOffWindowsCopilot=1 (user and machine policy), ShowCopilotButton=0 (taskbar button) and removal of the Copilot app packages. Restart Explorer for the taskbar button to disappear.",
          "TurnOffWindowsCopilot=1 (zasada użytkownika i komputera), ShowCopilotButton=0 (przycisk paska zadań) oraz usunięcie pakietów aplikacji Copilot. Aby przycisk zniknął z paska zadań, uruchom ponownie Eksploratora.");
        A("Windows 10: restrict Cortana (lock screen, consent)", "Windows 10: ogranicz Cortanę (ekran blokady, zgoda)");
        A("On Windows 10 Cortana is a separate component: this blocks it on the lock screen (AllowCortanaAboveLock=0) and withdraws user consent (CortanaConsent=0). Complements the AllowCortana=0 policy.",
          "Cortana na Windows 10 to osobny komponent: blokuje ją na ekranie blokady (AllowCortanaAboveLock=0) i cofa zgodę użytkownika (CortanaConsent=0). Uzupełnia zasadę AllowCortana=0.");
        A("Windows 10: disable \"News and interests\" on the taskbar", "Windows 10: wyłącz \"Wiadomości i zainteresowania\" na pasku zadań");
        A("The EnableFeeds=0 policy removes the weather/news widget from the taskbar (MSN ads and tracking, RAM used by Edge WebView).",
          "Zasada EnableFeeds=0 usuwa widżet pogody/wiadomości z paska zadań (reklamy i śledzenie MSN, zużycie RAM przez Edge WebView).");
        A("Disable web search (Bing) in Start", "Wyłącz wyszukiwanie internetowe (Bing) w Start");
        A("Search in Start shows local results only (BingSearchEnabled=0, DisableSearchBoxSuggestions=1).",
          "Wyszukiwanie w Start pokazuje tylko wyniki lokalne (BingSearchEnabled=0, DisableSearchBoxSuggestions=1).");

        // ---- Tweaks: Services ----
        A("Telemetry: Connected User Experiences and Telemetry (DiagTrack)", "Telemetria: Connected User Experiences and Telemetry (DiagTrack)");
        A("The main service that sends diagnostic data to Microsoft. Nothing in the system depends on it.",
          "Główna usługa wysyłająca dane diagnostyczne do Microsoft. Nic w systemie od niej nie zależy.");
        A("Telemetry: WAP Push Message Routing (dmwappushservice)", "Telemetria: WAP Push Message Routing (dmwappushservice)");
        A("Helper service for telemetry/MDM. Not needed on a home PC.", "Pomocnicza usługa telemetrii/MDM. Niepotrzebna na komputerze domowym.");
        A("Fax (Fax)", "Faks (Fax)");
        A("The FAX service - unrelated to printing. Printers and \"Print to PDF\" work as before.",
          "Usługa FAKSU - nie ma nic wspólnego z drukowaniem. Drukarki i \"Drukuj do PDF\" działają bez zmian.");
        A("Retail demo mode (RetailDemo)", "Tryb demonstracyjny sklepu (RetailDemo)");
        A("Only used on store display units.", "Używany tylko w egzemplarzach pokazowych w sklepach.");
        A("Downloaded maps (MapsBroker) -> manual", "Pobrane mapy (MapsBroker) -> ręczny");
        A("Offline map updates; started only when needed.", "Aktualizacje map offline; uruchamiany tylko gdy potrzebny.");
        A("Windows Error Reporting (WerSvc) -> manual", "Raportowanie błędów Windows (WerSvc) -> ręczny");
        A("Sends crash reports to Microsoft; starts on demand.", "Wysyłanie raportów awarii do Microsoft; uruchamia się na żądanie.");
        A("Remote Registry (RemoteRegistry)", "Zdalny rejestr (RemoteRegistry)");
        A("Remote registry editing over the network - a security risk, unnecessary at home. (Already disabled by default on Windows 11.)",
          "Zdalna edycja rejestru z sieci - ryzyko bezpieczeństwa, niepotrzebna w domu. (W Windows 11 domyślnie już wyłączona.)");
        A("SysMain (Superfetch) -> disabled", "SysMain (Superfetch) -> wyłączona");
        A("Preloads apps into RAM. Pointless on an SSD/NVMe and causes extra writes; on an HDD it may slow app launches.",
          "Wstępnie ładuje aplikacje do RAM. Na dysku SSD/NVMe zbędna i generuje zapisy; na HDD może pogorszyć start aplikacji.");
        A("Geolocation service (lfsvc)", "Usługa lokalizacji geograficznej (lfsvc)");
        A("Turns off the location service - apps (Maps, Weather) can't determine your position.",
          "Wyłącza usługę lokalizacji - aplikacje (Mapy, Pogoda) nie ustalą położenia.");
        A("Windows Search (WSearch) -> manual", "Windows Search (WSearch) -> ręczny");
        A("File indexing. Manual start = slower search in Explorer/Start, less disk load.",
          "Indeksowanie plików. Ręczny start = wolniejsze wyszukiwanie w Eksploratorze/Start, mniejsze obciążenie dysku.");

        // ---- Tweaks: Network ----
        A("Disable Nagle's algorithm (lower ping)", "Wyłącz algorytm Nagle'a (niższy ping)");
        A("TcpAckFrequency=1 and TCPNoDelay=1 on every interface - game packets are sent immediately. The effect on ping is small; doesn't touch Wi-Fi or drivers.",
          "TcpAckFrequency=1 i TCPNoDelay=1 na każdym interfejsie - pakiety gier wysyłane od razu. Wpływ na ping jest niewielki; nie rusza Wi-Fi ani sterowników.");
        A("Delivery Optimization: disable P2P sharing", "Delivery Optimization: wyłącz udostępnianie P2P");
        A("DODownloadMode=0 - updates are downloaded only from Microsoft servers (and not uploaded to other PCs). Windows Update works normally.",
          "DODownloadMode=0 - aktualizacje pobierane tylko z serwerów Microsoft (bez wysyłania ich innym komputerom). Windows Update działa normalnie.");
        A("Cloudflare DNS (1.1.1.1 / 1.0.0.1)", "DNS Cloudflare (1.1.1.1 / 1.0.0.1)");
        A("Sets a fast public DNS on active network adapters. Previous settings are remembered.",
          "Ustawia szybki publiczny DNS na aktywnych kartach sieciowych. Poprzednie ustawienia są zapamiętane.");
        A("Changes your DNS provider - on corporate or parental-filtered networks it may block something.",
          "Zmienia dostawcę DNS - w sieciach firmowych/z filtrowaniem rodzicielskim może coś zablokować.");

        // ---- Tweaks: Visual ----
        A("Visual effects tuned for performance (keeps fonts and thumbnails)", "Efekty wizualne pod wydajność (z zachowaniem czcionek i miniatur)");
        A("Turns off window and taskbar animations plus transparent shadows/selection. Font smoothing (ClearType) and thumbnails stay on.",
          "Wyłącza animacje okien i paska zadań oraz cienie/zaznaczenia przezroczyste. Wygładzanie czcionek (ClearType) i miniatury pozostają włączone.");
        A("Disable transparency effects", "Wyłącz efekty przezroczystości");
        A("Turns off acrylic/transparency in Start and windows - slightly less GPU load.",
          "Wyłącza akryl/przezroczystość w Start i oknach - odrobinę mniej obciążony GPU.");
        A("Remove the startup-app launch delay", "Usuń opóźnienie uruchamiania aplikacji autostartu");
        A("Serialize\\StartupDelayInMSec=0 - startup programs launch right after sign-in.",
          "Serialize\\StartupDelayInMSec=0 - programy autostartu startują od razu po zalogowaniu.");
        A("No menu delay (MenuShowDelay=0)", "Menu bez opóźnienia (MenuShowDelay=0)");
        A("Drop-down menus appear instantly.", "Menu rozwijane pojawiają się natychmiast.");
        A("Block background apps", "Zablokuj aplikacje działające w tle");
        A("Policy LetAppsRunInBackground=2 - Store apps don't run in the background (saves RAM/CPU). Notifications from such apps may stop arriving.",
          "Zasada LetAppsRunInBackground=2 - aplikacje Sklepu nie pracują w tle (oszczędza RAM/CPU). Mogą przestać przychodzić powiadomienia z takich aplikacji.");
        A("Windows 11: disable Widgets (policy)", "Windows 11: wyłącz Widżety (zasada)");
        A("Policy AllowNewsAndInterests=0 turns off the Widgets panel (weather, news, ads) and the Widgets.exe background process.",
          "Zasada AllowNewsAndInterests=0 wyłącza panel Widżetów (pogoda, wiadomości, reklamy) i proces Widgets.exe działający w tle.");
        A("Windows 10: hide the People bar", "Windows 10: ukryj pasek Osoby (People)");
        A("Removes the People icon from the taskbar (PeopleBand=0, HidePeopleBar=1). This element doesn't exist on Windows 11.",
          "Usuwa ikonę Osoby z paska zadań (PeopleBand=0, HidePeopleBar=1). W Windows 11 ten element nie istnieje.");
        A("Windows 10: hide the Meet Now icon", "Windows 10: ukryj ikonę Meet Now");
        A("Hides the Meet Now (Skype) button in the taskbar tray (HideSCAMeetNow=1).",
          "Ukrywa przycisk Meet Now (Skype) w zasobniku paska zadań (HideSCAMeetNow=1).");

        // ---- Tweaks: Storage ----
        A("Make sure TRIM is enabled", "Upewnij się, że TRIM jest włączony");
        A("fsutil behavior set DisableDeleteNotify 0 - the OS tells the SSD which blocks are free (performance and lifespan).",
          "fsutil behavior set DisableDeleteNotify 0 - system informuje SSD o zwolnionych blokach (wydajność i żywotność).");
        A("Disable NTFS last-access timestamps", "Wyłącz znacznik ostatniego dostępu NTFS");
        A("fsutil behavior set disablelastaccess 1 - fewer disk writes every time a file is read.",
          "fsutil behavior set disablelastaccess 1 - mniej zapisów na dysk przy każdym odczycie pliku.");

        // ---- Tweaks: Cleanup ----
        A("Clean temporary files (%TEMP% and Windows\\Temp)", "Wyczyść pliki tymczasowe (%TEMP% i Windows\\Temp)");
        A("Deletes user and system temporary files. Files in use by running programs are skipped. Prefetch is NOT cleaned.",
          "Usuwa pliki tymczasowe użytkownika i systemu. Pliki używane przez działające programy są pomijane. Prefetch NIE jest czyszczony.");
        A("Clear shader caches (DirectX/NVIDIA/AMD)", "Wyczyść pamięć podręczną shaderów (DirectX/NVIDIA/AMD)");
        A("Deletes D3DSCache, NVIDIA DXCache/GLCache and AMD DxCache. Games recompile shaders from scratch - the first launch may stutter a little.",
          "Usuwa D3DSCache, NVIDIA DXCache/GLCache i AMD DxCache. Gry skompilują shadery od nowa - pierwsze uruchomienie może się lekko dławić.");
        A("Empty the Recycle Bin", "Opróżnij Kosz");
        A("Permanently deletes everything in the Recycle Bin.", "Trwale usuwa wszystko z Kosza.");
        A("This cannot be undone - check the Recycle Bin contents first.", "Operacja nieodwracalna - sprawdź zawartość Kosza.");
        A("DISM: component cleanup (StartComponentCleanup)", "DISM: czyszczenie składników (StartComponentCleanup)");
        A("DISM /Online /Cleanup-Image /StartComponentCleanup - removes old versions of system components. Safe, but takes several to a dozen or so minutes.",
          "DISM /Online /Cleanup-Image /StartComponentCleanup - usuwa stare wersje składników systemu. Bezpieczne, ale trwa kilka-kilkanaście minut.");

        // ---- Tweaks: OneDrive ----
        A("OneDrive: disable sync via policy (DisableFileSyncNGSC)", "OneDrive: wyłącz synchronizację zasadą (DisableFileSyncNGSC)");
        A("A policy that blocks OneDrive file sync from running. Local files are left untouched.",
          "Zasada blokująca uruchamianie synchronizacji plików OneDrive. Pliki lokalne pozostają nietknięte.");
        A("Cloud sync will stop. Online-only files are not deleted.",
          "Synchronizacja z chmurą zostanie zatrzymana. Pliki dostępne tylko online nie są usuwane.");
        A("OneDrive: remove from the File Explorer navigation pane", "OneDrive: usuń z panelu nawigacji Eksploratora");
        A("Hides the OneDrive icon in the left pane of File Explorer.", "Ukrywa ikonę OneDrive w lewym panelu Eksploratora plików.");
        A("OneDrive: remove from startup", "OneDrive: usuń z autostartu");
        A("Removes the Run entries that launch OneDrive at sign-in (the backup lets you restore them).",
          "Usuwa wpisy Run uruchamiające OneDrive przy logowaniu (kopia zapasowa pozwala je przywrócić).");
        A("OneDrive: full uninstall", "OneDrive: pełna deinstalacja");
        A("Closes the OneDrive process and runs the official uninstaller (OneDriveSetup.exe /uninstall, winget as a fallback).",
          "Zamyka proces OneDrive i uruchamia oficjalny deinstalator (OneDriveSetup.exe /uninstall, awaryjnie winget).");
        A("Local sync will stop working. Files that exist ONLY in the cloud are not deleted, but become unavailable from this PC until you reinstall. Make sure your files are downloaded locally first.",
          "Lokalna synchronizacja przestanie działać. Pliki istniejące WYŁĄCZNIE w chmurze nie są kasowane, ale stają się niedostępne z tego komputera do czasu ponownej instalacji. Najpierw upewnij się, że pliki są pobrane lokalnie.");

        // ---- Tweaks: Debloat (apps) ----
        A("Cortana", "Cortana");
        A("The old Cortana voice assistant. On Windows 10 this removes only the app; the system component is limited by a Privacy policy.",
          "Stary asystent głosowy Cortana. Na Windows 10 usuwa tylko aplikację; sam komponent systemowy ogranicza zasada w Prywatności.");
        A("Microsoft News (Bing News)", "Microsoft News (Bing News)");
        A("News app.", "Aplikacja z wiadomościami.");
        A("Weather (Bing Weather)", "Pogoda (Bing Weather)");
        A("MSN weather app.", "Aplikacja pogodowa MSN.");
        A("Finance (Bing Finance)", "Finanse (Bing Finance)");
        A("MSN finance app.", "Aplikacja finansowa MSN.");
        A("Sports (Bing Sports)", "Sport (Bing Sports)");
        A("MSN sports app.", "Aplikacja sportowa MSN.");
        A("Bing Search (Start add-on)", "Bing Search (dodatek do Start)");
        A("Bing search integration.", "Integracja wyszukiwania Bing.");
        A("Get Help", "Uzyskaj pomoc (Get Help)");
        A("Support app.", "Aplikacja pomocy technicznej.");
        A("Tips", "Porady (Tips)");
        A("Windows tips app.", "Aplikacja z poradami Windows.");
        A("Microsoft 365 (Office Hub)", "Microsoft 365 (Office Hub)");
        A("Advertising shortcut to the Office suite.", "Reklamowy skrót do pakietu Office.");
        A("Microsoft Solitaire Collection", "Microsoft Solitaire Collection");
        A("Card games with ads.", "Gry karciane z reklamami.");
        A("People", "Osoby (People)");
        A("Address book.", "Książka adresowa.");
        A("Feedback Hub", "Centrum opinii (Feedback Hub)");
        A("Sends feedback to Microsoft.", "Wysyłanie opinii do Microsoft.");
        A("Windows Maps", "Mapy Windows");
        A("Maps app.", "Aplikacja map.");
        A("Clipchamp", "Clipchamp");
        A("Microsoft video editor.", "Edytor wideo Microsoft.");
        A("Microsoft To Do", "Microsoft To Do");
        A("To-do list.", "Lista zadań.");
        A("Power Automate", "Power Automate");
        A("Task automation.", "Automatyzacja zadań.");
        A("Microsoft Teams (consumer)", "Microsoft Teams (konsumencki)");
        A("Built-in Teams (Chat).", "Wbudowany Teams (Chat).");
        A("Microsoft Teams (new)", "Microsoft Teams (nowy)");
        A("The new Teams version preinstalled with the system.", "Nowa wersja Teams preinstalowana w systemie.");
        A("Microsoft Copilot", "Microsoft Copilot");
        A("The Copilot app.", "Aplikacja Copilot.");
        A("Windows Copilot provider", "Dostawca Windows Copilot");
        A("Copilot component.", "Komponent Copilot.");
        A("Skype", "Skype");
        A("Skype messenger.", "Komunikator Skype.");
        A("Mixed Reality Portal", "Portal rzeczywistości mieszanej");
        A("For Windows MR VR headsets.", "Dla gogli VR Windows MR.");
        A("3D Viewer", "Przeglądarka 3D (3D Viewer)");
        A("Windows 10: 3D model viewer.", "Windows 10: przeglądarka modeli 3D.");
        A("Paint 3D", "Paint 3D");
        A("Windows 10: Paint 3D (not the classic Paint - that one stays).", "Windows 10: Paint 3D (to nie jest klasyczny Paint - ten zostaje).");
        A("Print 3D", "Print 3D");
        A("Windows 10: 3D printing app.", "Windows 10: aplikacja do druku 3D.");
        A("Disney+", "Disney+");
        A("Preinstalled promotional app.", "Preinstalowana aplikacja promocyjna.");
        A("Spotify", "Spotify");
        A("Preinstalled promotional app (you can reinstall it).", "Preinstalowana aplikacja promocyjna (możesz ją zainstalować ponownie).");
        A("TikTok", "TikTok");
        A("Candy Crush and similar games", "Candy Crush i podobne gry");
        A("Promotional games (Candy Crush Saga/Soda etc.).", "Promocyjne gry (Candy Crush Saga/Soda itp.).");
        A("Facebook", "Facebook");
        A("Instagram", "Instagram");
        A("Twitter / X", "Twitter / X");
        A("Media Player (Groove/ZuneMusic)", "Odtwarzacz multimediów (Groove/ZuneMusic)");
        A("On Windows 11 this is the new Media Player - remove it only if you use another player.",
          "W Windows 11 to nowy Media Player - usuń tylko jeśli używasz innego odtwarzacza.");
        A("Movies & TV (ZuneVideo)", "Filmy i TV (ZuneVideo)");
        A("Movie app.", "Aplikacja do filmów.");
        A("Phone Link", "Łączność z telefonem (Phone Link)");
        A("Remove it if you don't connect your phone to Windows.", "Usuń, jeśli nie łączysz telefonu z Windows.");
        A("Clock & Alarms", "Zegar i alarmy");
        A("Alarms, timers, stopwatch.", "Alarmy, minutniki, stoper.");
        A("Xbox Game Bar (overlay)", "Xbox Game Bar (nakładka)");
        A("The Win+G overlay. Xbox identity and Gaming Services stay (needed by Game Pass games).",
          "Nakładka Win+G. Tożsamość Xbox i Gaming Services zostają (potrzebne grom z Game Pass).");
        A("Game Pass games keep working, but you lose the Win+G overlay.", "Gry z Game Pass nadal działają, ale stracisz nakładkę Win+G.");

        // ---- UI ----
        A("Windows 10/11 gaming optimizer - safe, reversible, with a restore point", "Optymalizator Windows 10/11 dla graczy - bezpieczny, odwracalny, z punktem przywracania");
        A("System:", "System:");
        A("WARNING", "UWAGA");
        A("DRY-RUN MODE", "TRYB DRY-RUN");
        A("Nothing will be changed - preview of actions only.", "Nic nie zostanie zmienione - tylko podgląd działań.");
        A("N/A on this system", "N/D w tym systemie");
        A("one-time action", "akcja jednorazowa");
        A("YES - applied", "TAK - zastosowane");
        A("no", "nie");
        A("yes", "tak");
        A("detection error", "błąd detekcji");
        A("Category", "Kategoria");
        A("Tweak", "Tweak");
        A("Risk", "Ryzyko");
        A("Description", "Opis");
        A("Warning:", "Uwaga:");
        A("Applies to: {0} only", "Dotyczy: tylko {0}");
        A("{0} only", "tylko {0}");
        A("Press any key to return to the menu...", "Naciśnij dowolny klawisz, aby wrócić do menu...");
        A("Result", "Wynik");
        A("Details", "Szczegóły");
        A("applied", "zastosowano");
        A("reverted", "cofnięto");
        A("skipped", "pominięto");
        A("ERROR", "BŁĄD");
        A("Summary:", "Podsumowanie:");
        A("Skipped", "Pominięto");
        A("Errors", "Błędy");
        A("Space freed:", "Zwolnione miejsce:");
        A("Log:", "Dziennik:");
        A("Applied", "Zastosowano");
        A("Reverted", "Cofnięto");
        A("Nothing selected.", "Nic nie wybrano.");
        A("You selected advanced tweaks - read their descriptions above carefully.", "Wybrano tweaki zaawansowane - przeczytaj uważnie ich opisy powyżej.");
        A("Apply [bold]{0}[/] change(s)? A system restore point will be created first.", "Zastosować [bold]{0}[/] zmian(y)? Najpierw zostanie utworzony punkt przywracania systemu.");
        A("Cancelled.", "Anulowano.");
        A("Creating system restore point...", "Tworzenie punktu przywracania systemu...");
        A("Restore point: skipped (dry-run).", "Punkt przywracania: (dry-run) pominięto.");
        A("Restore point created.", "Punkt przywracania utworzony.");
        A("Failed to create a restore point:", "Nie udało się utworzyć punktu przywracania:");
        A("Continue anyway? (The values are still backed up to backup.json)", "Kontynuować mimo to? (Backup wartości w backup.json i tak powstanie)");
        A("Aborted - nothing was changed.", "Przerwano - nic nie zmieniono.");
        A("Start", "Start");
        A("Done", "Gotowe");
        A("To apply (dry-run)", "Do zastosowania (dry-run)");
        A("Action details:", "Szczegóły działań:");
        A("A restart is recommended for all changes to take effect.", "Zalecany jest restart komputera, aby wszystkie zmiany zadziałały.");
        A("Restart now?", "Uruchomić ponownie teraz?");
        A("Winmizer: restart after optimization", "Winmizer: restart po optymalizacji");
        A("Restarting in 15 seconds. To cancel: shutdown /a", "Restart za 15 sekund. Aby anulować: shutdown /a");
        A("Remember to restart later.", "Pamiętaj o ponownym uruchomieniu później.");

        // ---- Menus ----
        A("Main menu", "Menu główne");
        A("1) Quick optimize (recommended safe set)", "1) Szybka optymalizacja (zalecany bezpieczny zestaw)");
        A("2) Custom selection (by category)", "2) Własny wybór (według kategorii)");
        A("3) App debloat and startup apps", "3) Odśmiecanie aplikacji i autostart");
        A("4) Power plans", "4) Plany zasilania");
        A("5) System info / tweak status", "5) Informacje o systemie / status tweaków");
        A("6) Undo / restore", "6) Cofnij / przywróć");
        A("8) Exit", "8) Wyjście");
        A("Quick optimize applies only [green]proven, safe[/] settings: gaming, privacy, telemetry services,",
          "Szybka optymalizacja stosuje wyłącznie [green]sprawdzone, bezpieczne[/] ustawienia: gaming, prywatność, usługi telemetryczne,");
        A("appearance, disk and temp-file cleanup. Printers, network, Windows Update, Defender, the Store and Xbox are left untouched.",
          "wygląd, dysk i czyszczenie plików tymczasowych. Drukarki, sieć, Windows Update, Defender, Sklep i Xbox pozostają nietknięte.");
        A("Laptop detected - power settings that need a battery were skipped.", "Wykryto laptopa - ustawienia zasilania wymagające baterii zostały pominięte.");
        A("Everything can be undone in the [bold]Undo / restore[/] menu.", "Wszystko można cofnąć w menu [bold]Cofnij / przywróć[/].");
        A("Quick optimize", "Szybka optymalizacja");
        A("Checking current state...", "Sprawdzanie obecnego stanu...");
        A("All recommended tweaks are already applied.", "Wszystkie zalecane tweaki są już zastosowane.");
        A("Choose [aqua]categories[/] to review:", "Wybierz [aqua]kategorie[/] do przejrzenia:");
        A("(Space - select, Enter - confirm; app debloat has its own menu)", "(Spacja - zaznacz, Enter - zatwierdź; Odśmiecanie aplikacji ma osobne menu)");
        A("Select tweaks to apply ([green]Safe[/]/[yellow]Moderate[/] are preselected, [red]Advanced[/] are not):",
          "Zaznacz tweaki do zastosowania ([green]Bezpieczne[/]/[yellow]Umiarkowane[/] zalecane są domyślnie, [red]Zaawansowane[/] - odznaczone):");
        A("(Space - select, Enter - confirm)", "(Spacja - zaznacz, Enter - zatwierdź)");
        A("Remove preinstalled apps", "Usuń preinstalowane aplikacje");
        A("Startup apps (disable selected)", "Aplikacje autostartu (wyłącz wybrane)");
        A("Back", "Wróć");
        A("Always kept: Microsoft Store, App Installer/winget, Calculator, Photos, Notepad, Terminal, Paint, Snipping Tool, Camera, Xbox identity and Gaming Services.",
          "Zawsze zachowane: Sklep Microsoft, App Installer/winget, Kalkulator, Zdjęcia, Notatnik, Terminal, Paint, Narzędzie do wycinania, Aparat, tożsamość Xbox i Gaming Services.");
        A("Looking for installed apps...", "Wyszukiwanie zainstalowanych aplikacji...");
        A("No apps from the list to remove - the system is already clean.", "Brak aplikacji z listy do usunięcia - system jest już czysty.");
        A("Select apps to [red]remove[/] (by default only the safe, promotional ones):", "Zaznacz aplikacje do [red]usunięcia[/] (domyślnie tylko bezpieczne, promocyjne):");
        A("No startup entries found.", "Nie znaleziono wpisów autostartu.");
        A("Name", "Nazwa");
        A("Source", "Źródło");
        A("Command", "Polecenie");
        A("Select entries to [yellow]disable[/] (you can restore them later in the Undo menu):", "Zaznacz wpisy do [yellow]wyłączenia[/] (można je później przywrócić w menu Cofnij):");
        A("Disable {0} startup entries?", "Wyłączyć {0} wpisów autostartu?");
        A("Disabled:", "Wyłączono:");
        A("Startup entry {0}: {1}", "Autostart {0}: {1}");
        A("Error:", "Błąd:");
        A("Active power plan:", "Aktywny plan zasilania:");
        A("laptop detected", "wykryto laptopa");
        A("desktop PC", "komputer stacjonarny");
        A("Note: Ultimate/High performance plans significantly shorten battery life.", "Uwaga: plany Ultimate/Wysoka wydajność znacząco skracają czas pracy na baterii.");
        A("Power plans", "Plany zasilania");
        A("Winmizer Ultimate (Ultimate Performance)", "Winmizer Ultimate (Ultimate Performance)");
        A("High performance", "Wysoka wydajność");
        A("Balanced (Windows default)", "Zrównoważony (domyślny Windows)");
        A("Extra settings (USB, PCIe, min. CPU state, hibernation, Fast Startup)", "Dodatkowe ustawienia (USB, PCIe, min. stan CPU, hibernacja, Fast Startup)");
        A("Show available plans", "Pokaż dostępne plany");
        A("Switched to the Balanced plan.", "Przełączono na plan Zrównoważony.");
        A("Select settings:", "Zaznacz ustawienia:");
        A("Plan", "Plan");
        A("Active", "Aktywny");
        A("YES", "TAK");
        A("Collecting system information...", "Zbieranie informacji o systemie...");
        A("System", "System");
        A("unsupported version", "nieobsługiwana wersja");
        A("Tweak compatibility", "Zgodność tweaków");
        A("{0} of {1} available on this system", "{0} z {1} dostępnych w tym systemie");
        A("Processor", "Procesor");
        A("Graphics card", "Karta graficzna");
        A("Memory (RAM)", "Pamięć RAM");
        A("Device type", "Typ urządzenia");
        A("Laptop (battery detected)", "Laptop (wykryto baterię)");
        A("Desktop PC", "Komputer stacjonarny");
        A("Power plan", "Plan zasilania");
        A("Winmizer restore point", "Punkt przywracania Winmizer");
        A("none", "brak");
        A("Backup", "Kopia zapasowa");
        A(" ({0} entries)", " ({0} wpisów)");
        A("Applied?", "Zastosowany?");
        A("Applied tweaks:", "Zastosowane tweaki:");
        A("Undo / restore", "Cofnij / przywróć");
        A("Undo everything (from backup.json)", "Cofnij wszystko (z backup.json)");
        A("Undo selected tweaks", "Cofnij wybrane tweaki");
        A("Open System Restore (restore point)", "Otwórz Przywracanie systemu (punkt przywracania)");
        A("No saved changes to undo.", "Brak zapisanych zmian do cofnięcia.");
        A("Restore the original values for {0} entries?", "Przywrócić oryginalne wartości dla {0} wpisów?");
        A("Select changes to undo:", "Zaznacz zmiany do cofnięcia:");
        A("(dry-run) Would open rstrui.exe", "(dry-run) Otworzyłbym rstrui.exe");
        A("Cannot launch rstrui:", "Nie można uruchomić rstrui:");
        A("Restoring values...", "Przywracanie wartości...");

        // ---- Program / CLI ----
        A("Winmizer - Windows 10/11 gaming optimizer", "Winmizer - optymalizator Windows 10/11 dla graczy");
        A("Usage:", "Użycie:");
        A("interactive menu (requires administrator)", "interaktywne menu (wymaga administratora)");
        A("list all tweaks and their detected status (no system changes)", "wypisz wszystkie tweaki i ich wykryty status (bez zmian w systemie)");
        A("menu in preview mode - nothing is changed", "menu w trybie podglądu - nic nie jest zmieniane");
        A("preview the Quick optimize set incl. app removal (non-interactive)", "podgląd zestawu Szybka optymalizacja wraz z usuwaniem aplikacji (bez interakcji)");
        A("preview selected tweaks", "podgląd wybranych tweaków");
        A("apply the Quick optimize set without prompts (also restarts Explorer if needed)", "zastosuj zestaw Szybka optymalizacja bez pytań (w razie potrzeby uruchamia też ponownie Eksploratora)");
        A("apply selected tweaks without prompts", "zastosuj wybrane tweaki bez pytań");
        A("undo everything from backup.json", "cofnij wszystko z backup.json");
        A("UI language for this run (English is the default; the menu can save a choice)", "język interfejsu dla tego uruchomienia (domyślnie angielski; menu zapamiętuje wybór)");
        A("Non-interactive mode requires --dry-run (preview) or --yes (confirm changes).", "Tryb nieinteraktywny wymaga --dry-run (podgląd) albo --yes (potwierdzenie zmian).");
        A("See you! Remember to restart after the changes.", "Do zobaczenia! Pamiętaj o restarcie po zmianach.");
        A("Unhandled exception: {0}", "Nieobsłużony wyjątek: {0}");
        A("Unexpected error:", "Nieoczekiwany błąd:");
        A("Launched a new window with administrator rights.", "Uruchomiono nowe okno z uprawnieniami administratora.");
        A("Winmizer must run with [bold]administrator rights[/] to change system settings.", "Winmizer musi działać z [bold]uprawnieniami administratora[/], aby zmieniać ustawienia systemu.");
        A("Close this window and start the program again: [bold]right-click -> Run as administrator[/].", "Zamknij to okno i uruchom program ponownie: [bold]prawy przycisk myszy -> Uruchom jako administrator[/].");
        A("Just want to see what would change? Run with [bold]--dry-run[/] or [bold]--list[/].", "Chcesz tylko zobaczyć, co zostałoby zmienione? Uruchom z parametrem [bold]--dry-run[/] albo [bold]--list[/].");
        A("Administrator rights missing", "Brak uprawnień administratora");
        A("Status", "Status");
        A("Total tweaks: [bold]{0}[/] (settings: {1}, actions: {2}); applied: [green]{3}[/]; unavailable on this system: [grey]{4}[/]",
          "Łącznie tweaków: [bold]{0}[/] (ustawienia: {1}, akcje: {2}); zastosowane: [green]{3}[/]; niedostępne w tym systemie: [grey]{4}[/]");
        A("Laptop: {0}; power plan: {1}", "Laptop: {0}; plan zasilania: {1}");
        A("Unknown ID:", "Nieznane ID:");
        A("(see --list)", "(zobacz --list)");

        // ---- v1.0.1: Quick removes bloatware, verifiable Appx removal, Explorer restart ----
        A("Windows Copilot (system package)", "Windows Copilot (pakiet systemowy)");
        A("The Copilot package of Windows 11 (taskbar button / sidebar).", "Pakiet Copilot w Windows 11 (przycisk paska zadań / panel boczny).");
        A("Not applied", "Nie zastosowano");
        A("Package {0} is still present after removal: {1}", "Pakiet {0} nadal jest obecny po usunięciu: {1}");
        A("Apply [bold]{0}[/] change(s), including removing [bold]{1}[/] app(s)? A system restore point will be created first.",
          "Zastosować [bold]{0}[/] zmian(y), w tym usunąć [bold]{1}[/] aplikacji? Najpierw zostanie utworzony punkt przywracania systemu.");
        A("Apps to be removed", "Aplikacje do usunięcia");
        A("Package", "Pakiet");
        A("State", "Stan");
        A("installed - will be removed", "zainstalowana - zostanie usunięta");
        A("not installed - skipped", "niezainstalowana - pominięto");
        A("needs Explorer restart", "wymaga restartu Eksploratora");
        A("needs sign-out", "wymaga wylogowania");
        A("needs reboot", "wymaga restartu komputera");
        A("no restart needed", "restart niepotrzebny");
        A("No restart needed - the changes are already in effect.", "Restart niepotrzebny - zmiany już działają.");
        A("For the changes to show:", "Aby zmiany były widoczne:");
        A("Restart the computer:", "Uruchom ponownie komputer:");
        A("Sign out and back in:", "Wyloguj się i zaloguj ponownie:");
        A("Restart Explorer (taskbar / Start / shell):", "Uruchom ponownie Eksploratora (pasek zadań / Start / powłoka):");
        A("Restart Explorer now? (y/N)", "Uruchomić ponownie Eksploratora teraz? (y/N)");
        A("Explorer restarted.", "Eksplorator uruchomiony ponownie.");
        A("Cannot restart Explorer:", "Nie można ponownie uruchomić Eksploratora:");
        A("Explorer not restarted - sign out or restart it later for the changes to show.", "Eksplorator nie został uruchomiony ponownie - wyloguj się lub zrestartuj go później, aby zmiany były widoczne.");
        A("Would restart Explorer (kill explorer.exe and start it again).", "Zrestartowałbym Eksploratora (zakończenie explorer.exe i ponowne uruchomienie).");
        A("taskkill explorer.exe failed: {0}", "taskkill explorer.exe nie powiódł się: {0}");
        A("Cannot start explorer.exe: {0}", "Nie można uruchomić explorer.exe: {0}");
        A("It also [bold]uninstalls the recommended bloatware[/] (Cortana, Copilot, Bing apps, consumer Teams, promo apps...) - you confirm the list first.",
          "Dodatkowo [bold]odinstalowuje zalecany bloatware[/] (Cortana, Copilot, aplikacje Bing, Teams konsumencki, aplikacje promocyjne...) - listę najpierw potwierdzasz.");

        return m;
    }
}
