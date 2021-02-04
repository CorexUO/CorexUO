using Server.Misc;
using Server.Spells;
using System;

namespace Server.Mobiles
{
	public abstract partial class BaseMobile : Mobile
	{
		public static readonly TimeSpan COMBAT_HEAT_DELAY = TimeSpan.FromSeconds(30.0);

		public BaseMobile() : base()
		{
		}

		public BaseMobile(Serial serial) : base(serial)
		{
		}

		public override double ArmorRating
		{
			get
			{
				return VirtualArmor + VirtualArmorMod;
			}
		}

		public override bool OnDragLift(Item item)
		{
			//Only check if the item don't have parent or the parent is different to the player mobile
			if (item.Parent == null || (item.Parent != null && item.RootParent != Backpack && item.RootParent != this))
			{
				if (WeightOverloading.IsOverloaded(this))
				{
					SendMessage(0x22, "You are too heavy to carry more weight.");
					return false;
				}
			}

			return base.OnDragLift(item);
		}

		public bool CheckCombat()
		{
			for (int i = 0; i < Aggressed.Count; ++i)
			{
				AggressorInfo info = Aggressed[i];

				if (info.Defender.Player && (DateTime.UtcNow - info.LastCombatTime) < COMBAT_HEAT_DELAY)
					return true;
			}

			if (Core.AOS)
			{
				for (int i = 0; i < Aggressors.Count; ++i)
				{
					AggressorInfo info = Aggressors[i];

					if (info.Attacker.Player && (DateTime.UtcNow - info.LastCombatTime) < COMBAT_HEAT_DELAY)
						return true;
				}
			}

			return false;
		}

		/// <summary>
		/// Overridable. Virtual event when the Mobile is killed by
		/// </summary>
		/// <param name="mob"></param>
		public virtual void OnKilledBy(Mobile mob)
		{
		}

		/// <summary>
		/// Overridable. Virtual event when the mobile hit to defender with specify damage
		/// </summary>
		/// <param name="mob"></param>
		public virtual void OnHit(Mobile defender, int damage)
		{
		}

		/// <summary>
		/// Overridable. Virtual event when the Mobile got a Melee Attack
		/// </summary>
		/// <param name="attacker"></param>
		public virtual void OnGotMeleeAttack(Mobile attacker)
		{
		}

		/// <summary>
		/// Overridable. Virtual event when the Mobile gave a Melee Attack
		/// </summary>
		/// <param name="defender"></param>
		public virtual void OnGaveMeleeAttack(Mobile defender)
		{
		}

		/// <summary>
		/// Overridable. Virtual event when the Mobile is damaged by Spell
		/// </summary>
		/// <param name="from"></param>
		public virtual void OnDamagedBySpell(Mobile from, Spell spell, int damage)
		{
		}

		/// <summary>
		/// Overridable. Virtual event when the Mobile is hit by OnHarmfulSpell
		/// </summary>
		/// <param name="from"></param>
		public virtual void OnHarmfulSpell(Mobile from, Spell spell = null)
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
