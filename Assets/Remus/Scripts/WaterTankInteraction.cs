public class WaterTankInteraction : InteractionObject
{
    public float waterAmount = 50f; // Remaining water

    public override string GetCustomDescription()
    {
        return $"Water Left: {waterAmount}L";
    }

    public override void Interact()
    {
        waterAmount -= 10f;
        PlayerStats.Instance.IncreaseStat(PlayerStats.StatType.Water, 10f);
    }
}