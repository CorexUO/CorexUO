using System;
using System.Collections.Generic;
using System.IO;

namespace Server
{
	public static class Settings
	{
		private const string m_FileName = "settings.ini";

		private static readonly Dictionary<string, Dictionary<string, Entry>> m_Settings = new Dictionary<string, Dictionary<string, Entry>>(StringComparer.OrdinalIgnoreCase);

		static Settings()
		{
			Init();
		}

		public sealed class Entry
		{
			public string Section { get; set; }
			public string Key { get; set; }
			public string Value { get; set; }

			public Entry(string section, string key, string value)
			{
				Section = section;
				Key = key;
				Value = value;
			}
		}

		public static void Init()
		{
			if (!Directory.Exists(Core.BaseDirectory))
			{
				Directory.CreateDirectory(Core.BaseDirectory);
			}

			try
			{
				LoadFile(Path.Combine(Core.BaseDirectory, m_FileName));
			}
			catch (Exception e)
			{
				Utility.WriteConsole(ConsoleColor.Red, $"Failed to load settings {e.Message}");

				Console.WriteLine("Press any key to exit...");
				Console.ReadKey();

				Core.Kill(false);

				return;
			}

			if (Core.Debug)
			{
				Utility.WriteConsole(ConsoleColor.Cyan, "\n[Server Settings]");
				foreach (var setting in m_Settings)
				{
					foreach (var entrie in setting.Value)
					{
						Utility.WriteConsole(ConsoleColor.Cyan, $"[{setting.Key}] {entrie.Value.Key}={entrie.Value.Value}");
					}
				}
				Console.WriteLine();
			}
		}

		private static void LoadFile(string path)
		{
			var info = new FileInfo(path);

			if (!info.Exists)
			{
				throw new FileNotFoundException();
			}

			var lines = File.ReadAllLines(info.FullName);
			string section = "";
			for (int i = 0; i < lines.Length; i++)
			{
				var line = lines[i].Trim();

				if (String.IsNullOrWhiteSpace(line) || line.StartsWith("//") || line.StartsWith("#"))
				{
					continue;
				}

				if (line.StartsWith("["))
				{
					section = line.TrimStart('[').TrimEnd(']').Trim();
					continue;
				}

				int io = line.IndexOf('=');
				if (io < 0)
				{
					throw new FormatException(String.Format("Bad format at line {0}", i + 1));
				}

				var key = line.Substring(0, io);
				var val = line.Substring(io + 1);

				if (String.IsNullOrWhiteSpace(key))
				{
					throw new NullReferenceException(String.Format("Key can not be null at line {0}", i + 1));
				}

				key = key.Trim();

				if (String.IsNullOrEmpty(val))
				{
					val = null;
				}

				if (m_Settings.TryGetValue(section, out Dictionary<string, Entry> entries))
				{
					entries[key] = new Entry(section, key, val);
				}
				else
				{
					var newEntries = new Dictionary<string, Entry>
					{
						{ key, new Entry(section, key, val) }
					};
					m_Settings.Add(section, newEntries);
				}
			}
		}

		private static string InternalGet(string section, string key)
		{
			string result = null;
			if (m_Settings.TryGetValue(section, out Dictionary<string, Entry> sec) && sec != null)
			{
				if (sec.TryGetValue(key, out Entry entry) && entry != null)
				{
					result = entry.Value;
				}
			}

			return result;
		}

		public static T Get<T>(string section, string key)
		{
			string returnValue = InternalGet(section, key);
			if (string.IsNullOrEmpty(returnValue))
			{
				Utility.WriteConsole(ConsoleColor.Red, $"[Settings] Failed to get {key} value in {section} section");
			}
			return (T)Convert.ChangeType(InternalGet(section, key), typeof(T));
		}

		public static T Get<T>(string section, string key, T defaultValue)
		{
			if (string.IsNullOrEmpty(InternalGet(section, key)))
			{
				return defaultValue;
			}
			return (T)Convert.ChangeType(InternalGet(section, key), typeof(T));
		}
	}
}
