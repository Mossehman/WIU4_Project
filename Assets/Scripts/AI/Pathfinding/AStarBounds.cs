using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Helper class for generating our pathfinding nodes within the entire map (separated to make my code less of a mess than it already is)
/// </summary>
[RequireComponent(typeof(MarchingCubesGenerator))]
public class AStarBounds : MonoBehaviour
{
    public Dictionary<Vector3Int, PathfindingNode> nodes = new Dictionary<Vector3Int, PathfindingNode>();
    // The range from the player where the chunks will generate with the AStar algorithm
    public uint AStarRange = 2; 
    
    /// <summary>
    /// Creates a path
    /// </summary>
    /// <param name="startIndex"></param>
    /// <param name="endIndex"></param>
    /// <returns></returns>
    public List<Vector3> Pathfind(Vector3Int startIndex, Vector3Int endIndex)
    {
        ///TODO: Try multithreading this code
        Debug.Log("Pathfinding start!");
        List<Vector3> path = new List<Vector3>();

        PathfindingNode startNode;
        PathfindingNode endNode;

        nodes.TryGetValue(startIndex, out startNode);
        nodes.TryGetValue(endIndex, out endNode);


        // we are attempting to access a node out of bounds
        if (startNode == null || endNode == null)
        {
            Debug.Log("Start/End node was null!!");
            return path;
        }

        // reset our nodes for pathfinding
        foreach (var node in nodes)
        {
            PathfindingNode nodeToReset = node.Value;
            nodeToReset.hasChecked = false;
            nodeToReset.globalGoal = float.MaxValue;
            nodeToReset.localGoal = float.MaxValue;
            nodeToReset.parent = null;
        }

        PathfindingNode currentNode = startNode; //keep track of which node we are checking 

        startNode.localGoal = 0.0f;
        startNode.globalGoal = Heuristic(startNode, endNode, startNode.weight);

        List<PathfindingNode> uncheckedNodes = new List<PathfindingNode> { startNode };

        while (uncheckedNodes.Count > 0 && currentNode != endNode)
        {
            uncheckedNodes.Sort((lhs, rhs) => lhs.globalGoal.CompareTo(rhs.globalGoal));

            // if we have already checked through the current top of the list, we backtrack
            while (uncheckedNodes.Count > 0 && uncheckedNodes[0].hasChecked) { uncheckedNodes.RemoveAt(0); }
            if (uncheckedNodes.Count <= 0) break; //assuming all possible paths have been checked, we exit the loop (no path found)

            currentNode = uncheckedNodes[0];
            currentNode.hasChecked = true;

            foreach (var neighbouringNodeIndex in currentNode.neighbouringNodeIndexes)
            {
                PathfindingNode neighbouringNode;
                nodes.TryGetValue(neighbouringNodeIndex, out neighbouringNode); // check if the path's neighbour exists, if it does not/is impassable, skip over it

                if (neighbouringNode == null) { continue; }
                
                float lowerGoalCheck = currentNode.localGoal + Heuristic(currentNode, neighbouringNode, neighbouringNode.weight); // check the node weightage and determine it's cost
                if (lowerGoalCheck < neighbouringNode.localGoal)
                {
                    neighbouringNode.parent = currentNode;
                    neighbouringNode.localGoal = lowerGoalCheck;
                    // Typically, globalGoal = localGoal + heuristic
                    neighbouringNode.globalGoal = neighbouringNode.localGoal + Heuristic(neighbouringNode, endNode, neighbouringNode.weight);

                    if (!uncheckedNodes.Contains(neighbouringNode))
                    {
                        uncheckedNodes.Add(neighbouringNode);
                    }
                }
            }
        }

        PathfindingNode currentWaypointNode = endNode;
        //populate the path data list with all the positional data (note: this goes from end to start, would reverse but kinda lazy, js do a i-- for loop lmao)
        while (currentWaypointNode != null && currentWaypointNode.parent != null)
        {
            path.Add(currentWaypointNode.position);
            currentWaypointNode = currentWaypointNode.parent;
        }

        return path;
    }

    /// <summary>
    /// Get the squared distance between 2 nodes
    /// </summary>
    /// <param name="a">Node to travel from</param>
    /// <param name="b">Node to travel to</param>
    /// <returns></returns>
    private float DistanceBetweenNodes(PathfindingNode a, PathfindingNode b)
    {
        return ((a.position.x - b.position.x) * (a.position.x - b.position.x) + (a.position.y - b.position.y) * (a.position.y - b.position.y));
    }

    /// <summary>
    /// Calculate the overall cost for travelling between 2 nodes
    /// </summary>
    /// <param name="a">Node to travel from</param>
    /// <param name="b">Node to travel to</param>
    /// <param name="weightingValue">An additional weight value to allow for more favorable paths</param>
    /// <returns></returns>
    private float Heuristic(PathfindingNode a, PathfindingNode b, float weightingValue = 1.0f)
    {
        return DistanceBetweenNodes(a, b) * weightingValue * weightingValue;
    }

