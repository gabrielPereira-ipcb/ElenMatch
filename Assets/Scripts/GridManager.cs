// GridManager.cs (Código Atualizado)
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using System.Collections;
using TMPro;
using UnityEngine.SceneManagement;
using System.Linq;  // Adicionado para usar LINQ

// Este script gere a grelha de jogo, o spawning e a queda das peças.
public class GridManager : MonoBehaviour
{
    public static GridManager Instance { get; private set; }

    // Dimensões da grelha
    [SerializeField] private int width = 6;
    [SerializeField] private int height = 4;

    
    // Referências para os prefabs
    [SerializeField] private GameObject elementPiecePrefab;

    // Referência para o ponto de spawn (posição onde as peças aparecem na guia)
    [SerializeField] private Transform spawnPoint;

    // Array 2D para armazenar os GameObjects na grelha principal (peças, barreiras - embora barreiras adiadas)
    private GameObject[,] grid;

    // Definimos os tipos de elementos disponíveis (abordagem simples com índices por agora)
    // Estes índices corresponderão mais tarde a elementos químicos específicos
    private readonly int[] availableElements = { 0, 1, 2, 3, 4, 5 }; // 6 tipos diferentes de elementos iniciais

    // Variáveis para controlar a peça atual que o jogador está a posicionar na guia
    private GameObject currentFallingPiece;
    private int currentColumnIndex; // Índice da coluna na guia (e onde cairá na grelha)

    // Variáveis para o espaçamento visual das colunas e linhas na grelha principal
    [SerializeField] private float columnWidth = 58f; // Distância entre os centros das colunas
    [SerializeField] private float rowHeight = 58f;   // Altura entre os centros das linhas

    // Posições base para o cálculo das posições na grelha
    [SerializeField] private float startGridX;
    [SerializeField] private float baseGridY;

    // Referência para o texto da UI que mostra a pontuação
    [SerializeField] private TextMeshProUGUI scoreText;

    // Variáveis de pontuação
    private int currentScore = 0;

    // Bónus baseado no número de peças
    private Dictionary<int, float> pieceCountMultiplier = new Dictionary<int, float>()
    {
        { 2, 1.0f },     // 2 peças = pontuação normal
        { 3, 1.2f },     // 3 peças = 20% bónus
        { 4, 1.5f },     // 4 peças = 50% bónus
        { 5, 2.0f }      // 5 ou mais peças = dobro da pontuação
    };

    // Nova estrutura para receitas de ligações químicas
    public List<ChemicalBondRecipe> chemicalBondRecipes = new List<ChemicalBondRecipe>();

    private void InitializeChemicalBondRecipes()
    {
        // ElementType key:
        // 0: Hidrogénio (H)
        // 1: Oxigénio (O)
        // 2: Sódio (Na)
        // 3: Cloro (Cl)
        // 4: Nitrogénio (N)
        // 5: Carbono (C)

        chemicalBondRecipes.Add(new ChemicalBondRecipe("H2O", new Dictionary<int, int> { { 0, 2 }, { 1, 1 } }, 100));  // Água
        chemicalBondRecipes.Add(new ChemicalBondRecipe("NaCl", new Dictionary<int, int> { { 2, 1 }, { 3, 1 } }, 150)); // Sal
        chemicalBondRecipes.Add(new ChemicalBondRecipe("CO2", new Dictionary<int, int> { { 5, 1 }, { 1, 2 } }, 200));  // Dióxido de Carbono (Corrigido para Carbono=5)
        chemicalBondRecipes.Add(new ChemicalBondRecipe("NH3", new Dictionary<int, int> { { 4, 1 }, { 0, 3 } }, 175)); // Amónia
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        // Inicializamos o array da grelha com as dimensões corretas
        grid = new GameObject[width, height];
        Debug.Log($"Grid initialized with dimensions: {width}x{height}");

        InitializeChemicalBondRecipes(); // Popula as receitas de ligações

        // Reinicia a pontuação
        currentScore = 0;
        PlayerPrefs.SetInt("LastScore", 0);
        PlayerPrefs.Save();
        UpdateScoreDisplay();

        // Geramos a primeira peça controlada pelo jogador
        SpawnNewPiece();
    }

