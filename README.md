# Lumen

Een moderne, strakke mediaspeler voor **Windows en Linux**, gebouwd op de VLC-engine.
Alles wat VLC kan afspelen, speelt Lumen ook af — maar dan in een interface die je zonder handleiding snapt.

## Functies

**Afspelen**
- Speelt vrijwel elk formaat af (MP4, MKV, AVI, MOV, WEBM, MP3, FLAC, …) met GPU-decodering, ook 4K
- Startscherm met slepen-en-neerzetten en **"Verder kijken"**: Lumen onthoudt waar je stopte
- Afspeelsnelheid 0,5× – 2×, volgend beeld, A-B herhalen, schermafbeelding
- Volledig scherm waarbij bediening en muisaanwijzer zichzelf verbergen

**Beeld & geluid** (paneel *Aanpassingen*)
- Helderheid, contrast, verzadiging, tint, gamma
- Beeldverhouding, bijsnijden, zoom, draaien/spiegelen, deinterlacen
- Equalizer met 10 banden en de voorinstellingen van VLC
- Audio- en ondertitelvertraging

**Ondertitels**
- Automatisch gevonden (zelfde naam, `Film.nl.srt`, mappen `Subs/`), Nederlands krijgt voorrang
- `.srt`, `.vtt` en `.ass` worden door Lumen zelf getekend; ingebouwde ondertitels (ook PGS) door VLC

**Afspeellijst**
- Herhalen (alles / één), shuffle, natuurlijk gesorteerde mappen
- `.m3u` openen en opslaan, ook IPTV-lijsten van internet

**Netwerk & casten**
- Netwerkstreams openen: http(s), HLS, rtsp, udp, radio, sommige videosites
- Casten naar Chromecast
- Een stream opnemen naar Video's\Lumen

**Converteren**
- Naar MP4 (H.264), MKV (H.265), WebM, MP3, FLAC, OGG, of snel hermuxen zonder hercoderen

**Overige**
- Mediainfo (codecs, resolutie, bitrate, statistieken), slaaptimer, altijd bovenop
- Licht en donker thema plus 8 kleurskins
- Automatische updates

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
| S | Schermafbeelding |
| E | Volgend beeld |
| A | A-B herhalen (A, B, uit) |
| G / H | Ondertitels vroeger / later |
| I | Mediainfo |
| Ctrl+O / Ctrl+N / Ctrl+L | Bestanden / netwerkstream / afspeellijst openen |

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
│   ├── Playback/       Afspelen: engine (VLC), videovlak, ondertitels, sporen, A-B herhalen
│   ├── Effects/        Beeld & geluid: kleur, beeldverhouding, equalizer, synchronisatie
│   ├── MediaInfo/      Codecs, resolutie en statistieken
│   ├── Network/        Netwerkstreams openen en omzetten
│   ├── Cast/           Chromecast zoeken en casten
│   ├── Convert/        Converteren en streams opnemen
│   ├── Tools/          Schermafbeelding, slaaptimer, meldingen
│   ├── Playlist/       Wachtrij, herhalen/shuffle, .m3u lezen en schrijven
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

- Op Windows tekent VLC de video via de GPU in een native venster; de bediening ligt daarom in een apart, doorzichtig venster erboven.
- Op Linux tekent Lumen de video zelf. Beeldverhouding, bijsnijden en zoom werken daar beperkt.
- Videosites (zoals YouTube) werken alleen zolang de scripts in VLC dat ondersteunen.
- Casten vereist een Chromecast op hetzelfde netwerk; VLC hercodeert indien nodig.
