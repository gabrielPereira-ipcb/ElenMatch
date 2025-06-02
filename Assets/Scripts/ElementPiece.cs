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
        StartCoroutine(FadeOut());
    }

    private IEnumerator FadeOut()
    {
        float fadeDuration = 0.5f;
        Renderer pieceRenderer = GetComponent<Renderer>();

        if (pieceRenderer == null)
        {
            Debug.LogError("Renderer not found on ElementPiece. Cannot perform fade animation.");
            Destroy(gameObject); // Destroy immediately if no renderer
            yield break;
        }

        // Check if material supports transparency (very basic check)
        // A more robust check would involve knowing the specific shader being used.
        // Standard shader needs to be set to "Fade" or "Transparent" mode.
        if (pieceRenderer.material.shader.name.Contains("Standard") && pieceRenderer.material.GetFloat("_Mode") > 1) // 2 is Fade, 3 is Transparent
        {
            // Shader is likely Standard and set to Fade or Transparent
        }
        else if (pieceRenderer.material.HasProperty("_Color"))
        {
             // This is a broad check. Many shaders have a _Color property.
             // If this is not a transparent shader, alpha changes won't be visible.
             Debug.LogWarning("ElementPiece material might not support transparency for fade out. Shader: " + pieceRenderer.material.shader.name + ". Ensure material uses a transparent shader (e.g., Standard with Rendering Mode set to Fade/Transparent).");
        }
        else
        {
            Debug.LogWarning("ElementPiece material does not seem to have a standard _Color property for transparency. Fade out may not be visible. Shader: " + pieceRenderer.material.shader.name);
        }


        Color originalColor = pieceRenderer.material.color;
        float elapsedTime = 0f;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float newAlpha = Mathf.Lerp(originalColor.a, 0f, elapsedTime / fadeDuration);
            pieceRenderer.material.color = new Color(originalColor.r, originalColor.g, originalColor.b, newAlpha);
            yield return null;
        }

        // Ensure alpha is fully 0
        pieceRenderer.material.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0f);
        
        Destroy(gameObject);
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

    // The static 'elementData' dictionary above serves as a central mapping for element properties.
    // Currently, this data is used internally by ElementPiece instances.
    // If other scripts require direct static access to this mapping in the future
    // (e.g., to get the symbol for an element type without an instance),
    // public static getter methods can be added here (e.g., public static string GetElementSymbol(int elementType)).
}