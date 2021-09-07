using Server.Network;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Server
{
	public delegate void FileCommitCallback(FileQueue.Chunk chunk);

	public sealed class FileQueue : IDisposable
	{
		public sealed class Chunk
		{
			private readonly FileQueue owner;
			private readonly int slot;
			private readonly int offset;

			public byte[] Buffer { get; }

			public static int Offset => 0;

			public int Size { get; }

			public Chunk(FileQueue owner, int slot, byte[] buffer, int offset, int size)
			{
				this.owner = owner;
				this.slot = slot;

				Buffer = buffer;
				this.offset = offset;
				Size = size;
			}

			public void Commit()
			{
				owner.Commit(this, slot);
			}
		}

		private struct Page
		{
			public byte[] buffer;
			public int length;
		}

		private static readonly int bufferSize;
		private static readonly BufferPool bufferPool;

		static FileQueue()
		{
			bufferSize = FileOperations.BufferSize;
			bufferPool = new BufferPool("File Buffers", 64, bufferSize);
		}

		private readonly object syncRoot;

		private readonly Chunk[] active;
		private int activeCount;

		private readonly Queue<Page> pending;
		private Page buffered;

		private readonly FileCommitCallback callback;

		private ManualResetEvent idle;

		public long Position { get; private set; }

		public FileQueue(int concurrentWrites, FileCommitCallback callback)
		{
			if (concurrentWrites < 1)
			{
				throw new ArgumentOutOfRangeException("concurrentWrites");
			}
			else if (bufferSize < 1)
			{
				throw new ArgumentOutOfRangeException("bufferSize");
			}
			else if (callback == null)
			{
				throw new ArgumentNullException("callback");
			}

			syncRoot = new object();

			active = new Chunk[concurrentWrites];
			pending = new Queue<Page>();

			this.callback = callback;

			idle = new ManualResetEvent(true);
		}

		private void Append(Page page)
		{
			lock (syncRoot)
			{
				if (activeCount == 0)
				{
					idle.Reset();
				}

				++activeCount;

				for (int slot = 0; slot < active.Length; ++slot)
				{
					if (active[slot] == null)
					{
						active[slot] = new Chunk(this, slot, page.buffer, 0, page.length);

						callback(active[slot]);

						return;
					}
				}

				pending.Enqueue(page);
			}
		}

		public void Dispose()
		{
			if (idle != null)
			{
				idle.Close();
				idle = null;
			}
		}

		public void Flush()
		{
			if (buffered.buffer != null)
			{
				Append(buffered);

				buffered.buffer = null;
				buffered.length = 0;
			}

			/*lock ( syncRoot ) {
				if ( pending.Count > 0 ) {
					idle.Reset();
				}

				for ( int slot = 0; slot < active.Length && pending.Count > 0; ++slot ) {
					if ( active[slot] == null ) {
						Page page = pending.Dequeue();

						active[slot] = new Chunk( this, slot, page.buffer, 0, page.length );

						++activeCount;

						callback( active[slot] );
					}
				}
			}*/

			idle.WaitOne();
		}

		private void Commit(Chunk chunk, int slot)
		{
			if (slot < 0 || slot >= active.Length)
			{
				throw new ArgumentOutOfRangeException("slot");
			}

			lock (syncRoot)
			{
				if (active[slot] != chunk)
				{
					throw new ArgumentException();
				}

				bufferPool.ReleaseBuffer(chunk.Buffer);

				if (pending.Count > 0)
				{
					Page page = pending.Dequeue();

					active[slot] = new Chunk(this, slot, page.buffer, 0, page.length);

					callback(active[slot]);
				}
				else
				{
					active[slot] = null;
				}

				--activeCount;

				if (activeCount == 0)
				{
					idle.Set();
				}
			}
		}

		public void Enqueue(byte[] buffer, int offset, int size)
		{
			if (buffer == null)
			{
				throw new ArgumentNullException("buffer");
			}
			else if (offset < 0)
			{
				throw new ArgumentOutOfRangeException("offset");
			}
			else if (size < 0)
			{
				throw new ArgumentOutOfRangeException("size");
			}
			else if ((buffer.Length - offset) < size)
			{
				throw new ArgumentException();
			}

			Position += size;

			while (size > 0)
			{
				if (buffered.buffer == null)
				{ // nothing yet buffered
					buffered.buffer = bufferPool.AcquireBuffer();
				}

				byte[] page = buffered.buffer; // buffer page
				int pageSpace = page.Length - buffered.length; // available bytes in page
				int byteCount = (size > pageSpace ? pageSpace : size); // how many bytes we can copy over

				Buffer.BlockCopy(buffer, offset, page, buffered.length, byteCount);

				buffered.length += byteCount;
				offset += byteCount;
				size -= byteCount;

				if (buffered.length == page.Length)
				{ // page full
					Append(buffered);

					buffered.buffer = null;
					buffered.length = 0;
				}
			}
		}
	}
}
