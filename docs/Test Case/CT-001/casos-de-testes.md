# Caso de Teste: CT-001 - Registro de Ponto (Fluxo de Sucesso Online)

**Data do Teste:** 12/09/2025
**Testador:** [Lucas]

---

### Objetivo do Teste
Verificar se um funcion�rio consegue registrar um ponto com sucesso quando todos os sistemas (aplica��o, hardware, API) est�o operando normalmente.

### IMPORTANTE
N�o foi definido limite m�nimo para registro entre o ponto de (Entrada) e (In�cio de pausa) e entre (In�cio de pausa) e (Fim de pausa). Portanto, 
o teste ser� considerado aprovado mesmo que os registros sejam feitos em sequ�ncia r�pida.

---

### Pr�-condi��es
- A aplica��o WPF est� instalada e pronta para ser executada.
- Uma webcam funcional est� conectada ao computador.
- O container Docker do RabbitMQ est� em execu��o.
- A API Mock (Postman) est� em execu��o e acess�vel na URL configurada no `appsettings.json`.

---

### Passos para Execu��o

| Passo | A��o                                               | Dados de Entrada             |
| :---- | :------------------------------------------------- | :--------------------------- |
| 1     | Iniciar a aplica��o `TimeClockSystem.UI.exe`.      | N/A                          |
| 2     | Observar o estado inicial da interface.            | N/A                          |
| 3     | Inserir uma matr�cula v�lida no campo de texto.    | `554411`                     |
| 4     | Clicar no bot�o "Bater Ponto".                     | N/A                          |
| 5     | Aguardar a finaliza��o da opera��o.                | N/A                          |
| 6     | (Verifica��o) Inspecionar o banco de dados local.  | `timeclock.db`               |
| 7     | (Verifica��o) Inspecionar o painel do RabbitMQ.    | `http://localhost:15672`     |

---

### Resultados Esperados

| Passo | Resultado Esperado                                                                                                                                        |
| :---- | :-------------------------------------------------------------------------------------------------------------------------------------------------------- |
| 1     | A janela principal da aplica��o � aberta sem erros.                                                                                                       |
| 2     | O preview da c�mera est� ativo. O indicador de status da API na barra de status est� **verde**. O bot�o "Bater Ponto" est� **habilitado**.                |
| 3     | O texto "554411" � exibido corretamente no campo "Matr�cula do Funcion�rio".                                                                              |
| 4     | O clique � aceito e o `StatusMessage` muda para "Registrando ponto...".                                                                                   |
| 5     | O campo de matr�cula � limpo. A `StatusBar` exibe uma mensagem de sucesso din�mica (ex: "Entrada registrada para '554411' com sucesso!").                 |
| 6     | Um novo registro � criado na tabela `TimeClockRecords`. O campo `EmployeeId` deve ser "554411", o `Timestamp` recente e o `Status` deve ser **`Synced`**. |
| 7     | Uma nova mensagem aparece na fila ligada ao evento `PontoRegistradoComSucesso`. O conte�do da mensagem (payload) deve conter os dados do registro.        |

---

### Resultado Final

- [x] **Passou**
- [ ] **Falhou**

### Evid�ncias!
![Entrada](../Images/IMG-CT-001/01-entrada.png)

![In�cio da pausa](../Images/IMG-CT-001/02-inicio-pausa.png)

![Fim da pausa](../Images/IMG-CT-001/03-fim-pausa.png)

![Negar sa�da](../Images/IMG-CT-001/04-jornada-nao-comprida.png)

![Registros no banco](../Images/IMG-CT-001/05-registros.png)