    // Método para criar uma nova peça e colocá-la na posição de spawn (guia)
    private void SpawnNewPiece()
    {
        // Verificações de segurança
        if (spawnPoint == null || elementPiecePrefab == null)
        {
            Debug.LogError("Spawn point or piece prefab not set!");
            return;
        }

        int randomElementType = availableElements[Random.Range(0, availableElements.Length)];

        // Começar a peça na coluna do meio da guia (arredondamento int)
        currentColumnIndex = width / 2;

        // Calcular a posição visual para a peça na guia
        Vector3 spawnPosition = CalculatePiecePosition();

        // Instanciar a nova peça a partir do prefab na posição calculada
        currentFallingPiece = Instantiate(elementPiecePrefab, spawnPosition, Quaternion.identity);

        // *** CÓDIGO ATUALIZADO: Obter o componente ElementPiece e definir o tipo de elemento ***
        // Assumimos que o prefab elementPiecePrefab tem o script ElementPiece anexado.
        ElementPiece pieceScript = currentFallingPiece.GetComponent<ElementPiece>();
        if (pieceScript != null)
        {
            pieceScript.SetElementType(randomElementType);
        }
        else
        {
            // Se o script não for encontrado, isto é um erro crítico para a jogabilidade futura.
            Debug.LogError("ElementPiece component not found on spawned prefab!");
            // Pode-se optar por destruir a peça ou desativá-la neste caso.
            Destroy(currentFallingPiece);
            currentFallingPiece = null;
        }

        Debug.Log($"New piece controlling in column {currentColumnIndex}");
    }

    // Calcula a posição visual 3D no mundo para a peça que está a ser controlada na GUIA (topo do tabuleiro)
    private Vector3 CalculatePiecePosition()
    {
        // Calcula a posição no mundo com base no índice da coluna
        // Usa o spawnPoint.position.x como referência para o centro da primeira coluna na guia (coluna 0)
        float xPosition = spawnPoint.position.x + (currentColumnIndex * columnWidth);

        // A altura é a do spawn point
        return new Vector3(xPosition, spawnPoint.position.y, spawnPoint.position.z);
    }

    // Atualiza a posição visual da peça controlada na GUIA para corresponder à coluna atual
    private void UpdatePiecePosition()
    {
        if (currentFallingPiece != null)
        {
            currentFallingPiece.transform.position = CalculatePiecePosition();
        }
    }

    private IEnumerator VibrateController(float duration, float lowFrequency, float highFrequency)
    {
        if (Gamepad.current != null)
        {
            Gamepad.current.SetMotorSpeeds(lowFrequency, highFrequency);
            yield return new WaitForSeconds(duration);
            Gamepad.current.SetMotorSpeeds(0f, 0f); // Parar vibração
        }
    }

    // Move a peça controlada horizontalmente na GUIA (chamado ao pressionar TAB)
    private void MovePieceHorizontally()
    {
        // Cicla entre as colunas (0 a width-1)
        currentColumnIndex = (currentColumnIndex + 1) % width;

        // Atualiza a posição visual da peça
        UpdatePiecePosition();

        if (Gamepad.current != null)
        {
            StartCoroutine(VibrateController(0.1f, 0.5f, 0.5f));
        }

        Debug.Log($"Moved to column {currentColumnIndex}");
    }

