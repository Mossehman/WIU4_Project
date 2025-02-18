using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using TMPro;
using UnityEngine.UI;
using UnityEditorInternal.Profiling.Memory.Experimental;
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
        TOTAL
    }

    public class PlayerInventory : MonoBehaviour
    {
        [Header("Inventory Logic")]
        [SerializeField]    private List<BaseItem>      _items;
        [SerializeField]    public float                _baseWeight = 0.0f;
        [SerializeField]    public float                _baseMaxWeight = 10.0f;
        [SerializeField]    private float               _currentWeight;
        [SerializeField]    private SortingType         _currentSort = SortingType.DATE_ADDED;
                            private BaseItem            _currentlySelected;
                            private List<bool>          _isLocked;

        [Header("Inventory UI")]
        [SerializeField]    private GameObject          _inventoryPanel;
        [SerializeField]    private GameObject          _itemPrefab;
        [SerializeField]    private GameObject          _itemDescPanel;

        void Start()
        {
            //_items = new List<BaseItem>();
            foreach (var item in _items) { _isLocked.Add(false); }
            _currentWeight = _baseWeight;

            RenderInventory();
        }

        void Update()
        {
            foreach (var item in _items)
            {
                _currentWeight += item.getWeight();
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
            _currentWeight += newItem.getWeight() * newItem._quantity;
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
                itemUI.GetComponent<Button>().onClick.AddListener( () => ShowItem(item) );
                itemUI.transform.Find("Qauntity").GetComponentInChildren<TextMeshProUGUI>().text = item._quantity.ToString();
            }
        }

        public void ShowItem(BaseItem item)
        {
            _currentlySelected = item;
            _itemDescPanel.transform.Find("Title").GetComponent<TextMeshProUGUI>().text = item.getDisplayName();
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
                    temp.Sort((a, b) => b.getWeight().CompareTo(a.getWeight()));
                    break;

                case SortingType.LIGHTEST:
                    temp.Sort((a, b) => a.getWeight().CompareTo(b.getWeight()));
                    break;

                case SortingType.QUANTITY:
                    temp.Sort((a, b) => b._quantity.CompareTo(a._quantity));
                    break;
            }

            return temp;
        }
        public void LockItem()
        {
            //if (_currentlySelected != null)
            //{
            //    int index = -1;
            //    foreach (BaseItem item in _items)
            //    {
            //        // find item id
            //        if (item.itemID == _currentlySelected.itemID) { break; }
            //        else { index++; }
            //    }
            //    if (_items[index].isLocked == true) { _items[index].isLocked = false; }
            //    else if (_items[index].isLocked == false) { _items[index].isLocked = true; }
            //}
            if (_currentlySelected != null)
            {
                int index = -1;
                Debug.Log("Locked");
            }
        }

        public void SwitchSort()
        {
            int newSort = ((int) _currentSort + 1) % ((int) SortingType.TOTAL);
            _currentSort = (SortingType) newSort;
        }
    }
}
