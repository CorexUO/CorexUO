namespace Server.Engines.Mahjong
{
	public class MahjongDices
	{
		public MahjongGame Game { get; }
		public int First { get; private set; }
		public int Second { get; private set; }

		public MahjongDices(MahjongGame game)
		{
			Game = game;
			First = Utility.Random(1, 6);
			Second = Utility.Random(1, 6);
		}

		public void RollDices(Mobile from)
		{
			First = Utility.Random(1, 6);
			Second = Utility.Random(1, 6);

			Game.Players.SendGeneralPacket(true, true);

			if (from != null)
				Game.Players.SendLocalizedMessage(1062695, string.Format("{0}\t{1}\t{2}", from.Name, First, Second)); // ~1_name~ rolls the dice and gets a ~2_number~ and a ~3_number~!
		}

		public void Save(GenericWriter writer)
		{
			writer.Write(0); // version

			writer.Write(First);
			writer.Write(Second);
		}

		public MahjongDices(MahjongGame game, GenericReader reader)
		{
			Game = game;

			int version = reader.ReadInt();

			First = reader.ReadInt();
			Second = reader.ReadInt();
		}
	}
}
