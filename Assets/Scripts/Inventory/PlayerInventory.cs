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
        [SerializeField]    private List<BaseItem>      _inventoryItems;
        [SerializeField]    public float                _baseWeight = 0.0f;
        [SerializeField]    public float                _baseMaxWeight = 10.0f;
        [SerializeField]    private float               _currentWeight;
        // SORTING
        [SerializeField]    private SortingType         _currentSort = SortingType.DATE_ADDED;
        // LOCKING ITEMS
                            private BaseItem            _currentlySelected;
                            private List<bool>          _isLocked;

        [Header("Inventory UI")]
        [SerializeField]    private GameObject          _inventory;
        [SerializeField]    private GameObject          _inventoryPanel;
        [SerializeField]    private GameObject          _itemPrefab;
        [SerializeField]    private GameObject          _itemDescPanel;

        [Header("Hotbar Logic")]
        [SerializeField]    private BaseItem[]          _hotbarItems;
                            private int                 _maxHotbarItems = 5;

        [Header("Hotbar UI")]
        [SerializeField]    private GameObject          _hotBarPanel;
        [SerializeField]    private GameObject          _hotbarItemPrefab;

        void Start()
        {
            //_items = new List<BaseItem>();
            _isLocked = new List<bool>();

            _hotbarItems = new BaseItem[_maxHotbarItems];

            if (_inventoryItems != null)
            {
                foreach (var item in _inventoryItems) { _isLocked.Add(false); }
            }

            for (int i = 0; i < _maxHotbarItems; i++)
            {
                GameObject hotbarItem =  Instantiate(_hotbarItemPrefab, _hotBarPanel.transform);
                if (_hotbarItems[i] != null)
                {
                    hotbarItem.transform.Find("Quantity").GetComponentInChildren<TextMeshProUGUI>().text = (_hotbarItems[i]._quantity.ToString());
                }
            }

            _currentWeight = _baseWeight;
        }

        void Update()
        {
            
        }

        public void AddItem(BaseItem newItem)
        {
            foreach (var item in _inventoryItems)
            {
                if (item.getID() == newItem.getID())
                {
                    item._quantity += newItem._quantity;
                    _currentWeight += newItem.getWeight() * newItem._quantity;
                    return;
                }
            }

            _inventoryItems.Add(newItem);
            _isLocked.Add(false);
            _currentWeight += newItem.getWeight() * newItem._quantity;
        }

        private void RenderInventory()
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
                itemUI.transform.Find("Quantity").GetComponentInChildren<TextMeshProUGUI>().text = item._quantity.ToString();
            }
        }

        public void ShowItem(BaseItem item)
        {
            _currentlySelected = item;
            _itemDescPanel.transform.Find("Title").GetComponent<TextMeshProUGUI>().text = item.getDisplayName();
            _itemDescPanel.transform.Find("Desc_Scroll").GetComponentInChildren<TextMeshProUGUI>().text = item.getItemDescription();
        }

        private List<BaseItem> SortInventory(SortingType type)
        {
            List <BaseItem> temp = _inventoryItems;

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
            if (_currentlySelected != null)
            {
                int index = -1;
                foreach (BaseItem item in _inventoryItems)
                {
                    index++;
                    if (item.getID() == _currentlySelected.getID()) { break; }
                }
                if (_isLocked[index] == true) { _isLocked[index] = false; }
                else if (_isLocked[index] == false) { _isLocked[index] = true; }
            }
        }

        public void SwitchSort()
        {
            int newSort = ((int) _currentSort + 1) % ((int) SortingType.TOTAL);
            _currentSort = (SortingType) newSort;
        }

        public void ToggleInventory()
        {
            if (_inventory.active == false)
            {
                _inventory.SetActive(true);
                RenderInventory();
            }
            else { _inventory.SetActive(false); }
        }
    }
}
