using Player.Inventory;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static UnityEditor.Progress;

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
                newRecipe.transform.Find("Name").GetComponent<TextMeshProUGUI>().text = item.getDisplayName();
                newRecipe.transform.Find("Icon").GetComponent<Image>().sprite = item.getItemIcon();
                newRecipe.GetComponent<Button>().onClick.AddListener(() => ShowRecipe(item, recipe));
            }
        }

    }

    public void ShowRecipe(BaseItem item, CraftingRecipe recipe)
    {
        _currentRecipe = recipe;
        _currentRecipePanel.transform.Find("ItemInfo").transform.Find("ItemName").GetComponent<TextMeshProUGUI>().text = item.getDisplayName();
        _currentRecipePanel.transform.Find("ItemInfo").transform.Find("ItemDesc").GetComponent<TextMeshProUGUI>().text = item.getItemDescription();
        _currentRecipePanel.transform.Find("ItemInfo").transform.Find("ItemIcon").GetComponent<Image>().sprite = item.getItemIcon();

        foreach (RecipeData ingredient in recipe.data)
        {
            BaseItem matchedSO = new BaseItem();
            foreach (BaseItem things in _playerInventory.GetInventory())
            {
                if (things.getID() == ingredient.itemID)
                {
                    matchedSO = things;
                }
            }
            if (matchedSO.getID() != null)
            {
                GameObject ingredientGO = Instantiate(_ingredientPrefab, _currentRecipePanel.transform.Find("RequiredMaterials"));
                ingredientGO.GetComponent<Image>().sprite = matchedSO.getItemIcon();
                ingredientGO.transform.Find("AmountNeeded").GetComponent<TextMeshProUGUI>().text = matchedSO._quantity.ToString() + " / " + ingredient.quantity.ToString();
            }
        }
    }

    public void Craft()
    {

    }

    public bool CanCraft(CraftingRecipe recipe)
    {
        int fulfilled = 0;

        foreach (RecipeData ingredient in recipe.data)
        {
            foreach (BaseItem things in _playerInventory.GetInventory())
            {
                BaseItem matchedSO = new BaseItem();
                if (things.getID() == ingredient.itemID)
                {
                    matchedSO = things;
                }
                if (matchedSO._quantity >= ingredient.quantity)
                {
                    fulfilled++;
                }
            }
        }

        return fulfilled == recipe.data.Length;
    }
}
