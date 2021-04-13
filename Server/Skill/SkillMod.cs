namespace Server
{
	public abstract class SkillMod
	{
		private Mobile m_Owner;
		private SkillName m_Skill;
		private bool m_Relative;
		private double m_Value;
		private bool m_ObeyCap;

		protected SkillMod(SkillName skill, bool relative, double value)
		{
			m_Skill = skill;
			m_Relative = relative;
			m_Value = value;
		}

		public bool ObeyCap
		{
			get => m_ObeyCap;
			set
			{
				m_ObeyCap = value;

				if (m_Owner != null)
				{
					Skill sk = m_Owner.Skills[m_Skill];

					if (sk != null)
						sk.Update();
				}
			}
		}

		public Mobile Owner
		{
			get => m_Owner;
			set
			{
				if (m_Owner != value)
				{
					if (m_Owner != null)
						m_Owner.RemoveSkillMod(this);

					m_Owner = value;

					if (m_Owner != value)
						m_Owner.AddSkillMod(this);
				}
			}
		}

		public void Remove()
		{
			Owner = null;
		}

		public SkillName Skill
		{
			get => m_Skill;
			set
			{
				if (m_Skill != value)
				{
					Skill oldUpdate = m_Owner?.Skills[m_Skill];

					m_Skill = value;

					if (m_Owner != null)
					{
						Skill sk = m_Owner.Skills[m_Skill];

						if (sk != null)
							sk.Update();
					}

					if (oldUpdate != null)
						oldUpdate.Update();
				}
			}
		}

		public bool Relative
		{
			get => m_Relative;
			set
			{
				if (m_Relative != value)
				{
					m_Relative = value;

					if (m_Owner != null)
					{
						Skill sk = m_Owner.Skills[m_Skill];

						if (sk != null)
							sk.Update();
					}
				}
			}
		}

		public bool Absolute
		{
			get => !m_Relative;
			set
			{
				if (m_Relative == value)
				{
					m_Relative = !value;

					if (m_Owner != null)
					{
						Skill sk = m_Owner.Skills[m_Skill];

						if (sk != null)
							sk.Update();
					}
				}
			}
		}

		public double Value
		{
			get => m_Value;
			set
			{
				if (m_Value != value)
				{
					m_Value = value;

					if (m_Owner != null)
					{
						Skill sk = m_Owner.Skills[m_Skill];

						if (sk != null)
							sk.Update();
					}
				}
			}
		}

		public abstract bool CheckCondition();
	}
}
