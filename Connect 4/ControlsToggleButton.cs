using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ControlsToggleButton : MonoBehaviour
{
    [Header("What to Show/Hide")]
    public GameObject controlsPanel;

    [Header("This Button & Its UI")]
    public Button toggleButton;
    public TextMeshProUGUI toggleLabel;
    public RectTransform buttonRect;

    [Header("UI Layouts")]
    public Vector2 openPosition;
    public Vector2 closedPosition;

    void Start()
    {
        toggleButton.onClick.AddListener(OnToggle);

        bool visible = GameSettings.ControlsVisible;
        ApplyState(visible);
    }

    private void OnToggle()
    {
        GameSettings.ControlsVisible = !GameSettings.ControlsVisible;
        ApplyState(GameSettings.ControlsVisible);
    }

    private void ApplyState(bool visible)
    {
        controlsPanel.SetActive(visible);
        toggleLabel.text = visible ? "Hide Controls" : "Show Controls";
        buttonRect.anchoredPosition = visible ? openPosition : closedPosition;
    }
}
