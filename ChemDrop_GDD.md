# Game Design Document: ChemDrop Puzzle (Título Provisório)

## 1. Visão Geral / Conceito

ChemDrop Puzzle é um jogo de quebra-cabeça onde os jogadores manipulam peças representando elementos químicos que caem em uma grade. O objetivo é posicionar estrategicamente essas peças para formar ligações químicas específicas (moléculas). Ao formar moléculas, as peças são removidas da grade, pontos são ganhos, e as peças restantes caem, potencialmente criando reações em cascata. O jogo desafia o raciocínio espacial, o planejamento e o conhecimento básico de algumas fórmulas químicas simples.

## 2. Público-Alvo (Suposição)

*   Jogadores casuais que gostam de jogos de quebra-cabeça de combinação e queda de blocos (ex: Tetris, Bejeweled, Dr. Mario).
*   Estudantes ou entusiastas de química que podem apreciar o tema educativo.
*   Jogadores que procuram um desafio mental que envolva estratégia e planejamento.

## 3. Gênero do Jogo

*   Quebra-cabeça (Puzzle)
*   Combinação de Peças (Tile-matching)
*   Queda de Blocos (Falling Block)

## 4. Mecânicas de Gameplay Principais

### 4.1. Objetivo do Jogador

*   Alcançar a maior pontuação possível formando ligações químicas antes que a grade se encha completamente.

### 4.2. Fluxo de Jogo (Loop Principal)

1.  **Spawn da Peça:** Uma nova peça de elemento químico aparece no topo da tela, em uma área de "guia" ou "spawn point".
2.  **Controle da Peça:** O jogador controla a posição horizontal da peça na guia.
3.  **Queda da Peça:** O jogador solta a peça, que cai na coluna selecionada até atingir a peça mais alta já presente naquela coluna ou o fundo da grade.
4.  **Verificação de Ligações:** Após a peça assentar, o sistema verifica se a nova peça, em conjunto com as peças adjacentes, forma alguma das moléculas predefinidas.
5.  **Formação de Ligação:**
    *   Se uma ligação válida é formada, as peças constituintes são removidas da grade.
    *   O jogador ganha pontos.
    *   As peças acima dos espaços vazios criados caem (`MakePiecesFall`).
    *   O sistema reavalia a grade em busca de novas ligações em cascata (`ProcessCascadingBonds`).
6.  **Fim de Jogo:** Se uma coluna encher até o topo impedindo que uma nova peça caia nela, ou se a grade inteira encher, o jogo termina.
7.  **Repetição:** O ciclo recomeça com o spawn de uma nova peça, a menos que seja fim de jogo.

### 4.3. Controles (Baseado no Código)

*   **Mover Peça Horizontalmente (na guia):** Tecla `Tab` (cicla entre as colunas).
*   **Soltar Peça:** Tecla `Return` (Enter).
*   **Voltar ao Menu:** Tecla `Backspace`.
    *(Controles por gamepad também são mencionados no código com vibração, mas os botões específicos não foram detalhados na refatoração).*

### 4.4. Sistema de Grade e Peças

*   **Grade:** O jogo ocorre em uma grade 2D com dimensões configuráveis (ex: `width=6`, `height=4` no código).
*   **Peças (ElementPiece):**
    *   Cada peça representa um elemento químico.
    *   Atributos visuais: Símbolo, Número Atômico (informativo), Nome, Cor distinta.
    *   As peças são instanciadas a partir de um prefab (`elementPiecePrefab`).
    *   Armazenam suas coordenadas `GridX`, `GridY` uma vez na grade.

### 4.5. Formação de Ligações Químicas

*   **Receitas de Ligações (`ChemicalBondRecipe`):** O jogo possui uma lista predefinida de moléculas que podem ser formadas. Cada receita especifica:
    *   `BondName`: Nome da molécula (ex: "H2O").
    *   `RequiredElements`: Um dicionário do tipo de elemento (`elementType` - int) e a quantidade necessária.
    *   `Score`: Pontos concedidos por formar esta molécula.