    // Lógica para soltar a peça na grelha (chamado ao pressionar ENTER)
    private void DropPiece()
    {
        // Verifica se há uma peça a ser controlada
        if (currentFallingPiece == null) return;

        // Encontra a célula vazia mais baixa na coluna ATUAL selecionada na grelha principal
        int targetRow = FindLowestEmptyCell(currentColumnIndex);
        Debug.Log($"Found empty cell at row {targetRow} in column {currentColumnIndex}");

        // Verifica se encontrou uma posição válida (se a coluna não estiver cheia)
        if (targetRow != -1)
        {
            // Calcula a posição visual FINAL 3D no mundo para a célula alvo na GRElHA PRINCIPAL
            Vector3 finalPosition = CalculateGridPosition(currentColumnIndex, targetRow);

            // Move a peça para a posição final
            currentFallingPiece.transform.position = finalPosition;

            // ATENÇÃO: Aqui é onde a peça é armazenada no array grid
            grid[currentColumnIndex, targetRow] = currentFallingPiece;
            Debug.Log($"Stored piece in grid at [{currentColumnIndex}, {targetRow}]");

            // Definir coordenadas na ElementPiece
            ElementPiece placedPieceScript = currentFallingPiece.GetComponent<ElementPiece>();
            if (placedPieceScript != null)
            {
                placedPieceScript.GridX = currentColumnIndex;
                placedPieceScript.GridY = targetRow;
            }
            else
            {
                Debug.LogError("Placed piece does not have an ElementPiece script!");
            }

            // Verifica o estado atual do grid
            DebugGridState();

            // Libera o controlo da peça atual ANTES de processar as ligações em cascata
            currentFallingPiece = null;

            // Processa todas as ligações em cascata
            ProcessCascadingBonds();

            // Verifica se a grelha está cheia após a queda da peça e processamento das ligações
            if (IsGridFull())
            {
                Debug.Log("Game Over - Grid is full!");
                // Carrega a cena de Game Over
                UnityEngine.SceneManagement.SceneManager.LoadScene("GameOverScene");
                return;
            }

            // Gera a próxima peça para o jogador controlar
            SpawnNewPiece();
        }
        else
        {
            // Se a coluna estiver cheia, não soltamos a peça
            Debug.LogWarning($"Column {currentColumnIndex} is full!");

            // Verifica se a grelha está cheia
            if (IsGridFull())
            {
                Debug.Log("Game Over - Grid is full!");
                // Carrega a cena de Game Over
                UnityEngine.SceneManagement.SceneManager.LoadScene("GameOverScene");
                return;
            }
        }
    }

    // Método auxiliar para debug do estado do grid
    private void DebugGridState()
    {
        Debug.Log("Current Grid State:");
        for (int row = 0; row < height; row++)
        {
            string rowState = "";
            for (int col = 0; col < width; col++)
            {
                rowState += grid[col, row] != null ? "X " : "O ";
            }
            Debug.Log($"Row {row}: {rowState}");
        }
    }

    // Procura a linha vazia mais baixa numa dada coluna (funciona de baixo para cima)
    // Retorna o índice da linha ou -1 se a coluna estiver cheia
    private int FindLowestEmptyCell(int column)
    {
        // Procura de baixo para cima (height-1 até 0)
        for (int row = height - 1; row >= 0; row--)
        {
            if (grid[column, row] == null)
            {
                Debug.Log($"Found empty cell at [{column}, {row}]");
                return row;
            }
        }
        Debug.Log($"Column {column} is full");
        return -1;
    }

    // Calcula a posição visual 3D no mundo para uma célula específica na GRElHA PRINCIPAL
    private Vector3 CalculateGridPosition(int column, int row)
    {
        // Calcula a posição X baseada na coluna
        float xPosition = startGridX + (column * columnWidth);
        
        // Calcula a posição Y baseada na linha
        // baseGridY é o centro da linha mais baixa, então:
        // - Para row = height-1: yPosition = baseGridY (linha mais baixa)
        // - Para row = height-2: yPosition = baseGridY + rowHeight (uma linha acima)
        // - E assim por diante...
        float yPosition = baseGridY + ((height - 1 - row) * rowHeight);
        
        // Mantém a mesma posição Z do spawn point
        float zPosition = spawnPoint.position.z;
        
        Debug.Log($"Calculating grid position for [{column}, {row}]: X={xPosition}, Y={yPosition}, Z={zPosition}");
        
        return new Vector3(xPosition, yPosition, zPosition);
    }

