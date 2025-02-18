using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using TMPro;
using UnityEngine;
using static UnityEditor.Progress;

namespace Player.Inventory
{
    enum SortingType
    {
        DATE_ADDED,
        ALPHABETICAL,
        HEAVIEST,
        LIGHTEST,
        QUANTITY,
    }

    public class PlayerInventory : MonoBehaviour
    {
        [Header("Inventory Logic")]
        [SerializeField] private List<BaseItem> _items;
        [SerializeField] public float _baseWeight = 0.0f;
        [SerializeField] public float _baseMaxWeight = 10.0f;
        [SerializeField] private float _currentWeight;
        [SerializeField] private SortingType _currentSort = SortingType.DATE_ADDED;

        [Header("Inventory UI")]
        [SerializeField] private GameObject _inventoryPanel;
        [SerializeField] private GameObject _itemPrefab;
        [SerializeField] private GameObject _itemDescPanel;

        void Start()
        {
            //_items = new List<BaseItem>();
            _currentWeight = _baseWeight;

            RenderInventory();
        }

        void Update()
        {
            foreach (var item in _items)
            {
                //_currentWeight += item.weight;
            }
        }

        public void AddItem(BaseItem newItem)
        {
            foreach (var item in _items)
            {
                //if (item.itemID == newItem.itemID)
                //{
                //    item.quantity += newItem.quantity;
                //    _currentWeight += newItem.weight * newItem.quantity;
                //    return;
                //}
            }

            _items.Add(newItem);
            //_currentWeight += newItem.weight * newItem.quantity;
        }


        public void RenderInventory()
        {
            foreach (Transform child in _inventoryPanel.transform)
            {
                Destroy(child.gameObject);
            }

            List<BaseItem> temp = SortInventory(_currentSort);

            foreach (var item in temp)
            {
                GameObject itemUI = Instantiate(_itemPrefab, _inventoryPanel.transform);
                itemUI.transform.Find("Name").GetComponent<TextMeshProUGUI>().text = item.getDisplayName();
                //itemUI.transform.Find("Qauntity").GetComponentInChildren<TextMeshProUGUI>().text = item.qauntity;
            }
        }


        private List<BaseItem> SortInventory(SortingType type)
        {
            List <BaseItem> temp = _items;

            switch (type)
            {
                case SortingType.DATE_ADDED:
                    return temp; // No sorting needed

                case SortingType.ALPHABETICAL:
                    temp.Sort((a, b) => a.name.CompareTo(b.name));
                    break;

                case SortingType.HEAVIEST:
                    //temp.Sort((a, b) => b.weight.CompareTo(a.weight));
                    break;

                case SortingType.LIGHTEST:
                    //temp.Sort((a, b) => a.weight.CompareTo(b.weight));
                    break;

                case SortingType.QUANTITY:
                    //temp.Sort((a, b) => b.quantity.CompareTo(a.quantity));
                    break;
            }

            return temp;
        }
        public void LockItem(int itemID)
        {
            //_items[itemID].isLocked = true;
        }
        public void UnlockItem(int itemID)
        {
            //_items[itemID].isLocked = false;
        }

        public void SwitchSort()
        {

        }
    }
}
