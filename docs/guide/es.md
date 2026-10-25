# Guía de CandeDisk

CandeDisk es un administrador de discos y particiones para Windows: muestra tus discos y permite formatear, crear, eliminar y cambiar el tamaño de las particiones con una interfaz sencilla.

> Las operaciones en los discos borran datos de forma irreversible. Haz siempre una copia de seguridad antes de continuar.

## Primeros pasos

- Inicia CandeDisk. Para modificar los discos se necesitan permisos de **administrador**: la versión Release los solicita sola (ventana UAC).
- Si el programa arranca sin ellos, muestra un banner amarillo y trabaja en modo de **solo lectura**. El botón **Reiniciar como administrador** lo vuelve a abrir con los permisos necesarios.
- A la izquierda está la lista de discos. A la derecha verás el mapa del disco seleccionado y la tabla de sus particiones.
- Para cambiar el idioma usa el menú **Idioma**: el cambio es inmediato y se recuerda.

## El mapa del disco

La barra de colores muestra cada partición en proporción a su tamaño. Las particiones muy pequeñas (EFI, MSR) tienen un ancho mínimo para que sigan siendo seleccionables.

- **Azul:** la partición de Windows.
- **Ámbar:** particiones especiales (EFI, recuperación, reservada).
- **Verde:** particiones de datos.
- **Gris:** espacio no asignado.

Haz clic en un recuadro para seleccionar la partición correspondiente en la tabla.

## Las operaciones

### Inicializar

Un disco nuevo aparece como «RAW» y no tiene tabla de particiones. **Inicializar** lo prepara con una tabla **GPT**. Solo está activo para los discos RAW.

### Crear una partición

1. Selecciona una fila **No asignado**.
2. Pulsa **Crear** (`Ctrl+N`).
3. Elige el tamaño en MB, el sistema de archivos y la etiqueta. Si eliges el tamaño máximo, Windows usa todo el espacio realmente disponible.

### Formatear

1. Selecciona la partición y pulsa **Formatear** (`Ctrl+Mayús+F`).
2. Elige el sistema de archivos (NTFS, exFAT o FAT32) y la etiqueta.
3. Se recomienda el **formateo rápido**. El formateo completo escribe todo el volumen y es mucho más lento.

**Se borran todos los datos de la partición.**

### Cambiar tamaño

1. Selecciona la partición y pulsa **Cambiar tamaño** (`Ctrl+R`).
2. CandeDisk pide a Windows el tamaño mínimo y máximo admitido y te lo muestra.
3. Introduce el nuevo tamaño en MB.

Para **ampliar** una partición hace falta espacio no asignado **justo a continuación**: si hay otra partición en medio, Windows no permite ampliarla. Una partición solo se puede **reducir** hasta el espacio ocupado por sus datos.

### Eliminar

Selecciona la partición y pulsa **Eliminar** (`Supr`, con el foco en la tabla). Las particiones protegidas (EFI, OEM, recuperación) que la Administración de discos no deja eliminar se quitan con `diskpart` en modo «override», tras confirmaciones adicionales.

## Las protecciones

CandeDisk intenta evitar los errores más graves:

- No puedes **formatear ni eliminar** la partición de Windows en uso, la unidad del sistema ni la partición EFI del disco desde el que arrancas el equipo.
- Cada operación que borra datos pide una **doble confirmación**, con el nombre del disco, y una tercera para las particiones especiales o las del disco del sistema.
- Los discos **sin conexión** o de **solo lectura** no se modifican.
- Las etiquetas y los sistemas de archivos se validan antes de pasarlos a Windows.

## Atajos de teclado

- `F5`: actualiza la lista de discos
- `F1`: abre esta guía
- `Ctrl+N`: crear partición
- `Ctrl+Mayús+F`: formatear
- `Ctrl+R`: cambiar tamaño
- `Supr`: eliminar (con el foco en la tabla)

## El registro de operaciones

Cada operación se registra (inicio, resultado y posible error) en un archivo de texto. Abre la carpeta desde **Ayuda > Abrir carpeta de registros**. Los archivos están en `%LocalAppData%\CandeDisk\logs`.

## Copiar el resumen

**Archivo > Copiar resumen de los discos** pone en el portapapeles una lista de texto de discos y particiones, útil para informar de un problema.

## Preguntas frecuentes

- **No veo mi disco.** Pulsa **Actualizar**. Un disco sin conexión hay que ponerlo en línea desde la Administración de discos de Windows.
- **Los botones están en gris.** El programa está en solo lectura (reinícialo como administrador), o no has seleccionado una partición o un espacio no asignado adecuados para la operación.
- **No puedo ampliar una partición.** Hace falta espacio no asignado inmediatamente a su derecha. CandeDisk no mueve particiones.
- **Aparece «Not enough available capacity».** Windows no ha encontrado suficiente espacio contiguo: elige un tamaño algo menor o usa el tamaño máximo.
- **¿Puedo unir dos particiones?** No directamente: elimina la segunda (tras guardar los datos) y amplía la primera, si son adyacentes.

## Limitaciones conocidas

- **Mover** particiones no es compatible.
- Los sistemas de archivos admitidos son **NTFS, exFAT y FAT32**.
- No se gestionan los discos dinámicos ni las configuraciones RAID por software.

Informa de problemas o ideas en [GitHub](https://github.com/candelab2026/candedisk/issues).
