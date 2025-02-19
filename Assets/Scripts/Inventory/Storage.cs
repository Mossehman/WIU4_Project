using Player.Inventory;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Storage : MonoBehaviour
{
    [Header("Storage Logic")]
    [SerializeField] private GameObject[] _storageItems;
    [SerializeField] private int _maxItems;
    [SerializeField] private SortingType _currentSort;
    [SerializeField] private GameObject _currentSelected;
    [SerializeField] private SlotStatus _slotStatus;

    [Header("Storage UI")]
    [SerializeField] private GameObject _storagePanel;
    [SerializeField] private GameObject _storageSlot;
    [SerializeField] private GameObject _itemPrefab;

    void Start()
    {
        
    }

    void Update()
    {
        
    }
}
