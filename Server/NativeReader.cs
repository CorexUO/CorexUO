using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace Server
{
	public static class NativeReader
	{
		static NativeReader()
		{
			if (Core.Unix)
				Reader = new NativeReaderUnix();
			else
				Reader = new NativeReaderWin32();
		}

		public static INativeReader Reader { get; private set; }

		public static unsafe void Read(IntPtr ptr, void* buffer, int length)
		{
			Reader.Read(ptr, buffer, length);
		}
	}

	public interface INativeReader
	{
		unsafe void Read(IntPtr ptr, void* buffer, int length);
	}

	public sealed class NativeReaderWin32 : INativeReader
	{
		internal class UnsafeNativeMethods
		{
			/*[DllImport("kernel32")]
			internal unsafe static extern int _lread(IntPtr hFile, void* lpBuffer, int wBytes);*/

			[DllImport("kernel32")]
			internal unsafe static extern bool ReadFile(IntPtr hFile, void* lpBuffer, uint nNumberOfBytesToRead, ref uint lpNumberOfBytesRead, NativeOverlapped* lpOverlapped);
		}

		public NativeReaderWin32()
		{
		}

		public unsafe void Read(IntPtr ptr, void* buffer, int length)
		{
			//UnsafeNativeMethods._lread( ptr, buffer, length );
			uint lpNumberOfBytesRead = 0;
			UnsafeNativeMethods.ReadFile(ptr, buffer, (uint)length, ref lpNumberOfBytesRead, null);
		}
	}

	public sealed class NativeReaderUnix : INativeReader
	{
		internal class UnsafeNativeMethods
		{
			[DllImport("libc")]
			internal unsafe static extern int read(IntPtr ptr, void* buffer, int length);
		}

		public NativeReaderUnix()
		{
		}

		public unsafe void Read(IntPtr ptr, void* buffer, int length)
		{
			_ = UnsafeNativeMethods.read(ptr, buffer, length);
		}
	}
}
