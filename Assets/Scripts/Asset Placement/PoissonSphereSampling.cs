using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Helper class for allowing us to sample points on a grid to see if an object has already occupied a location
/// </summary>
public class PoissonSphereSampling : MonoBehaviour
{
    public static PoissonSphereSampling instance { get; private set; }

    public Vector3 cellDimensions = Vector3.one; // allows us to define the grid cell size
    public bool drawCells = false;  // drawing cells for debugging
    private HashSet<Vector3Int> occupiedGridSpaces = new HashSet<Vector3Int>(); // maintain a list of all the occupied grid cells
    public Dictionary<Vector3Int, List<Vector3Int>> gridSpacesInChunk = new Dictionary<Vector3Int, List<Vector3Int>>(); // maintain a list of all points in a grid to it's respective chunk (when chunk unloaded, remove grid spaces)

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            DestroyImmediate(gameObject);
        }
    }

    public Vector3Int PositionToCellIndex(Vector3 position)
    {
        return new Vector3Int(
            Mathf.FloorToInt(position.x / cellDimensions.x),
            Mathf.FloorToInt(position.y / cellDimensions.y),
            Mathf.FloorToInt(position.z / cellDimensions.z)
        );
    }

    public Vector3 CellIndexToPosition(Vector3Int cellIndex)
    {
        return new Vector3(
            cellIndex.x * cellDimensions.x,
            cellIndex.y * cellDimensions.y,
            cellIndex.z * cellDimensions.z
        );
    }

    public bool AddDiscSample(Vector3 center, float radius)
    {
        Vector3Int minCell = PositionToCellIndex(center - Vector3.one * radius);
        Vector3Int maxCell = PositionToCellIndex(center + Vector3.one * radius);
        List<Vector3Int> cellSpaces = new List<Vector3Int>();

        for (int x = minCell.x; x <= maxCell.x; x++)
        {
            for (int y = minCell.y; y <= maxCell.y; y++)
            {
                for (int z = minCell.z; z <= maxCell.z; z++)
                {
                    Vector3Int cell = new Vector3Int(x, y, z);
                    
                    if (occupiedGridSpaces.Contains(cell)) { return false; }
                    cellSpaces.Add(cell);
                }
            }
        }

        foreach (Vector3Int cellSpace in cellSpaces) { occupiedGridSpaces.Add(cellSpace); }
        cellSpaces.Clear();
        return true;
    }

    private void OnDrawGizmos()
    {
        if (!drawCells) return;


    }
}
