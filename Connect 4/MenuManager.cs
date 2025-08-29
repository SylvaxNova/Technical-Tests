using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    [Header("Player 1 UI")]
    [SerializeField] private GameObject p1BlackOverlay;
    [SerializeField] private TMP_InputField p1NameInput;
    [SerializeField] private Button[] p1ColorButtons;
    [SerializeField] private GameObject[] p1ColorHighlights;
    [SerializeField] private Button p1ReadyButton;

    [Header("Player 2 UI")]
    [SerializeField] private GameObject p2BlackOverlay;
    [SerializeField] private TMP_InputField p2NameInput;
    [SerializeField] private Button[] p2ColorButtons;
    [SerializeField] private GameObject[] p2ColorHighlights;
    [SerializeField] private Button p2ReadyButton;

    // temp storage of which color index was clicked
    private int p1Color = -1, p2Color = -1;

    void Start()
    {
        p1BlackOverlay.SetActive(false);
        p2BlackOverlay.SetActive(true);

        // hide all highlight images
        foreach (var img in p1ColorHighlights) img.SetActive(false);
        foreach (var img in p2ColorHighlights) img.SetActive(false);

        // hook up all buttons
        for (int i = 0; i < p1ColorButtons.Length; i++)
        {
            int idx = i;
            p1ColorButtons[i].onClick.AddListener(() => OnColorClicked(1, idx));
        }
        p1ReadyButton.onClick.AddListener(OnPlayer1Ready);

        for (int i = 0; i < p2ColorButtons.Length; i++)
        {
            int idx = i;
            p2ColorButtons[i].onClick.AddListener(() => OnColorClicked(2, idx));
        }
        p2ReadyButton.onClick.AddListener(OnPlayer2Ready);
    }

    void OnColorClicked(int player, int colorIdx)
    {
        if (player == 1)
        {
            p1Color = colorIdx;
            for (int i = 0; i < p1ColorHighlights.Length; i++)
                p1ColorHighlights[i].SetActive(i == colorIdx);
        }
        else
        {
            p2Color = colorIdx;
            for (int i = 0; i < p2ColorHighlights.Length; i++)
                p2ColorHighlights[i].SetActive(i == colorIdx);
        }
    }

    void OnPlayer1Ready()
    {
        GameSettings.Player1Name = p1NameInput.text;
        if (GameSettings.Player1Name.Length == 0)
        {
            p1NameInput.text = "Player 1";
            GameSettings.Player1Name = "Player 1";
        }


        if (p1Color < 0)
        {
            p1Color = 0;
            GameSettings.Player1ColorIndex = p1Color;
        }

        GameSettings.Player1ColorIndex = p1Color;
        p2ColorButtons[p1Color].interactable = false;

        p1BlackOverlay.SetActive(true);
        p2BlackOverlay.SetActive(false);
    }

    void OnPlayer2Ready()
    {
        GameSettings.Player2Name = p2NameInput.text;
        if (GameSettings.Player2Name.Length == 0)
        {
            p2NameInput.text = "Player 2";
            GameSettings.Player2Name = "Player 2";
        }

        int max = p2ColorButtons.Length;
        if (p2Color < 0 || p2Color == GameSettings.Player1ColorIndex)
        {
            for (int i = 0; i < max; i++)
                if (i != GameSettings.Player1ColorIndex)
                {
                    p2Color = i;
                    break;
                }
        }
        GameSettings.Player2ColorIndex = p2Color;

        GameSettings.ControlsVisible = true;
        SceneManager.LoadScene("GameScene");
    }

    public void OnExitButton()
    {
#if UNITY_EDITOR
        // Stop in the Editor
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
