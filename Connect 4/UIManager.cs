using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Unity.VisualScripting;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    private GameManager gm;

    [Header("Player Colors")]
    [SerializeField] private Material[] colorOptions;

    [Header("In-Game UI")]
    [SerializeField] private TMP_Text player1NameText;
    [SerializeField] private TMP_Text player2NameText;
    [SerializeField] private GameObject player1Indicator;
    [SerializeField] private GameObject player2Indicator;
    [SerializeField] private GameObject drawGameText;
    private Image[] player1PanelImages;
    private Image[] player2PanelImages;

    [Header("Win VFX")]
    [SerializeField] private GameObject player1WinText;
    [SerializeField] private GameObject player2WinText;
    [SerializeField] private ParticleSystem player1WinVFX;
    [SerializeField] private ParticleSystem player2WinVFX;

    [Header("Flashing Pieces")]
    [SerializeField] private float flashInterval = 1f;
    [SerializeField] private Color flashColor = Color.white;
    private List<Renderer> winRenderers;
    private List<Color> originalColors;
    private bool toWhite;
    private float lastFlashTime;

    [Header("Controls UI")]
    [SerializeField] private GameObject controlsPanel;

    [Header("Restart Countdown")]
    [SerializeField] private TMP_Text countdownText;
    [SerializeField] private GameObject bottomBoardCollider;


    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        
        gm = GameManager.Instance;
        
        InitializeUI();
    }

    void OnEnable()
    {
        gm.PlayerSwitched += UpdateTurnIndicator;
        gm.GameDraw += ShowDraw;
        gm.PlayerWon += OnPlayerWon;
    }

    void OnDisable()
    {
        gm.PlayerSwitched -= UpdateTurnIndicator;
        gm.GameDraw -= ShowDraw;
        gm.PlayerWon -= OnPlayerWon;
    }

    void InitializeUI()
    {
        controlsPanel.SetActive(GameSettings.ControlsVisible);
        drawGameText.SetActive(false);
        player1WinText.SetActive(false);
        player2WinText.SetActive(false);
        player1WinVFX.gameObject.SetActive(false);
        player2WinVFX.gameObject.SetActive(false);
        bottomBoardCollider.SetActive(true);

        player1NameText.text = GameSettings.Player1Name;
        player2NameText.text = GameSettings.Player2Name;

        int p1Index = Mathf.Clamp(GameSettings.Player1ColorIndex, 0, colorOptions.Length - 1);
        int p2Index = Mathf.Clamp(GameSettings.Player2ColorIndex, 0, colorOptions.Length - 1);
        Material mat1 = colorOptions[p1Index];
        Material mat2 = colorOptions[p2Index];
        Color color1 = mat1.color;
        Color color2 = mat2.color;

        player1PanelImages = player1Indicator.GetComponentsInChildren<Image>();
        player2PanelImages = player2Indicator.GetComponentsInChildren<Image>();
        foreach (var img in player1PanelImages) 
            img.color = color1;
        foreach (var img in player2PanelImages) 
            img.color = color2;

        player1NameText.color = color1;
        player2NameText.color = color2;

        ParticleSystemRenderer particleRenderer1 = player1WinVFX.GetComponent<ParticleSystemRenderer>();
        ParticleSystemRenderer particleRenderer2 = player2WinVFX.GetComponent<ParticleSystemRenderer>();
        if (particleRenderer1 != null) 
            particleRenderer1.material = mat1;
        if (particleRenderer2 != null) 
            particleRenderer2.material = mat2;

        UpdateTurnIndicator(GameManager.Instance.currentPlayer);
    }

    void UpdateTurnIndicator(int currentPlayer)
    {
        bool isP1 = currentPlayer == 1;
        player1Indicator.SetActive(isP1);
        player2Indicator.SetActive(!isP1);

        Color activeColor = colorOptions[Mathf.Clamp(isP1 ? GameSettings.Player1ColorIndex : GameSettings.Player2ColorIndex, 0, colorOptions.Length - 1)].color;
        Color inactiveColor = Color.black;

        foreach (Image img in player1PanelImages)
            img.color = isP1 ? activeColor : inactiveColor;
        foreach (Image img in player2PanelImages)
            img.color = !isP1 ? activeColor : inactiveColor;

        player1NameText.color = isP1 ? activeColor : inactiveColor;
        player2NameText.color = !isP1 ? activeColor : inactiveColor;
    }

    void Update()
    {
        if (winRenderers != null && winRenderers.Count > 0)
        {
            float t = (Time.time - lastFlashTime) / flashInterval;
            t = Mathf.Clamp01(t);
            for (int i = 0; i < winRenderers.Count; i++)
            {
                Material mat = winRenderers[i].material;
                Color start = toWhite ? originalColors[i] : flashColor;
                Color end = toWhite ? flashColor : originalColors[i];
                mat.color = Color.Lerp(start, end, t);
            }
        }
    }

    private void OnPlayerWon(int winner, List<Vector2Int> winCells)
    {
        ShowWin(winner);

        int rows = gm.PieceGrid.GetLength(0);
        int cols = gm.PieceGrid.GetLength(1);
        for (int r = 0; r < rows; r++)
        for (int c = 0; c < cols; c++)
        {
            var coord = new Vector2Int(r, c);
            if (winCells.Contains(coord)) continue;
            var piece = gm.PieceGrid[r, c];
            if (piece == null) continue;
            var rend = piece.GetComponentInChildren<Renderer>();
            if (rend == null) continue;
            var mat  = rend.material;
            if (mat.HasProperty("_EmissionColor"))
            {
                mat.DisableKeyword("_EMISSION");
                mat.SetColor("_EmissionColor", Color.black);
            }
        }

        winRenderers   = new List<Renderer>();
        originalColors = new List<Color>();
        foreach (var coord in winCells)
        {
            var piece = gm.PieceGrid[coord.x, coord.y];
            if (piece == null) continue;
            var rend = piece.GetComponentInChildren<Renderer>();
            if (rend == null) continue;
            winRenderers.Add(rend);
            originalColors.Add(rend.material.color);
        }

        toWhite = false;
        lastFlashTime = Time.time;
        InvokeRepeating(nameof(ToggleFlashDirection), flashInterval, flashInterval);
    }

    private void ToggleFlashDirection()
    {
        toWhite = !toWhite;
        lastFlashTime = Time.time;
    }

    private void ShowWin(int winner)
    {
        player1Indicator.SetActive(false);
        player2Indicator.SetActive(false);
        drawGameText.SetActive(false);

        GameObject winText = (winner == 1) ? player1WinText : player2WinText;
        ParticleSystem vfx = (winner == 1) ? player1WinVFX : player2WinVFX;

        TMP_Text tmp = winText.GetComponentInChildren<TMP_Text>();
        tmp.color = colorOptions[Mathf.Clamp(winner == 1 ? GameSettings.Player1ColorIndex : GameSettings.Player2ColorIndex, 0, colorOptions.Length - 1)].color;

        vfx.gameObject.SetActive(true);
        vfx.Play();
        winText.SetActive(true);
    }

    void ShowDraw()
    {
        player1Indicator.SetActive(false);
        player2Indicator.SetActive(false);
        drawGameText.SetActive(true);
    }

    public void OnToggleControls()
    {
        GameSettings.ControlsVisible = !GameSettings.ControlsVisible;
        controlsPanel.SetActive(GameSettings.ControlsVisible);
    }

    public void OnRestartButton()
    {
        gm.gameOver = true;
        player1WinText.SetActive(false);
        player2WinText.SetActive(false);
        player1WinVFX.Stop();
        player2WinVFX.Stop();
        bottomBoardCollider.SetActive(false);
        StartCoroutine(RestartRoutine());
    }

    IEnumerator RestartRoutine()
    {
        for (int i = 3; i >= 1; i--)
        {
            countdownText.text = i.ToString();
            countdownText.gameObject.SetActive(true);
            yield return new WaitForSeconds(1f);
        }

        CancelInvoke(nameof(ToggleFlashDirection));
        if (winRenderers != null)
            for (int i = 0; i < winRenderers.Count; i++)
                winRenderers[i].material.color = originalColors[i];
        
        gm.DestroyPieces();

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void OnMenuButton()
    {
        gm.DestroyPieces();
        SceneManager.LoadScene("MenuScene");
    }

    public Material GetCurrentPlayerMaterial()
    {
        int index = (GameManager.Instance.currentPlayer == 1)
            ? GameSettings.Player1ColorIndex
            : GameSettings.Player2ColorIndex;
        index = Mathf.Clamp(index, 0, colorOptions.Length - 1);
        return colorOptions[index];
    }
}
