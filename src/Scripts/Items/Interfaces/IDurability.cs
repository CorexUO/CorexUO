namespace Server.Items
{
	public interface IDurability
	{
		bool CanFortify { get; }

		int InitMinHits { get; }
		int InitMaxHits { get; }

		int HitPoints { get; set; }
		int MaxHitPoints { get; set; }

		void ScaleDurability();
		void UnscaleDurability();
	}
}
