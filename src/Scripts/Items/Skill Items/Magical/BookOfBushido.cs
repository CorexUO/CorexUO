namespace Server.Items
{
	public class BookOfBushido : Spellbook
	{
		public override SpellbookType SpellbookType => SpellbookType.Samurai;
		public override int BookOffset => 400;
		public override int BookCount => 6;

		[Constructable]
		public BookOfBushido() : this((ulong)0x3F)
		{
		}

		[Constructable]
		public BookOfBushido(ulong content) : base(content, 0x238C)
		{
			Layer = (Core.ML ? Layer.OneHanded : Layer.Invalid);
		}

		public BookOfBushido(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0); // version
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			reader.ReadInt();
		}
	}
}
