using System;

namespace Server
{
	public sealed class SimpleRandom : IRandomImpl
	{
		private readonly Random m_Random = new();

		public SimpleRandom()
		{
		}

		public int Next(int c)
		{
			int r;
			lock (m_Random)
				r = m_Random.Next(c);
			return r;
		}

		public bool NextBool()
		{
			return NextDouble() >= .5;
		}

		public void NextBytes(byte[] b)
		{
			lock (m_Random)
				m_Random.NextBytes(b);
		}

		public double NextDouble()
		{
			double r;
			lock (m_Random)
				r = m_Random.NextDouble();
			return r;
		}
	}
}
