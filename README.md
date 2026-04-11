# 📚 RevisaFácil

![Dashboard Principal](assets/dashboard.PNG)

**RevisaFácil** é um gestor de estudos inteligente desenvolvido para Windows, focado na fixação de conteúdo por meio de ciclos de revisão automatizados. Ideal para concurseiros e estudantes de Direito que precisam dominar grandes volumes de matérias.

O sistema substitui planilhas complexas, automatizando o cálculo de prazos e oferecendo lembretes ativos via Telegram.

## 🚀 Funcionalidades

- **Ciclos de Revisão Automatizados:** Calcula automaticamente as revisões de 30, 60, 90, 120 e 150 dias.
- **Calendário Dinâmico:** Interface moderna para anotações diárias com cards coloridos que indicam atividades.
- **Painel de Desempenho:** Gráficos interativos (LiveCharts) com distribuição de assuntos por disciplina.
- **Alertas via Telegram:** Integração com Bot API para notificar revisões pendentes ou atrasadas.
- **Personalização de Intervalos:** Altere os dias de revisão diretamente no cabeçalho com persistência no banco de dados.
- **Interface Moderna:** Desenvolvido em WPF com foco em usabilidade e agilidade (atalhos de teclado e cliques inteligentes).

## 🛠️ Tecnologias Utilizadas

- **Linguagem:** C#
- **Framework:** .NET 8 (WPF)
- **Banco de Dados:** SQLite
- **ORM:** Entity Framework Core 8 (Lazy Loading Proxies)
- **Gráficos:** LiveCharts.Wpf
- **Notificações:** Telegram.Bot API

## 📂 Estrutura do Projeto

O projeto segue uma arquitetura organizada para fácil manutenção:
- `Data/`: Contexto do banco de dados e configurações Fluent API.
- `Models/`: Entidades de Disciplina, Assunto, Notas e Configurações.
- `Views/`: Interfaces XAML (Dashboard, Calendário, Formulários).
- `Helpers/`: Conversores de UI e gerenciadores de tema.
- `Services/`: Lógica de integração com o Telegram.

## 🔧 Como Executar

1. Clone o repositório:
   ```bash
   git clone [https://github.com/josecarlossouza/RevisaFacil.git](https://github.com/josecarlossouza/RevisaFacil.git)
   ```

2. Certifique-se de ter o SDK do .NET 8 instalado.

3. Abra o arquivo RevisaFacil.sln no Visual Studio 2022.

4. Restaure os pacotes NuGet.

5. Pressione F5 para compilar e rodar.


Desenvolvido por Jose Carlos da Silva Souza

