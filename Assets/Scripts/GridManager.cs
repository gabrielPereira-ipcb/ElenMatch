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
    private Dictionary<string, int> bondScores = new Dictionary<string, int>()
    {
        { "H2O", 100 },   // Água
        { "NaCl", 150 },  // Sal
        { "CO2", 200 },   // Dióxido de carbono
        { "NH3", 175 }    // Amónia
    };

    // Bónus baseado no número de peças
    private Dictionary<int, float> pieceCountMultiplier = new Dictionary<int, float>()
    {
        { 2, 1.0f },     // 2 peças = pontuação normal
        { 3, 1.2f },     // 3 peças = 20% bónus
        { 4, 1.5f },     // 4 peças = 50% bónus
        { 5, 2.0f }      // 5 ou mais peças = dobro da pontuação
    };

    // Dicionário para mapear combinações químicas
    // Dicionário para mapear combinações químicas (elementType: quantidade)
    public Dictionary<int, Dictionary<int, int>> chemicalBonds = new Dictionary<int, Dictionary<int, int>>()
    {
        // H2O (2 H e 1 O)
        { 0, new Dictionary<int, int> { { 0, 2 }, { 1, 1 } } },
        // NaCl (1 Na e 1 Cl)
        { 2, new Dictionary<int, int> { { 2, 1 }, { 3, 1 } } },
        // CO2 (1 C e 2 O)
        { 3, new Dictionary<int, int> { { 3, 1 }, { 1, 2 } } },
        // NH3 (1 N e 3 H)
        { 4, new Dictionary<int, int> { { 4, 1 }, { 0, 3 } } }
    };

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

            // Verifica o estado atual do grid
            DebugGridState();

            // Chama a função de verificação de combinações
            CheckForChemicalBonds(currentColumnIndex, targetRow);

            // Libera o controlo da peça atual
            currentFallingPiece = null;

            // Verifica se a grelha está cheia após a queda da peça
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

    // Obtém peças adjacentes numa posição específica
    private List<ElementPiece> GetAdjacentPieces(int col, int row)
    {
        List<ElementPiece> pieces = new List<ElementPiece>();
        HashSet<Vector2Int> checkedPositions = new HashSet<Vector2Int>();
        Queue<Vector2Int> positionsToCheck = new Queue<Vector2Int>();
        
        // Verifica se a posição inicial tem uma peça
        if (grid[col, row] == null)
            return pieces;
            
        // Adiciona a posição inicial à fila
        positionsToCheck.Enqueue(new Vector2Int(col, row));
        checkedPositions.Add(new Vector2Int(col, row));
        
        // Define as direções a verificar (horizontal e vertical)
        Vector2Int[] directions = new Vector2Int[]
        {
            new Vector2Int(-1, 0),  // Esquerda
            new Vector2Int(1, 0),   // Direita
            new Vector2Int(0, -1),  // Baixo
            new Vector2Int(0, 1)    // Cima
        };

        // Processa todas as posições na fila
        while (positionsToCheck.Count > 0)
        {
            Vector2Int currentPos = positionsToCheck.Dequeue();
            
            // Adiciona a peça atual à lista
            ElementPiece piece = grid[currentPos.x, currentPos.y].GetComponent<ElementPiece>();
            if (piece != null)
            {
                pieces.Add(piece);
            }

            // Verifica todas as direções adjacentes
            foreach (Vector2Int dir in directions)
            {
                Vector2Int newPos = currentPos + dir;
                
                // Verifica se a posição está dentro dos limites da grelha
                if (newPos.x >= 0 && newPos.x < width && newPos.y >= 0 && newPos.y < height)
                {
                    // Se a posição ainda não foi verificada e tem uma peça
                    if (!checkedPositions.Contains(newPos) && grid[newPos.x, newPos.y] != null)
                    {
                        checkedPositions.Add(newPos);
                        positionsToCheck.Enqueue(newPos);
                    }
                }
            }
        }

        return pieces;
    }

    // Método para fazer as peças caírem após a remoção
    private void MakePiecesFall()
    {
        Debug.Log("MakePiecesFall() called.");
        bool piecesFell = false;
        HashSet<Vector2Int> locationsToCheck = new HashSet<Vector2Int>();
        
        // Para cada coluna
        for (int col = 0; col < width; col++)
        {
            Debug.Log($"Checking column {col} for falling pieces.");
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
                Debug.Log($"Found empty row at {firstEmptyRow} in column {col}.");
                // Procura a primeira peça acima do espaço vazio
                for (int row = firstEmptyRow - 1; row >= 0; row--)
                {
                    if (grid[col, row] != null)
                    {
                        Debug.Log($"Found piece at [{col}, {row}] to move to [{col}, {firstEmptyRow}].");
                        ElementPiece pieceScript = grid[col, row].GetComponent<ElementPiece>();
                        if (pieceScript != null)
                        {
                            Debug.Log($"Moving piece of type: {pieceScript.elementType}");
                        }
                        
                        GameObject pieceToMove = grid[col, row];
                        grid[col, firstEmptyRow] = pieceToMove;
                        grid[col, row] = null;
                        
                        Vector3 newPosition = CalculateGridPosition(col, firstEmptyRow);
                        pieceToMove.transform.position = newPosition;
                        Debug.Log($"Piece moved to grid[{col}, {firstEmptyRow}] and position updated.");
                        
                        piecesFell = true;
                        locationsToCheck.Add(new Vector2Int(col, firstEmptyRow)); // Adiciona a nova posição da peça

                        // Adiciona posições adjacentes ortogonalmente à nova posição da peça
                        AddAdjacentLocations(locationsToCheck, col, firstEmptyRow);

                        firstEmptyRow--;
                    }
                }
            }
        }
        
        if (piecesFell)
        {
            Debug.Log($"Pieces fell, re-triggering bond checks for {locationsToCheck.Count} specific locations.");
            foreach (var loc in locationsToCheck)
            {
                if (grid[loc.x, loc.y] != null) // Verifica se ainda existe uma peça no local (pode ter sido removida por outra ligação)
                {
                    CheckForChemicalBonds(loc.x, loc.y);
                }
            }
        }
    }

    private void AddAdjacentLocations(HashSet<Vector2Int> locations, int col, int row)
    {
        int[] dCol = { -1, 1, 0, 0 };
        int[] dRow = { 0, 0, -1, 1 };

        for (int i = 0; i < 4; i++)
        {
            int adjacentCol = col + dCol[i];
            int adjacentRow = row + dRow[i];

            if (adjacentCol >= 0 && adjacentCol < width && adjacentRow >= 0 && adjacentRow < height)
            {
                if (grid[adjacentCol, adjacentRow] != null) // Só adiciona se houver uma peça lá
                {
                    locations.Add(new Vector2Int(adjacentCol, adjacentRow));
                }
            }
        }
    }

    // Verifica se duas peças estão adjacentes (horizontal ou vertical)
    private bool ArePiecesAdjacent(Vector3 pos1, Vector3 pos2)
    {
        float xDiff = Mathf.Abs(pos1.x - pos2.x);
        float yDiff = Mathf.Abs(pos1.y - pos2.y);
        
        // Peças estão adjacentes se estiverem na mesma linha ou coluna
        // e a distância for exatamente igual à largura da coluna ou altura da linha
        bool isAdjacent = (xDiff < 0.1f && Mathf.Abs(yDiff - rowHeight) < 0.1f) || // Mesma coluna
                         (yDiff < 0.1f && Mathf.Abs(xDiff - columnWidth) < 0.1f); // Mesma linha
        
        Debug.Log($"Checking adjacency between pieces at {pos1} and {pos2}: {isAdjacent}");
        return isAdjacent;
    }

    // Verifica se um grupo de peças está conectado
    private bool ArePiecesConnected(List<ElementPiece> pieces)
    {
        if (pieces.Count < 2) return true;

        // Cria um grafo de conexões
        Dictionary<ElementPiece, List<ElementPiece>> connections = new Dictionary<ElementPiece, List<ElementPiece>>();
        
        // Inicializa o grafo
        foreach (var piece in pieces)
        {
            connections[piece] = new List<ElementPiece>();
        }

        // Adiciona as conexões
        for (int i = 0; i < pieces.Count; i++)
        {
            for (int j = i + 1; j < pieces.Count; j++)
            {
                if (ArePiecesAdjacent(pieces[i].transform.position, pieces[j].transform.position))
                {
                    connections[pieces[i]].Add(pieces[j]);
                    connections[pieces[j]].Add(pieces[i]);
                }
            }
        }

        // Verifica se todas as peças estão conectadas usando BFS
        HashSet<ElementPiece> visited = new HashSet<ElementPiece>();
        Queue<ElementPiece> queue = new Queue<ElementPiece>();
        
        queue.Enqueue(pieces[0]);
        visited.Add(pieces[0]);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            foreach (var neighbor in connections[current])
            {
                if (!visited.Contains(neighbor))
                {
                    visited.Add(neighbor);
                    queue.Enqueue(neighbor);
                }
            }
        }

        // Se todas as peças foram visitadas, elas estão conectadas
        return visited.Count == pieces.Count;
    }

    // Verifica ligações químicas
    private static long bondCheckRecursionCounter = 0; // Contador de recursão
    private void CheckForChemicalBonds(int col, int row)
    {
        Debug.Log($"CheckForChemicalBonds called for piece at [{col}, {row}].");
        bondCheckRecursionCounter = 0; // Reseta o contador para cada chamada principal

        // Obtém todas as peças adjacentes e adjacentes das adjacentes
        List<ElementPiece> adjacentPieces = GetAdjacentPieces(col, row);
        Debug.Log($"GetAdjacentPieces returned {adjacentPieces.Count} pieces for check at [{col}, {row}].");
        
        // Se não há peças suficientes para formar uma ligação, retorna
        if (adjacentPieces.Count < 2)
            return;
        
        // Conta os elementos presentes
        Dictionary<int, int> elementCounts = new Dictionary<int, int>();
        foreach (var piece in adjacentPieces)
        {
            if (!elementCounts.ContainsKey(piece.elementType))
                elementCounts[piece.elementType] = 0;
            elementCounts[piece.elementType]++;
        }

        // Log das peças envolvidas
        Debug.Log("Checking for bonds between pieces:");
        foreach (var kvp in elementCounts)
        {
            Debug.Log($"- Element type {kvp.Key}: {kvp.Value} pieces");
        }

        // Verifica cada tipo de ligação conhecida
        foreach (var bond in chemicalBonds)
        {
            bool canFormBond = true;
            int totalRequiredPieces = 0;

            // Primeiro, verifica se temos todos os elementos necessários
            foreach (var element in bond.Value)
            {
                if (!elementCounts.ContainsKey(element.Key) || 
                    elementCounts[element.Key] < element.Value)
                {
                    canFormBond = false;
                    break;
                }
                totalRequiredPieces += element.Value;
            }

            // Se temos todos os elementos necessários
            if (canFormBond)
            {
                // Encontra todas as combinações possíveis de peças que satisfazem a ligação
                List<List<ElementPiece>> possibleCombinations = new List<List<ElementPiece>>();
                FindPossibleCombinations(adjacentPieces, bond.Value, new List<ElementPiece>(), possibleCombinations, ref bondCheckRecursionCounter);

                int combinationsCheckedByAreConnected = 0;
                // Verifica cada combinação possível
                foreach (var combination in possibleCombinations)
                {
                    combinationsCheckedByAreConnected++;
                    // Verifica se as peças estão conectadas
                    if (ArePiecesConnected(combination))
                    {
                        Debug.Log($"Successful bond: Checked {combinationsCheckedByAreConnected} combinations with ArePiecesConnected.");
                        string bondType = "";
                        switch (bond.Key)
                        {
                            case 0: bondType = "H2O"; break;
                            case 2: bondType = "NaCl"; break;
                            case 3: bondType = "CO2"; break;
                            case 4: bondType = "NH3"; break;
                            case 5: bondType = "CH4"; break;
                        }

                        Debug.Log($"Chemical bond formed! Type: {bondType}");
                        Debug.Log($"Elements involved: {string.Join(", ", elementCounts.Select(kvp => $"{kvp.Key}:{kvp.Value}"))}");
                        
                        // Adiciona pontos
                        if (bondScores.ContainsKey(bondType))
                        {
                            AddScore(bondScores[bondType], totalRequiredPieces);
                        }
                        
                        // Remove apenas as peças que formaram a ligação
                        foreach (ElementPiece piece in combination)
                        {
                            // Encontra a posição da peça na grelha
                            Vector3 piecePos = piece.transform.position;
                            int pieceCol = Mathf.RoundToInt((piecePos.x - startGridX) / columnWidth);
                            int pieceRow = Mathf.RoundToInt((baseGridY - piecePos.y) / rowHeight);

                            // Limpa a referência na grelha
                            if (pieceCol >= 0 && pieceCol < width && pieceRow >= 0 && pieceRow < height)
                            {
                                grid[pieceCol, pieceRow] = null;
                            }

                            // Inicia a animação de remoção
                            piece.StartBondAnimation();

                            // Destroi o GameObject
                            Destroy(piece.gameObject);
                        }

                        // Faz as peças caírem após a remoção
                        MakePiecesFall();

                        // Retorna após formar uma ligação válida
                        return;
                    }
                }
            }
        }

        // Se chegou aqui, não formou nenhuma ligação válida
        Debug.Log("No valid bond pattern found for these pieces");
        Debug.Log($"CheckForChemicalBonds at [{col}, {row}] finished. Total FindPossibleCombinations calls: {bondCheckRecursionCounter}");
    }

    // Encontra todas as combinações possíveis de peças que satisfazem uma ligação
    private void FindPossibleCombinations(List<ElementPiece> pieces, Dictionary<int, int> requiredElements, 
        List<ElementPiece> currentCombination, List<List<ElementPiece>> result, ref long recursionCounter)
    {
        recursionCounter++;
        // Se já temos todas as peças necessárias
        if (IsValidCombination(currentCombination, requiredElements))
        {
            result.Add(new List<ElementPiece>(currentCombination));
            return;
        }

        // Se ainda precisamos de mais peças
        foreach (var piece in pieces)
        {
            if (!currentCombination.Contains(piece))
            {
                currentCombination.Add(piece);
                FindPossibleCombinations(pieces, requiredElements, currentCombination, result, ref recursionCounter);
                currentCombination.RemoveAt(currentCombination.Count - 1);
            }
        }
    }

    // Verifica se uma combinação de peças satisfaz os requisitos de uma ligação
    private bool IsValidCombination(List<ElementPiece> combination, Dictionary<int, int> requiredElements)
    {
        Dictionary<int, int> elementCounts = new Dictionary<int, int>();
        foreach (var piece in combination)
        {
            if (!elementCounts.ContainsKey(piece.elementType))
                elementCounts[piece.elementType] = 0;
            elementCounts[piece.elementType]++;
        }

        foreach (var element in requiredElements)
        {
            if (!elementCounts.ContainsKey(element.Key) || 
                elementCounts[element.Key] != element.Value)
            {
                return false;
            }
        }

        return true;
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

    // Determina o tipo de ligação química formada
    private string DetermineBondType(List<ElementPiece> pieces)
    {
        // Conta os elementos presentes
        Dictionary<int, int> elementCounts = new Dictionary<int, int>();
        foreach (var piece in pieces)
        {
            if (!elementCounts.ContainsKey(piece.elementType))
                elementCounts[piece.elementType] = 0;
            elementCounts[piece.elementType]++;
        }

        // Verifica cada tipo de ligação conhecida
        foreach (var bond in chemicalBonds) 
        {
            bool matches = true;
            foreach (var element in bond.Value)
            {
                if (!elementCounts.ContainsKey(element.Key) || 
                    elementCounts[element.Key] != element.Value)
                {
                    matches = false;
                    break;
                }
            }
            if (matches)
            {
                switch (bond.Key)
                {
                    case 0: return "H2O";
                    case 2: return "NaCl";
                    case 3: return "CO2";
                    case 4: return "NH3";
                    case 5: return "CH4";
                    default: return "Unknown";
                }
            }
        }

        return "Unknown";
    }
}

