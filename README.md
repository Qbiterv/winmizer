# Winmizer

**Winmizer** to konsolowy optymalizator Windows 10/11 dla graczy (priorytet: Windows 11). Kolorowy interfejs (Spectre.Console), każdy tweak ma opis i poziom ryzyka, przed zmianami powstaje punkt przywracania systemu, a oryginalne wartości są zapisywane, więc wszystko można cofnąć.

> Interfejs jest po polsku, kod i komentarze po angielsku. Program wymaga Windows 10 1809 (build 17763) lub nowszego (x64, ARM64 albo x86) oraz uprawnień administratora do zmian w systemie.

## Funkcje

Menu główne:

1. **Szybka optymalizacja** - zalecany, bezpieczny zestaw (kolumna *Quick* w tabelach poniżej).
2. **Własny wybór** - wybór kategorii, potem tweaków (Bezpieczne/Umiarkowane zaznaczone domyślnie, **Zaawansowane odznaczone**).
3. **Odśmiecanie aplikacji i autostart** - lista aplikacji Appx do usunięcia oraz wyłączanie wpisów autostartu (HKCU/HKLM Run, foldery Autostart) z możliwością przywrócenia.
4. **Plany zasilania** - Winmizer Ultimate, Wysoka wydajność, powrót do Zrównoważonego, dodatkowe ustawienia powercfg.
5. **Informacje o systemie / status** - CPU, GPU, RAM, plan zasilania oraz tabela, które tweaki są już zastosowane.
6. **Cofnij / przywróć** - cofnięcie wszystkiego lub wybranych zmian z `backup.json`, skrót do Przywracania systemu.
7. **Wyjście**.

Po zastosowaniu pokazywane jest podsumowanie (zastosowano / pominięto / błędy, zwolnione miejsce) oraz propozycja restartu.

## Gwarancje bezpieczeństwa

- **Punkt przywracania systemu** jest tworzony przed każdą serią zmian (włączana jest ochrona systemu na C:, a limit 24 h jest tymczasowo zdejmowany przez `SystemRestorePointCreationFrequency=0` i potem przywracany). Jeśli się nie uda, program pyta, czy kontynuować.
- **Kopia oryginałów** każdego tweaku trafia do `%ProgramData%\Winmizer\backup.json` (wartości rejestru, typy startu usług, stany zadań, plany zasilania, DNS...). Zapisywany jest tylko *pierwszy* oryginał, więc ponowne zastosowanie nie nadpisze prawdziwego stanu.
- **Dziennik** działań: `%ProgramData%\Winmizer\logs\winmizer-*.log`.
- Każdy tweak działa w `try/catch` - błąd jednego nie przerywa reszty.
- Tweaki wykrywają, czy są już zastosowane (`IsApplied`) i wtedy są pomijane.
- **Chronione usługi** (kod odmawia ich zmiany): `Spooler`, `PrintNotify`, Windows Update (`wuauserv`, `UsoSvc`, `BITS`), Defender (`WinDefend`, `Sense`, `SecurityHealthService`), sieć/Wi-Fi/Bluetooth (`Dhcp`, `Dnscache`, `WlanSvc`, `bthserv`...), audio, Xbox/Gaming Services, Windows Hello, `TabletInputService` i inne.
- **Chronione aplikacje** (nie da się ich dodać do listy usuwania): Microsoft Store, App Installer/winget, Kalkulator, Zdjęcia, Notatnik, Terminal, Paint, Narzędzie do wycinania, Aparat, tożsamość Xbox, Gaming Services, Xbox TCUI, SecHealthUI (Defender), rozszerzenia kodeków i biblioteki uruchomieniowe.

### Co NIE jest ruszane

- **Drukowanie** działa bez zmian: Spooler, PrintNotify, sterowniki drukarek i "Drukuj do PDF". Wyłączana jest wyłącznie usługa *Faks* (to nie jest drukowanie).
- Sieć, Wi-Fi, Bluetooth, dźwięk, USB, sterowniki, Windows Hello.
- **Windows Update** - wyłączamy tylko udostępnianie P2P (Delivery Optimization); aktualizacje działają normalnie.
- **Windows Defender** pozostaje włączony.
- **Sklep Microsoft + App Installer/winget** zostają (potrzebne do reinstalacji aplikacji).
- **Xbox**: tożsamość i Gaming Services zostają (gry z Game Pass). Opcjonalnie można usunąć tylko nakładkę Game Bar.
- Żadnych hacków wyłączających zabezpieczenia Spectre/Meltdown, UAC ani innych niestabilnych modyfikacji.

