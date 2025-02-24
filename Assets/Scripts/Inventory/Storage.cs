using TMPro;
using UnityEngine.UI;
using UnityEngine;
using System.Linq;
using System;
using Unity.VisualScripting;

namespace Player.Inventory
{
    public class Storage : MonoBehaviour
    {
        [Header("Storage Logic")]
        [SerializeField]            public BaseItem[]       _storageItems;
        [SerializeField]            public int              _maxItems = 10;
        [SerializeField]            private SortingType     _currentSort;

        [Header("Storage UI")]
        [SerializeField]            private GameObject      _storage;
        [SerializeField]            private GameObject      _storagePanel;
        [SerializeField]            private GameObject      _storageSlotPrefab;
        [SerializeField]            private GameObject       _itemPrefab;
                                    private GameObject[]    _storageSlots;

        void Start()
        {
            _storageItems = new BaseItem[_maxItems];
            _storageSlots = new GameObject[_maxItems];

            RenderStorage();
        }

        void Update()
        {
            
        }

        public void AddStorageItem(BaseItem newItem)
        {
            BaseItem temp = newItem.GetComponent<ItemModelScript>().getSO();
            foreach (var item in _storageItems)
            {
                if (item != null)
                {
                    if (item.getID() == temp.getID())
                    {
                        item._quantity++;
                        RenderStorage();
                        return;
                    }
                }
            }
            for (int i = 0; i < _maxItems; i++)
            {
                if (_storageItems[i] == null)
                {
                    _storageItems[i] = newItem;
                    RenderStorage();
                    return;
                }
            }
        }

        [ContextMenu("Render Storage")]
        public void RenderStorage()
        {
            // Destroy any children inside the container
            foreach (Transform child in _storagePanel.transform)
            {
                Destroy(child.gameObject);
            }

            // Sort the storage according to the current sort
            BaseItem[] tempStorage = SortStorage(_currentSort);

            // Instantiate Storage slots
            for (int i = 0; i < _maxItems; i++)
            {
                GameObject slot = Instantiate(_storageSlotPrefab, _storagePanel.transform);
                slot.tag = "Storage";
                _storageSlots[i] = slot;
            }
            
            // Add items inside storage container to slots
            for (int i = 0; i <_storageItems.Length; i++)
            {
                if (_storageItems[i] != null)
                {
                    GameObject item = Instantiate(_itemPrefab, _storageSlots[i].transform);
                    item.GetComponent<Draggable>()._item = item.GetComponent<ItemModelScript>().getSO();
                    item.transform.Find("Name").GetComponent<TextMeshProUGUI>().text = tempStorage[i].getDisplayName();
                    item.transform.Find("Quantity").GetComponent<TextMeshProUGUI>().text = tempStorage[i]._quantity.ToString();
                    item.GetComponent<Image>().sprite = tempStorage[i].getItemIcon();
                }
            }
        }

        public void SwitchSort()
        {
            int newSort = ((int)_currentSort + 1) % ((int)SortingType.TOTAL);
            _currentSort = (SortingType)newSort;
            RenderStorage();
        }

        private BaseItem[] SortStorage(SortingType type)
        {
            BaseItem[] temp = _storageItems.Where(item => item != null).ToArray();

            if (temp != null)
            {
                switch (type)
                {
                    case SortingType.DATE_ADDED:
                        return temp;

                    case SortingType.ALPHABETICAL:
                        Array.Sort(temp, (a, b) =>
                            a.getID().CompareTo(b.getID()));
                        break;

                    case SortingType.HEAVIEST:
                        Array.Sort(temp, (a, b) =>
                            b.getWeight().CompareTo(a.getWeight()));
                        break;

                    case SortingType.LIGHTEST:
                        Array.Sort(temp, (a, b) =>
                            a.getWeight().CompareTo(b.getWeight()));
                        break;

                    case SortingType.QUANTITY:
                        Array.Sort(temp, (a, b) =>
                            b._quantity.CompareTo(a._quantity));
                        break;
                }
            }
            return temp;
        }

        public void OnStorageOpen()
        {
            if (_storage.activeInHierarchy == false)
            {
                _storage.SetActive(true);
                RenderStorage();
            }
            else { _storage.SetActive(false); }
        }
    }
}