*   **Lógica de Detecção (`TryProcessBondAt`, `TryFormBondRecursive`):**
    *   Quando uma peça é colocada ou após uma queda, o sistema verifica se ela e suas vizinhas conectadas correspondem a alguma receita.
    *   A busca por ligações é feita recursivamente, garantindo que as peças estejam conectadas e na quantidade correta.
    *   A primeira ligação válida encontrada (seguindo a ordem das receitas e dos pontos de partida na grade) é processada.

### 4.6. Sistema de Pontuação

*   **Pontos Base:** Cada `ChemicalBondRecipe` tem uma pontuação base.
*   **Multiplicador por Número de Peças:** Um bônus é aplicado com base no número total de peças que compõem a molécula formada (ex: 2 peças = 1.0x, 3 peças = 1.2x, etc., conforme `pieceCountMultiplier`).
*   A pontuação é exibida na UI (`scoreText`).
*   A última pontuação é salva em `PlayerPrefs`.

### 4.7. Condições de Fim de Jogo

*   **Grade Cheia:** Se qualquer coluna da grade principal encher até o topo de forma que a peça na guia não possa ser solta naquela coluna (detectado por `FindLowestEmptyCell` retornando -1 para uma coluna onde se tenta soltar, e subsequentemente `IsGridFull` se todas as colunas estiverem assim).
*   **Game Over Scene:** O jogo transita para uma "GameOverScene".

## 5. Elementos Químicos e Ligações (Baseado no Código)

### 5.1. Elementos Definidos (`elementData` em `ElementPiece`):

| elementType (int) | Símbolo | Nome       | Número Atômico | Cor (Aproximada) |
|-------------------|---------|------------|----------------|------------------|
| 0                 | H       | Hidrogénio | 1              | Branco           |
| 1                 | O       | Oxigénio   | 8              | Vermelho         |
| 2                 | Na      | Sódio      | 11             | Cinza            |
| 3                 | Cl      | Cloro      | 17             | Verde            |
| 4                 | N       | Nitrogénio | 7              | Azul             |
| 5                 | C       | Carbono    | 6              | Cinza Escuro     |

### 5.2. Receitas de Ligações Definidas (`chemicalBondRecipes`):

| Nome  | Componentes                                | Pontuação Base |
|-------|--------------------------------------------|----------------|
| H2O   | 2x Hidrogénio (0), 1x Oxigénio (1)         | 100            |
| NaCl  | 1x Sódio (2), 1x Cloro (3)                 | 150            |
| CO2   | 1x Carbono (5), 2x Oxigénio (1)            | 200            |
| NH3   | 1x Nitrogénio (4), 3x Hidrogénio (0)       | 175            |

## 6. Possíveis Melhorias Futuras / Ideias (Observacional)

*   **Novos Elementos e Ligações:** Expandir a variedade de elementos e moléculas para aumentar a complexidade e o interesse.
*   **Peças Especiais:** Introduzir peças com comportamentos únicos (ex: uma peça "ácido" que remove uma coluna, uma peça "catalisador" que facilita ligações).
*   **Design de Níveis / Modos de Jogo:**
    *   Modo campanha com objetivos específicos por nível.
    *   Modo de tempo limitado.
    *   Modo com desafios (ex: grade pré-preenchida, tipos de peças restritos).
*   **Melhorias Visuais e Sonoras:** Feedback visual mais elaborado para formação de ligações, animações de queda, efeitos sonoros, música.
*   **Interface de Usuário (UI) Aprimorada:** Melhor apresentação de pontuação, próxima peça, talvez um "grimório" de moléculas descobertas.
*   **Balanceamento:** Ajustar a frequência de spawn de cada tipo de elemento e a pontuação das ligações para uma curva de dificuldade equilibrada.
*   **Tutorial Interativo:** Ensinar as mecânicas básicas e a formação de algumas ligações iniciais.
*   **Suporte a Mais Controles:** Opções de controle mais configuráveis (mouse, touch em mobile).
*   **Prioridade de Ligação Explícita:** Se necessário, adicionar um sistema para que certas moléculas tenham prioridade de formação sobre outras em caso de ambiguidade.
*   **Objetivos Secundários:** Pequenas missões ou conquistas (ex: "Formar 10 H2O", "Criar uma cascata de 3 ligações").

Este documento serve como um ponto de partida. Um GDD completo evoluiria com o desenvolvimento do jogo, incorporando design de arte, design de som, planos de marketing, etc.
