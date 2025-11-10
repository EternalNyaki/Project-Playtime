using UnityEngine;

public class BlockPlacer : MonoBehaviour
{
    public Grid grid;

    public Transform placementCursor;

    //How far away from the player blocks can be placed (in grid cells)
    public int placementDistance = 3;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (grid == null)
        {
            grid = FindAnyObjectByType<Grid>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3Int cell = grid.WorldToCell(mouseWorldPos);

        placementCursor.position = grid.GetCellCenterWorld(cell);
        placementCursor.localScale = grid.cellSize;
    }
}
