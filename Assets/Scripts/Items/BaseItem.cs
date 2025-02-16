using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Items/Base")]
public class BaseItem : ScriptableObject
{
    [SerializeField, Tooltip("This item ID")]
    private string itemName;

    [SerializeField, Tooltip("The 2D sprite for our item, use this when displaying the item in UI")]
    private Sprite itemIcon;

    [SerializeField, Tooltip("The 3D model for our item that the player will hold/interact with")]
    private GameObject itemModel;

    public CraftingRecipe[] recipes;

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