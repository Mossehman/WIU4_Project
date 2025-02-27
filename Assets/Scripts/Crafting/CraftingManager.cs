using Player.Inventory;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CraftingManager : MonoBehaviour
{
    [Header("Crafting Logic")]
    [SerializeField] private PlayerInventory _playerInventory;
    [SerializeField] private List<BaseItem> _items;
    [SerializeField] private List<CraftingRecipe> _recipes;
    private CraftingRecipe _currentRecipe;

    [Header("Crafting UI")]
    [SerializeField] private GameObject _craftingUI;
    [SerializeField] private GameObject _catalogPanel;
    [SerializeField] private GameObject _recipePrefab;
    [SerializeField] private GameObject _currentRecipePanel;
    [SerializeField] private GameObject _ingredientPrefab;

    public static bool isCraftingUIOpened = false;

    // Start is called before the first frame update
    void Start()
    {
        _recipes = new List<CraftingRecipe>();

        foreach (BaseItem item in _items)
        {
            foreach(CraftingRecipe recipe in item.recipes)
            {
                _recipes.Add(recipe);
            }
        }

        RenderRecipes();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void RenderRecipes()
    {
        foreach (Transform child in _catalogPanel.transform)
        {
            Destroy(child.gameObject);
        }
        foreach (BaseItem item in _items)
        {
            foreach (CraftingRecipe recipe in item.recipes)
            {
                GameObject newRecipe = Instantiate(_recipePrefab, _catalogPanel.transform);
                newRecipe.transform.Find("NameText").GetComponent<TextMeshProUGUI>().text = item.getDisplayName();
                newRecipe.transform.Find("Icon").GetComponent<Image>().sprite = item.getItemIcon();
                newRecipe.GetComponent<Button>().onClick.AddListener(() => ShowRecipe(item, recipe));
            }
        }
    }

    public void ShowRecipe(BaseItem item, CraftingRecipe recipe)
    {
        _currentRecipe = recipe;
        _currentRecipePanel.transform.Find("InfoPanel").transform.Find("InfoName").GetComponent<TextMeshProUGUI>().text = item.getDisplayName();
        _currentRecipePanel.transform.Find("InfoPanel").transform.Find("InfoDesc").
                            transform.Find("Viewport").transform.Find("Content").GetComponent<TextMeshProUGUI>().text = item.getItemDescription();
        _currentRecipePanel.transform.Find("InfoPanel").transform.Find("InfoIcon").GetComponent<Image>().sprite = item.getItemIcon();

        Button craftButton = _currentRecipePanel.transform.Find("CraftPanel").transform.Find("CraftBtn").GetComponent<Button>();
        craftButton.onClick.RemoveAllListeners(); // Prevent duplicate listeners
        craftButton.onClick.AddListener(() => Craft(item, recipe));

        Transform materialsContent = _currentRecipePanel.transform.Find("MaterialsPanel").Find("Viewport").Find("Content");

        // **Clear the previous ingredients before adding new ones**
        foreach (Transform child in materialsContent)
        {
            Destroy(child.gameObject);
        }

        foreach (RecipeData ingredient in recipe.data)
        {
            GameObject ingredientGO = Instantiate(_ingredientPrefab, materialsContent);

            // Find the correct ingredient in the list
            BaseItem matchedSO = null;
            foreach (BaseItem things in _items)
            {
                if (things.getID() == ingredient.itemID)
                {
                    matchedSO = things;
                    break;
                }
            }

            if (matchedSO != null)
            {
                ingredientGO.transform.Find("Icon").GetComponent<Image>().sprite = matchedSO.getItemIcon();
                ingredientGO.transform.Find("Name").GetComponent<TextMeshProUGUI>().text = matchedSO.getDisplayName();

                // Get the actual number of items from the inventory and hotbar
                int ownedQuantity = _playerInventory.GetItemCount(ingredient.itemID);
                ingredientGO.transform.Find("AmountNeeded").GetComponent<TextMeshProUGUI>().text =
                    $"{ownedQuantity} / {ingredient.quantity}";
            }
            else
            {
                Debug.LogWarning($"Ingredient with ID {ingredient.itemID} not found!");
            }
        }
    }

    public bool CanCraft(CraftingRecipe recipe)
    {
        foreach (RecipeData ingredient in recipe.data)
        {
            int totalQuantity = 0;

            // Get inventory and hotbar items
            List<BaseItem> inventoryItems = _playerInventory.GetInventory();
            BaseItem[] hotbarItems = _playerInventory.GetHotbar();

            // Count inventory items
            foreach (BaseItem inventoryItem in inventoryItems)
            {
                if (inventoryItem.getID() == ingredient.itemID)
                {
                    totalQuantity += inventoryItem._quantity;
                }
            }

            // Count hotbar items
            foreach (BaseItem hotbarItem in hotbarItems)
            {
                if (hotbarItem != null && hotbarItem.getID() == ingredient.itemID)
                {
                    totalQuantity += hotbarItem._quantity;
                }
            }

            Debug.Log($"Checking ingredient: {ingredient.itemID} | Required: {ingredient.quantity} | Available: {totalQuantity}");

            // If any required ingredient is missing, return false
            if (totalQuantity < ingredient.quantity)
            {
                return false;
            }
        }

        Debug.Log($"Crafting check result: ALL ingredients met.");
        return true;
    }

    public void Craft(BaseItem item, CraftingRecipe recipe)
    {
        if (CanCraft(recipe))
        {
            Debug.Log($"Crafting {item.getDisplayName()}...");

            _playerInventory.AddItem(item); // Add crafted item to inventory
            EventManager.Fire("OnCraftMisson", item);

            // Get inventory and hotbar items
            List<BaseItem> inventoryItems = _playerInventory.GetInventory();
            BaseItem[] hotbarItems = _playerInventory.GetHotbar();

            foreach (RecipeData ingredient in recipe.data)
            {
                int remainingToRemove = ingredient.quantity;

                // Remove from HOTBAR first
                for (int i = 0; i < hotbarItems.Length; i++)
                {
                    if (hotbarItems[i] != null && hotbarItems[i].getID() == ingredient.itemID)
                    {
                        int removeAmount = Mathf.Min(remainingToRemove, hotbarItems[i]._quantity);
                        hotbarItems[i]._quantity -= removeAmount;
                        remainingToRemove -= removeAmount;

                        Debug.Log($"Removed {removeAmount} {ingredient.itemID} from HOTBAR. Remaining: {remainingToRemove}");

                        // If item is completely used, clear the hotbar slot
                        if (hotbarItems[i]._quantity <= 0)
                        {
                            hotbarItems[i] = null;
                        }

                        if (remainingToRemove <= 0) break; // Stop if we removed enough
                    }
                }

                // If still need to remove, remove from INVENTORY
                for (int i = 0; i < inventoryItems.Count && remainingToRemove > 0; i++)
                {
                    if (inventoryItems[i].getID() == ingredient.itemID)
                    {
                        int removeAmount = Mathf.Min(remainingToRemove, inventoryItems[i]._quantity);
                        inventoryItems[i]._quantity -= removeAmount;
                        remainingToRemove -= removeAmount;

                        Debug.Log($"Removed {removeAmount} {ingredient.itemID} from INVENTORY. Remaining: {remainingToRemove}");

                        // If inventory item is completely used, remove it
                        if (inventoryItems[i]._quantity <= 0)
                        {
                            inventoryItems.RemoveAt(i);
                            i--; // Adjust index after removing item
                        }

                        if (remainingToRemove <= 0) break; // Stop if we removed enough
                    }
                }
            }

            Debug.Log("Crafting successful!");

            // Update UI immediately
            _playerInventory.RefreshInventoryUI(); // Ensure inventory updates after crafting
            _playerInventory.RefreshHotbarUI();   // Ensure hotbar updates after crafting
            ShowRecipe(item, recipe);            // Refresh crafting UI to update ingredient amounts

        }
        else
        {
            Debug.Log("Not enough ingredients to craft!");
        }
    }

    private void PrintInventory()
    {
        Debug.Log("Current Inventory:");
        foreach (BaseItem inventoryItem in _playerInventory.GetInventory())
        {
            Debug.Log($"Item: {inventoryItem.getDisplayName()} | Quantity: {inventoryItem._quantity} | ID: {inventoryItem.getID()}");
        }
    }
}
