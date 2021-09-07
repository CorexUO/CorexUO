namespace Server.Items
{
	public class MysticSpellbook : Spellbook
	{
		public override SpellbookType SpellbookType => SpellbookType.Mystic;

		public override int BookOffset => 677;
		public override int BookCount => 16;

		[Constructable]
		public MysticSpellbook()
			: this((ulong)0)
		{
		}

		[Constructable]
		public MysticSpellbook(ulong content)
			: base(content, 0x2D9D)
		{
			Layer = Layer.OneHanded;
		}

		public MysticSpellbook(Serial serial)
			: base(serial)
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
