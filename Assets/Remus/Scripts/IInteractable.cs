using UnityEngine;

public interface IInteractable
{
    void Interact();
    Sprite GetInteractionIcon();
    string GetInteractionText();
    string GetCustomDescription();
}