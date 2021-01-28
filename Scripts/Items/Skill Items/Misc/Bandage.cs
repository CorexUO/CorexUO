using System;
using System.Collections.Generic;
using Server.Gumps;
using Server.Mobiles;
using Server.Targeting;

namespace Server.Items
{
	public class Bandage : BaseItem, IDyable
	{
		public static int Range = Core.AOS ? 2 : 1;

		public override double DefaultWeight
		{
			get { return 0.1; }
		}

		public static void Initialize()
		{
			EventSink.BandageTargetRequest += new BandageTargetRequestEventHandler(EventSink_BandageTargetRequest);
		}

		[Constructable]
		public Bandage() : this(1)
		{
		}

		[Constructable]
		public Bandage(int amount) : base(0xE21)
		{
			Stackable = true;
			Amount = amount;
		}

		public Bandage(Serial serial) : base(serial)
		{
		}

		public virtual bool Dye(Mobile from, DyeTub sender)
		{
			if (Deleted)
				return false;

			Hue = sender.DyedHue;

			return true;
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write((int)0); // version
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			reader.ReadInt();
		}

		public override void OnDoubleClick(Mobile from)
		{
			if (from.InRange(GetWorldLocation(), Range))
			{
				from.RevealingAction();

				from.SendLocalizedMessage(500948); // Who will you use the bandages on?

				from.Target = new InternalTarget(this);
			}
			else
			{
				from.SendLocalizedMessage(500295); // You are too far away to do that.
			}
		}

		private static void EventSink_BandageTargetRequest(BandageTargetRequestEventArgs e)
		{
			if (e.Bandage is not Bandage b || b.Deleted)
				return;

			Mobile from = e.Mobile;

			if (from.InRange(b.GetWorldLocation(), Range))
			{
				Target t = from.Target;

				if (t != null)
				{
					Target.Cancel(from);
					from.Target = null;
				}

				from.RevealingAction();

				from.SendLocalizedMessage(500948); // Who will you use the bandages on?

				new InternalTarget(b).Invoke(from, e.Target);
			}
			else
			{
				from.SendLocalizedMessage(500295); // You are too far away to do that.
			}
		}

		private class InternalTarget : Target
		{
			private readonly Bandage m_Bandage;

			public InternalTarget(Bandage bandage) : base(Bandage.Range, false, TargetFlags.Beneficial)
			{
				m_Bandage = bandage;
			}

			protected override void OnTarget(Mobile from, object targeted)
			{
				if (m_Bandage.Deleted)
					return;

				if (targeted is Mobile mobile)
				{
					if (from.InRange(m_Bandage.GetWorldLocation(), Bandage.Range))
					{
						if (BandageContext.BeginHeal(from, mobile) != null)
						{
							if (!Engines.ConPVP.DuelContext.IsFreeConsume(from))
								m_Bandage.Consume();
						}
					}
					else
					{
						from.SendLocalizedMessage(500295); // You are too far away to do that.
					}
				}
				else if (targeted is PlagueBeastInnard innard)
				{
					if (innard.OnBandage(from))
						m_Bandage.Consume();
				}
				else
				{
					from.SendLocalizedMessage(500970); // Bandages can not be used on that.
				}
			}

			protected override void OnNonlocalTarget(Mobile from, object targeted)
			{
				if (targeted is PlagueBeastInnard innard)
				{
					if (innard.OnBandage(from))
						m_Bandage.Consume();
				}
				else
					base.OnNonlocalTarget(from, targeted);
			}
		}
	}

	public class BandageContext
	{
		public Mobile Healer { get; }
		public Mobile Patient { get; }
		public int Slips { get; set; }
		public Timer Timer { get; private set; }

		public void Slip()
		{
			Healer.SendLocalizedMessage(500961); // Your fingers slip!
			++Slips;
		}

		public BandageContext(Mobile healer, Mobile patient, TimeSpan delay)
		{
			Healer = healer;
			Patient = patient;

			Timer = new InternalTimer(this, delay);
			Timer.Start();
		}

		public void StopHeal()
		{
			m_Table.Remove(Healer);

			if (Timer != null)
				Timer.Stop();

			Timer = null;
		}

		private static readonly Dictionary<Mobile, BandageContext> m_Table = new Dictionary<Mobile, BandageContext>();

		public static BandageContext GetContext(Mobile healer)
		{
			m_Table.TryGetValue(healer, out BandageContext bc);
			return bc;
		}

		public static SkillName GetPrimarySkill(Mobile m)
		{
			if (!m.Player && (m.Body.IsMonster || m.Body.IsAnimal))
				return SkillName.Veterinary;
			else
				return SkillName.Healing;
		}

		public static SkillName GetSecondarySkill(Mobile m)
		{
			if (!m.Player && (m.Body.IsMonster || m.Body.IsAnimal))
				return SkillName.AnimalLore;
			else
				return SkillName.Anatomy;
		}

