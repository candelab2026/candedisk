# Guida di CandeDisk

CandeDisk è un gestore di dischi e partizioni per Windows: mostra i tuoi dischi e permette di formattare, creare, eliminare e ridimensionare le partizioni con un'interfaccia semplice.

> Le operazioni sui dischi cancellano dati in modo irreversibile. Fai sempre un backup prima di procedere.

## Per iniziare

- Avvia CandeDisk. Per modificare i dischi servono i privilegi di **amministratore**: la versione Release li chiede da sola (finestra UAC).
- Se il programma parte senza privilegi, mostra un banner giallo e lavora in **sola lettura**. Il pulsante **Riavvia come amministratore** lo riapre con i permessi necessari.
- A sinistra c'è l'elenco dei dischi. A destra trovi la mappa del disco selezionato e la tabella delle partizioni.
- Per cambiare lingua usa il menu **Lingua**: il cambio è immediato e viene ricordato.

## La mappa del disco

La barra colorata mostra ogni partizione in proporzione alla sua dimensione. Le partizioni molto piccole (EFI, MSR) hanno una larghezza minima, così restano cliccabili.

- **Blu:** la partizione di Windows.
- **Ambra:** partizioni speciali (EFI, ripristino, riservata).
- **Verde:** partizioni di dati.
- **Grigio:** spazio non allocato.

Clicca una casella per selezionare la partizione corrispondente nella tabella.

## Le operazioni

### Inizializza

Un disco nuovo risulta "RAW" e non ha una tabella delle partizioni. **Inizializza** lo prepara con una tabella **GPT**. Compare attivo solo per i dischi RAW.

### Crea una partizione

1. Seleziona una riga **Non allocato**.
2. Premi **Crea** (`Ctrl+N`).
3. Scegli la dimensione in MB, il file system e l'etichetta. Se scegli la dimensione massima, Windows usa tutto lo spazio realmente disponibile.

### Formatta

1. Seleziona la partizione e premi **Formatta** (`Ctrl+Maiusc+F`).
2. Scegli file system (NTFS, exFAT o FAT32) ed etichetta.
3. La **formattazione rapida** è consigliata. Quella completa scrive tutto il volume ed è molto più lenta.

**Tutti i dati della partizione vengono cancellati.**

### Ridimensiona

1. Seleziona la partizione e premi **Ridimensiona** (`Ctrl+R`).
2. CandeDisk chiede a Windows la dimensione minima e massima supportata e te le mostra.
3. Inserisci la nuova dimensione in MB.

Per **estendere** una partizione serve spazio non allocato **subito dopo** di essa: se tra le due c'è un'altra partizione, Windows non permette di estenderla. Per **ridurre** una partizione si può andare solo fino allo spazio occupato dai dati.

### Elimina

Seleziona la partizione e premi **Elimina** (`Canc`, con il focus sulla tabella). Le partizioni protette (EFI, OEM, ripristino) che Gestione disco non fa cancellare vengono rimosse con `diskpart` in modalità "override", dopo conferme aggiuntive.

## Le protezioni

CandeDisk prova a impedirti gli errori più gravi:

- Non puoi **formattare né eliminare** la partizione di Windows in uso, l'unità di sistema o la partizione EFI del disco da cui avvii il PC.
- Ogni operazione che cancella dati chiede una **doppia conferma**, con il nome del disco, e una terza per partizioni speciali o sul disco di sistema.
- I dischi **offline** o **in sola lettura** non si modificano.
- Etichette e file system vengono validati prima di essere passati a Windows.

## Scorciatoie da tastiera

- `F5`: aggiorna l'elenco dei dischi
- `F1`: apre questa guida
- `Ctrl+N`: crea partizione
- `Ctrl+Maiusc+F`: formatta
- `Ctrl+R`: ridimensiona
- `Canc`: elimina (con il focus sulla tabella)

## Il registro delle operazioni

Ogni operazione viene registrata (inizio, esito ed eventuale errore) in un file di testo. Apri la cartella dal menu **Aiuto > Apri cartella dei log**. I file si trovano in `%LocalAppData%\CandeDisk\logs`.

## Copiare il riepilogo

**File > Copia riepilogo dei dischi** mette negli appunti un elenco testuale di dischi e partizioni, utile per segnalare un problema.

## Domande frequenti

- **Non vedo il mio disco.** Premi **Aggiorna**. Un disco offline va messo online da Gestione disco di Windows.
- **I pulsanti sono grigi.** Il programma è in sola lettura (riavvialo come amministratore), oppure non hai selezionato una partizione o uno spazio non allocato adatti all'operazione.
- **Non riesco a estendere una partizione.** Serve spazio non allocato immediatamente a destra. CandeDisk non sposta partizioni.
- **Appare "Not enough available capacity".** Windows non ha trovato abbastanza spazio contiguo: scegli una dimensione leggermente più piccola o usa la dimensione massima.
- **Posso unire due partizioni?** Non direttamente: elimina la seconda (dopo aver salvato i dati) ed estendi la prima, se sono adiacenti.

## Limiti noti

- Lo **spostamento** di partizioni non è supportato.
- Sono supportati i file system **NTFS, exFAT e FAT32**.
- I dischi dinamici e le configurazioni RAID software non sono gestiti.

Segnala problemi o idee su [GitHub](https://github.com/candelab2026/candedisk/issues).
