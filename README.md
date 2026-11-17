<div align="center">

<img src="docs/icon.png" alt="Winmizer" width="128" height="128">

# Winmizer

**Konsolowy optymalizator Windows 10/11 dla graczy: bezpieczny, w pełni odwracalny i po polsku.**

[![CI](https://github.com/Qbiterv/winmizer/actions/workflows/ci.yml/badge.svg)](https://github.com/Qbiterv/winmizer/actions/workflows/ci.yml)
[![Windows 10/11](https://img.shields.io/badge/Windows-10%20%7C%2011-0078D4?logo=windows&logoColor=white)](#zgodność-z-windows-10-i-11)
[![.NET 8](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/download/dotnet/8.0)
[![Licencja: MIT](https://img.shields.io/badge/licencja-MIT-green.svg)](LICENSE)
[![Najnowsze wydanie](https://img.shields.io/github/v/release/Qbiterv/winmizer?label=wydanie&color=cyan)](https://github.com/Qbiterv/winmizer/releases/latest)

[**Pobierz najnowsze wydanie**](https://github.com/Qbiterv/winmizer/releases/latest) · [Instalacja](#instalacja) · [Bezpieczeństwo](#bezpieczeństwo) · [FAQ](#faq)

</div>

---

## O programie

**Winmizer** to kolorowa aplikacja konsolowa (Spectre.Console), która poprawia wydajność Windows w grach i usuwa zbędny „bloat". Każdy tweak ma opis i poziom ryzyka, przed zmianami powstaje punkt przywracania systemu, a oryginalne wartości są zapisywane, więc **wszystko można cofnąć**.

> Interfejs jest po polsku, kod i komentarze po angielsku. Wymagany jest Windows 10 1809 (build 17763) lub nowszy (x64, ARM64 albo x86) oraz uprawnienia administratora do zmian w systemie.

## Funkcje

- **Szybka optymalizacja** - zalecany, bezpieczny zestaw jednym kliknięciem.
- **Własny wybór** - kategorie, potem pojedyncze tweaki (Bezpieczne i Umiarkowane zaznaczone domyślnie, Zaawansowane odznaczone).
- **Gaming** - Game Mode, HAGS, MMCSS, wyłączenie przyspieszenia myszy i Game DVR.
- **Zasilanie** - plan Ultimate Performance / Wysoka wydajność, wyłączenie USB suspend i oszczędzania PCIe.
- **Prywatność i telemetria** - ograniczenie telemetrii, reklam, Cortany, Copilota i wyszukiwania Bing w Start.
- **Usługi, sieć, wygląd, dysk, czyszczenie** - ponad 40 dodatkowych ustawień (lista poniżej).
- **Odśmiecanie aplikacji i autostartu** - usuwanie preinstalowanych aplikacji Appx oraz wyłączanie wpisów autostartu z możliwością przywrócenia.
- **Informacje o systemie** - CPU, GPU, RAM, plan zasilania i status każdego tweaku.
- **Cofnij / przywróć** - cofnięcie wszystkiego lub wybranych zmian z `backup.json`.
- **Tryb CLI** - `--list`, `--dry-run`, `--quick`, `--apply`, `--revert-all` do automatyzacji.

Po zastosowaniu zmian program pokazuje podsumowanie (zastosowano / pominięto / błędy, zwolnione miejsce) i proponuje restart.

## Bezpieczeństwo

- **Punkt przywracania systemu** jest tworzony przed każdą serią zmian. Włączana jest ochrona systemu na C:, a limit 24 h jest tymczasowo zdejmowany (`SystemRestorePointCreationFrequency=0`) i potem przywracany. Jeśli się nie uda, program pyta, czy kontynuować.
- **Kopia oryginałów** każdego tweaku trafia do `%ProgramData%\Winmizer\backup.json` (wartości rejestru, typy startu usług, stany zadań, plany zasilania, DNS...). Zapisywany jest tylko *pierwszy* oryginał, więc ponowne zastosowanie nie nadpisze prawdziwego stanu.
- **Dziennik** działań: `%ProgramData%\Winmizer\logs\winmizer-*.log`.
- Każdy tweak działa w `try/catch` - błąd jednego nie przerywa reszty. Tweaki wykrywają, czy są już zastosowane, i wtedy są pomijane.
- **`--dry-run`** pokazuje, co by się stało, bez żadnych zmian w systemie.

### Czego Winmizer nigdy nie rusza

| Obszar | Co zostaje bez zmian |
|--------|----------------------|
| **Drukowanie** | Spooler, PrintNotify, sterowniki drukarek, „Drukuj do PDF" (wyłączana jest tylko usługa *Faks*) |
| **Sieć** | Wi-Fi, Bluetooth, DHCP, DNS Client (chronione usługi; opcjonalnie tylko zmiana serwerów DNS, z kopią) |
| **Windows Defender** | pozostaje włączony (`WinDefend`, `Sense`, `SecurityHealthService`) |
| **Sklep Microsoft** | Store i App Installer/winget zostają (potrzebne do reinstalacji aplikacji) |
| **Xbox** | tożsamość Xbox i Gaming Services zostają (gry z Game Pass); opcjonalnie tylko nakładka Game Bar |
| **Windows Update** | aktualizacje działają normalnie (wyłączamy tylko udostępnianie P2P - Delivery Optimization) |
| **Pozostałe** | dźwięk, USB, sterowniki, Windows Hello, UAC, zabezpieczenia Spectre/Meltdown |

Kod **odmawia zmiany chronionych usług** i **nie pozwala dodać chronionych aplikacji** (Store, winget, Kalkulator, Zdjęcia, Notatnik, Terminal, Paint, Narzędzie do wycinania, Aparat, Xbox/Gaming Services, SecHealthUI, kodeki) do listy usuwania.

### Czego nie da się cofnąć automatycznie

- **Usunięte aplikacje Appx** - przywrócisz je ze Sklepu Microsoft lub przez `winget install` (program próbuje je też ponownie zarejestrować, jeśli pliki jeszcze istnieją).
- **Akcje czyszczenia** (pliki tymczasowe, Kosz, cache) - to usunięcie danych.
- **Deinstalacja OneDrive** - cofasz ją ponowną instalacją. Pliki istniejące tylko w chmurze nie są kasowane, ale lokalna synchronizacja się zatrzymuje.

## Zgodność z Windows 10 i 11

System jest wykrywany po numerze builda (>= 22000 = Windows 11); wersja, edycja i build widnieją na banerze. Tweaki niedostępne w danym systemie są ukrywane w menu, pomijane w trybie wsadowym i oznaczane w `--list` jako **N/D w tym systemie** - nigdy nie kończą się błędem.

| Tweak | Windows 11 | Windows 10 |
|-------|:----------:|:----------:|
| Zasada Copilot, Widżety (`privacy.copilot`, `visual.widgets`) | tak | N/D |
| Appx: Clipchamp, Teams, Copilot, Bing Search | tak | N/D |
| Cortana: ekran blokady i zgoda (`privacy.cortana.win10`) | N/D | tak |
| Wiadomości i zainteresowania (`privacy.newsfeed`, build 19041+) | N/D | tak |
| Pasek Osoby, Meet Now (`visual.peoplebar`, `visual.meetnow`) | N/D | tak |
| Appx: Przeglądarka 3D, Paint 3D, Print 3D | N/D | tak |
| HAGS (`gaming.hags`) | tak (GPU WDDM 2.7+) | od 2004 (19041) i GPU WDDM 2.7+ |
| Optymalizacje okienkowe / VRR (`gaming.vrr`) | tak | od 1903 (18362), częściowo |
| Plany zasilania, usługi, sieć, czyszczenie | tak | tak |

## Instalacja

### Zalecana: instalator MSI

1. Wejdź na stronę [**Releases**](https://github.com/Qbiterv/winmizer/releases/latest) i pobierz plik MSI pasujący do komputera:
   - `Winmizer-1.0.0-x64.msi` - Windows 10/11 64-bit (Intel/AMD, najczęstszy wybór),
   - `Winmizer-1.0.0-arm64.msi` - Windows na ARM,
   - `Winmizer-1.0.0-x86.msi` - Windows 10 32-bit.
2. Uruchom pobrany plik i przejdź przez instalator.
3. Winmizer instaluje się w `C:\Program Files\Winmizer` i tworzy skróty **na pulpicie** oraz w **Menu Start** z ikoną programu. Skrót prosi o uprawnienia administratora (UAC).

Instalator pojawia się w *Aplikacje i funkcje* (wydawca: Qbiterv) i czysto się odinstalowuje (usuwa program i skróty; kopie zapasowe i dzienniki w `%ProgramData%\Winmizer` zostają). Kolejne wersje aktualizują zainstalowaną w miejscu.

Instalacja cicha, z pominięciem skrótu na pulpicie:

```powershell
msiexec /i Winmizer-1.0.0-x64.msi /qb INSTALLDESKTOPSHORTCUT=0
```

### Wersja przenośna (zip)

Pobierz `Winmizer-1.0.0-win-x64.zip` (lub `win-arm64` / `win-x86`) z Releases, rozpakuj i uruchom `Winmizer.exe`. Nic nie wymaga instalacji ani .NET (aplikacja jest samodzielna).

### Budowanie ze źródeł

Wymagania: [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) (`winget install Microsoft.DotNet.SDK.8`). Instalatory MSI buduje WiX Toolset v4 (pakiet NuGet `WixToolset.Sdk 4.0.5`, pobierany automatycznie).

```powershell
git clone https://github.com/Qbiterv/winmizer.git
cd winmizer

dotnet build                       # zwykła kompilacja
.\build.ps1                        # exe + zip + MSI dla x64, arm64, x86 -> publish\
.\build.ps1 -Runtime win-x64       # tylko wybrany system
.\build.ps1 -NoInstaller           # bez budowania MSI
```

`build.ps1` publikuje single-file self-contained Release dla każdego RID i zapisuje w `publish\`:

| Plik | Dla |
|------|-----|
| `Winmizer-<wersja>-<arch>.msi` | instalator (x64, arm64, x86) |
| `Winmizer-<wersja>-win-<arch>.zip` | wersja przenośna |
| `win-<arch>\Winmizer.exe` | gotowy plik wykonywalny |

### Wydania / Release

Wydania buduje GitHub Actions (`.github/workflows/release.yml`) po wypchnięciu tagu `v*`. Wersja pochodzi z tagu (bez `v`) i trafia do exe, zipów oraz MSI.

```powershell
git tag v1.0.1
git push origin v1.0.1
```

Workflow zbuduje exe, zipy i MSI dla x64, arm64 i x86, wygeneruje `SHA256SUMS.txt` i opublikuje wszystko jako GitHub Release z automatycznymi notatkami. Lokalnie to samo: `.\build.ps1 -Version 1.0.1`. Zwykły build (CI) dla każdego pusha do `main` i pull requesta: `.github/workflows/ci.yml`.

### Alternatywa dla developerów: `install.ps1`

Skrypt bez MSI: kopiuje exe pasujący do architektury komputera do `%LOCALAPPDATA%\Programs\Winmizer` (bez uprawnień admina) i tworzy skróty dla bieżącego użytkownika. Niczego nie uruchamia.

```powershell
.\build.ps1 -NoInstaller    # najpierw zbuduj
.\install.ps1               # instalacja
.\install.ps1 -Uninstall    # usunięcie skrótów i kopii
```

> Nie mieszaj obu metod: jeśli masz wersję z MSI, nie instaluj jednocześnie `install.ps1` (powstałyby dwa skróty o tej samej nazwie).

## Użycie

Uruchom **Winmizer** ze skrótu (albo `Winmizer.exe` jako administrator). Menu główne:

1. **Szybka optymalizacja** - zalecany, bezpieczny zestaw.
2. **Własny wybór** - kategorie i pojedyncze tweaki.
3. **Odśmiecanie aplikacji i autostart**.
4. **Plany zasilania** - Winmizer Ultimate, Wysoka wydajność, powrót do Zrównoważonego.
5. **Informacje o systemie / status**.
6. **Cofnij / przywróć** - także skrót do Przywracania systemu.
7. **Wyjście**.

### Parametry wiersza poleceń

| Parametr | Działanie |
|----------|-----------|
| *(brak)* | interaktywne menu (wymaga administratora; program sam poprosi o UAC) |
| `--list` | lista wszystkich tweaków i wykrytego statusu (nic nie zmienia, nie wymaga admina) |
| `--list-md` | tabele tweaków w formacie Markdown |
| `--dry-run` | menu w trybie podglądu - brak zmian w systemie |
| `--dry-run --quick` | podgląd zestawu Szybka optymalizacja |
| `--dry-run --apply id1,id2` / `all` | podgląd wybranych tweaków |
| `--quick --yes` | zastosuj Szybką optymalizację bez pytań (wymaga admina) |
| `--apply id1,id2 --yes` | zastosuj wybrane tweaki bez pytań |
| `--revert-all --yes` | cofnij wszystko z `backup.json` |
| `--help` | pomoc |

Tryby nieinteraktywne (`--quick`, `--apply`, `--revert-all`) wymagają `--dry-run` albo `--yes`.

## Lista tweaków

Poziomy ryzyka: **Bezpieczny**, **Umiarkowany**, **Zaawansowany** (domyślnie odznaczony). Kolumna *Quick* oznacza udział w Szybkiej optymalizacji (na laptopach pomijane są ustawienia obciążające baterię). Odśmiecanie aplikacji i OneDrive nie wchodzą do trybu Quick.

<details>
<summary><b>Gaming (10)</b></summary>


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

</details>

<details>
<summary><b>Zasilanie (7)</b></summary>


| ID | Tweak | Ryzyko | Quick | Opis |
|----|-------|--------|:-----:|------|
| `power.ultimate` | Plan zasilania: Winmizer Ultimate (Ultimate Performance) | Umiarkowany | tak | Tworzy i aktywuje plan Ultimate Performance (najniższe opóźnienia). Oryginalny plan jest zapamiętany i można do niego wrócić. **Uwaga:** Laptopy: plan zużywa baterię znacznie szybciej - zalecany tylko przy zasilaniu z sieci. |
| `power.highperf` | Plan zasilania: Wysoka wydajność | Bezpieczny |  | Aktywuje wbudowany plan Wysoka wydajność (lżejsza alternatywa dla Ultimate). Można wrócić do Zrównoważonego. **Uwaga:** Laptopy: krótszy czas pracy na baterii. |
| `power.usbsuspend` | Wyłącz selektywne zawieszanie USB | Umiarkowany | tak | Zapobiega usypianiu portów USB (mniej zacięć myszy/klawiatury/padów). Na laptopach tylko zasilanie sieciowe (AC). |
| `power.pcie` | Wyłącz oszczędzanie energii łącza PCIe | Umiarkowany | tak | Link State Power Management = Wyłączone - mniejsze opóźnienia GPU/NVMe. Na laptopach tylko AC. |
| `power.procmin` | Minimalny stan procesora 100% (AC) | Umiarkowany | tak | Procesor nie obniża taktowania na zasilaniu sieciowym - stabilniejszy FPS, wyższy pobór prądu i temperatura w spoczynku. |
| `power.faststartup` | Wyłącz szybkie uruchamianie (Fast Startup) | Umiarkowany |  | Pełne zamknięcie systemu przy "Zamknij" - czysty stan sterowników i mniej problemów po aktualizacjach. Start może być nieco wolniejszy. |
| `power.hibernate` | Wyłącz hibernację (zwalnia miejsce) | Zaawansowany |  | powercfg -h off usuwa hiberfil.sys (kilka-kilkanaście GB). Wyłącza też Fast Startup i hibernację. **Uwaga:** Laptopy: utrata możliwości hibernacji. |

</details>

<details>
<summary><b>Prywatność i telemetria (13)</b></summary>


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

</details>

<details>
<summary><b>Usługi (10)</b></summary>


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

</details>

<details>
<summary><b>Sieć (3)</b></summary>


| ID | Tweak | Ryzyko | Quick | Opis |
|----|-------|--------|:-----:|------|
| `network.nagle` | Wyłącz algorytm Nagle'a (niższy ping) | Umiarkowany |  | TcpAckFrequency=1 i TCPNoDelay=1 na każdym interfejsie - pakiety gier wysyłane od razu. Wpływ na ping jest niewielki; nie rusza Wi-Fi ani sterowników. |
| `network.dop2p` | Delivery Optimization: wyłącz udostępnianie P2P | Bezpieczny | tak | DODownloadMode=0 - aktualizacje pobierane tylko z serwerów Microsoft (bez wysyłania ich innym komputerom). Windows Update działa normalnie. |
| `network.dns` | DNS Cloudflare (1.1.1.1 / 1.0.0.1) | Zaawansowany |  | Ustawia szybki publiczny DNS na aktywnych kartach sieciowych. Poprzednie ustawienia są zapamiętane. **Uwaga:** Zmienia dostawcę DNS - w sieciach firmowych/z filtrowaniem rodzicielskim może coś zablokować. |

</details>

<details>
<summary><b>Wygląd i interfejs (8)</b></summary>


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

</details>

<details>
<summary><b>Dysk (2)</b></summary>


| ID | Tweak | Ryzyko | Quick | Opis |
|----|-------|--------|:-----:|------|
| `storage.trim` | Upewnij się, że TRIM jest włączony | Bezpieczny | tak | fsutil behavior set DisableDeleteNotify 0 - system informuje SSD o zwolnionych blokach (wydajność i żywotność). |
| `storage.lastaccess` | Wyłącz znacznik ostatniego dostępu NTFS | Bezpieczny | tak | fsutil behavior set disablelastaccess 1 - mniej zapisów na dysk przy każdym odczycie pliku. |

</details>

<details>
<summary><b>Czyszczenie (4)</b></summary>


| ID | Tweak | Ryzyko | Quick | Opis |
|----|-------|--------|:-----:|------|
| `cleanup.temp` | Wyczyść pliki tymczasowe (%TEMP% i Windows\Temp) | Bezpieczny | tak | Usuwa pliki tymczasowe użytkownika i systemu. Pliki używane przez działające programy są pomijane. Prefetch NIE jest czyszczony. |
| `cleanup.shader` | Wyczyść pamięć podręczną shaderów (DirectX/NVIDIA/AMD) | Umiarkowany |  | Usuwa D3DSCache, NVIDIA DXCache/GLCache i AMD DxCache. Gry skompilują shadery od nowa - pierwsze uruchomienie może się lekko dławić. |
| `cleanup.recyclebin` | Opróżnij Kosz | Umiarkowany |  | Trwale usuwa wszystko z Kosza. **Uwaga:** Operacja nieodwracalna - sprawdź zawartość Kosza. |
| `cleanup.dism` | DISM: czyszczenie składników (StartComponentCleanup) | Zaawansowany |  | DISM /Online /Cleanup-Image /StartComponentCleanup - usuwa stare wersje składników systemu. Bezpieczne, ale trwa kilka-kilkanaście minut. |

</details>

<details>
<summary><b>Odśmiecanie aplikacji (37)</b></summary>


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

</details>

<details>
<summary><b>OneDrive (4)</b></summary>


| ID | Tweak | Ryzyko | Quick | Opis |
|----|-------|--------|:-----:|------|
| `onedrive.policy` | OneDrive: wyłącz synchronizację zasadą (DisableFileSyncNGSC) | Umiarkowany |  | Zasada blokująca uruchamianie synchronizacji plików OneDrive. Pliki lokalne pozostają nietknięte. **Uwaga:** Synchronizacja z chmurą zostanie zatrzymana. Pliki dostępne tylko online nie są usuwane. |
| `onedrive.navpane` | OneDrive: usuń z panelu nawigacji Eksploratora | Bezpieczny |  | Ukrywa ikonę OneDrive w lewym panelu Eksploratora plików. |
| `onedrive.startup` | OneDrive: usuń z autostartu | Bezpieczny |  | Usuwa wpisy Run uruchamiające OneDrive przy logowaniu (kopia zapasowa pozwala je przywrócić). |
| `onedrive.uninstall` | OneDrive: pełna deinstalacja | Umiarkowany |  | Zamyka proces OneDrive i uruchamia oficjalny deinstalator (OneDriveSetup.exe /uninstall, awaryjnie winget). **Uwaga:** Lokalna synchronizacja przestanie działać. Pliki istniejące WYŁĄCZNIE w chmurze nie są kasowane, ale stają się niedostępne z tego komputera do czasu ponownej instalacji. Najpierw upewnij się, że pliki są pobrane lokalnie. |
</details>

## Autostart

Wpisy rejestru (`HKCU/HKLM ...\Run`, także 32-bit) są przenoszone do `HKxx\SOFTWARE\Winmizer\DisabledStartup\...`, a skróty z folderów Autostart do `%ProgramData%\Winmizer\DisabledStartup`. Oryginały zapisuje `backup.json`, więc *Cofnij / przywróć* przywraca je na miejsce.

## FAQ

<details>
<summary><b>Jak cofnąć zmiany?</b></summary>

Uruchom Winmizer i wybierz **Cofnij / przywróć** (wszystko lub wybrane zmiany z `backup.json`), albo w wierszu poleceń: `Winmizer.exe --revert-all --yes`. Awaryjnie użyj punktu przywracania utworzonego przed zmianami (*Przywracanie systemu* w Windows).
</details>

<details>
<summary><b>SmartScreen ostrzega przed nieznanym wydawcą. Czy to bezpieczne?</b></summary>

Plik `Winmizer.exe` oraz MSI nie są podpisane certyfikatem (to koszt, na który projekt open source zwykle sobie nie pozwala), więc Windows SmartScreen może pokazać „Windows chronił ten komputer". Kliknij **Więcej informacji**, a potem **Uruchom mimo to**. Kod źródłowy jest w tym repozytorium i możesz zbudować program samodzielnie (`build.ps1`).
</details>

<details>
<summary><b>Usunąłem aplikację przez Winmizer. Jak ją przywrócić?</b></summary>

Zainstaluj ją ponownie ze **Sklepu Microsoft** albo poleceniem `winget install <nazwa>`. Sklep i winget nigdy nie są usuwane.
</details>

<details>
<summary><b>Czy Winmizer wyłącza Windows Defender, Windows Update albo drukowanie?</b></summary>

Nie. Defender, Windows Update (poza udostępnianiem P2P), drukarki, sieć i usługi Xbox są chronione i kod odmawia ich zmiany.
</details>

<details>
<summary><b>Dlaczego program prosi o uprawnienia administratora?</b></summary>

Zmiany w rejestrze (HKLM), usługach, planach zasilania i punkt przywracania wymagają admina. Tryby `--list` i `--dry-run` działają bez UAC.
</details>

<details>
<summary><b>Czy to zwiększy FPS?</b></summary>

Zależy od sprzętu. Efekty (HAGS, Nagle, priorytety) bywają niewielkie; największą różnicę zwykle widać w responsywności systemu, mniejszej liczbie procesów w tle i stabilniejszych klatkach. Wszystko jest odwracalne, więc możesz po prostu sprawdzić.
</details>

## Znane ograniczenia

- Wykrywanie aplikacji Appx bez uprawnień administratora (np. `--list`) obejmuje tylko bieżącego użytkownika.
- Plan Ultimate Performance bywa niedostępny w niektórych konfiguracjach - wtedy użyj planu Wysoka wydajność.
- HAGS, Nagle i inne ustawienia sprzętowe mają różny wpływ zależnie od sprzętu; wszystkie są odwracalne.

## Architektura

```
Winmizer.sln
build.ps1                  publish exe + zip + MSI
install.ps1                instalacja per-user (alternatywa dla MSI)
installer/                 WiX v4: Winmizer.Installer.wixproj, Package.wxs
tools/make-icon.ps1        generator ikony
src/Winmizer/
  Program.cs               punkt wejścia, parametry CLI
  Core/                    TweakBase, TweakEngine, BackupStore (backup.json), Restorer,
                           Reg/Proc, StartupManager
  Tweaks/<Kategoria>/      Gaming, Power, Privacy, Services, Network, Visual, Storage, Cleanup, Debloat, OneDrive
  Tweaks/TweakCatalog.cs   lista wszystkich tweaków
  UI/                      Spectre.Console: menu i renderowanie
```

Ikona (`src/Winmizer/Assets/winmizer.ico`, podgląd `docs/icon.png`) jest generowana skryptem `tools/make-icon.ps1`.

## Zastrzeżenie

Winmizer zmienia ustawienia systemu. Choć tworzy punkt przywracania i kopię oryginałów, oprogramowanie jest udostępniane **„tak jak jest"**, bez jakiejkolwiek gwarancji; używasz go na własną odpowiedzialność. Nie jest produktem ani nie jest powiązany z Microsoft. Zalecamy wykonanie własnej kopii ważnych danych przed użyciem.

## Licencja

[MIT](LICENSE) © 2026 Qbiterv

---

<p align="center">
  <a href="https://claude.com/claude-code">
    <img src="https://img.shields.io/badge/This%20project%20is-Claude%20Coded-D97757?style=for-the-badge&logo=claude&logoColor=white" alt="This project is Claude Coded">
  </a>
</p>
