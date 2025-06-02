using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class GameOverManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI finalScoreText;
    [SerializeField] private TextMeshProUGUI highScoreText;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button menuButton;
    private int currentButtonIndex = 0;
    private Button[] buttons;

    private void Start()
    {
        // Inicializa o array de botões
        buttons = new Button[] { restartButton, menuButton };
        
        // Seleciona o primeiro botão por padrão
        if (buttons.Length > 0)
        {
            EventSystem.current.SetSelectedGameObject(buttons[0].gameObject);
        }

        // Recupera e exibe a pontuação final
        int finalScore = PlayerPrefs.GetInt("LastScore", 0);
        if (finalScoreText != null)
        {
            finalScoreText.text = $"Pontuação Final: {finalScore}";
        }

        // Recupera e exibe a pontuação mais alta
        int highScore = PlayerPrefs.GetInt("HighScore", 0);
        if (highScoreText != null)
        {
            highScoreText.text = $"Recorde: {highScore}";
        }

        // Atualiza o recorde se necessário
        if (finalScore > highScore)
        {
            PlayerPrefs.SetInt("HighScore", finalScore);
            PlayerPrefs.Save();
            if (highScoreText != null)
            {
                highScoreText.text = $"Novo Recorde: {finalScore}!";
            }
        }
    }

    private void Update()
    {
        // Navegação com Tab
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            currentButtonIndex = (currentButtonIndex + 1) % buttons.Length;
            EventSystem.current.SetSelectedGameObject(buttons[currentButtonIndex].gameObject);
        }

        // Ativação com Enter
        if (Input.GetKeyDown(KeyCode.Return))
        {
            if (EventSystem.current.currentSelectedGameObject != null)
            {
                Button selectedButton = EventSystem.current.currentSelectedGameObject.GetComponent<Button>();
                if (selectedButton != null)
                {
                    selectedButton.onClick.Invoke();
                }
            }
        }
    }

    public void RestartGame()
    {
        Debug.Log("Restarting game...");
        // Carrega a cena do jogo novamente
        SceneManager.LoadScene("GameScene");
    }

    public void BackToMenu()
    {
        Debug.Log("Going back to menu...");
        // Volta para o menu principal
        SceneManager.LoadScene("MenuScene");
    }
} 