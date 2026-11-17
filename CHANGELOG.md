# Changelog

## 1.0.1

### Fixed
- **Quick optimize now removes the recommended bloatware.** Previously it only applied registry tweaks, so
  `privacy.cortana` set the AllowCortana policy but the Cortana app (`Microsoft.549981C3F5F10`) stayed installed.
  Every debloat entry marked as a safe default (Cortana, Copilot packages, Bing News/Weather, Get Help, Tips,
  Solitaire, Feedback Hub, Clipchamp, consumer Teams, Power Automate, promo apps...) is now part of Quick.
  Optional ones (Phone Link, Zune, Alarms, Maps, Xbox Game Bar) stay out. Winmizer lists the apps and asks once
  (skipped with `--yes`); the protected-packages list is always respected.
- **Windows 11 Copilot:** the tweak also sets `ShowCopilotButton=0` (with backup/revert), removes every `*Copilot*`
  Appx package (never a protected one) and only counts as applied when both policy and packages are in place.
  A new `MicrosoftWindows.Client.CoPilot` debloat entry was added.
- **Appx removal is verified.** After `Remove-AppxPackage -AllUsers` and `Remove-AppxProvisionedPackage` the package
  is queried again; if it is still installed for any user or provisioned, the tweak is reported as FAILED with the
  PowerShell error text in the log and the summary. Packages present only for other users or only as provisioned
  are handled.
- **Explorer restart.** Tweaks now declare what they need (None / Explorer / SignOut / Reboot). The final summary
  says what is required for the changes to show and offers "Restart Explorer now? (y/N)"; `--yes` restarts Explorer
  automatically, dry-run only logs it.
- **Status:** Cortana and Copilot show "Not applied" while their packages are still installed.

## 1.0.0

- First release: interactive Spectre.Console menu and CLI (`--list`, `--dry-run`, `--quick`, `--apply`,
  `--revert-all`), 90+ reversible tweaks for gaming, power, privacy, services, network, appearance, storage,
  cleanup, app debloat and OneDrive, backup to `backup.json`, system restore point, English and Polish UI,
  single-file builds and MSI installers for win-x64, win-arm64 and win-x86.