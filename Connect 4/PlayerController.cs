using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Hover Preview")]
    [SerializeField] private GameObject hoverPiecePrefab;
    [SerializeField] private float hoverMoveSpeed = 10f;

    private GameObject hoverPiece;
    private int currentIndex;

    private GameManager gm => GameManager.Instance;
    private UIManager ui => UIManager.Instance;

    void Start()
    {
        var columns = gm.ColumnPoints;
        hoverPiece = Instantiate(hoverPiecePrefab, columns[0].position,Quaternion.identity);

        // Recolor whenever the player switches
        gm.PlayerSwitched += newPlayer => UpdateHoverColor();
        UpdateHoverColor();
    }

    void Update()
    {
        if (gm.gameOver)
        {
            hoverPiece.GetComponent<Renderer>().enabled = false;
        }

        HandleNavigation();
        HandlePlacement();

        var columns = gm.ColumnPoints;
        var target = columns[currentIndex].position;
        hoverPiece.transform.position = Vector3.Lerp(hoverPiece.transform.position, target, Time.deltaTime * hoverMoveSpeed);
    }

    private void HandleNavigation()
    {
        int delta = 0;
        if (Input.GetKeyDown(KeyCode.A))
            delta = -1;
        if (Input.GetKeyDown(KeyCode.D))
            delta = +1;

        if (delta != 0)
        {
            int length = gm.ColumnPoints.Length;
            
            currentIndex += delta;

            if (currentIndex < 0)
                currentIndex = length - 1;      // wrap to last
            else if (currentIndex >= length)
                currentIndex = 0;            // wrap to first
        }
    }

    private void HandlePlacement()
    {
        if (Input.GetKeyDown(KeyCode.Space) && gm.ColumnHasSpace(currentIndex))
            gm.PlacePiece(currentIndex);
    }

    private void UpdateHoverColor()
    {
        Material mat = ui.GetCurrentPlayerMaterial();
        hoverPiece.GetComponentInChildren<Renderer>().material = mat;
    }
}
