using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    [SerializeField] private TextMeshProUGUI gameOverScoreText;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Mantém este objeto entre cenas
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        // Se estivermos na tela de Game Over, mostra a pontuação
        if (gameOverScoreText != null)
        {
            int finalScore = PlayerPrefs.GetInt("LastScore", 0);
            gameOverScoreText.text = $"Final Score: {finalScore}";
        }
    }
} 