		public void EndHeal()
		{
			StopHeal();

			int healerNumber, patientNumber;
			bool playSound = true;
			bool checkSkills = false;

			SkillName primarySkill = GetPrimarySkill(Patient);
			SkillName secondarySkill = GetSecondarySkill(Patient);

			BaseCreature petPatient = Patient as BaseCreature;
			if (!Healer.Alive)
			{
				healerNumber = 500962; // You were unable to finish your work before you died.
				patientNumber = -1;
				playSound = false;
			}
			else if (!Healer.InRange(Patient, Bandage.Range))
			{
				healerNumber = 500963; // You did not stay close enough to heal your target.
				patientNumber = -1;
				playSound = false;
			}
			else if (!Patient.Alive || (petPatient != null && petPatient.IsDeadPet))
			{
				double healing = Healer.Skills[primarySkill].Value;
				double anatomy = Healer.Skills[secondarySkill].Value;
				double chance = ((healing - 68.0) / 50.0) - (Slips * 0.02);

				if (((checkSkills = (healing >= 80.0 && anatomy >= 80.0)) && chance > Utility.RandomDouble())
					  || (Core.SE && petPatient is Factions.FactionWarHorse && petPatient.ControlMaster == Healer))   //TODO: Dbl check doesn't check for faction of the horse here?
				{
					if (Patient.Map == null || !Patient.Map.CanFit(Patient.Location, 16, false, false))
					{
						healerNumber = 501042; // Target can not be resurrected at that location.
						patientNumber = 502391; // Thou can not be resurrected there!
					}
					else if (Patient.Region != null && Patient.Region.IsPartOf("Khaldun"))
					{
						healerNumber = 1010395; // The veil of death in this area is too strong and resists thy efforts to restore life.
						patientNumber = -1;
					}
					else
					{
						healerNumber = 500965; // You are able to resurrect your patient.
						patientNumber = -1;

						Patient.PlaySound(0x214);
						Patient.FixedEffect(0x376A, 10, 16);

						if (petPatient != null && petPatient.IsDeadPet)
						{
							Mobile master = petPatient.ControlMaster;

							if (master != null && Healer == master)
							{
								petPatient.ResurrectPet();

								for (int i = 0; i < petPatient.Skills.Length; ++i)
								{
									petPatient.Skills[i].Base -= 0.1;
								}
							}
							else if (master != null && master.InRange(petPatient, 3))
							{
								healerNumber = 503255; // You are able to resurrect the creature.

								master.CloseGump(typeof(PetResurrectGump));
								master.SendGump(new PetResurrectGump(Healer, petPatient));
							}
							else
							{
								bool found = false;

								List<Mobile> friends = petPatient.Friends;

								for (int i = 0; friends != null && i < friends.Count; ++i)
								{
									Mobile friend = friends[i];

									if (friend.InRange(petPatient, 3))
									{
										healerNumber = 503255; // You are able to resurrect the creature.

										friend.CloseGump(typeof(PetResurrectGump));
										friend.SendGump(new PetResurrectGump(Healer, petPatient));

										found = true;
										break;
									}
								}

								if (!found)
									healerNumber = 1049670; // The pet's owner must be nearby to attempt resurrection.
							}
						}
						else
						{
							Patient.CloseGump(typeof(ResurrectGump));
							Patient.SendGump(new ResurrectGump(Patient, Healer));
						}
					}
				}
				else
				{
					if (petPatient != null && petPatient.IsDeadPet)
						healerNumber = 503256; // You fail to resurrect the creature.
					else
						healerNumber = 500966; // You are unable to resurrect your patient.

					patientNumber = -1;
				}
			}
			else if (Patient.Poisoned)
			{
				Healer.SendLocalizedMessage(500969); // You finish applying the bandages.

				double healing = Healer.Skills[primarySkill].Value;
				double anatomy = Healer.Skills[secondarySkill].Value;
				double chance = ((healing - 30.0) / 50.0) - (Patient.Poison.Level * 0.1) - (Slips * 0.02);

				if ((checkSkills = (healing >= 60.0 && anatomy >= 60.0)) && chance > Utility.RandomDouble())
				{
					if (Patient.CurePoison(Healer))
					{
						healerNumber = (Healer == Patient) ? -1 : 1010058; // You have cured the target of all poisons.
						patientNumber = 1010059; // You have been cured of all poisons.
					}
					else
					{
						healerNumber = -1;
						patientNumber = -1;
					}
				}
				else
				{
					healerNumber = 1010060; // You have failed to cure your target!
					patientNumber = -1;
				}
			}
			else if (BleedAttack.IsBleeding(Patient))
			{
				healerNumber = 1060088; // You bind the wound and stop the bleeding
				patientNumber = 1060167; // The bleeding wounds have healed, you are no longer bleeding!

				BleedAttack.EndBleed(Patient, false);
			}
			else if (MortalStrike.IsWounded(Patient))
			{
				healerNumber = (Healer == Patient ? 1005000 : 1010398);
				patientNumber = -1;
				playSound = false;
			}
			else if (Patient.Hits == Patient.HitsMax)
			{
				healerNumber = 500967; // You heal what little damage your patient had.
				patientNumber = -1;
			}
			else
			{
				checkSkills = true;
				patientNumber = -1;

				double healing = Healer.Skills[primarySkill].Value;
				double anatomy = Healer.Skills[secondarySkill].Value;
				double chance = ((healing + 10.0) / 100.0) - (Slips * 0.02);

				if (chance > Utility.RandomDouble())
				{
					healerNumber = 500969; // You finish applying the bandages.

					double min, max;

					if (Core.AOS)
					{
						min = (anatomy / 8.0) + (healing / 5.0) + 4.0;
						max = (anatomy / 6.0) + (healing / 2.5) + 4.0;
					}
					else
					{
						min = (anatomy / 5.0) + (healing / 5.0) + 3.0;
						max = (anatomy / 5.0) + (healing / 2.0) + 10.0;
					}

					double toHeal = min + (Utility.RandomDouble() * (max - min));

					if (Patient.Body.IsMonster || Patient.Body.IsAnimal)
						toHeal += Patient.HitsMax / 100;

					if (Core.AOS)
						toHeal -= toHeal * Slips * 0.35; // TODO: Verify algorithm
					else
						toHeal -= Slips * 4;

					if (toHeal < 1)
					{
						toHeal = 1;
						healerNumber = 500968; // You apply the bandages, but they barely help.
					}

					Patient.Heal((int)toHeal, Healer, false);
				}
				else
				{
					healerNumber = 500968; // You apply the bandages, but they barely help.
					playSound = false;
				}
			}

			if (healerNumber != -1)
				Healer.SendLocalizedMessage(healerNumber);

			if (patientNumber != -1)
				Patient.SendLocalizedMessage(patientNumber);

			if (playSound)
				Patient.PlaySound(0x57);

			if (checkSkills)
			{
				Healer.CheckSkill(secondarySkill, 0.0, 120.0);
				Healer.CheckSkill(primarySkill, 0.0, 120.0);
			}
		}

