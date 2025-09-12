# Decisões Técnicas Tomadas

Este documento resume as principais decisões de arquitetura e tecnologia tomadas durante o desenvolvimento do Sistema de Ponto Eletrônico.

### 1. Arquitetura: Clean Architecture
- **Decisão:** Adotar a Clean Architecture com separação explícita em camadas (`Core`, `Application`, `Infrastructure`, `UI`).
- **Justificativa:** Para maximizar a separação de responsabilidades (SRP), garantir alta testabilidade através da Inversão de Dependência (DIP) e desacoplar a lógica de negócio dos detalhes de implementação (framework de UI, banco de dados, etc.).

### 2. Chave Primária: `Guid` vs. `int`
- **Decisão:** Utilizar `Guid` como chave primária para a entidade `TimeClockRecord`.
- **Justificativa:** Essencial para um sistema "offline-first". Os IDs são gerados no cliente antes da sincronização, eliminando o risco de conflitos de chaves primárias quando múltiplos dispositivos sincronizam dados com o servidor central.

### 3. Resiliência de API: Polly (Retry e Circuit Breaker)
- **Decisão:** Implementar as políticas de Retry (com backoff exponencial) e Circuit Breaker para as chamadas `HttpClient`.
- **Justificativa:** A política de Retry lida com falhas transientes e momentâneas da rede. O Circuit Breaker protege a aplicação e a API de sobrecarga em caso de falhas prolongadas, parando de fazer chamadas desnecessárias.

### 4. Mensageria: MassTransit com RabbitMQ
- **Decisão:** Usar MassTransit sobre RabbitMQ para publicar eventos de sucesso e falha no registro de ponto, em vez de substituir completamente a chamada `HttpClient`.
- **Justificativa:** Esta abordagem de "arquitetura orientada a eventos" é mais flexível e escalável. Ela desacopla a notificação do ato de registrar, permitindo que outros sistemas (consumidores) reajam a esses eventos sem que o sistema de ponto precise conhecê-los. O MassTransit foi escolhido para abstrair a complexidade da biblioteca cliente do RabbitMQ.

### 5. Testabilidade: Abstrações e Factories
- **Decisão:** Refatorar classes com forte acoplamento a dependências externas (`VideoCapture` do OpenCV, lógica de negócio no `CommandHandler`) para dependerem de abstrações (`IVideoCaptureWrapper`, `ITimeClockService`).
- **Justificativa:** A refatoração foi crucial para permitir testes de unidade isolados. Ao depender de interfaces, podemos facilmente "mockar" o comportamento de serviços externos (como a câmera) e da lógica de negócio, garantindo que cada componente possa ser testado de forma independente.

### 6. Gerenciamento de Dependências: .NET Generic Host
- **Decisão:** Utilizar o `IHost` do .NET para configurar o contêiner de injeção de dependência (DI) e gerenciar o ciclo de vida dos serviços.
- **Justificativa:** Centraliza toda a configuração da aplicação no `App.xaml.cs`, simplifica o gerenciamento do ciclo de vida de serviços (incluindo `IDisposable`) e fornece uma base robusta para executar serviços em segundo plano (`IHostedService`).