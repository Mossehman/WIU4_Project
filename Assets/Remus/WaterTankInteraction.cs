public class WaterTankInteraction : InteractionObject
{
    public override void Interact()
    {
        PlayerStats.Instance.IncreaseStat(PlayerStats.StatType.Water, 10f);
    }
}