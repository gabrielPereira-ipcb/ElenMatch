using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField] private Button playButton;
    [SerializeField] private Button quitButton;
    private int currentButtonIndex = 0;
    private Button[] buttons;

    private void Start()
    {
        // Inicializa o array de botões
        buttons = new Button[] { playButton, quitButton };
        
        // Seleciona o primeiro botão por padrão
        if (buttons.Length > 0)
        {
            EventSystem.current.SetSelectedGameObject(buttons[0].gameObject);
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

    public void StartGame()
    {
        Debug.Log("Starting game...");
        // Carrega a cena do jogo
        SceneManager.LoadScene("GameScene");
    }

    public void QuitGame()
    {
        Debug.Log("Quitting game...");
        // Fecha o jogo (não funciona no editor, apenas na build)
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
} 