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
        _currentRecipePanel.transform.Find("ItemInfo").transform.Find("ItemDesc").
                            transform.Find("Viewport").transform.Find("Content").GetComponent<TextMeshProUGUI>().text = item.getItemDescription();
        _currentRecipePanel.transform.Find("ItemInfo").transform.Find("ItemIcon").GetComponent<Image>().sprite = item.getItemIcon();
        _currentRecipePanel.transform.Find("CraftButton").GetComponent<Button>().onClick.AddListener(() => Craft(item, recipe));

        foreach (RecipeData ingredient in recipe.data)
        {
            foreach (Transform child in _currentRecipePanel.transform.Find("RequiredMaterials").transform.Find("Viewport").transform.Find("Content"))
            {
                Destroy(child.gameObject);
            }

            GameObject ingredientGO = Instantiate(_ingredientPrefab, _currentRecipePanel.transform.Find("RequiredMaterials").
                                                                                            transform.Find("Viewport").transform.Find("Content"));
            ingredientGO.transform.Find("Icon").GetComponent<Image>().sprite = item.getItemIcon();

            BaseItem matchedSO = null;
            foreach (BaseItem things in _items)
            {
                if (things.getID() == ingredient.itemID)
                {
                    matchedSO = things;
                }
            }
            if (matchedSO.getID() != null)
            {
                ingredientGO.transform.Find("Name").GetComponent<TextMeshProUGUI>().text = matchedSO.getDisplayName();
                ingredientGO.transform.Find("AmountNeeded").GetComponent<TextMeshProUGUI>().text = matchedSO._quantity.ToString() + " / " + ingredient.quantity.ToString();
            }
        }
    }

    public void Craft(BaseItem item, CraftingRecipe recipe)
    {

        if (CanCraft(recipe))
        {
            _playerInventory.AddItem(item);
            foreach (RecipeData ingredient in recipe.data)
            {
                foreach (BaseItem things in _playerInventory.GetInventory())
                {
                    BaseItem matchedSO = null;
                    if (things.getID() == ingredient.itemID)
                    {
                        matchedSO = things;
                    }
                    if (matchedSO != null) { matchedSO._quantity -= ingredient.quantity; }
                    Debug.Log("Sucessful");
                }
            }
        }
    }

    public bool CanCraft(CraftingRecipe recipe)
    {
        int fulfilled = 0;

        foreach (RecipeData ingredient in recipe.data)
        {
            if (ingredient.quantity == 0)
            {
                fulfilled++;
            }
            foreach (BaseItem things in _playerInventory.GetInventory())
            {
                BaseItem matchedSO = null;
                if (things.getID() == ingredient.itemID)
                {
                    matchedSO = things;
                }
                if (matchedSO != null && matchedSO._quantity >= ingredient.quantity)
                {
                    fulfilled++;
                }
            }
        }

        return fulfilled == recipe.data.Length;
    }
}
