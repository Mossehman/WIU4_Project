using UnityEngine;

public class ProximityDoor : MonoBehaviour
{
    public Transform player; // Assign the player object in the Inspector
    public float activationDistance = 3f; // Distance at which the door opens
    public float deactivationDistance = 5f; // Distance at which the door closes
    public string animationBool = "IsOpen"; // Animator parameter name

    [SerializeField] private Animator animator;
    private bool isOpen = false;

    void Update()
    {
        if (player == null) return;

        float distance = Vector3.Distance(transform.position, player.position);

        if (!isOpen && distance <= activationDistance)
        {
            Open();
        }
        else if (isOpen && distance > deactivationDistance)
        {
            Close();
        }
    }

    void Open()
    {
        isOpen = true;
        animator.SetBool(animationBool, true);
    }

    void Close()
    {
        isOpen = false;
        animator.SetBool(animationBool, false);
    }
}