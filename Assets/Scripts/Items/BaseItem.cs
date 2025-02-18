using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Items/Base")]
public class BaseItem : ScriptableObject
{
    [SerializeField, Tooltip("This item ID")]
    private string itemName;

    [SerializeField, Tooltip("Item's Display name")]
    private string _displayName;

    [SerializeField, Tooltip("Item Description")]
    private string _itemDescription;

    [SerializeField, Tooltip("The 2D sprite for our item, use this when displaying the item in UI")]
    private Sprite itemIcon;

    [SerializeField, Tooltip("The 3D model for our item that the player will hold/interact with")]
    private GameObject itemModel;

    [SerializeField, Tooltip("Weight of item")]
    private float _weight;

    [SerializeField]
    public int _quantity;

    public CraftingRecipe[] recipes;

    public string getID() { return itemName; }
    public string getDisplayName() { return _displayName; }
    public string getItemDescription() { return _itemDescription; }
    public float getWeight() { return _weight; }
}

[System.Serializable]
public class RecipeData
{
    public string itemID;
    public int quantity = 1;
}

[System.Serializable]
public class CraftingRecipe
{
    public RecipeData[] data;
}