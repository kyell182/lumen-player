# Lumen

Een moderne, strakke mediaspeler voor **Windows en Linux**, gebouwd op de VLC-engine.
Alles wat VLC kan afspelen, speelt Lumen ook af — maar dan in een interface die je zonder handleiding snapt.

## Functies

- Speelt vrijwel elk formaat af (MP4, MKV, AVI, MOV, WEBM, MP3, FLAC, …) met hardwareversnelling
- Startscherm met slepen-en-neerzetten en **"Verder kijken"**: Lumen onthoudt waar je stopte
- Afspeellijst met volgende/vorige; mappen worden natuurlijk gesorteerd (aflevering 2 vóór 10)
- Audiosporen, ingebouwde ondertitels en externe ondertitelbestanden (of sleep een `.srt` op de video)
- Afspeelsnelheid 0,5× – 2×
- Volledig scherm waarbij de bediening zichzelf verbergt
- Licht en donker thema, volume en thema worden onthouden
- Visualisatie bij muziekbestanden

## Automatische updates

Lumen kijkt bij het opstarten of er op GitHub een nieuwere release is. Zo ja, dan verschijnt bovenaan een melding met **Nu bijwerken**:
Lumen downloadt de installer, installeert die (Windows vraagt om toestemming, Linux om je wachtwoord) en start daarna opnieuw op.

Een nieuwe versie uitbrengen: start de workflow **Installers bouwen** in het tabblad Actions met een versienummer (bv. `1.0.4`), of push een tag `v1.0.4`.

## Sneltoetsen

| Toets | Actie |
|---|---|
| Spatie / K | Afspelen / pauzeren |
| ← / J, → / L | 10 seconden terug / vooruit |
| ↑ / ↓ | Volume |
| M | Dempen |
| F / F11, Esc | Volledig scherm aan / uit |
| N / P | Volgende / vorige |
| + / − | Sneller / trager |
| Home | Opnieuw beginnen |
| Ctrl+O | Bestanden openen |

## Installeren

Download de installer uit de **Releases** van deze repository:

- **Windows:** `Lumen-Setup-<versie>-x64.exe` — installeert in Program Files, maakt een snelkoppeling en (optioneel) een "Openen met"-koppeling. De VLC-engine zit erin.
- **Linux (Debian/Ubuntu/Mint):** `sudo apt install ./lumen_<versie>_amd64.deb` — installeert automatisch `libvlc` als afhankelijkheid.

## Zelf bouwen

Vereist: [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0).

```bash
# Starten tijdens het ontwikkelen
dotnet run --project src/Lumen
```

Op Linux heb je daarvoor ook libVLC nodig: `sudo apt install libvlc5 vlc-plugin-base`.

### Installers

```powershell
# Windows (vereist Inno Setup 6: winget install JRSoftware.InnoSetup)
./installer/windows/build.ps1 -Version 1.0.0
```

```bash
# Linux
bash installer/linux/build-deb.sh 1.0.0
```

Resultaat staat in `artifacts/installer/`.

### Automatisch via GitHub

Push een tag en GitHub Actions bouwt beide installers en maakt een Release:

```bash
git tag v1.0.0
git push origin v1.0.0
```

## Architectuur

Elke functie heeft een eigen map met model, viewmodel en views. De UI praat nooit rechtstreeks met VLC,
alleen met `IMediaPlayerService` — een andere engine (bv. mpv) inpluggen raakt de rest van de app niet.

```
src/Lumen/
├── Composition/        AppServices — compositieroot, maakt alles één keer aan
├── Infrastructure/     JSON-opslag, bestandsdialogen, mediatypes, natuurlijk sorteren
├── Features/
│   ├── Playback/       IMediaPlayerService, VLC-implementatie, PlayerViewModel, videovlak, menu's
│   ├── Playlist/       Wachtrij + paneel
│   ├── History/        Recente bestanden + hervat-regels
│   ├── Home/           Startscherm met "Verder kijken"
│   ├── Settings/       Instellingen + thema
│   └── Shell/          Hoofdvenster, bedieningsbalk, sneltoetsen, auto-verbergen
└── Styles/             Kleuren (licht/donker), iconen, knopstijlen
installer/
├── windows/            Inno Setup-script + build.ps1
└── linux/              .deb-script, .desktop-bestand
```

Gebouwd met [Avalonia UI](https://avaloniaui.net), [LibVLCSharp](https://code.videolan.org/videolan/LibVLCSharp)
en [CommunityToolkit.Mvvm](https://learn.microsoft.com/dotnet/communitytoolkit/mvvm/).
Iconen gebaseerd op Material Design Icons (Apache 2.0). libVLC valt onder de LGPL 2.1.

## Bekende beperkingen

- De video wordt door VLC in een native venster getekend; daarom staat de bediening onder de video en niet erover.
  In volledig scherm haal je de bediening terug door de muis naar de onderrand te bewegen of een toets in te drukken.
- Slepen-en-neerzetten werkt op het startscherm en de bedieningsbalk, niet rechtstreeks op het videobeeld.
- Op Linux vereist de video-weergave X11 (onder Wayland via XWayland, standaard aanwezig).
