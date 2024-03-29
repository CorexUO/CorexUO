using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Server.Diagnostics
{
	public abstract class BasePacketProfile : BaseProfile
	{
		public long TotalLength { get; private set; }

		public double AverageLength => (double)TotalLength / Math.Max(1, Count);

		protected BasePacketProfile(string name)
			: base(name)
		{
		}

		public void Finish(int length)
		{
			Finish();

			TotalLength += length;
		}

		public override void WriteTo(TextWriter op)
		{
			base.WriteTo(op);

			op.Write("\t{0,12:F2} {1,-12:N0}", AverageLength, TotalLength);
		}
	}

	public class PacketSendProfile : BasePacketProfile
	{
		private static readonly Dictionary<Type, PacketSendProfile> _profiles = new();

		public static IEnumerable<PacketSendProfile> Profiles => _profiles.Values;

		[MethodImpl(MethodImplOptions.Synchronized)]
		public static PacketSendProfile Acquire(Type type)
		{
			if (!_profiles.TryGetValue(type, out PacketSendProfile prof))
			{
				_profiles.Add(type, prof = new PacketSendProfile(type));
			}

			return prof;
		}

		private long _created;

		public void Increment()
		{
			Interlocked.Increment(ref _created);
		}

		public PacketSendProfile(Type type)
			: base(type.FullName)
		{
		}

		public override void WriteTo(TextWriter op)
		{
			base.WriteTo(op);

			op.Write("\t{0,12:N0}", _created);
		}
	}

	public class PacketReceiveProfile : BasePacketProfile
	{
		private static readonly Dictionary<int, PacketReceiveProfile> _profiles = new();

		public static IEnumerable<PacketReceiveProfile> Profiles => _profiles.Values;

		[MethodImpl(MethodImplOptions.Synchronized)]
		public static PacketReceiveProfile Acquire(int packetId)
		{
			if (!_profiles.TryGetValue(packetId, out PacketReceiveProfile prof))
			{
				_profiles.Add(packetId, prof = new PacketReceiveProfile(packetId));
			}

			return prof;
		}

		public PacketReceiveProfile(int packetId)
			: base(string.Format("0x{0:X2}", packetId))
		{
		}
	}
}
