# Caso de Teste: CT-003 - Testes Gerais e Validação de Qualidade de Imagem

**Data do Teste:** 12/09/2025
**Testador:** [Lucas]

---

### Objetivo do Teste
Verificar a corretude da lógica interna dos componentes através da execução de testes unitários e validar o comportamento do sistema ao capturar imagens de baixa qualidade (escuras, claras ou borradas).

---

### Pré-condições
- O código-fonte completo da solução está disponível no ambiente de desenvolvimento (Visual Studio).
- O projeto de testes unitários (`TimeClockSystem.UnitTests`) está configurado e compilando.
- Uma webcam funcional está conectada ao computador para os testes de validação de imagem.
- A API Mock e o RabbitMQ estão em execução.

---

### Parte 1: Execução dos Testes Unitários

| Passo | Ação                                                                 | Dados de Entrada            | 
| :---- | :------------------------------------------------------------------- | :-------------------------- |
| 1     | No Visual Studio, abra o "Gerenciador de Testes" (Test Explorer).    | Menu: Test -> Test Explorer |
| 2     | Na janela do Gerenciador de Testes, clique em "Run All Tests".       | N/A                         |

---

### Resultados Esperados (Parte 1)

| Passo | Resultado Esperado                                                                                                                                                                    |
| :---- | :-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| 1     | O Gerenciador de Testes descobre e lista todos os testes unitários definidos no projeto `TimeClockSystem.UnitTests` (ex: `MainViewModelTests`, `WebcamFactoryTests`, etc.). |
| 2     | Todos os testes unitários são executados e finalizam com sucesso (indicador verde ✅). Isso valida a lógica isolada das camadas `Core`, `Application`, `Infrastructure` e `UI` (ViewModel). |

---

### Parte 2: Testes de Validação de Qualidade de Imagem

| Passo | Ação                                                                                       | Dados de Entrada                 |
| :---- | :----------------------------------------------------------------------------------------- | :------------------------------- |
| 3     | Inicie a aplicação WPF.                                                                    | N/A                              |
| 4     | **Teste (Imagem Escura):** Cubra a lente da webcam para criar uma imagem preta.            | Obstruir a câmera                |
| 5     | Insira uma matrícula válida (ex: "IMGTEST") e clique em "Bater Ponto".                     | `IMGTEST`                        |
| 6     | **Teste (Imagem Clara):** Aponte uma luz forte para a lente da webcam.                     | Fonte de luz intensa             |
| 7     | Insira uma matrícula válida e clique em "Bater Ponto".                                     | `IMGTEST`                        |
| 8     | **Teste (Imagem Borrada):** Mova a câmera rapidamente ou foque em um objeto muito próximo. | Desfocar a imagem                |
| 9     | Insira uma matrícula válida e clique em "Bater Ponto".                                     | `IMGTEST`                        |

---

### Resultados Esperados (Parte 2)

| Passo | Resultado Esperado                                                                                                                                                                                                          |
| :---- | :-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| 3     | A aplicação abre normalmente com o preview da câmera ativo.                                                                                                                                                                 |
| 4     | O preview na UI exibe uma imagem escura/preta.                                                                                                                                                                              |
| 5     | O ponto **não** é registrado. A `StatusBar` exibe a mensagem de erro: **"A imagem está muito escura. Por favor, melhore a iluminação."**. Nenhum registro novo é criado no banco de dados.                                  |
| 6     | O preview na UI exibe uma imagem muito clara/branca.                                                                                                                                                                        |
| 7     | O ponto **não** é registrado. A `StatusBar` exibe a mensagem de erro: **"A imagem está muito clara (superexposta). Por favor, ajuste a iluminação."**.                                                                      |
| 8     | O preview na UI exibe uma imagem visivelmente borrada.                                                                                                                                                                      |
| 9     | O ponto **não** é registrado. A `StatusBar` exibe a mensagem de erro: **"A imagem está sem foco (borrada). Por favor, fique parado e tente novamente."**.                                                                   |

---

### Resultado Final

- [x] **Passou**
- [ ] **Falhou**

### Evidências!
![Alta exposição](https://github.com/lucas-cost/TimeClockSystem/blob/developer/docs/Images/IMG-CT-003/alta-exposicao.png)

![Imagem borrada](https://github.com/lucas-cost/TimeClockSystem/blob/developer/docs/Images/IMG-CT-003/imagem-borrada.png)

![Imagem escura](https://github.com/lucas-cost/TimeClockSystem/blob/developer/docs/Images/IMG-CT-003/imagem-escura.png)

![Camera não funcional](https://github.com/lucas-cost/TimeClockSystem/blob/developer/docs/Images/IMG-CT-003/camera-nao-funcional.png)

![Testes unitários](https://github.com/lucas-cost/TimeClockSystem/blob/developer/docs/Images/IMG-CT-003/unit-tests.png)