    /// <summary>
    /// Function to generate a new AStar node at a given point in the map
    /// </summary>
    /// <param name="index">The positional index of the node that we can reference</param>
    /// <param name="position">The world space position of this node</param>
    /// <param name="isPassable">The passability of this node</param>
    /// <param name="weight">An additional weighting value for this node if needed</param>
    public void GenerateNode(Vector3Int index, Vector3 position, bool isPassable, float weight = 1.0f)
    {
        // initialise a new pathfinding node object
        PathfindingNode newNode = new PathfindingNode();
        newNode.position = position;
        newNode.weight = weight;
        newNode.passable = isPassable;

        // initialise the neighbouring nodes for checking
        // since we're using a dictionary and TryGet (plus an infinite map), we can blindly add all possible connections regardless if they actually exist 
        // (this looks so bloody ugly :skull:)
        newNode.neighbouringNodeIndexes.Add(new Vector3Int(index.x + 1, index.y, index.z)); // left node
        newNode.neighbouringNodeIndexes.Add(new Vector3Int(index.x - 1, index.y, index.z)); // right node

        newNode.neighbouringNodeIndexes.Add(new Vector3Int(index.x, index.y, index.z + 1)); // front node
        newNode.neighbouringNodeIndexes.Add(new Vector3Int(index.x, index.y, index.z - 1)); // back node

        newNode.neighbouringNodeIndexes.Add(new Vector3Int(index.x, index.y + 1, index.z)); // top node
        newNode.neighbouringNodeIndexes.Add(new Vector3Int(index.x, index.y - 1, index.z)); // bottom node


        newNode.neighbouringNodeIndexes.Add(new Vector3Int(index.x + 1, index.y, index.z + 1)); // front-right node
        newNode.neighbouringNodeIndexes.Add(new Vector3Int(index.x - 1, index.y, index.z + 1)); // front-left node

        newNode.neighbouringNodeIndexes.Add(new Vector3Int(index.x + 1, index.y, index.z - 1)); // back-right node
        newNode.neighbouringNodeIndexes.Add(new Vector3Int(index.x - 1, index.y, index.z - 1)); // back-left node




        newNode.neighbouringNodeIndexes.Add(new Vector3Int(index.x + 1, index.y + 1, index.z + 1)); // front-top-right node
        newNode.neighbouringNodeIndexes.Add(new Vector3Int(index.x - 1, index.y + 1, index.z + 1)); // front-top-left node

        newNode.neighbouringNodeIndexes.Add(new Vector3Int(index.x + 1, index.y + 1, index.z - 1)); // back-top-right node
        newNode.neighbouringNodeIndexes.Add(new Vector3Int(index.x - 1, index.y + 1, index.z - 1)); // back-top-left node

        newNode.neighbouringNodeIndexes.Add(new Vector3Int(index.x + 1, index.y + 1, index.z)); // top-right node
        newNode.neighbouringNodeIndexes.Add(new Vector3Int(index.x - 1, index.y + 1, index.z)); // top-left node

        newNode.neighbouringNodeIndexes.Add(new Vector3Int(index.x, index.y + 1, index.z + 1)); // top-front node
        newNode.neighbouringNodeIndexes.Add(new Vector3Int(index.x, index.y + 1, index.z - 1)); // top-back node




        newNode.neighbouringNodeIndexes.Add(new Vector3Int(index.x + 1, index.y - 1, index.z + 1)); // front-bottom-right node
        newNode.neighbouringNodeIndexes.Add(new Vector3Int(index.x - 1, index.y - 1, index.z + 1)); // front-bottom-left node
        
        newNode.neighbouringNodeIndexes.Add(new Vector3Int(index.x + 1, index.y - 1, index.z - 1)); // back-bottom-right node
        newNode.neighbouringNodeIndexes.Add(new Vector3Int(index.x - 1, index.y - 1, index.z - 1)); // back-bottom-left node

        newNode.neighbouringNodeIndexes.Add(new Vector3Int(index.x + 1, index.y - 1, index.z)); // bottom-right node
        newNode.neighbouringNodeIndexes.Add(new Vector3Int(index.x - 1, index.y - 1, index.z)); // bottom-left node

        newNode.neighbouringNodeIndexes.Add(new Vector3Int(index.x, index.y - 1, index.z + 1)); // bottom-front node
        newNode.neighbouringNodeIndexes.Add(new Vector3Int(index.x, index.y - 1, index.z - 1)); // bottom-back node


        nodes.TryAdd(index, newNode); // add our pathfinding node to the list of existing nodes
    }

    //private void OnDrawGizmos()
    //{
    //    if (nodes.ContainsKey(new Vector3Int(-3, 43, 0)))
    //    {
    //        Gizmos.color = Color.red;
    //        Gizmos.DrawCube(nodes[new Vector3Int(-3, 43, 0)].position, new Vector3(0.5f, 0.5f, 0.5f));
    //    }
    //}
}
