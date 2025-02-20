using Player.Inventory;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
using UnityEngine;
using System.Linq;
using System;

namespace Player.Inventory
{
    public class Storage : MonoBehaviour
    {
        [Header("Storage Logic")]
        [SerializeField]            public GameObject[]     _storageItems;
        [SerializeField]            public int              _maxItems = 10;
        [SerializeField]            private SortingType     _currentSort;

        [Header("Storage UI")]
        [SerializeField]            private GameObject      _storage;
        [SerializeField]            private GameObject      _storagePanel;
        [SerializeField]            private GameObject      _storageSlotPrefab;
        [SerializeField]            private GameObject      _itemPrefab;
                                    private GameObject[]    _storageSlots;

        void Start()
        {
            _storageItems = new GameObject[_maxItems];
            _storageSlots = new GameObject[_maxItems];

            RenderStorage();
        }

        void Update()
        {
            for (int i = 0; i < _storageItems.Length; i++)
            {
                if (_storageItems[i] != null)
                {
                    ItemModelScript item = _storageItems[i].GetComponent<ItemModelScript>();

                    if (item == null) continue;

                    string itemID = item.getSO().getID();

                    Transform slot = _storageSlots[i].transform;

                    if (slot.childCount > 0)
                    {
                        GameObject slotItem = slot.GetChild(0).gameObject;

                        ItemModelScript childItemModel = slotItem.GetComponent<ItemModelScript>();

                        if (childItemModel != null && childItemModel.getSO().getID() == itemID)
                        {
                            slotItem.transform.Find("Quantity").GetComponent<TextMeshProUGUI>().text = item.getSO()._quantity.ToString();
                        }
                    }
                }
            }
        }

        public void AddStorageItem(GameObject newItem)
        {
            BaseItem temp = newItem.GetComponent<ItemModelScript>().getSO();
            foreach (var item in _storageItems)
            {
                if (item != null)
                {
                    if (item.GetComponent<ItemModelScript>().getSO().getID() == temp.getID())
                    {
                        item.GetComponent<ItemModelScript>().getSO()._quantity++;
                        return;
                    }
                }
            }
            for (int i = 0; i < _maxItems; i++)
            {
                if (_storageItems[i] == null)
                {
                    _storageItems[i] = newItem;
                    return;
                }
            }
        }

        public void RenderStorage()
        {
            foreach (Transform child in _storagePanel.transform)
            {
                Destroy(child.gameObject);
            }

            GameObject[] tempStorage = SortStorage(_currentSort);

            for (int i = 0; i < _maxItems; i++)
            {
                GameObject slot = Instantiate(_storageSlotPrefab, _storagePanel.transform);
                slot.tag = "Storage";
            }
            for (int i = 0; i <_storageItems.Length; i++)
            {
                GameObject item = Instantiate(_itemPrefab, _storageSlots[i].transform);
                item.transform.Find("Name").GetComponent<TextMeshProUGUI>().text = tempStorage[i].GetComponent<ItemModelScript>().getSO().getDisplayName();
                item.transform.Find("Quantity").GetComponent<TextMeshProUGUI>().text = tempStorage[i].GetComponent<ItemModelScript>().getSO()._quantity.ToString();
                item.GetComponent<Image>().sprite = tempStorage[i].GetComponent<ItemModelScript>().getSO().getItemIcon();
            }
        }

        public void SwitchSort()
        {
            int newSort = ((int)_currentSort + 1) % ((int)SortingType.TOTAL);
            _currentSort = (SortingType)newSort;
        }

        private GameObject[] SortStorage(SortingType type)
        {
            GameObject[] temp = _storageItems.ToArray();

            switch (type)
            {
                case SortingType.DATE_ADDED:
                    return temp;

                case SortingType.ALPHABETICAL:
                    Array.Sort(temp, (a, b) =>
                        a.name.CompareTo(b.name));
                    break;

                case SortingType.HEAVIEST:
                    Array.Sort(temp, (a, b) =>
                        b.GetComponent<ItemModelScript>().getSO().getWeight().CompareTo(a.GetComponent<ItemModelScript>().getSO().getWeight()));
                    break;

                case SortingType.LIGHTEST:
                    Array.Sort(temp, (a, b) =>
                        a.GetComponent<ItemModelScript>().getSO().getWeight().CompareTo(b.GetComponent<ItemModelScript>().getSO().getWeight()));
                    break;

                case SortingType.QUANTITY:
                    Array.Sort(temp, (a, b) =>
                        b.GetComponent<ItemModelScript>().getSO()._quantity.CompareTo(a.GetComponent<ItemModelScript>().getSO()._quantity));
                    break;
            }

            return temp;
        }

        public void OnStorageOpen()
        {
            if (_storage.active == false)
            {
                _storage.SetActive(true);
                RenderStorage();
            }
            else { _storage.SetActive(false); }
        }
    }
}