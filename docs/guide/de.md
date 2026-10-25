# CandeDisk-Anleitung

CandeDisk ist eine Datenträger- und Partitionsverwaltung für Windows: Es zeigt Ihre Datenträger an und erlaubt es, Partitionen mit einer einfachen Oberfläche zu formatieren, zu erstellen, zu löschen und in der Größe zu ändern.

> Vorgänge auf Datenträgern löschen Daten unwiderruflich. Erstellen Sie vor dem Fortfahren immer eine Sicherung.

## Erste Schritte

- Starten Sie CandeDisk. Zum Ändern von Datenträgern sind **Administratorrechte** nötig: Die Release-Version fordert sie selbst an (UAC-Fenster).
- Startet das Programm ohne diese Rechte, zeigt es ein gelbes Banner und arbeitet im **schreibgeschützten** Modus. Die Schaltfläche **Als Administrator neu starten** öffnet es mit den nötigen Rechten erneut.
- Links steht die Liste der Datenträger. Rechts sehen Sie die Karte des gewählten Datenträgers und die Tabelle seiner Partitionen.
- Die Sprache ändern Sie im Menü **Sprache**: Die Änderung gilt sofort und wird gespeichert.

## Die Datenträgerkarte

Der farbige Balken zeigt jede Partition im Verhältnis zu ihrer Größe. Sehr kleine Partitionen (EFI, MSR) haben eine Mindestbreite, damit sie anklickbar bleiben.

- **Blau:** die Windows-Partition.
- **Bernstein:** spezielle Partitionen (EFI, Wiederherstellung, reserviert).
- **Grün:** Datenpartitionen.
- **Grau:** nicht zugeordneter Speicherplatz.

Klicken Sie auf ein Feld, um die passende Partition in der Tabelle auszuwählen.

## Die Vorgänge

### Initialisieren

Ein neuer Datenträger erscheint als „RAW“ und hat keine Partitionstabelle. **Initialisieren** bereitet ihn mit einer **GPT**-Tabelle vor. Die Funktion ist nur für RAW-Datenträger aktiv.

### Partition erstellen

1. Wählen Sie eine Zeile **Nicht zugeordnet** aus.
2. Klicken Sie auf **Erstellen** (`Strg+N`).
3. Wählen Sie die Größe in MB, das Dateisystem und die Bezeichnung. Wenn Sie die maximale Größe wählen, nutzt Windows den gesamten tatsächlich verfügbaren Platz.

### Formatieren

1. Wählen Sie die Partition aus und klicken Sie auf **Formatieren** (`Strg+Umschalt+F`).
2. Wählen Sie das Dateisystem (NTFS, exFAT oder FAT32) und die Bezeichnung.
3. Die **Schnellformatierung** wird empfohlen. Die vollständige Formatierung beschreibt das ganze Volume und ist viel langsamer.

**Alle Daten der Partition werden gelöscht.**

### Größe ändern

1. Wählen Sie die Partition aus und klicken Sie auf **Größe ändern** (`Strg+R`).
2. CandeDisk fragt Windows nach der minimal und maximal unterstützten Größe und zeigt sie Ihnen an.
3. Geben Sie die neue Größe in MB ein.

Zum **Erweitern** einer Partition wird nicht zugeordneter Platz **direkt dahinter** benötigt: Liegt eine andere Partition dazwischen, erlaubt Windows das Erweitern nicht. Eine Partition lässt sich nur bis zu dem Platz **verkleinern**, den ihre Daten belegen.

### Löschen

Wählen Sie die Partition aus und klicken Sie auf **Löschen** (`Entf`, mit Fokus auf der Tabelle). Geschützte Partitionen (EFI, OEM, Wiederherstellung), die die Datenträgerverwaltung nicht löschen lässt, werden nach zusätzlichen Bestätigungen mit `diskpart` im „override“-Modus entfernt.

## Die Schutzregeln

CandeDisk versucht, die schlimmsten Fehler zu verhindern:

- Sie können die laufende Windows-Partition, das Systemlaufwerk und die EFI-Partition des Datenträgers, von dem Sie starten, **weder formatieren noch löschen**.
- Jeder Vorgang, der Daten löscht, verlangt eine **doppelte Bestätigung** mit dem Namen des Datenträgers, und eine dritte bei speziellen Partitionen oder auf dem Systemdatenträger.
- **Offline**- oder **schreibgeschützte** Datenträger werden nie verändert.
- Bezeichnungen und Dateisysteme werden geprüft, bevor sie an Windows weitergegeben werden.

## Tastenkombinationen

- `F5`: Datenträgerliste aktualisieren
- `F1`: diese Anleitung öffnen
- `Strg+N`: Partition erstellen
- `Strg+Umschalt+F`: formatieren
- `Strg+R`: Größe ändern
- `Entf`: löschen (mit Fokus auf der Tabelle)

## Das Vorgangsprotokoll

Jeder Vorgang wird (Beginn, Ergebnis und ein möglicher Fehler) in einer Textdatei protokolliert. Öffnen Sie den Ordner über **Hilfe > Protokollordner öffnen**. Die Dateien liegen in `%LocalAppData%\CandeDisk\logs`.

## Die Übersicht kopieren

**Datei > Datenträger-Übersicht kopieren** legt eine Textliste der Datenträger und Partitionen in die Zwischenablage, praktisch beim Melden eines Problems.

## Häufige Fragen

- **Ich sehe meinen Datenträger nicht.** Klicken Sie auf **Aktualisieren**. Ein Offline-Datenträger muss in der Windows-Datenträgerverwaltung online geschaltet werden.
- **Die Schaltflächen sind grau.** Das Programm ist schreibgeschützt (als Administrator neu starten) oder Sie haben keine für den Vorgang passende Partition bzw. keinen passenden nicht zugeordneten Bereich ausgewählt.
- **Ich kann eine Partition nicht erweitern.** Direkt rechts davon wird nicht zugeordneter Platz benötigt. CandeDisk verschiebt keine Partitionen.
- **„Not enough available capacity“ erscheint.** Windows hat nicht genug zusammenhängenden Platz gefunden: Wählen Sie eine etwas kleinere Größe oder die maximale Größe.
- **Kann ich zwei Partitionen zusammenführen?** Nicht direkt: Löschen Sie die zweite (nachdem Sie die Daten gesichert haben) und erweitern Sie die erste, sofern sie nebeneinanderliegen.

## Bekannte Einschränkungen

- Das **Verschieben** von Partitionen wird nicht unterstützt.
- Unterstützt werden die Dateisysteme **NTFS, exFAT und FAT32**.
- Dynamische Datenträger und Software-RAID-Konfigurationen werden nicht verwaltet.

Melden Sie Probleme oder Ideen auf [GitHub](https://github.com/candelab2026/candedisk/issues).
