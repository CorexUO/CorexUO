using System;

namespace Server.Factions
{
	public class FactionGasTrap : BaseFactionTrap
	{
		public override int LabelNumber => 1044598;  // faction gas trap

		public override int AttackMessage => 1010542;  // A noxious green cloud of poison gas envelops you!
		public override int DisarmMessage => 502376;  // The poison leaks harmlessly away due to your deft touch.
		public override int EffectSound => 0x230;
		public override int MessageHue => 0x44;

		public override AllowedPlacing AllowedPlacing => AllowedPlacing.FactionStronghold;

		public override void DoVisibleEffect()
		{
			Effects.SendLocationEffect(Location, Map, 0x3709, 28, 10, 0x1D3, 5);
		}

		public override void DoAttackEffect(Mobile m)
		{
			m.ApplyPoison(m, Poison.Lethal);
		}

		[Constructable]
		public FactionGasTrap() : this(null)
		{
		}

		public FactionGasTrap(Faction f) : this(f, null)
		{
		}

		public FactionGasTrap(Faction f, Mobile m) : base(f, m, 0x113C)
		{
		}

		public FactionGasTrap(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0); // version
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadInt();
		}
	}

	public class FactionGasTrapDeed : BaseFactionTrapDeed
	{
		public override Type TrapType => typeof(FactionGasTrap);
		public override int LabelNumber => 1044602;  // faction gas trap deed

		public FactionGasTrapDeed() : base(0x11AB)
		{
		}

		public FactionGasTrapDeed(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0); // version
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);
			int version = reader.ReadInt();
		}
	}
}
