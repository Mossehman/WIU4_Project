using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
using UnityEngine;
using Unity.VisualScripting;
using UnityEditorInternal.Profiling.Memory.Experimental;

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

    enum SlotStatus
    {
        EMPTY,
        OCCUPIED
    }

    enum ItemDestination
    {
        INVENTORY,
        HOTBAR,
        STORAGE
    }
    enum ItemOrigin
    {
        INVENTORY,
        HOTBAR,
        STORAGE
    }

    public class PlayerInventory : MonoBehaviour
    {
        [Header("Inventory Logic")]
        [SerializeField]            private List<GameObject>    _inventoryItems;
        [SerializeField]            public float                _baseWeight = 0.0f;
        [SerializeField]            public float                _baseMaxWeight = 10.0f;
        [SerializeField]            private float               _currentWeight;
        // SORTING
        [SerializeField]            private SortingType         _currentSort = SortingType.DATE_ADDED;
        // LOCKING ITEMS
                                    private BaseItem            _currentlySelected;
                                    private List<bool>          _isLocked;
                                    private List<SlotStatus>    _inventoryItemStatus;

        [Header("Inventory UI")]
        [SerializeField]            private GameObject          _inventory;
        [SerializeField]            private GameObject          _inventoryPanel;
        [SerializeField]            private GameObject          _itemPrefab;
        [SerializeField]            private GameObject          _itemDescPanel;

        [Header("Hotbar Logic")]
        [SerializeField]            private GameObject[]          _hotbarItems;
                                    private int                 _maxHotbarItems = 5;
                                    private SlotStatus[]        _hotbarItemStatus;

        [Header("Hotbar UI")]
        [SerializeField]            private GameObject          _hotBarPanel;
        [SerializeField]            private GameObject          _hotBarSlotPrefab;
        [SerializeField]            private GameObject[]        _hotbarSlots;
        [SerializeField]            private GameObject          _hotbarItemPrefab;

        void Start()
        {
            EventManager.Connect("OnDropItem", OnDropItem);

            // INVENTORY
            _inventoryItems = new List<GameObject>();
            _isLocked = new List<bool>();
            _inventoryItemStatus = new List<SlotStatus>();
            _currentWeight = _baseWeight;

            // HOT BAR
            _hotbarItemStatus = new SlotStatus[_maxHotbarItems];
            _hotbarItems = new GameObject[_maxHotbarItems];

            foreach (Transform child in _hotBarPanel.transform)
            {
                Destroy(child.gameObject);
            }
            for (int i = 0; i <_maxHotbarItems; i++)
            {
                GameObject hotbarItem = Instantiate(_hotBarSlotPrefab, _hotBarPanel.transform);
                _hotbarSlots[i] = hotbarItem;
                _hotbarItemStatus[i] = SlotStatus.EMPTY;
            }
        }

        void Update()
        {
            SortInventory(_currentSort);
        }

        public void AddItem(GameObject newItem)
        {
            BaseItem tempItem = newItem.GetComponent<ItemModelScript>().getSO();
            int index = 0;
            foreach (var item in _inventoryItems)
            {
                if (item.GetComponent<ItemModelScript>().getSO().getID() == tempItem.getID())
                {
                    item.GetComponent<ItemModelScript>().getSO()._quantity++;
                    _currentWeight += tempItem.getWeight() * tempItem._quantity;
                    return;
                }
                _inventoryItemStatus[index] = SlotStatus.OCCUPIED;
                index++;
            }

            _inventoryItems.Add(newItem);
            _isLocked.Add(false);
            _currentWeight += tempItem.getWeight() * tempItem._quantity;
        }

        private void RenderInventory()
        {
            foreach (Transform child in _inventoryPanel.transform)
            {
                Destroy(child.gameObject);
            }

            List<GameObject> temp = SortInventory(_currentSort);

            foreach (var item in temp)
            {
                GameObject itemUI = Instantiate(_itemPrefab, _inventoryPanel.transform);
                itemUI.GetComponent<Draggable>()._item = item;
                itemUI.transform.Find("Name").GetComponent<TextMeshProUGUI>().text = item.GetComponent<ItemModelScript>().getSO().getDisplayName();
                itemUI.GetComponent<Button>().onClick.AddListener( () => ShowItem(item.GetComponent<ItemModelScript>().getSO()) );
                itemUI.transform.Find("Quantity").GetComponentInChildren<TextMeshProUGUI>().text = item.GetComponent<ItemModelScript>().getSO()._quantity.ToString();
                itemUI.GetComponent<Image>().sprite = item.GetComponent<ItemModelScript>().getSO().getItemIcon();
            }
        }

        public void ShowItem(BaseItem item)
        {
            _currentlySelected = item;
            _itemDescPanel.transform.Find("Title").GetComponent<TextMeshProUGUI>().text = item.getDisplayName();
            _itemDescPanel.transform.Find("Desc_Scroll").GetComponentInChildren<TextMeshProUGUI>().text = item.getItemDescription();
            _itemDescPanel.transform.Find("Image").GetComponent<Image>().sprite = item.getItemIcon();
        }

        private List<GameObject> SortInventory(SortingType type)
        {
            List <GameObject> temp = _inventoryItems;

            switch (type)
            {
                case SortingType.DATE_ADDED:
                    return temp; // No sorting needed

                case SortingType.ALPHABETICAL:
                    temp.Sort((a, b) => a.name.CompareTo(b.name));
                    break;

                case SortingType.HEAVIEST:
                    temp.Sort((a, b) => b.GetComponent<ItemModelScript>().getSO().getWeight().CompareTo(a.GetComponent<ItemModelScript>().getSO().getWeight()));
                    break;

                case SortingType.LIGHTEST:
                    temp.Sort((a, b) => a.GetComponent<ItemModelScript>().getSO().getWeight().CompareTo(b.GetComponent<ItemModelScript>().getSO().getWeight()));
                    break;

                case SortingType.QUANTITY:
                    temp.Sort((a, b) => b.GetComponent<ItemModelScript>().getSO()._quantity.CompareTo(a.GetComponent<ItemModelScript>().getSO()._quantity));
                    break;
            }

            return temp;
        }

        public void LockItem()
        {
            if (_currentlySelected != null)
            {
                int index = -1;
                foreach (GameObject item in _inventoryItems)
                {
                    index++;
                    if (item.GetComponent<ItemModelScript>().getSO().getID() == _currentlySelected.getID()) { break; }
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

        void OnDropItem(object[] args)
        {
            
        }
    }
}
