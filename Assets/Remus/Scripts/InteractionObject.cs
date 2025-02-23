using UnityEngine;

public class InteractionObject : MonoBehaviour, IInteractable
{
    public string interactionText = "Press E to Interact";
    public Sprite interactionIcon;

    public virtual Sprite GetInteractionIcon()
    {
        return interactionIcon;
    }


    public virtual string GetInteractionText()
    {
        return interactionText;
    }

    public virtual string GetCustomDescription()
    {
        return "• Generic Object";
    }

    public virtual void Interact()
    {
        Debug.Log("Interacted with " + gameObject.name);
    }
}