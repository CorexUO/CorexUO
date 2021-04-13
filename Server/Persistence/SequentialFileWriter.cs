using System;
using System.IO;

namespace Server
{
	public sealed class SequentialFileWriter : Stream
	{
		private FileStream fileStream;
		private FileQueue fileQueue;

		private AsyncCallback writeCallback;

		public SequentialFileWriter(string path)
		{
			if (path == null)
			{
				throw new ArgumentNullException("path");
			}

			this.fileStream = FileOperations.OpenSequentialStream(path, FileMode.Create, FileAccess.Write, FileShare.None);

			fileQueue = new FileQueue(
				Math.Max(1, FileOperations.Concurrency),
				FileCallback
			);
		}

		public override long Position
		{
			get => fileQueue.Position;
			set => throw new InvalidOperationException();
		}

		private void FileCallback(FileQueue.Chunk chunk)
		{
			if (FileOperations.AreSynchronous)
			{
				fileStream.Write(chunk.Buffer, FileQueue.Chunk.Offset, chunk.Size);

				chunk.Commit();
			}
			else
			{
				if (writeCallback == null)
				{
					writeCallback = this.OnWrite;
				}

				fileStream.BeginWrite(chunk.Buffer, FileQueue.Chunk.Offset, chunk.Size, writeCallback, chunk);
			}
		}

		private void OnWrite(IAsyncResult asyncResult)
		{
			FileQueue.Chunk chunk = asyncResult.AsyncState as FileQueue.Chunk;

			fileStream.EndWrite(asyncResult);

			chunk.Commit();
		}

		public override void Write(byte[] buffer, int offset, int size)
		{
			fileQueue.Enqueue(buffer, offset, size);
		}

		public override void Flush()
		{
			fileQueue.Flush();
			fileStream.Flush();
		}

		protected override void Dispose(bool disposing)
		{
			if (fileStream != null)
			{
				Flush();

				fileQueue.Dispose();
				fileQueue = null;

				fileStream.Close();
				fileStream = null;
			}

			base.Dispose(disposing);
		}

		public override bool CanRead => false;

		public override bool CanSeek => false;

		public override bool CanWrite => true;

		public override long Length => this.Position;

		public override int Read(byte[] buffer, int offset, int count)
		{
			throw new InvalidOperationException();
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			throw new InvalidOperationException();
		}

		public override void SetLength(long value)
		{
			fileStream.SetLength(value);
		}
	}
}
