using System;
using System.Diagnostics;

namespace Server
{
	public sealed class SaveMetrics : IDisposable
	{
		private const string PerformanceCategoryName = "CorexUO 1.0";
		private const string PerformanceCategoryDesc = "Performance counters for CorexUO 1.0";

		private readonly PerformanceCounter numberOfWorldSaves;

		private readonly PerformanceCounter itemsPerSecond;
		private readonly PerformanceCounter mobilesPerSecond;

		private readonly PerformanceCounter serializedBytesPerSecond;
		private readonly PerformanceCounter writtenBytesPerSecond;

		public SaveMetrics()
		{
			if (!PerformanceCounterCategory.Exists(PerformanceCategoryName))
			{
				CounterCreationDataCollection counters = new CounterCreationDataCollection
				{
					new CounterCreationData(
						"Save - Count",
						"Number of world saves.",
						PerformanceCounterType.NumberOfItems32
					),

					new CounterCreationData(
						"Save - Items/sec",
						"Number of items saved per second.",
						PerformanceCounterType.RateOfCountsPerSecond32
					),

					new CounterCreationData(
						"Save - Mobiles/sec",
						"Number of mobiles saved per second.",
						PerformanceCounterType.RateOfCountsPerSecond32
					),

					new CounterCreationData(
						"Save - Serialized bytes/sec",
						"Amount of world-save bytes serialized per second.",
						PerformanceCounterType.RateOfCountsPerSecond32
					),

					new CounterCreationData(
						"Save - Written bytes/sec",
						"Amount of world-save bytes written to disk per second.",
						PerformanceCounterType.RateOfCountsPerSecond32
					)
				};

				PerformanceCounterCategory.Create(PerformanceCategoryName, PerformanceCategoryDesc, PerformanceCounterCategoryType.SingleInstance, counters);
			}

			numberOfWorldSaves = new PerformanceCounter(PerformanceCategoryName, "Save - Count", false);

			itemsPerSecond = new PerformanceCounter(PerformanceCategoryName, "Save - Items/sec", false);
			mobilesPerSecond = new PerformanceCounter(PerformanceCategoryName, "Save - Mobiles/sec", false);

			serializedBytesPerSecond = new PerformanceCounter(PerformanceCategoryName, "Save - Serialized bytes/sec", false);
			writtenBytesPerSecond = new PerformanceCounter(PerformanceCategoryName, "Save - Written bytes/sec", false);

			// increment number of world saves
			numberOfWorldSaves.Increment();
		}

		public void OnItemSaved(int numberOfBytes)
		{
			itemsPerSecond.Increment();

			serializedBytesPerSecond.IncrementBy(numberOfBytes);
		}

		public void OnMobileSaved(int numberOfBytes)
		{
			mobilesPerSecond.Increment();

			serializedBytesPerSecond.IncrementBy(numberOfBytes);
		}

		public void OnGuildSaved(int numberOfBytes)
		{
			serializedBytesPerSecond.IncrementBy(numberOfBytes);
		}

		public void OnFileWritten(int numberOfBytes)
		{
			writtenBytesPerSecond.IncrementBy(numberOfBytes);
		}

		private bool isDisposed;

		public void Dispose()
		{
			if (!isDisposed)
			{
				isDisposed = true;

				numberOfWorldSaves.Dispose();

				itemsPerSecond.Dispose();
				mobilesPerSecond.Dispose();

				serializedBytesPerSecond.Dispose();
				writtenBytesPerSecond.Dispose();
			}
		}
	}
}
