# Arquitetura da Solu��o - Sistema de Ponto Eletr�nico

Este documento descreve a arquitetura do Sistema de Ponto Eletr�nico, que foi projetado seguindo os princ�pios da **Clean Architecture** para garantir uma clara separa��o de responsabilidades, alta testabilidade e manutenibilidade.

A solu��o � dividida em cinco projetos principais, representando as camadas da arquitetura:

### Camadas da Arquitetura

#### 1. `Core` (L�gica de Neg�cio)
� o cora��o da aplica��o. Cont�m as entidades de neg�cio (`TimeClockRecord`), enums (`RecordType`, `SyncStatus`), exce��es de neg�cio (`BusinessRuleException`) e as interfaces (contratos) para os servi�os e reposit�rios (`ITimeClockRepository`, `IApiClient`, `ITimeClockService`, `IEventPublisher`). Esta camada n�o tem depend�ncias externas e n�o sabe nada sobre banco de dados, UI ou APIs.

#### 2. `Application` (Casos de Uso)
Esta camada orquestra o fluxo de dados e as a��es do sistema. Ela cont�m os "Casos de Uso" implementados com o padr�o **CQRS** via MediatR.
- **Commands (`RegisterPointCommand`):** Representam a��es que modificam o estado do sistema.
- **Handlers (`RegisterPointCommandHandler`):** Cont�m a l�gica para executar os comandos, coordenando os servi�os e reposit�rios definidos no `Core`.
- **DTOs (Data Transfer Objects):** Objetos simples para transferir dados entre as camadas, como `RegisterPointRequestDto` (para entrada de dados) e `RegisterPointResponseDto` (para respostas de API).

#### 3. `Infrastructure` (Detalhes Externos)
Esta camada cont�m as implementa��es concretas das interfaces definidas no `Core` e `Application`. Ela lida com todo o "mundo exterior".
- **Persist�ncia:** `TimeClockRepository` implementa o `ITimeClockRepository` usando Entity Framework Core e SQLite.
- **Acesso � API:** `ApiClient` implementa o `IApiClient` usando `HttpClient` e pol�ticas de resili�ncia do Polly.
- **Hardware:** `WebcamService` e `WebcamFactory` implementam a `IWebcamService`, lidando com a captura de imagem via OpenCVSharp.
- **Mensageria:** `MassTransitEventPublisher` implementa o `IEventPublisher`, publicando eventos em um broker RabbitMQ.
- **Mapeamento:** `MappingProfile` cont�m as regras do AutoMapper para converter entre Entidades e DTOs.

#### 4. `UI` (Interface do Usu�rio - WPF)
A camada de apresenta��o. � a "casca" externa da aplica��o.
- **Padr�o MVVM:** Utiliza o padr�o Model-View-ViewModel para separar a l�gica de apresenta��o (`MainViewModel`) da apar�ncia visual (`MainWindow.xaml`).
- **Composi��o:** � o ponto de entrada da aplica��o (`App.xaml.cs`) e atua como a "Raiz de Composi��o", onde todas as depend�ncias s�o registradas e resolvidas usando o .NET Generic Host.

#### 5. `BackgroundServices`
Cont�m os servi�os que rodam em segundo plano.
- **`SyncWorker`:** Implementado como um `IHostedService`, este servi�o � respons�vel por verificar periodicamente por registros de ponto pendentes e sincroniz�-los com a API externa, garantindo a funcionalidade offline-first.

### Fluxo da Depend�ncia
A regra da depend�ncia � estritamente seguida: todas as depend�ncias apontam para o centro (`Core`), garantindo que a l�gica de neg�cio seja independente dos detalhes de implementa��o.
`UI` -> `Application` -> `Core` <- `Infrastructure`