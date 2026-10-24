# Contribuire a CandeDisk

Grazie per l'interesse! Segnalazioni, idee e pull request sono benvenute.

## Segnalare un problema

Apri una [issue](https://github.com/candelab2026/candedisk/issues/new/choose) e indica:

- versione di CandeDisk (menu **Aiuto > Informazioni**) e di Windows;
- cosa stavi facendo (operazione, tipo di disco, file system);
- il messaggio di errore completo;
- il **riepilogo dei dischi** (menu **File > Copia riepilogo dei dischi**) e, se utile, il file di log (**Aiuto > Apri cartella dei log**).

Controlla prima di non includere informazioni personali nel log o nel riepilogo.

Per le vulnerabilità di sicurezza **non** aprire una issue pubblica: leggi [SECURITY.md](SECURITY.md).

## Proporre una modifica

1. Fai un fork e crea un branch dedicato (`feature/nome` o `fix/nome`).
2. Compila e prova la modifica (vedi sotto).
3. Apri una pull request spiegando **cosa** cambia e **perché**.

Cambiamenti grandi o che toccano le operazioni sui dischi: apri prima una issue per discuterne.

## Ambiente di sviluppo

Serve l'[SDK .NET 9](https://dotnet.microsoft.com/download) su Windows.

```powershell
dotnet build src/CandeDisk
dotnet run --project src/CandeDisk -- --demo
```

`--demo` usa dati di esempio e non modifica nessun disco: è il modo più sicuro per lavorare sull'interfaccia.

### Provare le operazioni sui dischi

**Non provare mai le operazioni distruttive sui tuoi dischi reali.** Usa un disco virtuale (VHDX), da un PowerShell avviato come amministratore:

```powershell
# crea e collega un disco virtuale vuoto da 2 GB (compare in CandeDisk come disco RAW)
.\scripts\TestDisk.ps1 -Action Create -SizeMB 2048

# ... prova CandeDisk (build Release o Debug avviata come amministratore) ...

# a fine prova: scollega e cancella il disco virtuale
.\scripts\TestDisk.ps1 -Action Remove
```

Lo script tocca solo il file `%TEMP%\CandeDiskTest.vhdx`. Prima di aprire una pull request che modifica le operazioni, prova sul disco virtuale: inizializza, crea, formatta, ridimensiona (riduci ed estendi) ed elimina.

### Test automatici

```powershell
dotnet test
```

I test non modificano nessun disco: coprono il calcolo dello spazio non allocato, le regole di `Safety`, la validazione degli input e la pulizia dei messaggi di errore.

## Convenzioni

- Codice C# con `nullable` attivo, in stile coerente con quello esistente (vedi `.editorconfig`).
- Testi dell'interfaccia in italiano. I commenti spiegano il *perché*, non il *cosa*.
- Ogni nuova operazione sui dischi deve: passare dai controlli di `Safety`, chiedere conferma, scrivere nel log e **non** accettare input non validato negli script PowerShell.
- Aggiungi o aggiorna i test in `tests/CandeDisk.Tests` per la logica che cambi.
- Aggiorna `docs/GUIDA.md` e `CHANGELOG.md` se la modifica è visibile all'utente.

## Codice di condotta

Partecipando accetti il [Codice di condotta](CODE_OF_CONDUCT.md).
