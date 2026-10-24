<p align="center">
  <img src="assets/logo.png" alt="Logo di CandeDisk" width="128" />
</p>

<h1 align="center">CandeDisk</h1>

<p align="center">
  Gestione di dischi e partizioni per Windows: formatta, crea, elimina e ridimensiona con un'interfaccia semplice.
</p>

<p align="center">
  <a href="https://github.com/candelab2026/candedisk/actions/workflows/build.yml"><img alt="Build" src="https://github.com/candelab2026/candedisk/actions/workflows/build.yml/badge.svg" /></a>
  <a href="LICENSE"><img alt="Licenza MIT" src="https://img.shields.io/badge/licenza-MIT-blue.svg" /></a>
  <img alt="Piattaforma" src="https://img.shields.io/badge/piattaforma-Windows%2010%20%7C%2011-0078D6.svg" />
  <img alt=".NET 9" src="https://img.shields.io/badge/.NET-9-512BD4.svg" />
</p>

<p align="center">
  <b>Italiano</b> · <a href="README.en.md">English</a>
</p>

![Schermata di CandeDisk](docs/images/screenshot.png)

> La schermata usa la **modalità demo** con dati di esempio, non dischi reali.

## Perché CandeDisk

*Gestione disco* di Windows funziona, ma a volte si blocca: "Elimina volume" disattivato su alcune partizioni, "Estendi" grigio senza spiegazioni, nessun messaggio chiaro quando qualcosa non va. CandeDisk usa le stesse funzioni di archiviazione di Windows, ma:

- mostra **tutti i dischi, le partizioni e lo spazio non allocato** in una sola vista, con una mappa colorata;
- permette di eliminare anche le **partizioni protette** (EFI, OEM, ripristino) con `diskpart`, dopo conferme esplicite;
- spiega gli errori in modo leggibile e li registra in un **log delle operazioni**;
- ti **protegge** dagli errori più gravi (non tocca il Windows in uso né la partizione di avvio).

## Funzioni

| Funzione | Descrizione |
|---|---|
| Elenco dischi | Dischi, partizioni, file system, spazio libero, stato (Windows, EFI, attiva, nascosta) |
| Mappa del disco | Barra proporzionale cliccabile con colori per tipo di partizione |
| Inizializza | Prepara un disco nuovo con tabella GPT |
| Crea | Nuova partizione nello spazio non allocato, con file system ed etichetta |
| Formatta | NTFS, exFAT, FAT32; formattazione rapida o completa |
| Ridimensiona | Riduce o estende una partizione entro i limiti reali calcolati da Windows |
| Elimina | Anche le partizioni protette, con doppia/tripla conferma |
| Guida e log | Guida integrata (`F1`), registro delle operazioni, copia del riepilogo negli appunti |

Il programma è in italiano. Le versioni future potranno aggiungere altre lingue.

## Requisiti

- Windows 10 o Windows 11 (64 bit)
- Privilegi di **amministratore** per modificare i dischi (senza, il programma funziona in sola lettura)

## Installazione

1. Scarica l'ultima versione dalla pagina [**Releases**](https://github.com/candelab2026/candedisk/releases).
2. Estrai lo zip e avvia `CandeDisk.exe`: Windows chiederà la conferma dell'amministratore (UAC).

L'eseguibile della release include il runtime .NET, non devi installare nulla.

Per compilarlo tu stesso, vedi [Compilare dal codice sorgente](#compilare-dal-codice-sorgente).

## Uso rapido

1. Seleziona un disco a sinistra.
2. Seleziona una partizione (o una riga **Non allocato**) nella tabella o nella mappa.
3. Usa la barra degli strumenti, i menu o le scorciatoie:

| Scorciatoia | Azione |
|---|---|
| `F5` | Aggiorna |
| `Ctrl+N` | Crea partizione |
| `Ctrl+Maiusc+F` | Formatta |
| `Ctrl+R` | Ridimensiona |
| `Canc` | Elimina (con il focus sulla tabella) |
| `F1` | Guida |

Guida completa: [docs/GUIDA.md](docs/GUIDA.md).

## Sicurezza

Le operazioni sui dischi **cancellano dati in modo irreversibile**. Fai sempre un backup.

- Non si possono formattare né eliminare la partizione di Windows in uso, l'unità di sistema e la partizione EFI del disco di avvio.
- Ogni operazione distruttiva richiede una doppia conferma con il nome del disco; una terza per le partizioni speciali.
- Dischi offline o in sola lettura non vengono modificati.
- Il software è fornito **senza alcuna garanzia** (vedi [LICENSE](LICENSE)). Prima di usarlo su dati importanti, provalo su un disco virtuale o su una chiavetta di prova.

Per segnalare una vulnerabilità leggi [SECURITY.md](SECURITY.md).

## Modalità demo

Per provare l'interfaccia senza toccare nessun disco:

```powershell
CandeDisk.exe --demo
```

## Compilare dal codice sorgente

Serve l'[SDK .NET 9](https://dotnet.microsoft.com/download).

```powershell
git clone https://github.com/candelab2026/candedisk.git
cd candedisk
dotnet build src/CandeDisk -c Release
dotnet run --project src/CandeDisk
```

Una build **Debug** parte senza richiedere l'amministratore (sola lettura, con un pulsante per riavviarsi con i permessi). Una build **Release** richiede l'UAC all'avvio.

Per creare l'eseguibile autonomo:

```powershell
dotnet publish src/CandeDisk -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o dist
```

## Come funziona

CandeDisk è un'applicazione WPF (C#, .NET 9) che comanda il modulo *Storage* di Windows tramite PowerShell (`Get-Disk`, `Get-Partition`, `New-Partition`, `Resize-Partition`, `Format-Volume`, `Remove-Partition`) e, come ripiego per le partizioni protette, `diskpart`. Non scrive direttamente sui settori del disco. Dettagli in [docs/ARCHITETTURA.md](docs/ARCHITETTURA.md).

## Limiti noti e idee future

- Lo **spostamento** di partizioni (per unire spazio non adiacente) non è ancora supportato: è l'operazione più rischiosa.
- Dischi dinamici e RAID software non sono gestiti.
- Idee: clonazione di partizioni, controllo dello stato SMART, altre lingue, tema scuro.

Vedi il [CHANGELOG](CHANGELOG.md) e le [issue](https://github.com/candelab2026/candedisk/issues).

## Contribuire

Contributi, segnalazioni e idee sono benvenuti. Leggi [CONTRIBUTING.md](CONTRIBUTING.md) e il [Codice di condotta](CODE_OF_CONDUCT.md).

## Licenza

Rilasciato con licenza [MIT](LICENSE). Copyright © 2026 CandeLab.
