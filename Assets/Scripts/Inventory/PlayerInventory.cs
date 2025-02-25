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
        [SerializeField]    private BaseItem[]              _startingItems;
                            private List<BaseItem>          _inventoryItems;
        [SerializeField]    public float                    _baseWeight = 0.0f;
        [SerializeField]    public float                    _baseMaxWeight = 10.0f;
        [SerializeField]    private float                   _currentWeight;
        // SORTING
        [SerializeField]    private SortingType             _currentSort = SortingType.DATE_ADDED;
        // LOCKING ITEMS
                            private BaseItem                _currentlySelected;
                            private List<bool>              _isLocked;

        [Header("Inventory UI")]
        [SerializeField]    private GameObject              _inventory;
        [SerializeField]    private GameObject              _inventoryPanel;
        [SerializeField]    private GameObject              _itemPrefab;
        [SerializeField]    private GameObject              _itemDescPanel;

        [Header("Hotbar Logic")]
        [SerializeField]    private BaseItem[]              _hotbarItems;
                            private int                     _maxHotbarItems = 5;
        [SerializeField]    private int                     _selectedHotbarIndex = 0;
        [SerializeField]    private Color                   _selectedSlotColor = Color.red;
        [SerializeField]    private Color                   _defaultSlotColor = Color.white;

        [Header("Hotbar UI")]
        [SerializeField]    private GameObject              _hotBarPanel;
        [SerializeField]    private GameObject              _hotBarSlotPrefab;
        [SerializeField]    private GameObject[]            _hotbarSlots;

        [SerializeField]    private GameObject              _backgroundPanel;
        [SerializeField]    private GameObject              _vitalsPanel;
        [SerializeField]    private GameObject              _infoPanel;
        [SerializeField]    private GameObject              _mapPanel;

        [Header("Hotbar Transform Settings")]
        private RectTransform _hotbarRect;
        private GridLayoutGroup _hotbarGrid;

        [Header("Item Visuals")]
        [SerializeField]    private Transform               _playerHand;

        // Closed (default) state
        private Vector2 _closedOffsetMin;
        private Vector2 _closedOffsetMax;
        private Vector2 _closedSpacing;
        private TextAnchor _closedAlignment;

        // Opened state
        private Vector2 _openedOffsetMin;
        private Vector2 _openedOffsetMax;
        private Vector2 _openedSpacing;
        private TextAnchor _openedAlignment;

        void Start()
        {
            EventManager.Connect("OnItemMove", OnItemMove);

            // INVENTORY
            _inventoryItems = new List<BaseItem>();

            if (_startingItems != null && _startingItems.Length > 0)
            {
                foreach (var item in _startingItems)
                {
                    if (item == null) continue;
                    BaseItem itemInstance = Instantiate(item);
                    itemInstance.Init();
                    _inventoryItems.Add(itemInstance);
                }
            }

            _isLocked = new List<bool>();
            _currentWeight = _baseWeight;

            // HOT BAR
            _hotbarItems = new BaseItem[_maxHotbarItems];

            foreach (Transform child in _hotBarPanel.transform)
            {
                Destroy(child.gameObject);
            }

            for (int i = 0; i < _maxHotbarItems; i++)
            {
                GameObject hotbarItem = Instantiate(_hotBarSlotPrefab, _hotBarPanel.transform);
                _hotbarSlots[i] = hotbarItem;
                _hotbarSlots[i].tag = "Hotbar";
            }

            _hotbarRect = _hotBarPanel.GetComponent<RectTransform>();
            _hotbarGrid = _hotBarPanel.GetComponent<GridLayoutGroup>();

            // Store Closed State (from Image 1)
            _closedOffsetMin = _hotbarRect.offsetMin; // Left, Bottom
            _closedOffsetMax = _hotbarRect.offsetMax; // Right, Top
            _closedSpacing = _hotbarGrid.spacing;
            _closedAlignment = _hotbarGrid.childAlignment;

            // Store Opened State (from Image 2)
            _openedOffsetMin = new Vector2(170.9836f, 12.13998f); // Left, Bottom
            _openedOffsetMax = new Vector2(-518.9836f, -875.54f);  // Right, Top
            _openedSpacing = new Vector2(-9.4f, 76.81f);
            _openedAlignment = TextAnchor.UpperCenter;
        }

        private void DisplayItem(BaseItem item)
        {
            for (int i = 0; i < _playerHand.transform.childCount; i++)
            {
                Destroy(_playerHand.GetChild(i).gameObject);
            }
            if (_playerHand == null || item == null || item.getItemModel() == null) { return; }
            GameObject itemToDisplay = Instantiate(item.getItemModel(), _playerHand.transform);
            if (itemToDisplay.TryGetComponent(out ItemModelScript itemData))
            {
                itemData.isDropped = false;
                itemData.modelRB.isKinematic = true;
                itemData.modelCollider.isTrigger = true;
            }
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.I))
            {
                Cursor.lockState = CursorLockMode.None;
                ToggleInventory();
            }

            // Detect keys 1-5 to select a hotbar slot
            for (int i = 0; i < _maxHotbarItems; i++)
            {
                if (Input.GetKeyDown(KeyCode.Alpha1 + i))
                {
                    _selectedHotbarIndex = i;
                    Debug.Log($"Hotbar slot {i + 1} selected.");
                    UpdateHotbarUI();

                    if (_hotbarItems[i] != null)
                    {
                        DisplayItem(_hotbarItems[i]);
                    }
                }
            }

            if (_selectedHotbarIndex >= 0 && _hotbarItems[_selectedHotbarIndex] != null)
            {
                _hotbarItems[_selectedHotbarIndex].OnItemHeld(gameObject); 

                if (Input.GetMouseButton(0))
                {
                    _hotbarItems[_selectedHotbarIndex].OnItemLeftClick(gameObject);
                }
                else if (Input.GetMouseButton(1))
                {
                    _hotbarItems[_selectedHotbarIndex].OnItemRightClick(gameObject);
                }
                else if (Input.GetMouseButtonUp(0))
                {
                    _hotbarItems[_selectedHotbarIndex].OnItemLeftUp(gameObject);
                }
                else if (Input.GetMouseButtonUp(1))
                {
                    _hotbarItems[_selectedHotbarIndex].OnItemRightUp(gameObject);
                }
            }

            foreach (var item in _inventoryItems)
            {
                if (item == null) continue;
                item.Update();
            }

            foreach (var item in _hotbarItems)
            {
                if (item == null) continue;
                item.Update();
            }

            if (Input.GetKeyDown(KeyCode.Y))
            {
                TryDropSelectedItem();
            }

            

            SortInventory(_currentSort);
        }

        public void TryDropSelectedItem()
        {
            // Ensure a hotbar slot is selected
            if (_selectedHotbarIndex == -1)
            {
                Debug.LogWarning("No hotbar slot selected. Press 1-5 to select a slot.");
                return;
            }

            // Ensure there is an item in the selected slot
            BaseItem itemToDrop = _hotbarItems[_selectedHotbarIndex];
            if (itemToDrop == null)
            {
                Debug.LogWarning($"No item in selected hotbar slot {_selectedHotbarIndex + 1}.");
                return;
            }

            Debug.Log($"Dropping item {itemToDrop.getDisplayName()} from hotbar slot {_selectedHotbarIndex + 1}");

            // Remove item from hotbar
            _hotbarItems[_selectedHotbarIndex] = null;

            if (_playerHand != null)
            {
                for (int i = 0; i < _playerHand.transform.childCount; i++)
                {
                    Destroy(_playerHand.GetChild(i).gameObject);
                }
            }

            // **Destroy the hotbar UI prefab for this slot**
            Transform slotTransform = _hotbarSlots[_selectedHotbarIndex].transform;
            if (slotTransform.childCount > 0)
            {
                Destroy(slotTransform.GetChild(0).gameObject); // Removes item UI
            }

            // Update hotbar UI after dropping
            UpdateHotbarUI();

            // Calculate a safe drop position in front of the player
            Vector3 dropOffset = transform.forward * 2.0f + Vector3.up * 1.0f; // Move 2 units forward, 1 unit up
            Vector3 dropPosition = transform.position + dropOffset;

            // Instantiate the item model at the offset position
            GameObject droppedModel = Instantiate(itemToDrop.getItemModel(), dropPosition, Quaternion.identity);
            ItemModelScript itemScript = droppedModel.GetComponent<ItemModelScript>();

            if (itemScript != null)
            {
                itemScript.OnDropItem(itemToDrop, transform.forward, 5.0f); // Apply forward force
            }

            Debug.Log($"Item {itemToDrop.getDisplayName()} dropped and removed from UI.");
        }

        private void UpdateHotbarUI()
        {
            for (int i = 0; i < _maxHotbarItems; i++)
            {
                Image slotImage = _hotbarSlots[i].GetComponent<Image>();
                if (slotImage != null)
                {
                    slotImage.color = (i == _selectedHotbarIndex) ? _selectedSlotColor : _defaultSlotColor;
                }
            }

            if (_selectedHotbarIndex != -1)
            {
                DisplayItem(_hotbarItems[_selectedHotbarIndex]);
            }
        }

        public void AddItem(BaseItem newItem)
        {
            //// try to add to hotbar first
            //for (int i = 0; i < _maxHotbarItems; i++)
            //{
            //    if (_hotbarItems[i] != null && _hotbarItems[i].getID() == newItem.getID())
            //    {
            //        _hotbarItems[i]._quantity++;
            //        _currentWeight += newItem.getWeight() * newItem._quantity;
            //        return;
            //    }
            //
            //    if (_hotbarItems[i] != null) continue;
            //    _hotbarItems[i] = newItem;
            //    _isLocked.Add(false);
            //    _currentWeight += newItem.getWeight() * newItem._quantity;
            //    return;
            //}

            // if hotbar is full, try adding to inventory
            foreach (var item in _inventoryItems)
            {
                if (item.getID() == newItem.getID())
                {
                    item._quantity++;
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

            int index = -1;

            foreach (var item in temp)
            {
                index++;
                GameObject itemUI = Instantiate(_itemPrefab, _inventoryPanel.transform);
                itemUI.GetComponent<Draggable>()._item = item;
                itemUI.transform.Find("ItemIcon").GetComponent<Image>().sprite = item.getItemIcon();

                Button button = itemUI.GetComponent<Button>();

                if (button == null)
                {
                    Debug.LogError("Button component missing from itemUI prefab!");
                }
                else
                {
                    Debug.Log("Button found on itemUI prefab, adding listener...");
                    button.onClick.AddListener(() => ShowItem(item));
                }

                itemUI.transform.Find("Quantity").GetComponentInChildren<TextMeshProUGUI>().text = item._quantity.ToString();
                itemUI.transform.Find("Lock").GetComponent<Image>().enabled = _isLocked[index];
            }
        }

        public void ShowItem(BaseItem item)
        {
            _currentlySelected = item;
            _itemDescPanel.transform.Find("ItemName").GetComponent<TextMeshProUGUI>().text = item.getDisplayName();
            _itemDescPanel.transform.Find("ItemFrame/ItemImage").GetComponent<Image>().sprite = item.getItemIcon();
            _itemDescPanel.transform.Find("DescScroll").GetComponentInChildren<TextMeshProUGUI>().text = item.getItemDescription();
        }

        private List<BaseItem> SortInventory(SortingType type)
        {
            List<BaseItem> temp = _inventoryItems;

            switch (type)
            {
                case SortingType.DATE_ADDED:
                    return temp; // No sorting needed

                case SortingType.ALPHABETICAL:
                    temp.Sort((a, b) => a.getDisplayName().CompareTo(b.getDisplayName()));
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
            int newSort = ((int)_currentSort + 1) % ((int)SortingType.TOTAL);
            _currentSort = (SortingType)newSort;
        }

        public void ToggleInventory()
        {
            if (_inventory.activeInHierarchy == false)
            {
                Cursor.lockState = CursorLockMode.Confined;
                _inventory.SetActive(true);
                _backgroundPanel.SetActive(false);
                _vitalsPanel.SetActive(false);
                _infoPanel.SetActive(false);
                _mapPanel.SetActive(false);
                RenderInventory();

                // Apply Opened Hotbar settings (from Image 2)
                _hotbarRect.offsetMin = _openedOffsetMin; // Adjust Left/Bottom
                _hotbarRect.offsetMax = _openedOffsetMax; // Adjust Right/Top

                _hotbarGrid.spacing = _openedSpacing;
                _hotbarGrid.childAlignment = _openedAlignment;
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
                _inventory.SetActive(false);
                _backgroundPanel.SetActive(true);
                _vitalsPanel.SetActive(true);
                _infoPanel.SetActive(true);
                _mapPanel.SetActive(true);

                // Restore Closed Hotbar settings (from Image 1)
                _hotbarRect.offsetMin = _closedOffsetMin; // Restore Left/Bottom
                _hotbarRect.offsetMax = _closedOffsetMax; // Restore Right/Top

                _hotbarGrid.spacing = _closedSpacing;
                _hotbarGrid.childAlignment = _closedAlignment;
            }
        }

        void OnItemMove(object[] args)
        {
            // UI Object that got dragged
            GameObject item = args[0] as GameObject;
            ItemOrigin origin = (ItemOrigin)args[1];
            ItemDestination destination = (ItemDestination)args[2];
            PlacableSlot slot = (PlacableSlot)args[3];

            BaseItem matchedSO;

            if (origin == ItemOrigin.INVENTORY)
            {
                for (int i = _inventoryItems.Count - 1; i >= 0; i--)
                {
                    if (_inventoryItems[i].getID() == item.GetComponent<Draggable>()._item.getID())
                    {
                        matchedSO = _inventoryItems[i];
                        _inventoryItems.RemoveAt(i);
                        break;
                    }
                }
            }
            else if (origin == ItemOrigin.HOTBAR)
            {
                for (int i = 0; i < _maxHotbarItems; i++)
                {
                    if (_hotbarItems[i] == null) { continue; }
                    if (_hotbarItems[i] == item.GetComponent<Draggable>()._item)
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
                for (int i = 0; i < _hotBarPanel.transform.childCount; i++)
                {
                    if (_hotBarPanel.transform.GetChild(i).TryGetComponent<PlacableSlot>(out PlacableSlot slotData))
                    {
                        if (slotData != slot) { continue; }
                        _hotbarItems[i] = item.GetComponent<Draggable>()._item;
                        break;
                    }
                }
            }
            else if (destination == ItemDestination.STORAGE)
            {
            
            }

            if (_selectedHotbarIndex != -1)
            {
                DisplayItem(_hotbarItems[_selectedHotbarIndex]);
            }
        }

        public List<BaseItem> GetInventory() { return _inventoryItems; }
    }
}