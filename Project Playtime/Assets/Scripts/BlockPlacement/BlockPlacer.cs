using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Tilemaps;

public class BlockPlacer : MonoBehaviour
{
    public SpriteRenderer placementCursor;
    public Color validColor, invalidColor;

    public Tile tile;

    //How far away from the player blocks can be placed (in grid cells)
    public int placementDistance = 3;

    private Tilemap m_tilemap;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //Get tilemap for player block placement
        foreach (var map in FindObjectsByType<Tilemap>(FindObjectsSortMode.None))
        {
            if (map.tag == "PlayerTilemap")
            {
                m_tilemap = map;
                break;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        //Get selected grid cell
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3Int cell = m_tilemap.WorldToCell(mouseWorldPos);

        DrawCursor(cell);

        if (IsTileWithinPlacementDistance(cell)) ;
        {
            if (Input.GetMouseButtonDown(0))
            {
                PlaceTile(cell, tile);
            }

            if (Input.GetMouseButtonDown(1))
            {
                RemoveTile(cell);
            }
        }
    }

    private void DrawCursor(Vector3Int cell)
    {
        //Set cursor position
        placementCursor.transform.position = m_tilemap.GetCellCenterWorld(cell);
        placementCursor.transform.localScale = m_tilemap.cellSize;

        //Set cursor colour
        placementCursor.color = IsTileWithinPlacementDistance(cell) ? validColor : invalidColor;
    }

    private bool IsTileWithinPlacementDistance(Vector3Int cell)
    {
        Vector3Int playerCell = m_tilemap.WorldToCell(transform.position);
        int xDist = Mathf.Abs(playerCell.x - cell.x);
        int yDist = Mathf.Abs(playerCell.y - cell.y);
        return xDist <= placementDistance && yDist <= placementDistance;
    }

    private void PlaceTile(Vector3Int cell, Tile tile)
    {
        m_tilemap.SetTile(cell, tile);
    }

    private void RemoveTile(Vector3Int cell)
    {
        m_tilemap.SetTile(cell, Tile.CreateInstance<Tile>());
    }
}
