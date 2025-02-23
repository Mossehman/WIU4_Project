public class OxygenTankInteraction : InteractionObject
{
    public float oxygenAmount = 100f;

    public override string GetCustomDescription()
    {
        return $"Oxygen Left: {oxygenAmount}%";
    }

    public override void Interact()
    {
        PlayerStats.Instance.IncreaseStat(PlayerStats.StatType.Oxygen, 10f);
        oxygenAmount -= 10f;
    }
}