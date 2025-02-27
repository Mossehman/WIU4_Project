using TMPro;
using UnityEngine;

public class CompassManager : MonoBehaviour
{
    [SerializeField] GameObject player;

    [SerializeField] private TextMeshProUGUI compassText;

    void Update()
    {
        compassText.text = $"{Mathf.FloorToInt(player.transform.position.x)}, {Mathf.FloorToInt(player.transform.position.y)}, {Mathf.FloorToInt(player.transform.position.z)}";
    }
}