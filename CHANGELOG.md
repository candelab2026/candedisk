# Changelog

Il formato segue [Keep a Changelog](https://keepachangelog.com/it-IT/1.1.0/) e il progetto usa il [versionamento semantico](https://semver.org/lang/it/).

## [1.0.0] - 2026-10-24

Prima versione.

### Aggiunto
- Elenco di dischi e partizioni con mappa proporzionale cliccabile e spazio non allocato.
- Inizializzazione dei dischi (GPT).
- Creazione di partizioni (NTFS, exFAT, FAT32 o senza formattazione).
- Formattazione rapida o completa.
- Ridimensionamento (riduzione ed estensione) entro i limiti supportati da Windows.
- Eliminazione di partizioni, comprese quelle protette (EFI, OEM, ripristino) tramite `diskpart`.
- Protezioni: blocco di Windows in uso e partizione di avvio, doppia e tripla conferma, dischi offline o in sola lettura.
- Menu, barra degli strumenti con icone, menu contestuale e scorciatoie da tastiera.
- Guida integrata (`F1`) e finestra Informazioni.
- Registro delle operazioni in `%LocalAppData%\CandeDisk\logs`.
- Modalità `--demo` con dati di esempio.
- Logo e icona dell'applicazione.
