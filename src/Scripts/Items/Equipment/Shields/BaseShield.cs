using Server.Network;

namespace Server.Items
{
	public class BaseShield : BaseArmor
	{
		public override ArmorMaterialType MaterialType => ArmorMaterialType.Plate;

		public BaseShield(int itemID) : base(itemID)
		{
		}

		public BaseShield(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0);//version
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadInt();
		}

		public override double ArmorRating
		{
			get
			{
				double ar = base.ArmorRating;

				if (Parent is Mobile m)
					return (m.Skills[SkillName.Parry].Value * ar / 200.0) + 1.0;
				else
					return ar;
			}
		}

		public override int OnHit(BaseWeapon weapon, int damage)
		{
			if (Core.AOS)
			{
				if (ArmorAttributes.SelfRepair > Utility.Random(10))
				{
					HitPoints += 2;
				}
				else
				{
					double halfArmor = ArmorRating / 2.0;
					int absorbed = (int)(halfArmor + (halfArmor * Utility.RandomDouble()));

					if (absorbed < 2)
						absorbed = 2;

					int wear;

					if (weapon.Type == WeaponType.Bashing)
						wear = absorbed / 2;
					else
						wear = Utility.Random(2);

					if (wear > 0 && MaxHitPoints > 0)
					{
						if (HitPoints >= wear)
						{
							HitPoints -= wear;
							wear = 0;
						}
						else
						{
							wear -= HitPoints;
							HitPoints = 0;
						}

						if (wear > 0)
						{
							if (MaxHitPoints > wear)
							{
								MaxHitPoints -= wear;

								if (Parent is Mobile)
									((Mobile)Parent).LocalOverheadMessage(MessageType.Regular, 0x3B2, 1061121); // Your equipment is severely damaged.
							}
							else
							{
								Delete();
							}
						}
					}
				}

				return 0;
			}
			else
			{
				if (Parent is not Mobile owner)
					return damage;

				double chance = owner.GetHitBlockChance();

				if (chance < 0.01)
					chance = 0.01;
				/*
				FORMULA: Displayed AR = ((Parrying Skill * Base AR of Shield) <F7> 200) + 1

				FORMULA: % Chance of Blocking = parry skill - (shieldAR * 2)

				FORMULA: Melee Damage Absorbed = (AR of Shield) / 2 | Archery Damage Absorbed = AR of Shield
				*/
				if (owner.CheckSkill(SkillName.Parry, chance))
				{
					if (weapon.Skill == SkillName.Archery)
						damage -= (int)ArmorRating;
					else
						damage -= (int)(ArmorRating / 2.0);

					if (damage < 0)
						damage = 0;

					owner.FixedEffect(0x37B9, 10, 16);

					if (25 > Utility.Random(100)) // 25% chance to lower durability
					{
						int wear = Utility.Random(2);

						if (wear > 0 && MaxHitPoints > 0)
						{
							if (HitPoints >= wear)
							{
								HitPoints -= wear;
								wear = 0;
							}
							else
							{
								wear -= HitPoints;
								HitPoints = 0;
							}

							if (wear > 0)
							{
								if (MaxHitPoints > wear)
								{
									MaxHitPoints -= wear;

									if (Parent is Mobile)
										((Mobile)Parent).LocalOverheadMessage(MessageType.Regular, 0x3B2, 1061121); // Your equipment is severely damaged.
								}
								else
								{
									Delete();
								}
							}
						}
					}
				}

				return damage;
			}
		}
	}
}
