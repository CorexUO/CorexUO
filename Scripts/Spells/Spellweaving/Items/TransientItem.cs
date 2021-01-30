using System;

namespace Server.Items
{
	public class TransientItem : BaseItem
	{
		[CommandProperty(AccessLevel.GameMaster)]
		public TimeSpan LifeSpan { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public DateTime CreationTime { get; set; }

		private Timer m_Timer;

		public override bool Nontransferable { get { return true; } }
		public override void HandleInvalidTransfer(Mobile from)
		{
			if (InvalidTransferMessage != null)
				TextDefinition.SendMessageTo(from, InvalidTransferMessage);

			this.Delete();
		}

		public virtual TextDefinition InvalidTransferMessage { get { return null; } }


		public virtual void Expire(Mobile parent)
		{
			if (parent != null)
				parent.SendLocalizedMessage(1072515, (this.Name == null ? String.Format("#{0}", LabelNumber) : this.Name)); // The ~1_name~ expired...

			Effects.PlaySound(GetWorldLocation(), Map, 0x201);

			this.Delete();
		}

		public virtual void SendTimeRemainingMessage(Mobile to)
		{
			to.SendLocalizedMessage(1072516, String.Format("{0}\t{1}", (this.Name == null ? String.Format("#{0}", LabelNumber) : this.Name), (int)LifeSpan.TotalSeconds)); // ~1_name~ will expire in ~2_val~ seconds!
		}

		public override void OnDelete()
		{
			if (m_Timer != null)
				m_Timer.Stop();

			base.OnDelete();
		}

		public virtual void CheckExpiry()
		{
			if ((CreationTime + LifeSpan) < DateTime.UtcNow)
				Expire(RootParent as Mobile);
			else
				InvalidateProperties();
		}

		[Constructable]
		public TransientItem(int itemID, TimeSpan lifeSpan)
			: base(itemID)
		{
			CreationTime = DateTime.UtcNow;
			LifeSpan = lifeSpan;

			m_Timer = Timer.DelayCall(TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(5), new TimerCallback(CheckExpiry));
		}

		public TransientItem(Serial serial)
			: base(serial)
		{
		}

		public override void GetProperties(ObjectPropertyList list)
		{
			base.GetProperties(list);

			TimeSpan remaining = ((CreationTime + LifeSpan) - DateTime.UtcNow);

			list.Add(1072517, ((int)remaining.TotalSeconds).ToString()); // Lifespan: ~1_val~ seconds
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);
			writer.Write(0);

			writer.Write(LifeSpan);
			writer.Write(CreationTime);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);
			int version = reader.ReadInt();

			LifeSpan = reader.ReadTimeSpan();
			CreationTime = reader.ReadDateTime();

			m_Timer = Timer.DelayCall(TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(5), new TimerCallback(CheckExpiry));
		}
	}
}
