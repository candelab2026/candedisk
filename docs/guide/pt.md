# Guia do CandeDisk

O CandeDisk é um gerenciador de discos e partições para Windows: mostra seus discos e permite formatar, criar, excluir e redimensionar partições com uma interface simples.

> As operações nos discos apagam dados de forma irreversível. Faça sempre um backup antes de continuar.

## Primeiros passos

- Abra o CandeDisk. Para alterar os discos são necessários privilégios de **administrador**: a versão Release os solicita sozinha (janela do UAC).
- Se o programa iniciar sem eles, mostra um banner amarelo e trabalha em modo **somente leitura**. O botão **Reiniciar como administrador** o reabre com as permissões necessárias.
- À esquerda fica a lista de discos. À direita você vê o mapa do disco selecionado e a tabela de suas partições.
- Para trocar o idioma, use o menu **Idioma**: a mudança é imediata e fica salva.

## O mapa do disco

A barra colorida mostra cada partição em proporção ao seu tamanho. As partições muito pequenas (EFI, MSR) têm uma largura mínima para continuarem clicáveis.

- **Azul:** a partição do Windows.
- **Âmbar:** partições especiais (EFI, recuperação, reservada).
- **Verde:** partições de dados.
- **Cinza:** espaço não alocado.

Clique em uma caixa para selecionar a partição correspondente na tabela.

## As operações

### Inicializar

Um disco novo aparece como "RAW" e não tem tabela de partições. **Inicializar** o prepara com uma tabela **GPT**. Fica ativo apenas para discos RAW.

### Criar uma partição

1. Selecione uma linha **Não alocado**.
2. Clique em **Criar** (`Ctrl+N`).
3. Escolha o tamanho em MB, o sistema de arquivos e o rótulo. Se escolher o tamanho máximo, o Windows usa todo o espaço realmente disponível.

### Formatar

1. Selecione a partição e clique em **Formatar** (`Ctrl+Shift+F`).
2. Escolha o sistema de arquivos (NTFS, exFAT ou FAT32) e o rótulo.
3. A **formatação rápida** é recomendada. A formatação completa grava todo o volume e é muito mais lenta.

**Todos os dados da partição são apagados.**

### Redimensionar

1. Selecione a partição e clique em **Redimensionar** (`Ctrl+R`).
2. O CandeDisk pergunta ao Windows o tamanho mínimo e máximo suportado e mostra para você.
3. Digite o novo tamanho em MB.

Para **estender** uma partição é preciso espaço não alocado **logo após** ela: se houver outra partição no meio, o Windows não permite estendê-la. Uma partição só pode ser **reduzida** até o espaço ocupado pelos seus dados.

### Excluir

Selecione a partição e clique em **Excluir** (`Del`, com o foco na tabela). As partições protegidas (EFI, OEM, recuperação) que o Gerenciamento de Disco não deixa excluir são removidas com o `diskpart` em modo "override", após confirmações adicionais.

## As proteções

O CandeDisk tenta evitar os erros mais graves:

- Você não pode **formatar nem excluir** a partição do Windows em uso, a unidade do sistema nem a partição EFI do disco pelo qual o computador inicia.
- Toda operação que apaga dados pede uma **dupla confirmação**, com o nome do disco, e uma terceira para partições especiais ou no disco do sistema.
- Discos **offline** ou **somente leitura** nunca são modificados.
- Rótulos e sistemas de arquivos são validados antes de serem passados ao Windows.

## Atalhos de teclado

- `F5`: atualiza a lista de discos
- `F1`: abre este guia
- `Ctrl+N`: criar partição
- `Ctrl+Shift+F`: formatar
- `Ctrl+R`: redimensionar
- `Del`: excluir (com o foco na tabela)

## O registro de operações

Cada operação é registrada (início, resultado e eventual erro) em um arquivo de texto. Abra a pasta em **Ajuda > Abrir pasta de logs**. Os arquivos ficam em `%LocalAppData%\CandeDisk\logs`.

## Copiar o resumo

**Arquivo > Copiar resumo dos discos** coloca na área de transferência uma lista em texto de discos e partições, útil para relatar um problema.

## Perguntas frequentes

- **Não vejo meu disco.** Clique em **Atualizar**. Um disco offline precisa ser colocado online pelo Gerenciamento de Disco do Windows.
- **Os botões estão cinza.** O programa está em modo somente leitura (reinicie como administrador), ou você não selecionou uma partição ou um espaço não alocado adequado à operação.
- **Não consigo estender uma partição.** É preciso espaço não alocado imediatamente à direita dela. O CandeDisk não move partições.
- **Aparece "Not enough available capacity".** O Windows não encontrou espaço contíguo suficiente: escolha um tamanho um pouco menor ou use o tamanho máximo.
- **Posso juntar duas partições?** Não diretamente: exclua a segunda (depois de salvar os dados) e estenda a primeira, se forem adjacentes.

## Limitações conhecidas

- **Mover** partições não é suportado.
- Os sistemas de arquivos suportados são **NTFS, exFAT e FAT32**.
- Discos dinâmicos e configurações de RAID por software não são gerenciados.

Relate problemas ou ideias no [GitHub](https://github.com/candelab2026/candedisk/issues).
