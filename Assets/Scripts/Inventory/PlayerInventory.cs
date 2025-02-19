using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
using UnityEngine;

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

    public class PlayerInventory : MonoBehaviour
    {
        [Header("Inventory Logic")]
        [SerializeField]    private List<GameObject>    _inventoryItems;
        [SerializeField]    public float                _baseWeight = 0.0f;
        [SerializeField]    public float                _baseMaxWeight = 10.0f;
        [SerializeField]    private float               _currentWeight;
        // SORTING
        [SerializeField]    private SortingType         _currentSort = SortingType.DATE_ADDED;
        // LOCKING ITEMS
                            private BaseItem            _currentlySelected;
                            private List<bool>          _isLocked;
                            private List<SlotStatus>    _inventoryItemStatus;

        [Header("Inventory UI")]
        [SerializeField]    private GameObject          _inventory;
        [SerializeField]    private GameObject          _inventoryPanel;
        [SerializeField]    private GameObject          _itemPrefab;
        [SerializeField]    private GameObject          _itemDescPanel;

        [Header("Hotbar Logic")]
        [SerializeField]    private BaseItem[]          _hotbarItems;
                            private int                 _maxHotbarItems = 5;
                            private SlotStatus[]        _hotbarItemStatus;

        [Header("Hotbar UI")]
        [SerializeField]    private GameObject          _hotBarPanel;
        [SerializeField]    private GameObject          _hotBarSlotPrefab;
        [SerializeField]    private GameObject[]        _hotbarSlots;
        [SerializeField]    private GameObject          _hotbarItemPrefab;

        void Start()
        {
            EventManager.Connect("OnDropItem", OnDropItem);

            //_items = new List<BaseItem>();
            _isLocked = new List<bool>();
            _hotbarItemStatus = new SlotStatus[_maxHotbarItems];

            foreach (Transform child in _hotBarPanel.transform)
            {
                Destroy(child.gameObject);
            }
            for (int i = 0; i <_maxHotbarItems; i++)
            {
                GameObject hotbarItem = Instantiate(_hotBarSlotPrefab, _hotBarPanel.transform);
                _hotbarItemStatus[i] = SlotStatus.EMPTY;
            }

            _currentWeight = _baseWeight;
        }

        void Update()
        {
            
        }

        public void AddItem(GameObject newItem)
        {
            foreach (var item in _inventoryItems)
            {
                if (item.GetComponent<BaseItem>().getID() == newItem.GetComponent<BaseItem>().getID())
                {
                    item.GetComponent<BaseItem>()._quantity += newItem.GetComponent<BaseItem>()._quantity;
                    _currentWeight += newItem.GetComponent<BaseItem>().getWeight() * newItem.GetComponent<BaseItem>()._quantity;
                    return;
                }
            }

            _inventoryItems.Add(newItem);
            _isLocked.Add(false);
            _currentWeight += newItem.GetComponent<BaseItem>().getWeight() * newItem.GetComponent<BaseItem>()._quantity;
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
                itemUI.transform.Find("Name").GetComponent<TextMeshProUGUI>().text = item.GetComponent<BaseItem>().getDisplayName();
                itemUI.GetComponent<Button>().onClick.AddListener( () => ShowItem(item.GetComponent<BaseItem>()) );
                itemUI.transform.Find("Quantity").GetComponentInChildren<TextMeshProUGUI>().text = item.GetComponent<BaseItem>()._quantity.ToString();
            }
        }

        public void ShowItem(BaseItem item)
        {
            _currentlySelected = item;
            _itemDescPanel.transform.Find("Title").GetComponent<TextMeshProUGUI>().text = item.getDisplayName();
            _itemDescPanel.transform.Find("Desc_Scroll").GetComponentInChildren<TextMeshProUGUI>().text = item.getItemDescription();
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
                    temp.Sort((a, b) => b.GetComponent<BaseItem>().getWeight().CompareTo(a.GetComponent<BaseItem>().getWeight()));
                    break;

                case SortingType.LIGHTEST:
                    temp.Sort((a, b) => a.GetComponent<BaseItem>().getWeight().CompareTo(b.GetComponent<BaseItem>().getWeight()));
                    break;

                case SortingType.QUANTITY:
                    temp.Sort((a, b) => b.GetComponent<BaseItem>()._quantity.CompareTo(a.GetComponent<BaseItem>()._quantity));
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
                    if (item.GetComponent<BaseItem>().getID() == _currentlySelected.getID()) { break; }
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
            // Check if the gameobject is not null
            if ((GameObject)args[0] != null)
            {
                switch ((ItemDestination)args[1])
                {
                    case ItemDestination.INVENTORY:
                        for (int i = 0; i < _inventoryPanel.transform.childCount; i++)
                        {
                            if (_inventoryItemStatus[i] == SlotStatus.EMPTY && _inventory.transform.GetChild(i) != null)
                            {
                                _inventoryItems.Add((GameObject)args[0]);
                            }
                            if (_inventoryItemStatus[i] == SlotStatus.OCCUPIED && _inventory.transform.GetChild(i) == null)
                            {
                                _inventoryItems.Remove(_inventoryItems[i]);
                            }
                        }
                        break;
                    case ItemDestination.HOTBAR:
                        for (int i = 0; i < _hotBarPanel.transform.childCount; i++)
                        {
                            if (_hotbarItemStatus[i] == SlotStatus.EMPTY && _hotbarSlots[i].transform.GetChild(0) != null)
                            {
                                _hotbarItems[i] = (BaseItem)args[0];
                            }
                            if (_inventoryItemStatus[i] == SlotStatus.OCCUPIED && _inventory.transform.GetChild(i) == null)
                            {
                                _hotbarItems[i] = null;
                            }
                        }
                        break;
                    case ItemDestination.STORAGE:
                        break;
                }
            }
        }
    }
}
