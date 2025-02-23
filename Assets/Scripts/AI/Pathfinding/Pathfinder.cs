using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Script for providing a list of pathfinding data for AI traversal
/// </summary
public class Pathfinder : MonoBehaviour
{

    private AStarBounds bounds;                          // reference to the AStar manager class to get the global AStar nodes
    public List<Vector3> pathData = new List<Vector3>(); // the list of nodes for traversal (NOTE: Goes from end -> start, so the for-loop should be i-- and start from pathData.count - 1
    [SerializeField] private LayerMask terrainLayerMask; // the terrain layer (for finding the nearest node)
    [SerializeField] private bool drawPath;              // helper tool to draw our path
    [SerializeField] private Transform startPosition;    // additional transform we can set to offset our Start position when performing AStar (will auto set to the transform component if left null)

    private void Start()
    {
        if (MarchingCubesGenerator.instance == null) {
            Debug.LogWarning("No terrain found! Did you create a MarchingCubesGenerator object?");
            return;
        }

        bounds = MarchingCubesGenerator.instance.GetAStar();
        if (bounds == null)
        {
            Debug.LogWarning("No AStar generator found! Did you add a AStarBounds script to the terrain generator?");
            return;
        }

        if (startPosition == null)
        {
            startPosition = transform;
        }

    }

    /// <summary>
    /// Function for populating the pathData with the AStar node positions
    /// </summary>
    /// <param name="endPosition">The destination we want to travel to</param>
    /// <param name="upwardsRaycast">Whether we should only check downwards OR both downwards and upwards for the nearest node to the endPosition</param>
    public void GeneratePath(Vector3 endPosition, bool upwardsRaycast = true)
    {

        //if (!characterController.isGrounded) { Debug.Log("Character controller isn't grounded!!!"); return; }

        pathData.Clear();

        if (bounds == null)
        {
            Debug.LogWarning("No AStar generator found! Did you add a AStarBounds script to the terrain generator?");
            pathData.Add(endPosition);  
            return;
        }

        Vector3 upHitPoint = Vector3.zero;
        Vector3 downHitPoint = Vector3.zero;

        float upwardsDistance = float.MaxValue;
        float downwardsDistance = float.MaxValue;
        bool hitSomething = false;

        RaycastHit hitDown;
        if (Physics.Raycast(endPosition, Vector3.down, out hitDown, 1000.0f, terrainLayerMask))
        {
            downwardsDistance = endPosition.y - hitDown.point.y;
            downHitPoint = hitDown.point;
            hitSomething = true;


        }

        if (upwardsRaycast)
        {
            RaycastHit hitUp;
            if (Physics.Raycast(endPosition, Vector3.up, out hitUp, 1000.0f, terrainLayerMask))
            {
                upwardsDistance = hitUp.point.y - endPosition.y;
                upHitPoint = hitUp.point;
                hitSomething = true;
            }
        }

        // assuming we are out of bounds from the terrain AStar nodes, just simply move towards the general direction of the end point
        if (!hitSomething)
        {
            pathData.Add(endPosition);
            return;
        }

        RaycastHit pos;
        Vector3Int startIndex;
        if (Physics.Raycast(startPosition.position, Vector3.down, out pos, terrainLayerMask))
        {
            startIndex = MarchingCubesGenerator.instance.PositionToGlobalVoxelIndex(pos.point);
        }
        else
        {
            startIndex = MarchingCubesGenerator.instance.PositionToGlobalVoxelIndex(startPosition.position);
        }


        Vector3Int endIndex;

        if (upwardsDistance < downwardsDistance)
        {
            endIndex = MarchingCubesGenerator.instance.PositionToGlobalVoxelIndex(upHitPoint);
        }
        else
        {
            endIndex = MarchingCubesGenerator.instance.PositionToGlobalVoxelIndex(downHitPoint);
        }

        endIndex.y -= 1;
        startIndex.y -= 1;

        List<Vector3> path = bounds.Pathfind(startIndex, endIndex);
        if (path.Count == 0)
        {
            pathData.Add(endPosition);
            return;
        }

        pathData = path;

    }

    private void OnDrawGizmos()
    {
        if (!drawPath) { return; }
        Gizmos.color = Color.yellow;
        for (int i = pathData.Count - 1; i > 0; i--)
        {
            Gizmos.DrawLine(pathData[i], pathData[i - 1]);
        }
    }
}
