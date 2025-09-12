# Arquitetura da Solução - Sistema de Ponto Eletrônico

Este documento descreve a arquitetura do Sistema de Ponto Eletrônico, que foi projetado seguindo os princípios da **Clean Architecture** para garantir uma clara separação de responsabilidades, alta testabilidade e manutenibilidade.

A solução é dividida em cinco projetos principais, representando as camadas da arquitetura:

### Camadas da Arquitetura

#### 1. `Core` (Lógica de Negócio)
É o coração da aplicação. Contém as entidades de negócio (`TimeClockRecord`), enums (`RecordType`, `SyncStatus`), exceções de negócio (`BusinessRuleException`) e as interfaces (contratos) para os serviços e repositórios (`ITimeClockRepository`, `IApiClient`, `ITimeClockService`, `IEventPublisher`). Esta camada não tem dependências externas e não sabe nada sobre banco de dados, UI ou APIs.

#### 2. `Application` (Casos de Uso)
Esta camada orquestra o fluxo de dados e as ações do sistema. Ela contém os "Casos de Uso" implementados com o padrão **CQRS** via MediatR.
- **Commands (`RegisterPointCommand`):** Representam ações que modificam o estado do sistema.
- **Handlers (`RegisterPointCommandHandler`):** Contêm a lógica para executar os comandos, coordenando os serviços e repositórios definidos no `Core`.
- **DTOs (Data Transfer Objects):** Objetos simples para transferir dados entre as camadas, como `RegisterPointRequestDto` (para entrada de dados) e `RegisterPointResponseDto` (para respostas de API).

#### 3. `Infrastructure` (Detalhes Externos)
Esta camada contém as implementações concretas das interfaces definidas no `Core` e `Application`. Ela lida com todo o "mundo exterior".
- **Persistência:** `TimeClockRepository` implementa o `ITimeClockRepository` usando Entity Framework Core e SQLite.
- **Acesso à API:** `ApiClient` implementa o `IApiClient` usando `HttpClient` e políticas de resiliência do Polly.
- **Hardware:** `WebcamService` e `WebcamFactory` implementam a `IWebcamService`, lidando com a captura de imagem via OpenCVSharp.
- **Mensageria:** `MassTransitEventPublisher` implementa o `IEventPublisher`, publicando eventos em um broker RabbitMQ.
- **Mapeamento:** `MappingProfile` contém as regras do AutoMapper para converter entre Entidades e DTOs.

#### 4. `UI` (Interface do Usuário - WPF)
A camada de apresentação. É a "casca" externa da aplicação.
- **Padrão MVVM:** Utiliza o padrão Model-View-ViewModel para separar a lógica de apresentação (`MainViewModel`) da aparência visual (`MainWindow.xaml`).
- **Composição:** É o ponto de entrada da aplicação (`App.xaml.cs`) e atua como a "Raiz de Composição", onde todas as dependências são registradas e resolvidas usando o .NET Generic Host.

#### 5. `BackgroundServices`
Contém os serviços que rodam em segundo plano.
- **`SyncWorker`:** Implementado como um `IHostedService`, este serviço é responsável por verificar periodicamente por registros de ponto pendentes e sincronizá-los com a API externa, garantindo a funcionalidade offline-first.

### Fluxo da Dependência
A regra da dependência é estritamente seguida: todas as dependências apontam para o centro (`Core`), garantindo que a lógica de negócio seja independente dos detalhes de implementação.
`UI` -> `Application` -> `Core` <- `Infrastructure`