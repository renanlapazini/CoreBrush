# CoreBrush - Editor de Imagens (em desenvolvimento)

**Desenvolvido por:** Renan Lapazini

## Descrição

CoreBrush é um software de edição de imagens desenvolvido em C# com Windows Forms, que oferece diversas funcionalidades para processamento e manipulação de imagens.

## Funcionalidades

### Interface
- Interface com duas janelas: uma para exibir a imagem original e outra para a imagem transformada
- Menus organizados por categorias de funcionalidades
- Layout responsivo que se adapta ao redimensionamento da janela

### Menu Arquivo
- **Abrir imagem:** Carrega uma imagem nos formatos JPG, PNG, BMP, GIF
- **Salvar imagem:** Salva a imagem transformada
- **Sobre:** Informações sobre o software
- **Sair:** Fecha a aplicação

### Menu Transformações Geométricas
- **Transladar:** Move a imagem no plano
- **Rotacionar:** Rotaciona a imagem
- **Espelhar:** Cria espelhamento da imagem
- **Aumentar:** Aumenta o tamanho da imagem
- **Diminuir:** Diminui o tamanho da imagem

### Menu Filtros
- **Brilho e Contraste:** Ajusta brilho e contraste
- **Grayscale:** Converte para tons de cinza
- **Passa Baixa:** Aplica filtro passa-baixa
- **Passa Alta:** Aplica filtro passa-alta
- **Threshold:** Aplica limiarização
- **Afinamento:** Aplica afinamento na imagem

### Menu Morfologia Matemática
- **Dilatação:** Operação morfológica de dilatação
- **Erosão:** Operação morfológica de erosão
- **Abertura:** Operação morfológica de abertura
- **Fechamento:** Operação morfológica de fechamento

### Menu Extração de Características
- **Desafio:** Funcionalidade especial (em definição)

## Requisitos do Sistema

- .NET 8.0 ou superior
- Windows (devido ao uso de Windows Forms)

## Como Executar

1. Clone o repositório:
```bash
git clone https://gitlab.com/Renan_Lapazini/corebrush.git
```

2. Navegue até a pasta do projeto:
```bash
cd corebrush
```

3. Compile e execute:
```bash
dotnet run
```

## Estrutura do Projeto

- `Program.cs` - Ponto de entrada da aplicação
- `MainForm.cs` - Formulário principal com toda a interface e lógica
- `corebrush.csproj` - Arquivo de projeto .NET

## Status de Desenvolvimento

🚧 **Em desenvolvimento** - A estrutura da interface está completa. As funcionalidades de processamento de imagem estão sendo implementadas gradualmente.

## Tecnologias Utilizadas

- C# .NET 8.0
- Windows Forms
- System.Drawing para manipulação de imagens


