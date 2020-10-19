using System;
using System.Collections.Generic;
using Server.Engines.Craft;

namespace Server.Items
{
	public enum PotionEffect
	{
		Nightsight,
		CureLesser,
		Cure,
		CureGreater,
		Agility,
		AgilityGreater,
		Strength,
		StrengthGreater,
		PoisonLesser,
		Poison,
		PoisonGreater,
		PoisonDeadly,
		Refresh,
		RefreshTotal,
		HealLesser,
		Heal,
		HealGreater,
		ExplosionLesser,
		Explosion,
		ExplosionGreater,
		Conflagration,
		ConflagrationGreater,
		MaskOfDeath,        // Mask of Death is not available in OSI but does exist in cliloc files
		MaskOfDeathGreater, // included in enumeration for compatability if later enabled by OSI
		ConfusionBlast,
		ConfusionBlastGreater,
		Invisibility,
		Parasitic,
		Darkglow,
	}

	public abstract class BasePotion : BaseItem, ICraftable, ICommodity
	{
		public virtual bool RequireFreeHand { get { return Settings.Get<bool>("Gameplay", "PotionsRequiredFreeHands"); } }

		public override int LabelNumber { get { return 1041314 + (int)m_PotionEffect; } }

		int ICommodity.DescriptionNumber { get { return LabelNumber; } }
		bool ICommodity.IsDeedable { get { return (Core.ML); } }

		[CommandProperty(AccessLevel.GameMaster)]
		public Mobile Crafter { get; set; }

		private PotionEffect m_PotionEffect;

		public PotionEffect PotionEffect
		{
			get
			{
				return m_PotionEffect;
			}
			set
			{
				m_PotionEffect = value;
				InvalidateProperties();
			}
		}

		public BasePotion(int itemID, PotionEffect effect) : base(itemID)
		{
			m_PotionEffect = effect;

			Stackable = Core.ML;
			Weight = 1.0;
		}

		public BasePotion(Serial serial) : base(serial)
		{
		}

		public static bool HasFreeHand(Mobile m)
		{
			Item handOne = m.FindItemOnLayer(Layer.OneHanded);
			Item handTwo = m.FindItemOnLayer(Layer.TwoHanded);

			if (handTwo is BaseWeapon)
				handOne = handTwo;
			if (handTwo is BaseRanged)
			{
				BaseRanged ranged = (BaseRanged)handTwo;

				if (ranged.Balanced)
					return true;
			}

			return (handOne == null || handTwo == null);
		}

		public override void OnDoubleClick(Mobile from)
		{
			if (!Movable)
				return;

			if (from.InRange(this.GetWorldLocation(), 1))
			{
				if (!RequireFreeHand || HasFreeHand(from))
				{
					if (this is BaseExplosionPotion && Amount > 1)
					{
						BasePotion pot = (BasePotion)Activator.CreateInstance(this.GetType());

						if (pot != null)
						{
							Amount--;

							if (from.Backpack != null && !from.Backpack.Deleted)
							{
								from.Backpack.DropItem(pot);
							}
							else
							{
								pot.MoveToWorld(from.Location, from.Map);
							}
							pot.Drink(from);
						}
					}
					else
					{
						this.Drink(from);
					}
				}
				else
				{
					from.SendLocalizedMessage(502172); // You must have a free hand to drink a potion.
				}
			}
			else
			{
				from.SendLocalizedMessage(502138); // That is too far away for you to use
			}
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write((int)0); // version

			writer.Write((int)m_PotionEffect);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadInt();

			switch (version)
			{
				case 0:
					{
						m_PotionEffect = (PotionEffect)reader.ReadInt();
						break;
					}
			}
		}

		public abstract void Drink(Mobile from);

		public static void PlayDrinkEffect(Mobile m)
		{
			m.RevealingAction();

			m.PlaySound(0x2D6);

			#region Dueling
			if (!Engines.ConPVP.DuelContext.IsFreeConsume(m))
				m.AddToBackpack(new Bottle());
			#endregion

			if (m.Body.IsHuman && !m.Mounted)
				m.Animate(34, 5, 1, true, false, 0);
		}

		public static int EnhancePotions(Mobile m)
		{
			int EP = AosAttributes.GetValue(m, AosAttribute.EnhancePotions);
			int skillBonus = m.Skills.Alchemy.Fixed / 330 * 10;

			if (Core.ML && EP > 50 && m.AccessLevel <= AccessLevel.Player)
				EP = 50;

			return (EP + skillBonus);
		}

		public static TimeSpan Scale(Mobile m, TimeSpan v)
		{
			if (!Core.AOS)
				return v;

			double scalar = 1.0 + (0.01 * EnhancePotions(m));

			return TimeSpan.FromSeconds(v.TotalSeconds * scalar);
		}

		public static double Scale(Mobile m, double v)
		{
			if (!Core.AOS)
				return v;

			double scalar = 1.0 + (0.01 * EnhancePotions(m));

			return v * scalar;
		}

		public static int Scale(Mobile m, int v)
		{
			if (!Core.AOS)
				return v;

			return AOS.Scale(v, 100 + EnhancePotions(m));
		}

		public override bool StackWith(Mobile from, Item dropped, bool playSound)
		{
			if (dropped is BasePotion && ((BasePotion)dropped).m_PotionEffect == m_PotionEffect)
				return base.StackWith(from, dropped, playSound);

			return false;
		}

		#region ICraftable Members

		public ItemQuality OnCraft(ItemQuality quality, bool makersMark, Mobile from, CraftSystem craftSystem, Type typeRes, BaseTool tool, CraftItem craftItem, int resHue)
		{
			if (craftSystem is DefAlchemy)
			{
				Container pack = from.Backpack;

				if (pack != null)
				{
					if ((int)PotionEffect >= (int)PotionEffect.Invisibility)
						return ItemQuality.Normal;

					List<PotionKeg> kegs = pack.FindItemsByType<PotionKeg>();

					for (int i = 0; i < kegs.Count; ++i)
					{
						PotionKeg keg = kegs[i];

						if (keg == null)
							continue;

						if (keg.Held <= 0 || keg.Held >= 100)
							continue;

						if (keg.Type != PotionEffect)
							continue;

						++keg.Held;

						Consume();
						from.AddToBackpack(new Bottle());

						return ItemQuality.Low; // signal placed in keg
					}
				}
			}

			return ItemQuality.Normal; ;
		}

		#endregion
	}
}
