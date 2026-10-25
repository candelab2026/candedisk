# Guide de CandeDisk

CandeDisk est un gestionnaire de disques et de partitions pour Windows : il affiche vos disques et permet de formater, créer, supprimer et redimensionner les partitions avec une interface simple.

> Les opérations sur les disques effacent les données de manière irréversible. Faites toujours une sauvegarde avant de continuer.

## Pour commencer

- Lancez CandeDisk. Modifier les disques nécessite les droits d’**administrateur** : la version Release les demande d’elle-même (fenêtre UAC).
- Si le programme démarre sans ces droits, il affiche une bannière jaune et fonctionne en **lecture seule**. Le bouton **Redémarrer en tant qu’administrateur** le rouvre avec les permissions nécessaires.
- À gauche se trouve la liste des disques. À droite, vous voyez la carte du disque sélectionné et le tableau de ses partitions.
- Pour changer de langue, utilisez le menu **Langue** : le changement est immédiat et mémorisé.

## La carte du disque

La barre colorée montre chaque partition proportionnellement à sa taille. Les très petites partitions (EFI, MSR) ont une largeur minimale pour rester cliquables.

- **Bleu :** la partition de Windows.
- **Ambre :** les partitions spéciales (EFI, restauration, réservée).
- **Vert :** les partitions de données.
- **Gris :** l’espace non alloué.

Cliquez sur une case pour sélectionner la partition correspondante dans le tableau.

## Les opérations

### Initialiser

Un nouveau disque apparaît comme « RAW » et n’a pas de table de partitions. **Initialiser** le prépare avec une table **GPT**. Le bouton n’est actif que pour les disques RAW.

### Créer une partition

1. Sélectionnez une ligne **Non alloué**.
2. Cliquez sur **Créer** (`Ctrl+N`).
3. Choisissez la taille en Mo, le système de fichiers et le nom. Si vous choisissez la taille maximale, Windows utilise tout l’espace réellement disponible.

### Formater

1. Sélectionnez la partition et cliquez sur **Formater** (`Ctrl+Maj+F`).
2. Choisissez le système de fichiers (NTFS, exFAT ou FAT32) et le nom.
3. Le **formatage rapide** est recommandé. Le formatage complet écrit tout le volume et est beaucoup plus lent.

**Toutes les données de la partition sont effacées.**

### Redimensionner

1. Sélectionnez la partition et cliquez sur **Redimensionner** (`Ctrl+R`).
2. CandeDisk demande à Windows la taille minimale et maximale prise en charge et vous les affiche.
3. Saisissez la nouvelle taille en Mo.

Pour **étendre** une partition, il faut de l’espace non alloué **juste après** elle : si une autre partition se trouve entre les deux, Windows ne permet pas de l’étendre. Une partition ne peut être **réduite** que jusqu’à l’espace occupé par ses données.

### Supprimer

Sélectionnez la partition et cliquez sur **Supprimer** (`Suppr`, avec le focus sur le tableau). Les partitions protégées (EFI, OEM, restauration) que la Gestion des disques refuse de supprimer sont retirées avec `diskpart` en mode « override », après des confirmations supplémentaires.

## Les protections

CandeDisk essaie de vous éviter les erreurs les plus graves :

- Vous ne pouvez **ni formater ni supprimer** la partition de Windows en cours d’utilisation, le lecteur système ni la partition EFI du disque depuis lequel vous démarrez.
- Toute opération qui efface des données demande une **double confirmation**, avec le nom du disque, et une troisième pour les partitions spéciales ou celles du disque système.
- Les disques **hors ligne** ou en **lecture seule** ne sont jamais modifiés.
- Les noms et les systèmes de fichiers sont validés avant d’être transmis à Windows.

## Raccourcis clavier

- `F5` : actualise la liste des disques
- `F1` : ouvre ce guide
- `Ctrl+N` : créer une partition
- `Ctrl+Maj+F` : formater
- `Ctrl+R` : redimensionner
- `Suppr` : supprimer (avec le focus sur le tableau)

## Le journal des opérations

Chaque opération est enregistrée (début, résultat et éventuelle erreur) dans un fichier texte. Ouvrez le dossier depuis **Aide > Ouvrir le dossier des journaux**. Les fichiers se trouvent dans `%LocalAppData%\CandeDisk\logs`.

## Copier le résumé

**Fichier > Copier le résumé des disques** place dans le presse-papiers une liste textuelle des disques et des partitions, utile pour signaler un problème.

## Questions fréquentes

- **Je ne vois pas mon disque.** Cliquez sur **Actualiser**. Un disque hors ligne doit être remis en ligne depuis la Gestion des disques de Windows.
- **Les boutons sont grisés.** Le programme est en lecture seule (redémarrez-le en tant qu’administrateur), ou vous n’avez pas sélectionné de partition ou d’espace non alloué adapté à l’opération.
- **Je n’arrive pas à étendre une partition.** Il faut de l’espace non alloué immédiatement à sa droite. CandeDisk ne déplace pas les partitions.
- **« Not enough available capacity » s’affiche.** Windows n’a pas trouvé assez d’espace contigu : choisissez une taille un peu plus petite ou utilisez la taille maximale.
- **Puis-je fusionner deux partitions ?** Pas directement : supprimez la seconde (après avoir sauvegardé les données) et étendez la première, si elles sont adjacentes.

## Limites connues

- Le **déplacement** de partitions n’est pas pris en charge.
- Les systèmes de fichiers pris en charge sont **NTFS, exFAT et FAT32**.
- Les disques dynamiques et les configurations RAID logicielles ne sont pas gérés.

Signalez problèmes et idées sur [GitHub](https://github.com/candelab2026/candedisk/issues).