### Czego nie da się cofnąć automatycznie

- **Usunięte aplikacje Appx** - przywracasz je ze sklepu Microsoft Store lub przez `winget install`. Program próbuje je ponownie zarejestrować, jeśli pliki jeszcze istnieją.
- **Akcje czyszczenia** (pliki tymczasowe, Kosz, cache) - to usunięcie danych.
- **Deinstalacja OneDrive** jest cofana przez ponowną instalację (OneDriveSetup/winget). Pliki istniejące wyłącznie w chmurze nie są kasowane, ale lokalna synchronizacja się zatrzymuje.

## Budowanie

Wymagania: .NET 8 SDK (`winget install Microsoft.DotNet.SDK.8`).

```powershell
# zwykła kompilacja
dotnet build

# trzy wydania single-file (self-contained, Release): win-x64, win-arm64, win-x86
.\build.ps1

# tylko wybrany system
.\build.ps1 -Runtime win-x64
```

`build.ps1` wykonuje `dotnet publish -c Release -r <rid> --self-contained true -p:PublishSingleFile=true` dla każdego RID i tworzy:

| RID | Plik | Dla |
|-----|------|-----|
| `win-x64` | `publish\win-x64\Winmizer.exe` | Windows 10/11 64-bit (Intel/AMD) |
| `win-arm64` | `publish\win-arm64\Winmizer.exe` | Windows 11 / 10 na ARM |
| `win-x86` | `publish\win-x86\Winmizer.exe` | Windows 10 32-bit |

oraz archiwa `publish\Winmizer-<wersja>-<rid>.zip` (wersja z `Winmizer.csproj`, obecnie 1.0.0).

Uprawnienia: manifest ma `asInvoker`, a program sam prosi o podniesienie uprawnień (UAC) przy prawdziwych zmianach. `--list`, `--help` i `--dry-run` działają bez UAC.

## Instalacja i skrót na pulpicie

```powershell
# kopiuje exe pasujący do architektury komputera i tworzy skróty (nic nie uruchamia)
.\install.ps1

# usunięcie skrótów i zainstalowanej kopii
.\install.ps1 -Uninstall
```

`install.ps1` kopiuje `Winmizer.exe` (wersję x64/ARM64/x86 zależnie od systemu) do `%LOCALAPPDATA%\Programs\Winmizer\` - poza repozytorium, więc ponowne budowanie niczego nie psuje - i tworzy skróty z ikoną programu:

- Pulpit bieżącego użytkownika: `Winmizer.lnk` (także gdy pulpit jest przekierowany do OneDrive),
- Menu Start: `%APPDATA%\Microsoft\Windows\Start Menu\Programs\Winmizer.lnk`.

Aby zaktualizować program, uruchom `.\build.ps1`, a potem `.\install.ps1`. Skrót uruchamia program z prośbą o UAC; kopie zapasowe i dzienniki w `%ProgramData%\Winmizer` zostają po odinstalowaniu.

Ikona (`src\Winmizer\Assets\winmizer.ico`) jest generowana skryptem `tools\make-icon.ps1` (System.Drawing).

## Zgodność z Windows 10 i 11

System jest wykrywany po numerze builda (>= 22000 = Windows 11); wersja, edycja i build są widoczne na banerze i w ekranie *Informacje o systemie*.

- **Windows 11** jest głównym celem - jego zachowanie jest takie samo jak wcześniej.
- **Windows 10**: minimalna obsługiwana wersja to 1809 (build 17763). Na starszych buildach program pokazuje ostrzeżenie.
- Każdy tweak może mieć ograniczenie systemu (`SupportedOn`: Windows 10 / Windows 11 / oba) i minimalny build. Tweaki niedostępne w danym systemie są ukrywane w menu i pomijane w trybie wsadowym, a w `--list` oraz w podsumowaniu mają status **N/D w tym systemie** - nigdy nie kończą się błędem.

| Tweak | Windows 11 | Windows 10 |
|-------|-----------|------------|
| Zasada Copilot (`privacy.copilot`), Widżety (`visual.widgets`) | tak | N/D |
| Appx: Clipchamp, Teams (nowy `MSTeams` i `MicrosoftTeams`), Copilot, Bing Search | tak | N/D |
| Klucze Start `Start_IrisRecommendations` / `Start_AccountNotifications` (w `privacy.startads`) | zapisywane | pomijane |
| Cortana: ekran blokady/zgoda (`privacy.cortana.win10`) | N/D | tak |
| Wiadomości i zainteresowania (`privacy.newsfeed`, zasada `EnableFeeds=0`, build 19041+) | N/D | tak |
| Pasek Osoby (`visual.peoplebar`), Meet Now (`visual.meetnow`) | N/D | tak |
| Appx: Przeglądarka 3D, Paint 3D, Print 3D | N/D | tak |
| HAGS (`gaming.hags`) | tak (wymaga GPU WDDM 2.7+) | od 2004 (build 19041) i GPU WDDM 2.7+ |
| Optymalizacje okienkowe / VRR (`gaming.vrr`) | tak | od 1903 (build 18362), wsparcie częściowe |

Zasada `AllowCortana=0`, plany zasilania, usługi, sieć i czyszczenie działają na obu systemach.
## Parametry wiersza poleceń

| Parametr | Działanie |
|----------|-----------|
| *(brak)* | interaktywne menu |
| `--list` | lista wszystkich tweaków i wykrytego statusu, także N/D dla bieżącego systemu (nic nie zmienia, nie wymaga admina) |
| `--dry-run` | tryb podglądu: logi "co by się stało", brak zmian w systemie, brak plików w ProgramData |
| `--dry-run --quick` | podgląd zestawu Szybka optymalizacja |
| `--dry-run --apply id1,id2` / `all` | podgląd wybranych tweaków |
| `--quick --yes` / `--apply id1,id2 --yes` | zastosuj bez pytań (wymaga admina) |
| `--revert-all --yes` | cofnij wszystko z `backup.json` |
| `--list-md` | tabele tweaków w formacie Markdown |

## Architektura

```
Winmizer.sln
build.ps1
src/Winmizer/
  Program.cs               punkt wejścia, parametry CLI
  app.manifest             requireAdministrator
  Core/                    ITweak, TweakBase, RegistryTweak, TweakEngine, RestorePoint,
                           BackupStore (backup.json), Restorer, Reg/Proc/PS, ServiceHelper, PowerCfg, StartupManager
  Tweaks/<Kategoria>/      Gaming, Power, Privacy, Services, Network, Visual, Storage, Cleanup, Debloat, OneDrive
  Tweaks/TweakCatalog.cs   lista wszystkich tweaków
  UI/                      Spectre.Console: menu i renderowanie
