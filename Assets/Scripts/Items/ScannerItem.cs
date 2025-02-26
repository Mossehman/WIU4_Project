using UnityEngine;

[CreateAssetMenu(fileName = "New Scanner Item", menuName = "Items/Scanner Item")]
public class ScannerItem : BaseItem
{

    public override void Update()
    {
        TerrainScanner.isHoldingScanner = isHeld;
    }
}
