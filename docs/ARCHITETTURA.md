# Architettura

CandeDisk è un'applicazione WPF (C#, .NET 9) per Windows. Non scrive direttamente sui settori del disco: delega ogni operazione al modulo *Storage* di Windows, esposto da PowerShell, e a `diskpart` solo come ripiego.

## Struttura del codice

```
src/CandeDisk/
  MainWindow.xaml(.cs)   interfaccia principale: menu, barra, mappa del disco, tabella, azioni
  OptionsDialog.xaml(.cs) finestra unica per formatta / crea / ridimensiona
  HelpWindow, AboutWindow guida (da docs/GUIDA.md) e informazioni
  DiskService.cs         operazioni sui dischi (elenco, formatta, crea, elimina, ridimensiona)
  PowerShellRunner.cs    esecuzione di PowerShell e diskpart, pulizia degli errori
  Safety.cs              regole di protezione e validazione di etichette
  Models.cs              DiskInfo, PartitionInfo, calcolo dello spazio non allocato
  DemoData.cs            dischi di esempio per --demo e screenshot
  MarkdownView.cs        mostra la guida (Markdown minimo) in un FlowDocument
  Logger.cs              registro delle operazioni
  Icons.xaml             icone vettoriali a tratto
  Assets/                logo e icona dell'applicazione
```

## Flusso di un'operazione

1. L'utente seleziona una partizione e sceglie un comando (menu, barra, scorciatoia).
2. `Safety.BlockReason` verifica se l'azione è vietata (Windows in uso, disco offline, ...).
3. Una finestra raccoglie i parametri; i valori vengono validati (`Safety.IsValidLabel`, file system in lista chiusa).
4. L'app mostra una o più conferme, con il nome del disco.
5. `DiskService` costruisce lo script PowerShell e lo esegue con `PowerShellRunner`.
6. Esito e dettagli vengono scritti nel log; l'elenco dei dischi viene riletto.

## Scelte di progetto

- **PowerShell invece delle API native.** Il modulo Storage è lo stesso usato dagli strumenti di Windows, è documentato e stabile. Il costo è l'avvio del processo a ogni chiamata, accettabile per operazioni rare.
- **`-EncodedCommand`.** Gli script passano in Base64 UTF-16 per eliminare problemi di quoting. I valori variabili sono numeri o stringhe validate con una lista di caratteri ammessi, e le virgolette singole vengono raddoppiate.
- **Messaggi d'errore.** PowerShell con stderr rediretto produce XML (CLIXML): `PowerShellRunner` ne estrae solo il testo dell'errore e disattiva la barra di avanzamento.
- **Spazio non allocato calcolato in C#.** Windows non lo elenca: `DiskInfo.BuildRows` ricava gli intervalli liberi dalle posizioni delle partizioni. Windows riserva però qualche MB in più (es. fine disco MBR), quindi, se l'utente chiede quasi tutto lo spazio, `New-Partition` riceve `-UseMaximumSize` e decide da solo.
- **Eliminazione protetta.** `Remove-Partition` rifiuta alcune partizioni (EFI, OEM): in quel caso si usa `diskpart` con `delete partition override`.
- **Elevazione.** La build Release incorpora un manifest `requireAdministrator`; la Debug no, e mostra un banner con il pulsante per riavviarsi come amministratore.

## Opzioni da riga di comando

| Opzione | Effetto |
|---|---|
| `--demo` | Usa dati di esempio; nessuna operazione viene eseguita |
| `--screenshot <file.png>` | Salva uno screenshot della finestra e chiude (usato per la documentazione) |
| `--disk <n>`, `--row <n>` | Con `--screenshot`: disco e riga da selezionare |
| `--window help\|about` | Con `--screenshot`: cattura la finestra Guida o Informazioni |

Esempio per rigenerare lo screenshot del README:

```powershell
dotnet run --project src/CandeDisk -- --demo --screenshot docs/images/screenshot.png --disk 0 --row 2
```

## Logo e icona

`assets/logo.svg` è il logo vettoriale. Le immagini PNG e `src/CandeDisk/Assets/app.ico` sono derivate dallo stesso disegno: anello di partizioni colorate (blu, verde, ambra e grigio per lo spazio libero) attorno al mozzo del disco.
