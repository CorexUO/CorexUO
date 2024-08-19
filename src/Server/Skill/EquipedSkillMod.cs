namespace Server
{
	public class EquipedSkillMod : SkillMod
	{
		private readonly Item m_Item;
		private readonly Mobile m_Mobile;

		public EquipedSkillMod(SkillName skill, bool relative, double value, Item item, Mobile mobile)
			: base(skill, relative, value)
		{
			m_Item = item;
			m_Mobile = mobile;
		}

		public override bool CheckCondition()
		{
			return !m_Item.Deleted && !m_Mobile.Deleted && m_Item.Parent == m_Mobile;
		}
	}
}
