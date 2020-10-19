using System;
//using System.Collections.Generic;

namespace Server.Items.Holiday
{
	[TypeAlias("Server.Items.ClownMask", "Server.Items.DaemonMask", "Server.Items.PlagueMask")]
	public class BasePaintedMask : BaseItem
	{
		public override string DefaultName
		{
			get
			{
				if (m_Staffer != null)
				{
					return String.Format("{0} hand painted by {1}", MaskName, m_Staffer);
				}

				return MaskName;
			}
		}

		public virtual string MaskName { get { return "A Mask"; } }

		private string m_Staffer;

		private static string[] m_Staffers =
		{
				  "Ryan",
				  "Mark",
				  "Krrios",
				  "Zippy",
				  "Athena",
				  "Eos",
				  "Xavier"
		};

		public BasePaintedMask(int itemid)
			: this(m_Staffers[Utility.Random(m_Staffers.Length)], itemid)
		{

		}

		public BasePaintedMask(string staffer, int itemid)
			: base(itemid + Utility.Random(2))
		{
			m_Staffer = Utility.Intern(staffer);
		}

		public BasePaintedMask(Serial serial) : base(serial)
		{

		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write((int)0); // version
			writer.Write((string)m_Staffer);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadInt();

			m_Staffer = Utility.Intern(reader.ReadString());
		}
	}
}
