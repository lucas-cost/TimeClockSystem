# Decis�es T�cnicas Tomadas

Este documento resume as principais decis�es de arquitetura e tecnologia tomadas durante o desenvolvimento do Sistema de Ponto Eletr�nico.

### 1. Arquitetura: Clean Architecture
- **Decis�o:** Adotar a Clean Architecture com separa��o expl�cita em camadas (`Core`, `Application`, `Infrastructure`, `UI`).
- **Justificativa:** Para maximizar a separa��o de responsabilidades (SRP), garantir alta testabilidade atrav�s da Invers�o de Depend�ncia (DIP) e desacoplar a l�gica de neg�cio dos detalhes de implementa��o (framework de UI, banco de dados, etc.).

### 2. Chave Prim�ria: `Guid` vs. `int`
- **Decis�o:** Utilizar `Guid` como chave prim�ria para a entidade `TimeClockRecord`.
- **Justificativa:** Essencial para um sistema "offline-first". Os IDs s�o gerados no cliente antes da sincroniza��o, eliminando o risco de conflitos de chaves prim�rias quando m�ltiplos dispositivos sincronizam dados com o servidor central.

### 3. Resili�ncia de API: Polly (Retry e Circuit Breaker)
- **Decis�o:** Implementar as pol�ticas de Retry (com backoff exponencial) e Circuit Breaker para as chamadas `HttpClient`.
- **Justificativa:** A pol�tica de Retry lida com falhas transientes e moment�neas da rede. O Circuit Breaker protege a aplica��o e a API de sobrecarga em caso de falhas prolongadas, parando de fazer chamadas desnecess�rias.

### 4. Mensageria: MassTransit com RabbitMQ
- **Decis�o:** Usar MassTransit sobre RabbitMQ para publicar eventos de sucesso e falha no registro de ponto, em vez de substituir completamente a chamada `HttpClient`.
- **Justificativa:** Esta abordagem de "arquitetura orientada a eventos" � mais flex�vel e escal�vel. Ela desacopla a notifica��o do ato de registrar, permitindo que outros sistemas (consumidores) reajam a esses eventos sem que o sistema de ponto precise conhec�-los. O MassTransit foi escolhido para abstrair a complexidade da biblioteca cliente do RabbitMQ.

### 5. Testabilidade: Abstra��es e Factories
- **Decis�o:** Refatorar classes com forte acoplamento a depend�ncias externas (`VideoCapture` do OpenCV, l�gica de neg�cio no `CommandHandler`) para dependerem de abstra��es (`IVideoCaptureWrapper`, `ITimeClockService`).
- **Justificativa:** A refatora��o foi crucial para permitir testes de unidade isolados. Ao depender de interfaces, podemos facilmente "mockar" o comportamento de servi�os externos (como a c�mera) e da l�gica de neg�cio, garantindo que cada componente possa ser testado de forma independente.

### 6. Gerenciamento de Depend�ncias: .NET Generic Host
- **Decis�o:** Utilizar o `IHost` do .NET para configurar o cont�iner de inje��o de depend�ncia (DI) e gerenciar o ciclo de vida dos servi�os.
- **Justificativa:** Centraliza toda a configura��o da aplica��o no `App.xaml.cs`, simplifica o gerenciamento do ciclo de vida de servi�os (incluindo `IDisposable`) e fornece uma base robusta para executar servi�os em segundo plano (`IHostedService`).