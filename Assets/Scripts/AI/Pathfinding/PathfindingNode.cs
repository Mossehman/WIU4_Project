using System.Collections.Generic;
using System.Linq.Expressions;
using UnityEngine;

/// <summary>
/// Represents a point in world space that our entities can use for AStar pathfinding
/// </summary>
[System.Serializable]
public class PathfindingNode
{
    public Vector3 position; // node position
    public bool passable = true;
    public List<Vector3Int> neighbouringNodeIndexes = new List<Vector3Int>(); // the surrounding nodes that connect to this current node, should be 18 points (we allow for diagonal movement)

    // weighting values for determining if the path is the most efficient route
    public float localGoal = 0.0f;
    public float globalGoal = 0.0f;

    public bool hasChecked = false; // prevents us from re-checking the same node (this will lead to a stack overflow if not present {not the helpful one!!})

    public PathfindingNode parent; // a reference to the next node that this node leads to in the path (assuming that this pathnode is part of an entities' path)

    public float weight; // an additional weighting value that affects the overall efficiency of the path (allows for
}


/// <summary>
/// Class to read AStar node data from the compute buffer
/// </summary>
[System.Serializable]
public struct AStarComputeNode
{
    public Vector3Int voxelIndex; // we only need to get the index, can calculate the positional coordinates on the CPU-side (this REDUCES the risk of overworking our GPU)
    public int passable;          // we also need to determine whether the cell is passable in the compute shader (since that's where the mesh is being generated)

    public AStarComputeNode(int passable = 1)
    {
        voxelIndex = new Vector3Int(1, 1, 1);
        this.passable = passable;
    }
}