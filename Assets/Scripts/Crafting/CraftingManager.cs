using Player.Inventory;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CraftingManager : MonoBehaviour
{
    [Header("Crafting Logic")]
    [SerializeField] private PlayerInventory _playerInventory;

    [Header("Crafting UI")]
    [SerializeField] private GameObject _craftingUI;
    [SerializeField] private GameObject _catalogPanel;
    [SerializeField] private GameObject _recipePrefab;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
