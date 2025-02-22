using UnityEngine;

public class InteractionObject : MonoBehaviour, IInteractable
{
    public string interactionText = "Press E to Interact";

    public virtual string GetInteractionText()
    {
        return interactionText;
    }

    public virtual void Interact()
    {
        Debug.Log("Interacted with " + gameObject.name);
        // Default interaction logic (if needed)
    }
}