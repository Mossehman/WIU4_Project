public class OxygenTankInteraction : InteractionObject
{
    public override void Interact()
    {
        PlayerStats.Instance.IncreaseStat(PlayerStats.StatType.Oxygen, 10f);
    }
}