		private class InternalTimer : Timer
		{
			private readonly BandageContext m_Context;

			public InternalTimer(BandageContext context, TimeSpan delay) : base(delay)
			{
				m_Context = context;
				Priority = TimerPriority.FiftyMS;
			}

			protected override void OnTick()
			{
				m_Context.EndHeal();
			}
		}

		public static BandageContext BeginHeal(Mobile healer, Mobile patient)
		{
			bool isDeadPet = patient is BaseCreature creature && creature.IsDeadPet;

			if (patient is Golem)
			{
				healer.SendLocalizedMessage(500970); // Bandages cannot be used on that.
			}
			else if (patient is BaseCreature bc && bc.IsAnimatedDead)
			{
				healer.SendLocalizedMessage(500951); // You cannot heal that.
			}
			else if (!patient.Poisoned && patient.Hits == patient.HitsMax && !BleedAttack.IsBleeding(patient) && !isDeadPet)
			{
				healer.SendLocalizedMessage(500955); // That being is not damaged!
			}
			else if (!patient.Alive && (patient.Map == null || !patient.Map.CanFit(patient.Location, 16, false, false)))
			{
				healer.SendLocalizedMessage(501042); // Target cannot be resurrected at that location.
			}
			else if (healer.CanBeBeneficial(patient, true, true))
			{
				healer.DoBeneficial(patient);

				bool onSelf = (healer == patient);
				int dex = healer.Dex;

				double seconds;
				double resDelay = (patient.Alive ? 0.0 : 5.0);

				if (onSelf)
				{
					if (Core.AOS)
						seconds = 5.0 + (0.5 * ((double)(120 - dex) / 10)); // TODO: Verify algorithm
					else
						seconds = 9.4 + (0.6 * ((double)(120 - dex) / 10));
				}
				else
				{
					if (Core.AOS && GetPrimarySkill(patient) == SkillName.Veterinary)
					{
						seconds = 2.0;
					}
					else if (Core.AOS)
					{
						if (dex < 204)
						{
							seconds = 3.2 - (Math.Sin((double)dex / 130) * 2.5) + resDelay;
						}
						else
						{
							seconds = 0.7 + resDelay;
						}
					}
					else
					{
						if (dex >= 100)
							seconds = 3.0 + resDelay;
						else if (dex >= 40)
							seconds = 4.0 + resDelay;
						else
							seconds = 5.0 + resDelay;
					}
				}

				BandageContext context = GetContext(healer);

				if (context != null)
					context.StopHeal();
				seconds *= 1000;

				context = new BandageContext(healer, patient, TimeSpan.FromMilliseconds(seconds));

				m_Table[healer] = context;

				if (!onSelf)
					patient.SendLocalizedMessage(1008078, false, healer.Name); //  : Attempting to heal you.

				healer.SendLocalizedMessage(500956); // You begin applying the bandages.
				return context;
			}

			return null;
		}
	}
}
