# Politica di sicurezza

CandeDisk lavora con privilegi di amministratore e modifica i dischi: la sicurezza è importante.

## Versioni supportate

Viene mantenuta solo l'ultima versione pubblicata nella pagina [Releases](https://github.com/candelab2026/candedisk/releases).

## Segnalare una vulnerabilità

**Non aprire una issue pubblica.** Usa la segnalazione privata di GitHub:

1. Vai nella scheda **Security** del repository.
2. Scegli **Report a vulnerability**.
3. Descrivi il problema, come riprodurlo e l'impatto.

Risponderemo appena possibile e ti terremo aggiornato fino alla correzione. Una volta rilasciata la correzione, il problema potrà essere reso pubblico, citandoti se lo desideri.

## Cosa consideriamo un problema di sicurezza

- Esecuzione di comandi non previsti (ad esempio iniezione nei comandi PowerShell o `diskpart` tramite etichette, nomi o altri campi).
- Aggiramento delle protezioni che impediscono di cancellare il sistema in uso.
- Scrittura di file o dati sensibili in posizioni non sicure.

## Nota sull'uso

CandeDisk è fornito senza garanzie (vedi [LICENSE](LICENSE)). La perdita di dati causata da un uso errato non è una vulnerabilità, ma una segnalazione di miglioramento delle protezioni è sempre benvenuta come issue.
