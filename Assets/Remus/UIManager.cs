using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static bool IsAnyMenuOpen { get; set; } = false;
    public static bool IsInventoryOpen { get; set; } = false;
    public static bool IsCraftingOpen { get; set; } = false;

    public static void LockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public static void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}