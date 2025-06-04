// ElementPiece.cs
using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

// Este script será anexado a cada peça de elemento químico
// para armazenar informações sobre o elemento que representa
public class ElementPiece : MonoBehaviour
{
    // Variável pública para armazenar o tipo de elemento (usando um índice simples por agora)
    public int elementType;

    // Coordenadas da peça na grade (para acesso mais rápido).
    // Estes valores são gerenciados pelo GridManager quando a peça é colocada ou movida na grade.
    public int GridX { get; set; }
    public int GridY { get; set; }

    // Referências para os componentes TextMeshPro filhos para exibir as informações do elemento
    [SerializeField] private TextMeshPro atomicNumberText; // Para o número atómico
    [SerializeField] private TextMeshPro symbolText;       // Para o símbolo do elemento
    [SerializeField] private TextMeshPro nameText;         // Para o nome do elemento

    // Estrutura de dados para armazenar informações dos elementos
    private static readonly Dictionary<int, (int atomicNumber, string symbol, string name, Color color)> elementData = new Dictionary<int, (int, string, string, Color)>
    {
        { 0, (1, "H", "Hidrogénio", new Color(1f, 1f, 1f)) },      // Branco
        { 1, (8, "O", "Oxigénio", new Color(1f, 0f, 0f)) },         // Vermelho
        { 2, (11, "Na", "Sódio", new Color(0.5f, 0.5f, 0.5f)) },    // Cinza
        { 3, (17, "Cl", "Cloro", new Color(0f, 1f, 0f)) },          // Verde
        { 4, (7, "N", "Nitrogénio", new Color(0f, 0f, 1f)) },             // Azul
        { 5, (6, "C", "Carbono", new Color(0.5f, 0.5f, 0.5f)) },
    };

    // Método para definir o tipo de elemento desta peça e atualizar a sua representação visual
    public void SetElementType(int type)
    {
        elementType = type;
        Debug.Log($"ElementPiece assigned type: {elementType}");

        // Atualiza os textos com base no tipo de elemento
        if (elementData.TryGetValue(elementType, out var data))
        {
            if (atomicNumberText != null) atomicNumberText.text = data.atomicNumber.ToString();
            if (symbolText != null) symbolText.text = data.symbol;
            if (nameText != null) nameText.text = data.name;

            // Atualiza a cor do material
            if (GetComponent<Renderer>() != null)
            {
                GetComponent<Renderer>().material.color = data.color;
            }
        }
        else
        {
            Debug.LogWarning($"Unknown element type: {elementType}");
        }
    }

    // Método para iniciar a animação de remoção quando formar uma ligação
    public void StartBondAnimation()
    {
        // Aqui você pode adicionar uma animação visual para indicar que a peça está formando uma ligação
        // Por exemplo:
        // StartCoroutine(FadeOut());
    }

    private IEnumerator FadeOut()
    {
        // Implementar animação de fade out
        yield return null;
    }

    // Você pode adicionar outros métodos aqui que as peças possam precisar,
    // como reações a combinações, animações, etc.

    // Métodos Start e Update são deixados aqui por convenção, mas podem ser removidos
    // se não forem usados por este script.
    void Start()
    {
        // Lógica de inicialização específica da peça, se houver.
    }

    void Update()
    {
        // Lógica de atualização específica da peça, se houver.
    }

    // TODO: Adicionar métodos auxiliares para obter informações do elemento com base no índice,
    // ou ter uma estrutura de dados central (possivelmente no GridManager ou outro script)
    // que mapeie o índice do elemento para os seus dados (número, símbolo, nome, cor).
    // private int GetAtomicNumber(int typeIndex) { /* Implementar */ return 0; }
    // private string GetSymbol(int typeIndex) { /* Implementar */ return "N/A"; }
    // private string GetName(int typeIndex) { /* Implementar */ return "Unknown"; }
    // private Color GetElementColor(int typeIndex) { /* Implementar */ return Color.gray; }

    /// <summary>
    /// Retorna o símbolo textual do elemento (ex: "H", "O").
    /// Usado principalmente para logging e debug.
    /// </summary>
    /// <returns>O símbolo do elemento como string, ou "N/A" se o componente de texto não estiver disponível.</returns>
    public string GetSymbol()
    {
        return symbolText != null ? symbolText.text : "N/A";
    }
}