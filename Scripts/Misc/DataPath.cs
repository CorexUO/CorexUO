#region References
using System;
using System.IO;
using System.Linq;
#endregion

namespace Server.Misc
{
	public class DataPath
	{
		private static string m_Path;

		public static string FilePath
		{
			get
			{
				if (m_Path != null)
					return m_Path;

				return m_Path = Settings.Get<string>("Server", "FilePath");
			}
		}

		[CallPriority(int.MinValue + 1)]
		public static void Configure()
		{
			if (FilePath != null && Directory.Exists(FilePath))
			{
				Core.DataDirectories.Add(FilePath);
			}
			else if (!Core.Service)
			{
				SetDirectory();

				Core.DataDirectories.Add(m_Path);
			}

			Utility.WriteConsole(ConsoleColor.Cyan, $"DataPath: {string.Join("\n", Core.DataDirectories)}");
		}

		private static void SetDirectory()
		{
			do
			{
				Utility.WriteConsole(ConsoleColor.Cyan, "Enter the Ultima Online directory:");
				Console.Write("> ");

				m_Path = Console.ReadLine();

				if (m_Path != null)
					m_Path = m_Path.Trim();
			}
			while (m_Path == null || !Directory.Exists(m_Path));

			Core.DataDirectories.Add(m_Path);
		}
	}
}
