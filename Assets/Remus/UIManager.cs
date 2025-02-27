using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static bool IsAnyMenuOpen { get; private set; } = false;
    public static bool IsInventoryOpen { get; private set; } = false;
    public static bool IsCraftingOpen { get; private set; } = false;

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