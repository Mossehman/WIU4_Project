using UnityEngine;

public class ProximityDoor : MonoBehaviour
{
    public Transform player;
    public float activationDistance = 3f;
    public float deactivationDistance = 5f;
    public string animationBool = "IsOpen";

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