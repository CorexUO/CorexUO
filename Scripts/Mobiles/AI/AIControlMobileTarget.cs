using Server.Mobiles;
using Server.Targeting;
using System.Collections.Generic;

namespace Server.Targets
{
	public class AIControlMobileTarget : Target
	{
		private readonly List<BaseAI> m_List;

		public OrderType Order { get; }

		public AIControlMobileTarget(BaseAI ai, OrderType order) : base(-1, false, (order == OrderType.Attack ? TargetFlags.Harmful : TargetFlags.None))
		{
			m_List = new List<BaseAI>();
			Order = order;

			AddAI(ai);
		}

		public void AddAI(BaseAI ai)
		{
			if (!m_List.Contains(ai))
				m_List.Add(ai);
		}

		protected override void OnTarget(Mobile from, object o)
		{
			if (o is Mobile m)
			{
				for (int i = 0; i < m_List.Count; ++i)
					m_List[i].EndPickTarget(from, m, Order);
			}
		}
	}
}
