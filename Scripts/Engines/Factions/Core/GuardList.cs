using System;
using System.Collections.Generic;

namespace Server.Factions
{
	public class GuardList
	{
		private readonly GuardDefinition m_Definition;
		private readonly List<BaseFactionGuard> m_Guards;

		public GuardDefinition Definition => m_Definition;
		public List<BaseFactionGuard> Guards => m_Guards;

		public BaseFactionGuard Construct()
		{
			try { return Activator.CreateInstance(m_Definition.Type) as BaseFactionGuard; }
			catch { return null; }
		}

		public GuardList(GuardDefinition definition)
		{
			m_Definition = definition;
			m_Guards = new List<BaseFactionGuard>();
		}
	}
}