    // Update é chamado uma vez por frame
    void Update()
    {
        if (currentFallingPiece != null)
        {
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                MovePieceHorizontally();
            }
            if (Input.GetKeyDown(KeyCode.Backspace))
            {
                // Volta para o menu inicial (MenuScene)
                UnityEngine.SceneManagement.SceneManager.LoadScene("MenuScene");
            }
            if (Input.GetKeyDown(KeyCode.Return))
            {
                DropPiece();
            }
        }
    }

    // Exemplo simples de verificação de combinações (pode ser expandido)
    private void CheckForMatches(int droppedColumn, int droppedRow)
    {
        // Aqui você pode chamar CheckForChemicalBonds ou implementar sua lógica de match
        CheckForChemicalBonds(droppedColumn, droppedRow);
    }

    // Busca horizontal
    private List<ElementPiece> FindHorizontalMatches(int col, int row)
    {
        List<ElementPiece> matches = new List<ElementPiece>();
        if (grid[col, row] == null) return matches;

        ElementPiece startPiece = grid[col, row].GetComponent<ElementPiece>();
        if (startPiece == null) return matches;

        matches.Add(startPiece);

        // Check left
        for (int c = col - 1; c >= 0; c--)
        {
            if (grid[c, row] == null) break;
            ElementPiece piece = grid[c, row].GetComponent<ElementPiece>();
            if (piece != null && piece.elementType == startPiece.elementType)
                matches.Add(piece);
            else
                break;
        }

        // Check right
        for (int c = col + 1; c < width; c++)
        {
            if (grid[c, row] == null) break;
            ElementPiece piece = grid[c, row].GetComponent<ElementPiece>();
            if (piece != null && piece.elementType == startPiece.elementType)
                matches.Add(piece);
            else
                break;
        }

        return matches;
    }

    // Busca vertical
    private List<ElementPiece> FindVerticalMatches(int col, int row)
    {
        List<ElementPiece> matches = new List<ElementPiece>();
        if (grid[col, row] == null) return matches;

        ElementPiece startPiece = grid[col, row].GetComponent<ElementPiece>();
        if (startPiece == null) return matches;

        matches.Add(startPiece);

        // Check down
        for (int r = row - 1; r >= 0; r--)
        {
            if (grid[col, r] == null) break;
            ElementPiece piece = grid[col, r].GetComponent<ElementPiece>();
            if (piece != null && piece.elementType == startPiece.elementType)
                matches.Add(piece);
            else
                break;
        }

        // Check up
        for (int r = row + 1; r < height; r++)
        {
            if (grid[col, r] == null) break;
            ElementPiece piece = grid[col, r].GetComponent<ElementPiece>();
            if (piece != null && piece.elementType == startPiece.elementType)
                matches.Add(piece);
            else
                break;
        }

        return matches;
    }

    // Definição de um padrão químico
    public class ChemicalBondPattern
    {
        public string name;
        public Dictionary<int, int> requiredElements;
        public List<Vector2Int> relativePositions;
    }

    // Checa padrão químico
    private bool CheckChemicalBondPattern(int centerCol, int centerRow, ChemicalBondPattern pattern, out List<ElementPiece> matchedPieces)
    {
        matchedPieces = new List<ElementPiece>();
        Dictionary<int, int> foundElements = new Dictionary<int, int>();

        if (grid[centerCol, centerRow] == null) return false;
        ElementPiece centerPiece = grid[centerCol, centerRow].GetComponent<ElementPiece>();
        if (centerPiece == null) return false;

        if (!foundElements.ContainsKey(centerPiece.elementType))
            foundElements[centerPiece.elementType] = 0;
        foundElements[centerPiece.elementType]++;
        matchedPieces.Add(centerPiece);

        foreach (Vector2Int offset in pattern.relativePositions)
        {
            int col = centerCol + offset.x;
            int row = centerRow + offset.y;

            if (col < 0 || col >= width || row < 0 || row >= height)
                continue;

            if (grid[col, row] == null)
                continue;

            ElementPiece piece = grid[col, row].GetComponent<ElementPiece>();
            if (piece != null)
            {
                if (!foundElements.ContainsKey(piece.elementType))
                    foundElements[piece.elementType] = 0;
                foundElements[piece.elementType]++;
                matchedPieces.Add(piece);
            }
        }

        foreach (var req in pattern.requiredElements)
        {
            int elementType = req.Key;
            int requiredCount = req.Value;
            if (!foundElements.ContainsKey(elementType) || foundElements[elementType] < requiredCount)
                return false;
        }

        return true;
    }

    // Método para fazer as peças caírem após a remoção (simplificado)
    private void MakePiecesFall()
    {
        Debug.Log("MakePiecesFall() called.");
        // Para cada coluna
        for (int col = 0; col < width; col++)
        {
            // Encontra a primeira posição vazia de BAIXO para CIMA
            int firstEmptyRow = -1;
            for (int row = height - 1; row >= 0; row--)
            {
                if (grid[col, row] == null)
                {
                    firstEmptyRow = row;
                    break;
                }
            }

            // Se encontrou um espaço vazio
            if (firstEmptyRow != -1)
            {
                // Procura a primeira peça acima do espaço vazio
                for (int rowToMoveFrom = firstEmptyRow - 1; rowToMoveFrom >= 0; rowToMoveFrom--)
                {
                    if (grid[col, rowToMoveFrom] != null)
                    {
                        GameObject pieceToMove = grid[col, rowToMoveFrom];
                        grid[col, firstEmptyRow] = pieceToMove;
                        grid[col, rowToMoveFrom] = null;
                        
                        Vector3 newPosition = CalculateGridPosition(col, firstEmptyRow);
                        pieceToMove.transform.position = newPosition;

                        // Atualizar coordenadas na ElementPiece
                        ElementPiece movedPieceScript = pieceToMove.GetComponent<ElementPiece>();
                        if (movedPieceScript != null)
                        {
                            movedPieceScript.GridX = col;
                            movedPieceScript.GridY = firstEmptyRow;
                        }
                        else
                        {
                             Debug.LogError("Moved piece does not have an ElementPiece script!");
                        }
                        Debug.Log($"Piece moved from [{col},{rowToMoveFrom}] to [{col},{firstEmptyRow}] and its coordinates updated.");

                        firstEmptyRow--; // A próxima peça cairá na linha vazia acima da atual
                    }
                }
            }
        }
    }

    // Novo método de busca recursiva para formar ligações
    private List<ElementPiece> TryFormBondRecursive(ElementPiece currentPiece, Vector2Int currentCoords,
                                                    Dictionary<int, int> requiredElements,
                                                    List<ElementPiece> currentPath,
                                                    HashSet<Vector2Int> visitedInPath,
                                                    ref long recursionCounter)
    {
        recursionCounter++;
        currentPath.Add(currentPiece);
        visitedInPath.Add(currentCoords);

        // Contar elementos no caminho atual
        Dictionary<int, int> currentPathElementsCount = new Dictionary<int, int>();
        int totalPiecesInCurrentPath = currentPath.Count;
        foreach (var pieceInPath in currentPath)
        {
            if (!currentPathElementsCount.ContainsKey(pieceInPath.elementType))
                currentPathElementsCount[pieceInPath.elementType] = 0;
            currentPathElementsCount[pieceInPath.elementType]++;
        }

        // Calcular o número total de peças necessárias para a ligação
        int totalPiecesRequiredForBond = 0;
        foreach (var req in requiredElements.Values)
            totalPiecesRequiredForBond += req;

        // Verificação de validade e conclusão
        if (totalPiecesInCurrentPath > totalPiecesRequiredForBond)
        {
            // Caminho muito longo
            currentPath.RemoveAt(currentPath.Count - 1);
            visitedInPath.Remove(currentCoords);
            return null;
        }

        foreach (var requiredType in requiredElements.Keys)
        {
            int currentCountOfType = currentPathElementsCount.ContainsKey(requiredType) ? currentPathElementsCount[requiredType] : 0;
            if (currentCountOfType > requiredElements[requiredType])
            {
                // Excesso de um tipo de elemento
                currentPath.RemoveAt(currentPath.Count - 1);
                visitedInPath.Remove(currentCoords);
                return null;
            }
        }

        if (totalPiecesInCurrentPath == totalPiecesRequiredForBond)
        {
            bool bondFormed = true;
            foreach (var requiredType in requiredElements.Keys)
            {
                int currentCountOfType = currentPathElementsCount.ContainsKey(requiredType) ? currentPathElementsCount[requiredType] : 0;
                if (currentCountOfType != requiredElements[requiredType])
                {
                    bondFormed = false;
                    break;
                }
            }
            if (bondFormed)
            {
                return new List<ElementPiece>(currentPath); // Ligação formada!
            }
            // Se o número de peças é o correto, mas a combinação de tipos não é, este caminho é inválido.
            // No entanto, a lógica de excesso de tipo acima já deve tratar isso.
            // Se chegou aqui, significa que o número de peças é certo, mas algum tipo falta (ou seja, currentCountOfType < requiredElements[requiredType])
            // o que é permitido para continuar a busca, mas não para finalizar.
            // Se especificamente TODOS os requiredElements.Keys foram verificados e algum não bateu,
            // E o totalPiecesInCurrentPath == totalPiecesRequiredForBond, então é um beco sem saída para este caminho.
            bool exactMatch = true;
            foreach(var kvp in requiredElements)
            {
                if (!currentPathElementsCount.ContainsKey(kvp.Key) || currentPathElementsCount[kvp.Key] != kvp.Value)
                {
                    exactMatch = false;
                    break;
                }
            }
            if (!exactMatch && currentPathElementsCount.Count == requiredElements.Count) // Garante que todos os tipos requeridos foram pelo menos considerados
            {
                 // Backtrack se o número de peças é o correto mas a combinação de tipos não é perfeita
                currentPath.RemoveAt(currentPath.Count - 1);
                visitedInPath.Remove(currentCoords);
                return null;
            }

        }


        // Exploração de Vizinhos
        Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
        foreach (var dir in directions)
        {
            Vector2Int neighborCoords = currentCoords + dir;
            if (neighborCoords.x >= 0 && neighborCoords.x < width &&
                neighborCoords.y >= 0 && neighborCoords.y < height &&
                grid[neighborCoords.x, neighborCoords.y] != null &&
                !visitedInPath.Contains(neighborCoords))
            {
                ElementPiece neighborPiece = grid[neighborCoords.x, neighborCoords.y].GetComponent<ElementPiece>();
                if (neighborPiece != null)
                {
                    List<ElementPiece> result = TryFormBondRecursive(neighborPiece, neighborCoords, requiredElements, currentPath, visitedInPath, ref recursionCounter);
                    if (result != null)
                    {
                        return result; // Propaga a solução encontrada
                    }
                }
            }
        }

        // Backtrack se nenhum vizinho levou a uma solução
        currentPath.RemoveAt(currentPath.Count - 1);
        visitedInPath.Remove(currentCoords);
        return null;
    }


    // Tenta processar uma ligação química no local especificado e seus arredores.
    // Retorna true se uma ligação foi formada e processada, false caso contrário.
    private static long bondCheckRecursionCounter = 0; // Contador de recursão
    private bool TryProcessBondAt(int col, int row)
    {
        Debug.Log($"TryProcessBondAt called for piece at [{col}, {row}]. Recursion count reset.");
        bondCheckRecursionCounter = 0; // Resetar para cada tentativa de ligação em um ponto.

        // Lista de pontos de partida: a peça na posição (col, row) e suas vizinhas diretas.
        List<ElementPiece> startingPoints = new List<ElementPiece>();
        HashSet<GameObject> addedStartingObjects = new HashSet<GameObject>(); // Para evitar duplicatas na lista startingPoints

        // Adiciona a peça que acabou de cair (se existir)
        if (grid[col, row] != null)
        {
            ElementPiece placedPiece = grid[col, row].GetComponent<ElementPiece>();
            if (placedPiece != null)
            {
                startingPoints.Add(placedPiece);
                addedStartingObjects.Add(placedPiece.gameObject);
            }
        }

        // Adiciona vizinhos da peça que caiu como pontos de partida alternativos
        int[] dCol = { -1, 1, 0, 0 }; // Esquerda, Direita
        int[] dRow = { 0, 0, -1, 1 }; // Baixo, Cima (lembre-se que a grade é y-para-baixo no array)

        for (int i = 0; i < 4; ++i)
        {
            int newC = col + dCol[i];
            int newR = row + dRow[i];

            if (newC >= 0 && newC < width && newR >= 0 && newR < height && grid[newC, newR] != null)
            {
                ElementPiece neighborPiece = grid[newC, newR].GetComponent<ElementPiece>();
                if (neighborPiece != null && !addedStartingObjects.Contains(neighborPiece.gameObject))
                {
                    startingPoints.Add(neighborPiece);
                    addedStartingObjects.Add(neighborPiece.gameObject);
                }
            }
        }

        if (startingPoints.Count == 0)
        {
            Debug.Log("No starting pieces found for bond check (should not happen if a piece was just placed).");
            return;
        }

        // Iterar sobre cada definição de ligação em `chemicalBondRecipes`
        foreach (ChemicalBondRecipe recipe in chemicalBondRecipes)
        {
            // Tentar formar a ligação começando com cada ponto de partida potencial
            foreach (ElementPiece startPiece in startingPoints)
            {
                if (startPiece == null) continue; // Segurança extra

                // Usar coordenadas cacheadas da ElementPiece
                Vector2Int startCoords = new Vector2Int(startPiece.GridX, startPiece.GridY);

                // Só tenta iniciar uma ligação com esta peça se o seu tipo for um dos requeridos pela ligação atual
                // E se a ligação requer pelo menos uma peça desse tipo.
                // Isso evita iniciar buscas desnecessárias.
                if (!recipe.RequiredElements.ContainsKey(startPiece.elementType) || recipe.RequiredElements[startPiece.elementType] == 0)
                {
                    //Debug.Log($"Skipping start piece {startPiece.name} (type {startPiece.elementType}) for bond {recipe.BondName} as its type is not required or count is zero.");
                    continue;
                }

                List<ElementPiece> foundBondPieces = TryFormBondRecursive(
                    startPiece,
                    startCoords,
                    recipe.RequiredElements, // Usar os elementos da receita atual
                    new List<ElementPiece>(),    // Caminho inicial vazio
                    new HashSet<Vector2Int>(),   // Visitados no caminho inicial vazio
                    ref bondCheckRecursionCounter
                );

                if (foundBondPieces != null && foundBondPieces.Count > 0)
                {
                    Debug.Log($"Chemical bond '{recipe.BondName}' formed with {foundBondPieces.Count} pieces! Starting piece: {startPiece.name} at [{startCoords.x},{startCoords.y}]");

                    AddScore(recipe.Score, recipe.GetTotalPiecesInRecipe());

                    foreach (ElementPiece pieceInBond in foundBondPieces)
                    {
                        // Usar coordenadas cacheadas da ElementPiece para remoção
                        grid[pieceInBond.GridX, pieceInBond.GridY] = null;
                        Debug.Log($"Removed piece {pieceInBond.name} from grid at [{pieceInBond.GridX},{pieceInBond.GridY}]");

                        // Inicia a animação de remoção (se houver)
                        // pieceInBond.StartBondAnimation(); // Descomente se tiver essa animação
                        Destroy(pieceInBond.gameObject);
                        Debug.Log($"Destroyed GameObject for piece {pieceInBond.name}");
                    }

                    MakePiecesFall(); // Peças caem APÓS uma ligação ser processada.
                    Debug.Log($"TryProcessBondAt completed. Bond '{recipe.BondName}' formed. Total TryFormBondRecursive calls: {bondCheckRecursionCounter}");
                    return true; // Ligação formada e processada
                }
            }
        }
        Debug.Log($"No chemical bond formed from piece at [{col},{row}]. Total TryFormBondRecursive calls: {bondCheckRecursionCounter}");
        return false; // Nenhuma ligação formada
    }

    private void ProcessCascadingBonds()
    {
        bool bondFormedInLastScan;
        int safetyBreak = 0;
        const int maxIterations = 100; // Limite de iterações para evitar loops infinitos

        do
        {
            bondFormedInLastScan = false;
            if (safetyBreak++ > maxIterations)
            {
                Debug.LogError("Safety break triggered in ProcessCascadingBonds. Check for infinite loop logic.");
                break;
            }

            // A grade deve estar "assentada" antes de cada varredura.
            // MakePiecesFall() é chamado dentro de TryProcessBondAt se uma ligação é feita,
            // o que já assenta a grade para a próxima iteração do ProcessCascadingBonds.

            for (int r = height - 1; r >= 0; r--) // De baixo para cima
            {
                for (int c = 0; c < width; c++)
                {
                    if (grid[c, r] != null)
                    {
                        if (TryProcessBondAt(c, r)) // TryProcessBondAt chama MakePiecesFall internamente se necessário
                        {
                            bondFormedInLastScan = true;
                            // Uma ligação foi formada e peças podem ter caído.
                            // Reiniciar a varredura para considerar o novo estado da grade.
                            goto nextGridScanIteration;
                        }
                    }
                }
            }
            nextGridScanIteration:; // Rótulo para o goto
        } while (bondFormedInLastScan);
        Debug.Log($"ProcessCascadingBonds finished. Safety counter: {safetyBreak-1}");
    }

    // Método para adicionar pontos
    private void AddScore(int basePoints, int pieceCount)
    {
        float multiplier = 1.0f;
        
        // Encontra o multiplicador apropriado baseado no número de peças
        foreach (var entry in pieceCountMultiplier)
        {
            if (pieceCount >= entry.Key)
            {
                multiplier = entry.Value;
            }
        }

        int pointsToAdd = Mathf.RoundToInt(basePoints * multiplier);
        currentScore += pointsToAdd;
        
        // Salva a pontuação atual para uso na tela de Game Over
        PlayerPrefs.SetInt("LastScore", currentScore);
        PlayerPrefs.Save();

        UpdateScoreDisplay();
        
        Debug.Log($"Added {pointsToAdd} points (Base: {basePoints}, Multiplier: {multiplier}x)");
    }

    // Atualiza o display da pontuação na UI
    private void UpdateScoreDisplay()
    {
        if (scoreText != null)
        {
            scoreText.text = $"{currentScore}";
        }
    }

    // Verifica se a grelha está completamente cheia
    private bool IsGridFull()
    {
        for (int col = 0; col < width; col++)
        {
            if (FindLowestEmptyCell(col) != -1)
            {
                return false;
            }
        }
        return true;
    }

    // Determina o tipo de ligação química formada (usado anteriormente, pode ser obsoleto ou adaptado se necessário)
    // private string DetermineBondType(List<ElementPiece> pieces) // Comentado pois DetermineBondName(int bondKey) é usado agora.
    // {
    //     // ... lógica anterior ...
    // }
}

// Definição da classe ChemicalBondRecipe
public class ChemicalBondRecipe
{
    public string BondName { get; private set; }
    public Dictionary<int, int> RequiredElements { get; private set; }
    public int Score { get; private set; }
    private readonly int totalPieces; // Cache para o número total de peças

    public ChemicalBondRecipe(string name, Dictionary<int, int> requiredElements, int score)
    {
        BondName = name;
        RequiredElements = requiredElements;
        Score = score;

        // Calcula e armazena o total de peças no construtor
        int calculatedTotal = 0;
        foreach (var count in RequiredElements.Values)
        {
            calculatedTotal += count;
        }
        this.totalPieces = calculatedTotal;
    }

    public int GetTotalPiecesInRecipe()
    {
        return this.totalPieces; // Retorna o valor cacheado
    }
}

