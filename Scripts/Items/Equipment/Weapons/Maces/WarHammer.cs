namespace Server.Items
{
	[FlipableAttribute(0x1439, 0x1438)]
	public class WarHammer : BaseBashing
	{
		public override WeaponAbility PrimaryAbility { get { return WeaponAbility.WhirlwindAttack; } }
		public override WeaponAbility SecondaryAbility { get { return WeaponAbility.CrushingBlow; } }

		public override int StrReq { get { return Core.AOS ? 95 : 40; } }

		public override int MinDamageBase { get { return Core.AOS ? 17 : 8; } }
		public override int MaxDamageBase { get { return Core.AOS ? 18 : 36; } }
		public override float SpeedBase { get { return Core.ML ? 3.75f : Core.AOS ? 28 : 31; } }

		public override int InitMinHits { get { return 31; } }
		public override int InitMaxHits { get { return 110; } }

		public override WeaponAnimation DefAnimation { get { return WeaponAnimation.Bash2H; } }

		[Constructable]
		public WarHammer() : base(0x1439)
		{
			Weight = 10.0;
			Layer = Layer.TwoHanded;
		}

		public WarHammer(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write((int)0); // version
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadInt();
		}
	}
}
