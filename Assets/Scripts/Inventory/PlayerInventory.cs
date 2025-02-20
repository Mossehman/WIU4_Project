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
                                    private GameObject          _currentlySelected;
                                    private List<bool>          _isLocked;

        [Header("Inventory UI")]
        [SerializeField]            private GameObject          _inventory;
        [SerializeField]            private GameObject          _inventoryPanel;
        [SerializeField]            private GameObject          _itemPrefab;
        [SerializeField]            private GameObject          _itemDescPanel;

        [Header("Hotbar Logic")]
        [SerializeField]            private GameObject[]        _hotbarItems;
                                    private int                 _maxHotbarItems = 5;

        [Header("Hotbar UI")]
        [SerializeField]            private GameObject          _hotBarPanel;
        [SerializeField]            private GameObject          _hotBarSlotPrefab;
        [SerializeField]            private GameObject[]        _hotbarSlots;
        [SerializeField]            private GameObject          _hotbarItemPrefab;

        void Start()
        {
            EventManager.Connect("OnItemMove", OnItemMove);

            // INVENTORY
            _inventoryItems = new List<GameObject>();
            _isLocked = new List<bool>();
            _currentWeight = _baseWeight;

            // HOT BAR
            _hotbarItems = new GameObject[_maxHotbarItems];

            foreach (Transform child in _hotBarPanel.transform)
            {
                Destroy(child.gameObject);
            }
            for (int i = 0; i <_maxHotbarItems; i++)
            {
                GameObject hotbarItem = Instantiate(_hotBarSlotPrefab, _hotBarPanel.transform);
                _hotbarSlots[i] = hotbarItem;
                _hotbarSlots[i].tag = "Hotbar";
            }
        }

        void Update()
        {
            SortInventory(_currentSort);
        }

        public void DropItem(GameObject droppedItem)
        {
            foreach (var item in _inventoryItems)
            {
                if (item.GetComponent<ItemModelScript>().getSO().getID() == droppedItem.GetComponent<ItemModelScript>().getSO().getID())
                {
                    droppedItem.GetComponent<ItemModelScript>().OnDropItem(droppedItem.GetComponent<ItemModelScript>().getSO());
                }
            }    
        }    

        public void AddItem(GameObject newItem)
        {
            BaseItem tempItem = newItem.GetComponent<ItemModelScript>().getSO();
            foreach (var item in _inventoryItems)
            {
                if (item.GetComponent<ItemModelScript>().getSO().getID() == tempItem.getID())
                {
                    item.GetComponent<ItemModelScript>().getSO()._quantity++;
                    _currentWeight += tempItem.getWeight() * tempItem._quantity;
                    return;
                }
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

            int index = -1;

            foreach (var item in temp)
            {
                index++;
                GameObject itemUI = Instantiate(_itemPrefab, _inventoryPanel.transform);
                itemUI.GetComponent<Draggable>()._item = item;
                itemUI.transform.Find("Name").GetComponent<TextMeshProUGUI>().text = item.GetComponent<ItemModelScript>().getSO().getDisplayName();
                itemUI.GetComponent<Button>().onClick.AddListener( () => ShowItem(item) );
                itemUI.transform.Find("Quantity").GetComponentInChildren<TextMeshProUGUI>().text = item.GetComponent<ItemModelScript>().getSO()._quantity.ToString();
                itemUI.GetComponent<Image>().sprite = item.GetComponent<ItemModelScript>().getSO().getItemIcon();
                itemUI.transform.Find("Lock").GetComponent<Image>().enabled = _isLocked[index];
            }
        }

        public void ShowItem(GameObject item)
        {
            _currentlySelected = item;
            _itemDescPanel.transform.Find("Title").GetComponent<TextMeshProUGUI>().text = item.GetComponent<ItemModelScript>().getSO().getDisplayName();
            _itemDescPanel.transform.Find("Desc_Scroll").GetComponentInChildren<TextMeshProUGUI>().text = item.GetComponent<ItemModelScript>().getSO().getItemDescription();
            _itemDescPanel.transform.Find("Image").GetComponent<Image>().sprite = item.GetComponent<ItemModelScript>().getSO().getItemIcon();
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
                    if (item.GetComponent<ItemModelScript>().getSO().getID() == _currentlySelected.GetComponent<ItemModelScript>().getSO().getID()) { break; }
                }
                if (_isLocked[index] == true) 
                { 
                    _isLocked[index] = false;
                    _inventoryPanel.transform.GetChild(index).gameObject.transform.Find("Lock").GetComponent<Image>().enabled = false;
                }
                else if (_isLocked[index] == false) 
                { 
                    _isLocked[index] = true;
                    _inventoryPanel.transform.GetChild(index).gameObject.transform.Find("Lock").GetComponent<Image>().enabled = true;
                }
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

        void OnItemMove(object[] args)
        {
            // UI Object that got dragged
            GameObject item = args[0] as GameObject;
            ItemOrigin origin = (ItemOrigin)args[1];
            ItemDestination destination = (ItemDestination)args[2];

            GameObject matchedGO;

            if (origin == ItemOrigin.INVENTORY)
            {
                foreach(var invItem in _inventoryItems)
                {
                    if (invItem.GetComponent<ItemModelScript>().getSO().getID() == item.GetComponent<Draggable>()._item.GetComponent<ItemModelScript>().getSO().getID())
                    {
                        matchedGO = invItem;
                        _inventoryItems.Remove(invItem);
                    }
                }
            }
            else if (origin == ItemOrigin.HOTBAR)
            {
                for (int i = 0; i < _maxHotbarItems; i++)
                {
                    if (_hotbarItems[i].GetComponent<ItemModelScript>().getSO().getID() == item.GetComponent<Draggable>()._item.GetComponent<ItemModelScript>().getSO().getID())
                    {
                        _hotbarItems[i] = null;
                        break;
                    }
                }
            }
            else if (origin == ItemOrigin.STORAGE)
            {

            }

            if (destination == ItemDestination.INVENTORY)
            {
                _inventoryItems.Add(item.GetComponent<Draggable>()._item);
            }
            else if (destination == ItemDestination.HOTBAR)
            {
                for (int i = 0; i < _maxHotbarItems; i++)
                {
                    if (_hotbarItems[i] == null) 
                    { 
                        _hotbarItems[i] = item.GetComponent<Draggable>()._item;
                        break;
                    }
                }
            }
            else if (destination == ItemDestination.STORAGE)
            {

            }
        }
    }
}
