# Melhorias Futuras Identificadas

Esta é uma lista de possíveis melhorias e funcionalidades que podem ser implementadas para aprimorar o Sistema de Ponto Eletrônico.

### Resiliência e Monitoramento
- **Health Checks Dedicados:** Criar um endpoint de health check na API mock e um serviço mais robusto no cliente que possa ser exposto para ferramentas de monitoramento.
- **Métricas e Tracing (Observability):** Integrar ferramentas como OpenTelemetry para coletar métricas (ex: tempo de chamada da API, taxa de falhas do Circuit Breaker) e traces distribuídos, fornecendo uma visão mais profunda do comportamento do sistema.

### Código e Regras de Negócio
- **Expandir CQRS (Queries):** Implementar a parte de "Query" do CQRS para otimizar operações de leitura, como a exibição de relatórios de pontos, usando projeções diretas para DTOs.
- **Validações Adicionais:** Implementar regras de negócio mais complexas, como tempo mínimo de almoço ou validação de intervalo entre quaisquer dois pontos (ex: 1 minuto) para evitar registros duplicados.
- **Validação com FluentValidation:** Substituir as validações manuais por um sistema robusto como o FluentValidation, que se integra bem com o MediatR através de behaviors de pipeline.
- **Mensagens via arquivo .resx:** Externalizar mensagens de erro e validação para arquivos de recursos (.resx) para facilitar a manutenção e internacionalização.

### Segurança
- **Criptografia de Dados:** Criptografar o arquivo de banco de dados SQLite local para proteger os dados do funcionário em repouso.
- **Autenticação Real:** Implementar um fluxo de autenticação real (ex: OAuth 2.0) onde a aplicação obtém um JWT (JSON Web Token) de um servidor de identidade, em vez de usar um token fixo no `appsettings.json`.

### Experiência do Usuário (UX/UI)
- **Seleção de Câmera:** Se múltiplas webcams forem detectadas, permitir que o usuário escolha qual delas utilizar.
- **Feedback de Operação:** Adicionar indicadores visuais (ex: spinners de carregamento) para operações de longa duração, como a sincronização de um grande número de pontos.

### DevOps
- **Containerização da Aplicação:** Criar um `Dockerfile` para a aplicação WPF, permitindo que ela seja distribuída e executada em ambientes de contêiner (útil para cenários de virtualização de desktop).
- **Expandir Pipeline de CI/CD:** Adicionar etapas ao pipeline do GitHub Actions para gerar um instalador (ex: MSIX), assinar o código e publicar os artefatos em um "release".