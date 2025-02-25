using UnityEngine;

[CreateAssetMenu(fileName = "Entity Spawn Data", menuName = "AI/Spawn Data")]
public sealed class EntitySpawnData : ScriptableObject
{
    public GameObject entityPrefab;
    public uint minSpawn = 1;
    public uint maxSpawn = 2;

    
}
