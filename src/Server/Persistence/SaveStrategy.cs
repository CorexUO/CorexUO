namespace Server
{
	public abstract class SaveStrategy
	{
		public abstract string Name { get; }

		public static SaveStrategy Acquire()
		{
			if (Core.MultiProcessor)
			{
				int processorCount = Core.ProcessorCount;

				if (processorCount > 2)
				{
					return new DualSaveStrategy(); // return new DynamicSaveStrategy(); (4.0 or return new ParallelSaveStrategy(processorCount); (2.0)
				}
				else
				{
					return new DualSaveStrategy();
				}
			}
			else
			{
				return new StandardSaveStrategy();
			}
		}

		public abstract void Save(bool permitBackgroundWrite);
		public abstract void ProcessDecay();
	}
}
