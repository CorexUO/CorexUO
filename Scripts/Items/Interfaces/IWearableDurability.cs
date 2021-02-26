namespace Server.Items
{
	public interface IWearableDurability : IDurability
	{
		int OnHit(BaseWeapon weapon, int damageTaken);
	}
}
