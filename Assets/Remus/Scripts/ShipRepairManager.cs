using UnityEngine;
using UnityEngine.SceneManagement; // Required for scene switching

public class ShipRepairManager : MonoBehaviour
{
    public static ShipRepairManager Instance; // Singleton instance

    private bool engineRepaired = false;
    private bool thrusterRepaired = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void MarkEngineRepaired()
    {
        engineRepaired = true;
        CheckForCompletion();
    }

    public void MarkThrusterRepaired()
    {
        thrusterRepaired = true;
        CheckForCompletion();
    }

    private void CheckForCompletion()
    {
        if (engineRepaired && thrusterRepaired)
        {
            Debug.Log("Both components repaired! Switching to next scene...");
            SceneManager.LoadScene("EndCutscene");
        }
    }
}