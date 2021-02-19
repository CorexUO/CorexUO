using System;
using System.IO;

namespace Server
{
	public interface IHardwareRNG
	{
		bool IsSupported();
	}

	public enum RDRandError : int
	{
		Unknown = -4,
		Unsupported = -3,
		Supported = -2,
		NotReady = -1,

		Failure = 0,

		Success = 1,
	}

	public interface IRandomImpl
	{
		int Next(int c);
		bool NextBool();
		void NextBytes(byte[] b);
		double NextDouble();
	}

	/// <summary>
	/// Handles random number generation.
	/// </summary>
	public static class RandomImpl
	{
		private static readonly IRandomImpl _Random;

		static RandomImpl()
		{
			if (Core.Is64Bit && File.Exists("rdrand64.dll"))
			{
				_Random = new RDRand64();
			}
			else if (!Core.Is64Bit && File.Exists("rdrand32.dll"))
			{
				_Random = new RDRand32();
			}
			else
			{
				_Random = new SimpleRandom();
			}

			if (_Random is IHardwareRNG rNG)
			{
				if (!rNG.IsSupported())
				{
					_Random = new CSPRandom();
				}
			}
		}

		public static bool IsHardwareRNG
		{
			get { return _Random is IHardwareRNG; }
		}

		public static Type Type
		{
			get { return _Random.GetType(); }
		}

		public static int Next(int c)
		{
			return _Random.Next(c);
		}

		public static bool NextBool()
		{
			return _Random.NextBool();
		}

		public static void NextBytes(byte[] b)
		{
			_Random.NextBytes(b);
		}

		public static double NextDouble()
		{
			return _Random.NextDouble();
		}
	}
}
