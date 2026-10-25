# CandeDisk guide

CandeDisk is a disk and partition manager for Windows: it shows your disks and lets you format, create, delete and resize partitions with a simple interface.

> Disk operations permanently erase data. Always back up before proceeding.

## Getting started

- Start CandeDisk. Changing disks needs **administrator** rights: the Release version asks for them by itself (UAC window).
- If the program starts without them, it shows a yellow banner and works in **read-only** mode. The **Restart as administrator** button reopens it with the right permissions.
- On the left is the list of disks. On the right you find the map of the selected disk and the table of its partitions.
- To change the language, use the **Language** menu: the change is immediate and is remembered.

## The disk map

The coloured bar shows each partition in proportion to its size. Very small partitions (EFI, MSR) have a minimum width so they stay clickable.

- **Blue:** the Windows partition.
- **Amber:** special partitions (EFI, recovery, reserved).
- **Green:** data partitions.
- **Grey:** unallocated space.

Click a box to select the matching partition in the table.

## Operations

### Initialize

A new disk shows up as "RAW" and has no partition table. **Initialize** prepares it with a **GPT** table. It is enabled only for RAW disks.

### Create a partition

1. Select an **Unallocated** row.
2. Press **Create** (`Ctrl+N`).
3. Choose the size in MB, the file system and the label. If you choose the maximum size, Windows uses all the space that is really available.

### Format

1. Select the partition and press **Format** (`Ctrl+Shift+F`).
2. Choose the file system (NTFS, exFAT or FAT32) and the label.
3. **Quick format** is recommended. A full format writes the whole volume and is much slower.

**All data on the partition is erased.**

### Resize

1. Select the partition and press **Resize** (`Ctrl+R`).
2. CandeDisk asks Windows for the minimum and maximum supported size and shows them to you.
3. Enter the new size in MB.

To **extend** a partition you need unallocated space **right after** it: if another partition sits in between, Windows does not allow extending it. A partition can only be **shrunk** down to the space used by its data.

### Delete

Select the partition and press **Delete** (`Del`, with the focus on the table). Protected partitions (EFI, OEM, recovery) that Disk Management refuses to delete are removed with `diskpart` in "override" mode, after extra confirmations.

## Safety rules

CandeDisk tries to prevent the worst mistakes:

- You cannot **format or delete** the running Windows partition, the system drive, or the EFI partition of the disk you boot from.
- Every operation that erases data asks for a **double confirmation**, showing the disk name, and a third one for special partitions or the system disk.
- **Offline** or **read-only** disks are never modified.
- Labels and file systems are validated before being passed to Windows.

## Keyboard shortcuts

- `F5`: refresh the disk list
- `F1`: open this guide
- `Ctrl+N`: create partition
- `Ctrl+Shift+F`: format
- `Ctrl+R`: resize
- `Del`: delete (with the focus on the table)

## The operation log

Every operation is recorded (start, result and any error) in a text file. Open the folder from **Help > Open logs folder**. The files are in `%LocalAppData%\CandeDisk\logs`.

## Copying the summary

**File > Copy disk summary** puts a text list of disks and partitions on the clipboard, handy when reporting a problem.

## Frequently asked questions

- **I can't see my disk.** Press **Refresh**. An offline disk must be brought online from Windows Disk Management.
- **The buttons are grey.** The program is read-only (restart it as administrator), or you have not selected a partition or unallocated space suitable for the operation.
- **I can't extend a partition.** You need unallocated space immediately to its right. CandeDisk does not move partitions.
- **"Not enough available capacity" appears.** Windows did not find enough contiguous space: choose a slightly smaller size or use the maximum size.
- **Can I merge two partitions?** Not directly: delete the second one (after saving your data) and extend the first, if they are adjacent.

## Known limitations

- **Moving** partitions is not supported.
- The supported file systems are **NTFS, exFAT and FAT32**.
- Dynamic disks and software RAID configurations are not handled.

Report problems or ideas on [GitHub](https://github.com/candelab2026/candedisk/issues).
