namespace Server
{
	public class KeywordList
	{
		public KeywordList()
		{
			Keywords = new int[8];
			Count = 0;
		}

		public int[] Keywords { get; set; }
		public int Count { get; set; }

		public bool Contains(int keyword)
		{
			bool contains = false;

			for (int i = 0; !contains && i < Count; ++i)
				contains = (keyword == Keywords[i]);

			return contains;
		}

		public void Add(int keyword)
		{
			if ((Count + 1) > Keywords.Length)
			{
				int[] old = Keywords;
				Keywords = new int[old.Length * 2];

				for (int i = 0; i < old.Length; ++i)
					Keywords[i] = old[i];
			}

			Keywords[Count++] = keyword;
		}

		private static readonly int[] m_EmptyInts = System.Array.Empty<int>();

		public int[] ToArray()
		{
			if (Count == 0)
				return m_EmptyInts;

			int[] keywords = new int[Count];

			for (int i = 0; i < Count; ++i)
				keywords[i] = Keywords[i];

			Count = 0;

			return keywords;
		}
	}
}
