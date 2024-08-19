using System;
using System.Security.Cryptography;
using System.Threading;

namespace Server
{
	public sealed class CSPRandom : IRandomImpl
	{
		private readonly RandomNumberGenerator _CSP = RandomNumberGenerator.Create();

		private static readonly int BUFFER_SIZE = 0x4000;
		private static readonly int LARGE_REQUEST = 0x40;

		private byte[] _Working = new byte[BUFFER_SIZE];
		private byte[] _Buffer = new byte[BUFFER_SIZE];

		private int _Index = 0;

		private readonly object _sync = new();

		private readonly ManualResetEvent _filled = new(false);

		public CSPRandom()
		{
			_CSP.GetBytes(_Working);
			ThreadPool.QueueUserWorkItem(new WaitCallback(Fill));
		}

		private void CheckSwap(int c)
		{
			if (_Index + c < BUFFER_SIZE)
				return;

			_filled.WaitOne();

			byte[] b = _Working;
			_Working = _Buffer;
			_Buffer = b;
			_Index = 0;

			_filled.Reset();

			ThreadPool.QueueUserWorkItem(new WaitCallback(Fill));
		}

		private void Fill(object o)
		{
			lock (_CSP)
				_CSP.GetBytes(_Buffer);

			_filled.Set();
		}

		private void GetBytes(byte[] b)
		{
			int c = b.Length;

			lock (_sync)
			{
				CheckSwap(c);
				Buffer.BlockCopy(_Working, _Index, b, 0, c);
				_Index += c;
			}
		}

		private void GetBytes(byte[] b, int offset, int count)
		{
			lock (_sync)
			{
				CheckSwap(count);
				Buffer.BlockCopy(_Working, _Index, b, offset, count);
				_Index += count;
			}
		}

		public int Next(int c)
		{
			return (int)(c * NextDouble());
		}

		public bool NextBool()
		{
			return (NextByte() & 1) == 1;
		}

		private byte NextByte()
		{
			lock (_sync)
			{
				CheckSwap(1);
				return _Working[_Index++];
			}
		}

		public void NextBytes(byte[] b)
		{
			int c = b.Length;

			if (c >= LARGE_REQUEST)
			{
				lock (_CSP)
					_CSP.GetBytes(b);
				return;
			}
			GetBytes(b);
		}

		public unsafe double NextDouble()
		{
			byte[] b = new byte[8];

			if (BitConverter.IsLittleEndian)
			{
				b[7] = 0;
				GetBytes(b, 0, 7);
			}
			else
			{
				b[0] = 0;
				GetBytes(b, 1, 7);
			}

			ulong r = 0;
			fixed (byte* buf = b)
				r = *(ulong*)&buf[0] >> 3;

			/* double: 53 bits of significand precision
			 * ulong.MaxValue >> 11 = 9007199254740991
			 * 2^53 = 9007199254740992
			 */

			return (double)r / 9007199254740992;
		}
	}
}
