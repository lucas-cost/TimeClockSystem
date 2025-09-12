# Caso de Teste: CT-001 - Registro de Ponto (Fluxo de Sucesso Online)

**Data do Teste:** 12/09/2025
**Testador:** [Lucas]

---

### Objetivo do Teste
Verificar se um funcionário consegue registrar um ponto com sucesso quando todos os sistemas (aplicação, hardware, API) estão operando normalmente.

### IMPORTANTE
Não foi definido limite mínimo para registro entre o ponto de (Entrada) e (Início de pausa) e entre (Início de pausa) e (Fim de pausa). Portanto, 
o teste será considerado aprovado mesmo que os registros sejam feitos em sequência rápida.

---

### Pré-condições
- A aplicação WPF está instalada e pronta para ser executada.
- Uma webcam funcional está conectada ao computador.
- O container Docker do RabbitMQ está em execução.
- A API Mock (Postman) está em execução e acessível na URL configurada no `appsettings.json`.

---

### Passos para Execução

| Passo | Ação                                               | Dados de Entrada             |
| :---- | :------------------------------------------------- | :--------------------------- |
| 1     | Iniciar a aplicação `TimeClockSystem.UI.exe`.      | N/A                          |
| 2     | Observar o estado inicial da interface.            | N/A                          |
| 3     | Inserir uma matrícula válida no campo de texto.    | `554411`                     |
| 4     | Clicar no botão "Bater Ponto".                     | N/A                          |
| 5     | Aguardar a finalização da operação.                | N/A                          |
| 6     | (Verificação) Inspecionar o banco de dados local.  | `timeclock.db`               |
| 7     | (Verificação) Inspecionar o painel do RabbitMQ.    | `http://localhost:15672`     |

---

### Resultados Esperados

| Passo | Resultado Esperado                                                                                                                                        |
| :---- | :-------------------------------------------------------------------------------------------------------------------------------------------------------- |
| 1     | A janela principal da aplicação é aberta sem erros.                                                                                                       |
| 2     | O preview da câmera está ativo. O indicador de status da API na barra de status está **verde**. O botão "Bater Ponto" está **habilitado**.                |
| 3     | O texto "554411" é exibido corretamente no campo "Matrícula do Funcionário".                                                                              |
| 4     | O clique é aceito e o `StatusMessage` muda para "Registrando ponto...".                                                                                   |
| 5     | O campo de matrícula é limpo. A `StatusBar` exibe uma mensagem de sucesso dinâmica (ex: "Entrada registrada para '554411' com sucesso!").                 |
| 6     | Um novo registro é criado na tabela `TimeClockRecords`. O campo `EmployeeId` deve ser "554411", o `Timestamp` recente e o `Status` deve ser **`Synced`**. |
| 7     | Uma nova mensagem aparece na fila ligada ao evento `PontoRegistradoComSucesso`. O conteúdo da mensagem (payload) deve conter os dados do registro.        |

---

### Resultado Final

- [x] **Passou**
- [ ] **Falhou**

### Evidências!
![Entrada](../Images/IMG-CT-001/01-entrada.png)

![Início da pausa](../Images/IMG-CT-001/02-inicio-pausa.png)

![Fim da pausa](../Images/IMG-CT-001/03-fim-pausa.png)

![Negar saída](../Images/IMG-CT-001/04-jornada-nao-comprida.png)

![Registros no banco](../Images/IMG-CT-001/05-registros.png)