```

`ITweak` udostępnia `Id, Name, Description, Category, RiskLevel (Safe/Moderate/Advanced), IsApplied(), Apply(), Revert()`.

## Lista tweaków

Poziomy ryzyka: **Bezpieczny**, **Umiarkowany**, **Zaawansowany** (domyślnie odznaczony). Kolumna *Quick* oznacza udział w Szybkiej optymalizacji (na laptopach pomijane są ustawienia zasilania obciążające baterię). Odśmiecanie aplikacji i OneDrive nie wchodzą do trybu Quick.

### Gaming (10)

| ID | Tweak | Ryzyko | Quick | Opis |
|----|-------|--------|:-----:|------|
| `gaming.gamemode` | Tryb gry (Game Mode) włączony | Bezpieczny | tak | Windows priorytetyzuje grę i ogranicza zadania w tle (Windows Update nie instaluje sterowników w trakcie gry). |
| `gaming.hags` | Planowanie GPU przyspieszane sprzętowo (HAGS) | Umiarkowany |  | Ustawia HwSchMode=2. Może zmniejszyć opóźnienia i obciążenie CPU; wymaga wspieranej karty i sterownika. Wymaga restartu. **Uwaga:** Na starszych GPU/sterownikach może powodować artefakty - w razie problemów użyj Cofnij. *[Wymaga Windows 10 2004+ (build 19041) oraz karty z WDDM 2.7+ i nowego sterownika; bez tego ustawienie jest ignorowane.]* |
| `gaming.gamedvr` | Wyłącz Game DVR / nagrywanie w tle | Bezpieczny | tak | Wyłącza nagrywanie w tle Xbox Game Bar (GameDVR_Enabled=0, AllowGameDVR=0). Sam Game Bar i usługi Xbox pozostają. |
| `gaming.fso` | Wyłącz optymalizacje pełnoekranowe globalnie | Zaawansowany |  | Ustawia globalnie zachowanie FSE (GameDVR_FSEBehaviorMode=2). Część gier działa płynniej w prawdziwym trybie pełnoekranowym, inne gorzej (alt-tab). |
| `gaming.mmcss` | MMCSS: responsywność systemu i sieć | Bezpieczny | tak | SystemResponsiveness=10 (nie 0 - zostawia zapas dla audio/systemu) oraz NetworkThrottlingIndex=0xFFFFFFFF (wyłącza dławienie sieci multimediów). |
| `gaming.mmcss.games` | MMCSS: profil zadań "Games" | Bezpieczny | tak | Priorytet GPU=8, Priority=6, Scheduling Category=High, SFIO Priority=High dla procesów gier. |
| `gaming.prioritysep` | Win32PrioritySeparation = 0x26 | Umiarkowany |  | Zwiększa udział czasu CPU dla aplikacji pierwszoplanowej (gry) względem procesów w tle. |
| `gaming.mouse` | Wyłącz przyspieszenie myszy | Bezpieczny | tak | Wyłącza "Zwiększ precyzję wskaźnika" (MouseSpeed/Threshold = 0) - stała, przewidywalna czułość w grach FPS. |
| `gaming.stickykeys` | Wyłącz okna Sticky/Filter/Toggle Keys | Bezpieczny | tak | Wyłącza skróty uruchamiające okna dialogowe (5x Shift itd.), które potrafią zminimalizować grę. |
| `gaming.vrr` | Optymalizacje gier w oknie + VRR | Bezpieczny | tak | Włącza SwapEffectUpgradeEnable i VRROptimizeEnable (DirectX) - mniejsze opóźnienie w grach okienkowych i zmienne odświeżanie. |

### Zasilanie (7)

| ID | Tweak | Ryzyko | Quick | Opis |
|----|-------|--------|:-----:|------|
| `power.ultimate` | Plan zasilania: Winmizer Ultimate (Ultimate Performance) | Umiarkowany | tak | Tworzy i aktywuje plan Ultimate Performance (najniższe opóźnienia). Oryginalny plan jest zapamiętany i można do niego wrócić. **Uwaga:** Laptopy: plan zużywa baterię znacznie szybciej - zalecany tylko przy zasilaniu z sieci. |
| `power.highperf` | Plan zasilania: Wysoka wydajność | Bezpieczny |  | Aktywuje wbudowany plan Wysoka wydajność (lżejsza alternatywa dla Ultimate). Można wrócić do Zrównoważonego. **Uwaga:** Laptopy: krótszy czas pracy na baterii. |
| `power.usbsuspend` | Wyłącz selektywne zawieszanie USB | Umiarkowany | tak | Zapobiega usypianiu portów USB (mniej zacięć myszy/klawiatury/padów). Na laptopach tylko zasilanie sieciowe (AC). |
| `power.pcie` | Wyłącz oszczędzanie energii łącza PCIe | Umiarkowany | tak | Link State Power Management = Wyłączone - mniejsze opóźnienia GPU/NVMe. Na laptopach tylko AC. |
| `power.procmin` | Minimalny stan procesora 100% (AC) | Umiarkowany | tak | Procesor nie obniża taktowania na zasilaniu sieciowym - stabilniejszy FPS, wyższy pobór prądu i temperatura w spoczynku. |
| `power.faststartup` | Wyłącz szybkie uruchamianie (Fast Startup) | Umiarkowany |  | Pełne zamknięcie systemu przy "Zamknij" - czysty stan sterowników i mniej problemów po aktualizacjach. Start może być nieco wolniejszy. |
| `power.hibernate` | Wyłącz hibernację (zwalnia miejsce) | Zaawansowany |  | powercfg -h off usuwa hiberfil.sys (kilka-kilkanaście GB). Wyłącza też Fast Startup i hibernację. **Uwaga:** Laptopy: utrata możliwości hibernacji. |

### Prywatność i telemetria (13)

| ID | Tweak | Ryzyko | Quick | Opis |
|----|-------|--------|:-----:|------|
| `privacy.telemetry` | Telemetria: poziom podstawowy | Bezpieczny | tak | AllowTelemetry=1 (Podstawowe/Wymagane). Wartość 0 działa tylko w Enterprise, dlatego jej nie używamy. Usługi DiagTrack wyłączasz w kategorii Usługi. |
| `privacy.adid` | Wyłącz identyfikator reklamowy | Bezpieczny | tak | Aplikacje nie mogą używać Advertising ID do profilowania reklam. |
| `privacy.tailored` | Wyłącz spersonalizowane środowiska | Bezpieczny | tak | Microsoft nie używa danych diagnostycznych do dopasowywania porad i reklam. |
| `privacy.activity` | Wyłącz historię aktywności | Bezpieczny | tak | Wyłącza zbieranie i wysyłanie historii aktywności (Oś czasu). |
| `privacy.consumer` | Wyłącz funkcje konsumenckie i sugerowane aplikacje | Bezpieczny | tak | DisableWindowsConsumerFeatures=1 - Windows nie instaluje automatycznie promowanych aplikacji (Candy Crush itp.). |
| `privacy.startads` | Wyłącz reklamy i sugestie w Menu Start | Bezpieczny | tak | Usuwa sugerowane aplikacje/reklamy w Start i Ustawieniach oraz polecane elementy. |
| `privacy.lockscreen` | Wyłącz wskazówki i reklamy na ekranie blokady | Bezpieczny | tak | Ekran blokady bez "ciekawostek", Spotlight-reklam i wskazówek. |
| `privacy.tasks` | Wyłącz zadania telemetryczne | Bezpieczny | tak | Wyłącza harmonogramowane zadania: Compatibility Appraiser, ProgramDataUpdater, Consolidator, UsbCeip, DmClient. Windows Update nie jest naruszony. |
| `privacy.cortana` | Wyłącz Cortanę (zasada AllowCortana=0) | Bezpieczny | tak | Wyłącza Cortanę przez zasadę systemową. Bezpieczne - Wyszukiwanie Windows działa normalnie. |
| `privacy.copilot` | Wyłącz Windows Copilot (zasada) | Bezpieczny | tak | TurnOffWindowsCopilot=1 - ukrywa i wyłącza Copilot w systemie. *[tylko Windows 11]* |
| `privacy.cortana.win10` | Windows 10: ogranicz Cortanę (ekran blokady, zgoda) | Bezpieczny | tak | Cortana na Windows 10 to osobny komponent: blokuje ją na ekranie blokady (AllowCortanaAboveLock=0) i cofa zgodę użytkownika (CortanaConsent=0). Uzupełnia zasadę AllowCortana=0. *[tylko Windows 10]* |
| `privacy.newsfeed` | Windows 10: wyłącz "Wiadomości i zainteresowania" na pasku zadań | Bezpieczny | tak | Zasada EnableFeeds=0 usuwa widżet pogody/wiadomości z paska zadań (reklamy i śledzenie MSN, zużycie RAM przez Edge WebView). *[tylko Windows 10]* |
| `privacy.websearch` | Wyłącz wyszukiwanie internetowe (Bing) w Start | Bezpieczny | tak | Wyszukiwanie w Start pokazuje tylko wyniki lokalne (BingSearchEnabled=0, DisableSearchBoxSuggestions=1). |

### Usługi (10)

| ID | Tweak | Ryzyko | Quick | Opis |
|----|-------|--------|:-----:|------|
| `services.diagtrack` | Telemetria: Connected User Experiences and Telemetry (DiagTrack) | Bezpieczny | tak | Główna usługa wysyłająca dane diagnostyczne do Microsoft. Nic w systemie od niej nie zależy. |
| `services.dmwappushservice` | Telemetria: WAP Push Message Routing (dmwappushservice) | Bezpieczny | tak | Pomocnicza usługa telemetrii/MDM. Niepotrzebna na komputerze domowym. |
| `services.fax` | Faks (Fax) | Bezpieczny | tak | Usługa FAKSU - nie ma nic wspólnego z drukowaniem. Drukarki i "Drukuj do PDF" działają bez zmian. |
| `services.retaildemo` | Tryb demonstracyjny sklepu (RetailDemo) | Bezpieczny | tak | Używany tylko w egzemplarzach pokazowych w sklepach. |
| `services.mapsbroker` | Pobrane mapy (MapsBroker) -> ręczny | Bezpieczny | tak | Aktualizacje map offline; uruchamiany tylko gdy potrzebny. |
| `services.wersvc` | Raportowanie błędów Windows (WerSvc) -> ręczny | Bezpieczny | tak | Wysyłanie raportów awarii do Microsoft; uruchamia się na żądanie. |
| `services.remoteregistry` | Zdalny rejestr (RemoteRegistry) | Bezpieczny | tak | Zdalna edycja rejestru z sieci - ryzyko bezpieczeństwa, niepotrzebna w domu. (W Windows 11 domyślnie już wyłączona.) |
| `services.sysmain` | SysMain (Superfetch) -> wyłączona | Umiarkowany |  | Wstępnie ładuje aplikacje do RAM. Na dysku SSD/NVMe zbędna i generuje zapisy; na HDD może pogorszyć start aplikacji. |
| `services.lfsvc` | Usługa lokalizacji geograficznej (lfsvc) | Umiarkowany |  | Wyłącza usługę lokalizacji - aplikacje (Mapy, Pogoda) nie ustalą położenia. |
| `services.wsearch` | Windows Search (WSearch) -> ręczny | Zaawansowany |  | Indeksowanie plików. Ręczny start = wolniejsze wyszukiwanie w Eksploratorze/Start, mniejsze obciążenie dysku. |

### Sieć (3)

| ID | Tweak | Ryzyko | Quick | Opis |
|----|-------|--------|:-----:|------|
| `network.nagle` | Wyłącz algorytm Nagle'a (niższy ping) | Umiarkowany |  | TcpAckFrequency=1 i TCPNoDelay=1 na każdym interfejsie - pakiety gier wysyłane od razu. Wpływ na ping jest niewielki; nie rusza Wi-Fi ani sterowników. |
| `network.dop2p` | Delivery Optimization: wyłącz udostępnianie P2P | Bezpieczny | tak | DODownloadMode=0 - aktualizacje pobierane tylko z serwerów Microsoft (bez wysyłania ich innym komputerom). Windows Update działa normalnie. |
| `network.dns` | DNS Cloudflare (1.1.1.1 / 1.0.0.1) | Zaawansowany |  | Ustawia szybki publiczny DNS na aktywnych kartach sieciowych. Poprzednie ustawienia są zapamiętane. **Uwaga:** Zmienia dostawcę DNS - w sieciach firmowych/z filtrowaniem rodzicielskim może coś zablokować. |

### Wygląd i interfejs (8)

| ID | Tweak | Ryzyko | Quick | Opis |
|----|-------|--------|:-----:|------|
| `visual.effects` | Efekty wizualne pod wydajność (z zachowaniem czcionek i miniatur) | Bezpieczny | tak | Wyłącza animacje okien i paska zadań oraz cienie/zaznaczenia przezroczyste. Wygładzanie czcionek (ClearType) i miniatury pozostają włączone. |
| `visual.transparency` | Wyłącz efekty przezroczystości | Bezpieczny |  | Wyłącza akryl/przezroczystość w Start i oknach - odrobinę mniej obciążony GPU. |
| `visual.startupdelay` | Usuń opóźnienie uruchamiania aplikacji autostartu | Bezpieczny | tak | Serialize\StartupDelayInMSec=0 - programy autostartu startują od razu po zalogowaniu. |
| `visual.menudelay` | Menu bez opóźnienia (MenuShowDelay=0) | Bezpieczny | tak | Menu rozwijane pojawiają się natychmiast. |
| `visual.bgapps` | Zablokuj aplikacje działające w tle | Umiarkowany |  | Zasada LetAppsRunInBackground=2 - aplikacje Sklepu nie pracują w tle (oszczędza RAM/CPU). Mogą przestać przychodzić powiadomienia z takich aplikacji. |
| `visual.widgets` | Windows 11: wyłącz Widżety (zasada) | Bezpieczny |  | Zasada AllowNewsAndInterests=0 wyłącza panel Widżetów (pogoda, wiadomości, reklamy) i proces Widgets.exe działający w tle. *[tylko Windows 11]* |
| `visual.peoplebar` | Windows 10: ukryj pasek Osoby (People) | Bezpieczny | tak | Usuwa ikonę Osoby z paska zadań (PeopleBand=0, HidePeopleBar=1). W Windows 11 ten element nie istnieje. *[tylko Windows 10]* |
| `visual.meetnow` | Windows 10: ukryj ikonę Meet Now | Bezpieczny | tak | Ukrywa przycisk Meet Now (Skype) w zasobniku paska zadań (HideSCAMeetNow=1). *[tylko Windows 10]* |

### Dysk (2)

| ID | Tweak | Ryzyko | Quick | Opis |
|----|-------|--------|:-----:|------|
| `storage.trim` | Upewnij się, że TRIM jest włączony | Bezpieczny | tak | fsutil behavior set DisableDeleteNotify 0 - system informuje SSD o zwolnionych blokach (wydajność i żywotność). |
| `storage.lastaccess` | Wyłącz znacznik ostatniego dostępu NTFS | Bezpieczny | tak | fsutil behavior set disablelastaccess 1 - mniej zapisów na dysk przy każdym odczycie pliku. |

### Czyszczenie (4)

| ID | Tweak | Ryzyko | Quick | Opis |
|----|-------|--------|:-----:|------|
| `cleanup.temp` | Wyczyść pliki tymczasowe (%TEMP% i Windows\Temp) | Bezpieczny | tak | Usuwa pliki tymczasowe użytkownika i systemu. Pliki używane przez działające programy są pomijane. Prefetch NIE jest czyszczony. |
| `cleanup.shader` | Wyczyść pamięć podręczną shaderów (DirectX/NVIDIA/AMD) | Umiarkowany |  | Usuwa D3DSCache, NVIDIA DXCache/GLCache i AMD DxCache. Gry skompilują shadery od nowa - pierwsze uruchomienie może się lekko dławić. |
| `cleanup.recyclebin` | Opróżnij Kosz | Umiarkowany |  | Trwale usuwa wszystko z Kosza. **Uwaga:** Operacja nieodwracalna - sprawdź zawartość Kosza. |
| `cleanup.dism` | DISM: czyszczenie składników (StartComponentCleanup) | Zaawansowany |  | DISM /Online /Cleanup-Image /StartComponentCleanup - usuwa stare wersje składników systemu. Bezpieczne, ale trwa kilka-kilkanaście minut. |

### Odśmiecanie aplikacji (37)

| ID | Tweak | Ryzyko | Quick | Opis |
|----|-------|--------|:-----:|------|
| `debloat.microsoft.549981c3f5f10` | Cortana | Bezpieczny |  | Stary asystent głosowy Cortana. Na Windows 10 usuwa tylko aplikację; sam komponent systemowy ogranicza zasada w Prywatności. |
| `debloat.microsoft.bingnews` | Microsoft News (Bing News) | Bezpieczny |  | Aplikacja z wiadomościami. |
| `debloat.microsoft.bingweather` | Pogoda (Bing Weather) | Bezpieczny |  | Aplikacja pogodowa MSN. |
| `debloat.microsoft.bingfinance` | Finanse (Bing Finance) | Bezpieczny |  | Aplikacja finansowa MSN. |
| `debloat.microsoft.bingsports` | Sport (Bing Sports) | Bezpieczny |  | Aplikacja sportowa MSN. |
| `debloat.microsoft.bingsearch` | Bing Search (dodatek do Start) | Bezpieczny |  | Integracja wyszukiwania Bing. *[tylko Windows 11]* |
| `debloat.microsoft.gethelp` | Uzyskaj pomoc (Get Help) | Bezpieczny |  | Aplikacja pomocy technicznej. |
| `debloat.microsoft.getstarted` | Porady (Tips) | Bezpieczny |  | Aplikacja z poradami Windows. |
| `debloat.microsoft.microsoftofficehub` | Microsoft 365 (Office Hub) | Bezpieczny |  | Reklamowy skrót do pakietu Office. |
| `debloat.microsoft.microsoftsolitairecollection` | Microsoft Solitaire Collection | Bezpieczny |  | Gry karciane z reklamami. |
| `debloat.microsoft.people` | Osoby (People) | Bezpieczny |  | Książka adresowa. |
| `debloat.microsoft.windowsfeedbackhub` | Centrum opinii (Feedback Hub) | Bezpieczny |  | Wysyłanie opinii do Microsoft. |
| `debloat.microsoft.windowsmaps` | Mapy Windows | Bezpieczny |  | Aplikacja map. |
| `debloat.clipchamp.clipchamp` | Clipchamp | Bezpieczny |  | Edytor wideo Microsoft. *[tylko Windows 11]* |
| `debloat.microsoft.todos` | Microsoft To Do | Bezpieczny |  | Lista zadań. |
| `debloat.microsoft.powerautomatedesktop` | Power Automate | Bezpieczny |  | Automatyzacja zadań. |
| `debloat.microsoftteams` | Microsoft Teams (konsumencki) | Bezpieczny |  | Wbudowany Teams (Chat). *[tylko Windows 11]* |
| `debloat.msteams` | Microsoft Teams (nowy) | Bezpieczny |  | Nowa wersja Teams preinstalowana w systemie. *[tylko Windows 11]* |
| `debloat.microsoft.copilot` | Microsoft Copilot | Bezpieczny |  | Aplikacja Copilot. *[tylko Windows 11]* |
| `debloat.microsoft.windows.ai.copilot.provider` | Dostawca Windows Copilot | Bezpieczny |  | Komponent Copilot. *[tylko Windows 11]* |
| `debloat.microsoft.skypeapp` | Skype | Bezpieczny |  | Komunikator Skype. |
| `debloat.microsoft.mixedreality.portal` | Portal rzeczywistości mieszanej | Bezpieczny |  | Dla gogli VR Windows MR. |
| `debloat.microsoft.microsoft3dviewer` | Przeglądarka 3D (3D Viewer) | Bezpieczny |  | Windows 10: przeglądarka modeli 3D. *[tylko Windows 10]* |
| `debloat.microsoft.mspaint` | Paint 3D | Bezpieczny |  | Windows 10: Paint 3D (to nie jest klasyczny Paint - ten zostaje). *[tylko Windows 10]* |
| `debloat.microsoft.print3d` | Print 3D | Bezpieczny |  | Windows 10: aplikacja do druku 3D. *[tylko Windows 10]* |
| `debloat.disney` | Disney+ | Bezpieczny |  | Preinstalowana aplikacja promocyjna. |
| `debloat.spotifyab.spotifymusic` | Spotify | Bezpieczny |  | Preinstalowana aplikacja promocyjna (możesz ją zainstalować ponownie). |
| `debloat.tiktok` | TikTok | Bezpieczny |  | Preinstalowana aplikacja promocyjna. |
| `debloat.candycrush` | Candy Crush i podobne gry | Bezpieczny |  | Promocyjne gry (Candy Crush Saga/Soda itp.). |
| `debloat.facebook` | Facebook | Bezpieczny |  | Preinstalowana aplikacja promocyjna. |
| `debloat.instagram` | Instagram | Bezpieczny |  | Preinstalowana aplikacja promocyjna. |
| `debloat.twitter` | Twitter / X | Bezpieczny |  | Preinstalowana aplikacja promocyjna. |
| `debloat.microsoft.zunemusic` | Odtwarzacz multimediów (Groove/ZuneMusic) | Umiarkowany |  | W Windows 11 to nowy Media Player - usuń tylko jeśli używasz innego odtwarzacza. |
| `debloat.microsoft.zunevideo` | Filmy i TV (ZuneVideo) | Umiarkowany |  | Aplikacja do filmów. |
| `debloat.microsoft.yourphone` | Łączność z telefonem (Phone Link) | Umiarkowany |  | Usuń, jeśli nie łączysz telefonu z Windows. |
| `debloat.microsoft.windowsalarms` | Zegar i alarmy | Umiarkowany |  | Alarmy, minutniki, stoper. |
| `debloat.microsoft.xboxgamingoverlay` | Xbox Game Bar (nakładka) | Umiarkowany |  | Nakładka Win+G. Tożsamość Xbox i Gaming Services zostają (potrzebne grom z Game Pass). **Uwaga:** Gry z Game Pass nadal działają, ale stracisz nakładkę Win+G. |

### OneDrive (4)

| ID | Tweak | Ryzyko | Quick | Opis |
|----|-------|--------|:-----:|------|
| `onedrive.policy` | OneDrive: wyłącz synchronizację zasadą (DisableFileSyncNGSC) | Umiarkowany |  | Zasada blokująca uruchamianie synchronizacji plików OneDrive. Pliki lokalne pozostają nietknięte. **Uwaga:** Synchronizacja z chmurą zostanie zatrzymana. Pliki dostępne tylko online nie są usuwane. |
| `onedrive.navpane` | OneDrive: usuń z panelu nawigacji Eksploratora | Bezpieczny |  | Ukrywa ikonę OneDrive w lewym panelu Eksploratora plików. |
| `onedrive.startup` | OneDrive: usuń z autostartu | Bezpieczny |  | Usuwa wpisy Run uruchamiające OneDrive przy logowaniu (kopia zapasowa pozwala je przywrócić). |
| `onedrive.uninstall` | OneDrive: pełna deinstalacja | Umiarkowany |  | Zamyka proces OneDrive i uruchamia oficjalny deinstalator (OneDriveSetup.exe /uninstall, awaryjnie winget). **Uwaga:** Lokalna synchronizacja przestanie działać. Pliki istniejące WYŁĄCZNIE w chmurze nie są kasowane, ale stają się niedostępne z tego komputera do czasu ponownej instalacji. Najpierw upewnij się, że pliki są pobrane lokalnie. |

## Autostart

Wpisy rejestru (`HKCU/HKLM ...\Run`, także 32-bit) są przenoszone do `HKxx\SOFTWARE\Winmizer\DisabledStartup\...`, a skróty z folderów Autostart do `%ProgramData%\Winmizer\DisabledStartup`. Oryginały zapisuje `backup.json`, więc *Cofnij / przywróć* przywraca je na miejsce.

## Znane ograniczenia

- Wykrywanie aplikacji Appx bez uprawnień administratora (np. `--list`) obejmuje tylko bieżącego użytkownika.
- Plan Ultimate Performance bywa niedostępny w niektórych konfiguracjach - wtedy użyj planu Wysoka wydajność.
- HAGS, Nagle i inne ustawienia sprzętowe mają różny wpływ zależnie od sprzętu; wszystkie są odwracalne.
- Program nie był uruchamiany w trybie zmieniającym system podczas tworzenia (tylko `--list`/`--dry-run`).
