using Server.Items;
using Server.Network;
using Server.Targeting;

namespace Server.Spells.Third
{
	public class UnlockSpell : MagerySpell
	{
		private static readonly SpellInfo m_Info = new(
				"Unlock Spell", "Ex Por",
				215,
				9001,
				Reagent.Bloodmoss,
				Reagent.SulfurousAsh
			);

		public override SpellCircle Circle => SpellCircle.Third;

		public UnlockSpell(Mobile caster, Item scroll) : base(caster, scroll, m_Info)
		{
		}

		public override void OnCast()
		{
			if (Precast)
			{
				Caster.Target = new InternalTarget(this);
			}
			else
			{
				if (SpellTarget is LockableContainer target)
					Target(target);
				else
					FinishSequence();
			}
		}

		public void Target(LockableContainer cont)
		{
			if (cont is LockableContainer && cont is IPoint3D loc)
			{
				if (CheckSequence())
				{
					SpellHelper.Turn(Caster, loc);

					Effects.SendLocationParticles(EffectItem.Create(new Point3D(loc), Caster.Map, EffectItem.DefaultDuration), 0x376A, 9, 32, 5024);

					Effects.PlaySound(loc, Caster.Map, 0x1FF);

					if (Multis.BaseHouse.CheckSecured(cont))
						Caster.SendLocalizedMessage(503098); // You cannot cast this on a secure item.
					else if (!cont.Locked)
						Caster.LocalOverheadMessage(MessageType.Regular, 0x3B2, 503101); // That did not need to be unlocked.
					else if (cont.LockLevel == 0)
						Caster.SendLocalizedMessage(501666); // You can't unlock that!
					else
					{
						int level = (int)(Caster.Skills[SkillName.Magery].Value * 0.8) - 4;

						if (level >= cont.RequiredSkill && !(cont is TreasureMapChest && ((TreasureMapChest)cont).Level > 2))
						{
							cont.Locked = false;

							if (cont.LockLevel == -255)
								cont.LockLevel = cont.RequiredSkill - 10;
						}
						else
							Caster.LocalOverheadMessage(MessageType.Regular, 0x3B2, 503099); // My spell does not seem to have an effect on that lock.
					}
				}
			}

			FinishSequence();
		}

		private class InternalTarget : Target
		{
			private readonly UnlockSpell m_Owner;

			public InternalTarget(UnlockSpell owner) : base(owner.SpellRange, false, TargetFlags.None)
			{
				m_Owner = owner;
			}

			protected override void OnTarget(Mobile from, object o)
			{
				if (o is LockableContainer)
					m_Owner.Target((LockableContainer)o);
			}

			protected override void OnTargetFinish(Mobile from)
			{
				m_Owner.FinishSequence();
			}
		}
	}
}
