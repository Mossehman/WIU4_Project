using Player.Inventory;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Player.Inventory
{
    public class Storage : MonoBehaviour
    {
        [Header("Storage Logic")]
        [SerializeField]            private GameObject[]    _storageItems;
        [SerializeField]            private int             _maxItems;
        [SerializeField]            private SortingType     _currentSort;
        [SerializeField]            private GameObject      _currentSelected;

        [Header("Storage UI")]
        [SerializeField]            private GameObject      _storagePanel;
        [SerializeField]            private GameObject      _storageSlotPrefab;
        [SerializeField]            private GameObject      _itemPrefab;
                                    private GameObject[]    _storageSlots;

        void Start()
        {
            _storageItems = new GameObject[_maxItems];
            _storageSlots = new GameObject[_maxItems];
        }

        void Update()
        {
        
        }

        public void RenderStorage()
        {
            for (int i = 0; i < _maxItems; i++)
            {
                GameObject slot = Instantiate(_storageSlotPrefab, _storagePanel.transform);
                slot.tag = "Storage";
            }
            for (int i = 0; i <_storageItems.Length; i++)
            {
                GameObject item = Instantiate(_itemPrefab, _storageSlots[i].transform);
            }
        }    
    }
}