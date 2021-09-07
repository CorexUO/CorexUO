using System;

namespace Server.Items
{
	public enum TrapType
	{
		None,
		MagicTrap,
		ExplosionTrap,
		DartTrap,
		PoisonTrap
	}

	public abstract class TrapableContainer : BaseContainer, ITelekinesisable
	{
		[CommandProperty(AccessLevel.GameMaster)]
		public TrapType TrapType { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public int TrapPower { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public int TrapLevel { get; set; }

		public virtual bool TrapOnOpen => true;

		public TrapableContainer(int itemID) : base(itemID)
		{
		}

		public TrapableContainer(Serial serial) : base(serial)
		{
		}

		private void SendMessageTo(Mobile to, int number, int hue)
		{
			if (Deleted || !to.CanSee(this))
				return;

			to.Send(new Network.MessageLocalized(Serial, ItemID, Network.MessageType.Regular, hue, 3, number, "", ""));
		}

		private void SendMessageTo(Mobile to, string text, int hue)
		{
			if (Deleted || !to.CanSee(this))
				return;

			to.Send(new Network.UnicodeMessage(Serial, ItemID, Network.MessageType.Regular, hue, 3, "ENU", "", text));
		}

		public virtual bool ExecuteTrap(Mobile from)
		{
			if (TrapType != TrapType.None)
			{
				Point3D loc = GetWorldLocation();
				Map facet = Map;

				if (from.AccessLevel >= AccessLevel.GameMaster)
				{
					SendMessageTo(from, "That is trapped, but you open it with your godly powers.", 0x3B2);
					return false;
				}

				switch (TrapType)
				{
					case TrapType.ExplosionTrap:
						{
							SendMessageTo(from, 502999, 0x3B2); // You set off a trap!

							if (from.InRange(loc, 3))
							{
								int damage;

								if (TrapLevel > 0)
									damage = Utility.RandomMinMax(10, 30) * TrapLevel;
								else
									damage = TrapPower;

								AOS.Damage(from, damage, 0, 100, 0, 0, 0);

								// Your skin blisters from the heat!
								from.LocalOverheadMessage(Network.MessageType.Regular, 0x2A, 503000);
							}

							Effects.SendLocationEffect(loc, facet, 0x36BD, 15, 10);
							Effects.PlaySound(loc, facet, 0x307);

							break;
						}
					case TrapType.MagicTrap:
						{
							if (from.InRange(loc, 1))
								from.Damage(TrapPower);
							//AOS.Damage( from, m_TrapPower, 0, 100, 0, 0, 0 );

							Effects.PlaySound(loc, Map, 0x307);

							Effects.SendLocationEffect(new Point3D(loc.X - 1, loc.Y, loc.Z), Map, 0x36BD, 15);
							Effects.SendLocationEffect(new Point3D(loc.X + 1, loc.Y, loc.Z), Map, 0x36BD, 15);

							Effects.SendLocationEffect(new Point3D(loc.X, loc.Y - 1, loc.Z), Map, 0x36BD, 15);
							Effects.SendLocationEffect(new Point3D(loc.X, loc.Y + 1, loc.Z), Map, 0x36BD, 15);

							Effects.SendLocationEffect(new Point3D(loc.X + 1, loc.Y + 1, loc.Z + 11), Map, 0x36BD, 15);

							break;
						}
					case TrapType.DartTrap:
						{
							SendMessageTo(from, 502999, 0x3B2); // You set off a trap!

							if (from.InRange(loc, 3))
							{
								int damage;

								if (TrapLevel > 0)
									damage = Utility.RandomMinMax(5, 15) * TrapLevel;
								else
									damage = TrapPower;

								AOS.Damage(from, damage, 100, 0, 0, 0, 0);

								// A dart imbeds itself in your flesh!
								from.LocalOverheadMessage(Network.MessageType.Regular, 0x62, 502998);
							}

							Effects.PlaySound(loc, facet, 0x223);

							break;
						}
					case TrapType.PoisonTrap:
						{
							SendMessageTo(from, 502999, 0x3B2); // You set off a trap!

							if (from.InRange(loc, 3))
							{
								Poison poison;

								if (TrapLevel > 0)
								{
									poison = Poison.GetPoison(Math.Max(0, Math.Min(4, TrapLevel - 1)));
								}
								else
								{
									AOS.Damage(from, TrapPower, 0, 0, 0, 100, 0);
									poison = Poison.Greater;
								}

								from.ApplyPoison(from, poison);

								// You are enveloped in a noxious green cloud!
								from.LocalOverheadMessage(Network.MessageType.Regular, 0x44, 503004);
							}

							Effects.SendLocationEffect(loc, facet, 0x113A, 10, 20);
							Effects.PlaySound(loc, facet, 0x231);

							break;
						}
				}

				TrapType = TrapType.None;
				TrapPower = 0;
				TrapLevel = 0;
				return true;
			}

			return false;
		}

		public virtual void OnTelekinesis(Mobile from)
		{
			Effects.SendLocationParticles(EffectItem.Create(Location, Map, EffectItem.DefaultDuration), 0x376A, 9, 32, 5022);
			Effects.PlaySound(Location, Map, 0x1F5);

			if (TrapOnOpen)
			{
				ExecuteTrap(from);
			}
		}

		public override void Open(Mobile from)
		{
			if (!TrapOnOpen || !ExecuteTrap(from))
				base.Open(from);
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0); // version

			writer.Write(TrapLevel);

			writer.Write(TrapPower);
			writer.Write((int)TrapType);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadInt();

			switch (version)
			{
				case 0:
					{
						TrapLevel = reader.ReadInt();

						TrapPower = reader.ReadInt();
						TrapType = (TrapType)reader.ReadInt();
						break;
					}
			}
		}
	}
}
