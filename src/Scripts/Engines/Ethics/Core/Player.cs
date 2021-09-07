using Server.Mobiles;
using System;

namespace Server.Ethics
{
	public class PlayerCollection : System.Collections.ObjectModel.Collection<Player>
	{
	}

	[PropertyObject]
	public class Player
	{
		public static Player Find(Mobile mob)
		{
			return Find(mob, false);
		}

		public static Player Find(Mobile mob, bool inherit)
		{
			PlayerMobile pm = mob as PlayerMobile;

			if (pm == null)
			{
				if (inherit && mob is BaseCreature)
				{
					BaseCreature bc = mob as BaseCreature;

					if (bc != null && bc.Controlled)
						pm = bc.ControlMaster as PlayerMobile;
					else if (bc != null && bc.Summoned)
						pm = bc.SummonMaster as PlayerMobile;
				}

				if (pm == null)
					return null;
			}

			Player pl = pm.EthicPlayer;

			if (pl != null && !pl.Ethic.IsEligible(pl.Mobile))
				pm.EthicPlayer = pl = null;

			return pl;
		}

		private DateTime m_Shield;

		public Ethic Ethic { get; }
		public Mobile Mobile { get; }

		[CommandProperty(AccessLevel.GameMaster, AccessLevel.Administrator)]
		public int Power { get; set; }

		[CommandProperty(AccessLevel.GameMaster, AccessLevel.Administrator)]
		public int History { get; set; }

		[CommandProperty(AccessLevel.GameMaster, AccessLevel.Administrator)]
		public Mobile Steed { get; set; }

		[CommandProperty(AccessLevel.GameMaster, AccessLevel.Administrator)]
		public Mobile Familiar { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public bool IsShielded
		{
			get
			{
				if (m_Shield == DateTime.MinValue)
					return false;

				if (DateTime.UtcNow < (m_Shield + TimeSpan.FromHours(1.0)))
					return true;

				FinishShield();
				return false;
			}
		}

		public void BeginShield()
		{
			m_Shield = DateTime.UtcNow;
		}

		public void FinishShield()
		{
			m_Shield = DateTime.MinValue;
		}

		public Player(Ethic ethic, Mobile mobile)
		{
			Ethic = ethic;
			Mobile = mobile;

			Power = 5;
			History = 5;
		}

		public void CheckAttach()
		{
			if (Ethic.IsEligible(Mobile))
				Attach();
		}

		public void Attach()
		{
			if (Mobile is PlayerMobile)
				(Mobile as PlayerMobile).EthicPlayer = this;

			Ethic.Players.Add(this);
		}

		public void Detach()
		{
			if (Mobile is PlayerMobile)
				(Mobile as PlayerMobile).EthicPlayer = null;

			Ethic.Players.Remove(this);
		}

		public Player(Ethic ethic, GenericReader reader)
		{
			Ethic = ethic;

			int version = reader.ReadEncodedInt();

			switch (version)
			{
				case 0:
					{
						Mobile = reader.ReadMobile();

						Power = reader.ReadEncodedInt();
						History = reader.ReadEncodedInt();

						Steed = reader.ReadMobile();
						Familiar = reader.ReadMobile();

						m_Shield = reader.ReadDeltaTime();

						break;
					}
			}
		}

		public void Serialize(GenericWriter writer)
		{
			writer.WriteEncodedInt(0); // version

			writer.Write(Mobile);

			writer.WriteEncodedInt(Power);
			writer.WriteEncodedInt(History);

			writer.Write(Steed);
			writer.Write(Familiar);

			writer.WriteDeltaTime(m_Shield);
		}
	}
}
