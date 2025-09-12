# Melhorias Futuras Identificadas

Esta � uma lista de poss�veis melhorias e funcionalidades que podem ser implementadas para aprimorar o Sistema de Ponto Eletr�nico.

### Resili�ncia e Monitoramento
- **Health Checks Dedicados:** Criar um endpoint de health check na API mock e um servi�o mais robusto no cliente que possa ser exposto para ferramentas de monitoramento.
- **M�tricas e Tracing (Observability):** Integrar ferramentas como OpenTelemetry para coletar m�tricas (ex: tempo de chamada da API, taxa de falhas do Circuit Breaker) e traces distribu�dos, fornecendo uma vis�o mais profunda do comportamento do sistema.

### C�digo e Regras de Neg�cio
- **Expandir CQRS (Queries):** Implementar a parte de "Query" do CQRS para otimizar opera��es de leitura, como a exibi��o de relat�rios de pontos, usando proje��es diretas para DTOs.
- **Valida��es Adicionais:** Implementar regras de neg�cio mais complexas, como tempo m�nimo de almo�o ou valida��o de intervalo entre quaisquer dois pontos (ex: 1 minuto) para evitar registros duplicados.
- **Valida��o com FluentValidation:** Substituir as valida��es manuais por um sistema robusto como o FluentValidation, que se integra bem com o MediatR atrav�s de behaviors de pipeline.
- **Mensagens via arquivo .resx:** Externalizar mensagens de erro e valida��o para arquivos de recursos (.resx) para facilitar a manuten��o e internacionaliza��o.

### Seguran�a
- **Criptografia de Dados:** Criptografar o arquivo de banco de dados SQLite local para proteger os dados do funcion�rio em repouso.
- **Autentica��o Real:** Implementar um fluxo de autentica��o real (ex: OAuth 2.0) onde a aplica��o obt�m um JWT (JSON Web Token) de um servidor de identidade, em vez de usar um token fixo no `appsettings.json`.

### Experi�ncia do Usu�rio (UX/UI)
- **Sele��o de C�mera:** Se m�ltiplas webcams forem detectadas, permitir que o usu�rio escolha qual delas utilizar.
- **Feedback de Opera��o:** Adicionar indicadores visuais (ex: spinners de carregamento) para opera��es de longa dura��o, como a sincroniza��o de um grande n�mero de pontos.

### DevOps
- **Containeriza��o da Aplica��o:** Criar um `Dockerfile` para a aplica��o WPF, permitindo que ela seja distribu�da e executada em ambientes de cont�iner (�til para cen�rios de virtualiza��o de desktop).
- **Expandir Pipeline de CI/CD:** Adicionar etapas ao pipeline do GitHub Actions para gerar um instalador (ex: MSIX), assinar o c�digo e publicar os artefatos em um "release".