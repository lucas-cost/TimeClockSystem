# Caso de Teste: CT-002 - Fluxo Offline e Sincronização Automática

**Data do Teste:** 11/09/2025
**Testador:** [Lucas]

---

### Objetivo do Teste
Verificar a resiliência do sistema em registrar pontos com a API indisponível (offline), observar a política de Retry, e confirmar a sincronização automática dos registros pendentes quando a API volta a ficar online.

---

### Pré-condições
- A aplicação WPF está instalada e pronta para ser executada.
- Uma webcam funcional está conectada ao computador.
- O container Docker do RabbitMQ está em execução.
- O arquivo `appsettings.json` está configurado com uma **URL inválida** para a API (ex: `http://localhost:9999/`) para simular o estado "offline".

---

### Parte 1: Registro em Modo Offline e Política de Retry

| Passo | Ação                                                                    | Dados de Entrada |
| :---- | :---------------------------------------------------------------------- | :--------------- |
| 1     | Iniciar a aplicação `TimeClockSystem.UI.exe`.                           | N/A              |
| 2     | Observar o estado inicial da interface e a janela de "Output" (Debug).  | N/A              |
| 3     | Inserir a matrícula "445566" e clicar em "Bater Ponto".                 | `445566`         |
| 4     | Observar o Output do Visual Studio e o feedback na UI.                  | N/A              |
| 5     | (Verificação) Inspecionar o banco de dados local.                       | `timeclock.db`   |

---

### Resultados Esperados (Parte 1)

| Passo | Resultado Esperado                                                                                                                                                                   |
| :---- | :----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| 1     | A janela principal da aplicação é aberta sem erros fatais.                                                                                                                           |
| 2     | O preview da câmera está ativo. O indicador de status da API na barra de status está **vermelho**.                                                                                   |
| 3     | A operação é aceita.                                                                                                                                                                 |
| 4     | A janela de "Output" (Debug) exibe as **3 tentativas da política de Retry** (ex: "RETRY: Tentativa 1..."). A `StatusBar` da aplicação exibe a mensagem de sucesso do registro **local**. |
| 5     | Um novo registro é criado na tabela `TimeClockRecords` para o `EmployeeId` "445566". O `Status` deste registro deve ser **`Pending`**.                                               |

---

### Parte 2: Sincronização Automática

| Passo | Ação                                                                                                           | Dados de Entrada             |
| :---- | :------------------------------------------------------------------------------------------------------------- | :--------------------------- |
| 6     | **Sem fechar a aplicação**, altere o arquivo `appsettings.json` para a **URL correta** da API Mock do Postman. | `appsettings.json`           |
| 7     | Aguarde o próximo ciclo do `SyncWorker` (configurado para 1 minuto).                                           | N/A                          |
| 8     | Observe a janela de "Output" (Debug) do Visual Studio.                                                         | N/A                          |
| 9     | (Verificação) Inspecione o banco de dados local novamente.                                                     | `timeclock.db`               |
| 10    | (Verificação) Inspecione o painel do RabbitMQ.                                                                 | `http://localhost:15672`     |

---

### Resultados Esperados (Parte 2)

| Passo | Resultado Esperado                                                                                                                                                                   |
| :---- | :----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| 6     | O arquivo `appsettings.json` é salvo com a URL correta. O indicador de status da API na UI deve mudar para **verde** após alguns segundos.                                           |
| 7     | A aplicação continua rodando.                                                                                                                                                        |
| 8     | O "Output" exibe os logs do `SyncWorker`, como "Iniciando ciclo de sincronização...", "Encontrados 1 registros pendentes..." e "Registro ... sincronizado com sucesso.".              |
| 9     | O `Status` do registro para o `EmployeeId` "445566" na tabela `TimeClockRecords` foi atualizado de `Pending` para **`Synced`**.                                                    |
| 10    | Uma nova mensagem referente ao `EmployeeId` "445566" aparece na fila do RabbitMQ.                                                                                                 |

---

### Resultado Final

- [x] **Passou**
- [ ] **Falhou**

### Evidências!
![Step 1](https://github.com/lucas-cost/TimeClockSystem/blob/developer/docs/Images/IMG-CT-002/001.png)

![Step 2](https://github.com/lucas-cost/TimeClockSystem/blob/developer/docs/Images/IMG-CT-002/002.png)

![Step 3](https://github.com/lucas-cost/TimeClockSystem/blob/developer/docs/Images/IMG-CT-002/003.png)

![Step 4](https://github.com/lucas-cost/TimeClockSystem/blob/developer/docs/Images/IMG-CT-002/004-DB-01.png)

![Step 5](https://github.com/lucas-cost/TimeClockSystem/blob/developer/docs/Images/IMG-CT-002/005.png)

![Step 6](https://github.com/lucas-cost/TimeClockSystem/blob/developer/docs/Images/IMG-CT-002/006.png)

![Step 7](https://github.com/lucas-cost/TimeClockSystem/blob/developer/docs/Images/IMG-CT-002/007-publish-rabbit.png)