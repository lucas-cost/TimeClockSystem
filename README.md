# Desafio Técnico: Sistema de Ponto Eletrônico

Este repositório contém a solução para o desafio técnico de desenvolvimento de um sistema de ponto eletrônico offline-first, utilizando C# com .NET 8 e WPF.

## 🚀 Resumo das Funcionalidades

A aplicação foi desenvolvida seguindo os princípios da **Clean Architecture** e implementa um conjunto robusto de funcionalidades, incluindo:

- **Registro de Ponto com Captura de Imagem:** Utiliza a webcam do dispositivo para capturar uma foto no momento do registro.
- **Arquitetura Offline-First:** O sistema é totalmente funcional sem conexão com a internet, persistindo todos os dados em um banco de dados local SQLite.
- **Sincronização Automática:** Um serviço em segundo plano (`BackgroundService`) detecta registros pendentes e os sincroniza com o servidor quando a conexão é restabelecida.
- **Resiliência de Rede:** Implementa políticas de **Retry** (com backoff exponencial) e **Circuit Breaker** com Polly para lidar com falhas de comunicação com a API.
- **Validação de Qualidade de Imagem:** Analisa o brilho e o foco da imagem capturada para garantir uma qualidade mínima.
- **Validação de Regras de Negócio:** Determina o tipo de ponto (Entrada, Pausa, Saída) e valida a jornada mínima de trabalho.
- **Arquitetura Orientada a Eventos:** Publica eventos de sucesso e falha em um broker **RabbitMQ** (via MassTransit) para desacoplar a notificação de outras partes do sistema.
- **Integração Contínua (CI):** Um pipeline simples com **GitHub Actions** garante que o projeto compile e passe nos testes a cada alteração.

## 🛠️ Tecnologias Utilizadas

- **Plataforma:** .NET 8, C#, WPF
- **Padrões e Arquitetura:** Clean Architecture, SOLID, MVVM, Repository Pattern, CQRS (com MediatR)
- **Banco de Dados:** Entity Framework Core 8, SQLite
- **Resiliência:** Polly
- **Hardware:** OpenCvSharp
- **Mensageria:** MassTransit, RabbitMQ (via Docker)
- **Testes:** NUnit, Moq
- **DevOps:** Docker, GitHub Actions

## 📋 Pré-requisitos

Para executar e testar este projeto, você precisará ter os seguintes softwares instalados:

1.  **.NET 8 SDK:** [Link para download](https://dotnet.microsoft.com/download/dotnet/8.0)
2.  **Visual Studio 2022:** Com a carga de trabalho "Desenvolvimento para desktop com .NET".
3.  **Docker Desktop:** Para executar o container do RabbitMQ. [Link para download](https://www.docker.com/products/docker-desktop/)
4.  **Uma webcam:** Física ou virtual (ex: Iriun Webcam).
5.  **Postman:** Para executar a API Mock.

## ▶️ Como Executar o Projeto

Siga estes passos para configurar e executar a aplicação em sua máquina.

### 1. Clonar o Repositório
```bash
git clone [https://github.com/lucas-cost/TimeClockSystem.git](https://github.com/lucas-cost/TimeClockSystem.git)
cd TimeClockSystem
```
### 2. Configurar o RabbitMQ com Docker
```bash
docker run -d --name timeclock-rabbit -p 5672:5672 -p 15672:15672 rabbitmq:3-management
```

![Container Docker](https://github.com/lucas-cost/TimeClockSystem/blob/developer/docs/Images/IMG-CT-003/docker.png)

### 2. Execução Manual Migrations
```bash
dotnet ef migrations add InitialCreate --startup-project ".\src\TimeClockSystem.UI\TimeClockSystem.UI.csproj" --project ".\src\TimeClockSystem.Infrastructure\TimeClockSystem.Infrastructure.csproj"
```
```bash
dotnet ef database update --startup-project ".\src\TimeClockSystem.UI\TimeClockSystem.UI.csproj" --project ".\src\TimeClockSystem.Infrastructure\TimeClockSystem.Infrastructure.csproj"
```