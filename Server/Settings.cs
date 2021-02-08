using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace Server
{
	public class Settings
	{
		public static readonly Settings Configuration = new Settings("settings.ini");

		public Dictionary<string, Dictionary<string, Entry>> Values { get; set; } = new Dictionary<string, Dictionary<string, Entry>>(StringComparer.OrdinalIgnoreCase);

		public string Filename { get; set; }

		public Settings(string name)
		{
			Filename = name;
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

		public void Init()
		{
			if (!Directory.Exists(Core.BaseDirectory))
			{
				_ = Directory.CreateDirectory(Core.BaseDirectory);
			}

			try
			{
				LoadFile(Path.Combine(Core.BaseDirectory, Filename));
			}
			catch (Exception e)
			{
				Utility.WriteConsole(ConsoleColor.Red, $"Failed to load settings {e.Message}");

				Console.WriteLine("Press any key to exit...");
				_ = Console.ReadKey();

				Core.Kill(false);

				return;
			}

			if (Core.Debug)
			{
				Utility.WriteConsole(ConsoleColor.Cyan, "\n[Server Settings]");
				foreach (var setting in Values)
				{
					foreach (var entrie in setting.Value)
					{
						Utility.WriteConsole(ConsoleColor.Cyan, $"[{setting.Key}] {entrie.Value.Key}={entrie.Value.Value}");
					}
				}
				Console.WriteLine();
			}
		}

		private void LoadFile(string path)
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

				if (string.IsNullOrWhiteSpace(line) || line.StartsWith("//") || line.StartsWith("#"))
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
					throw new FormatException(string.Format("Bad format at line {0}", i + 1));
				}

				var key = line.Substring(0, io);
				var val = line.Substring(io + 1);

				if (string.IsNullOrWhiteSpace(key))
				{
					throw new NullReferenceException(string.Format("Key can not be null at line {0}", i + 1));
				}

				key = key.Trim();

				if (string.IsNullOrEmpty(val))
				{
					val = null;
				}

				if (Values.TryGetValue(section, out Dictionary<string, Entry> entries))
				{
					entries[key] = new Entry(section, key, val);
				}
				else
				{
					var newEntries = new Dictionary<string, Entry>
					{
						{ key, new Entry(section, key, val) }
					};
					Values.Add(section, newEntries);
				}
			}
		}

		private string InternalGet(string section, string key)
		{
			string result = null;
			if (Values.TryGetValue(section, out Dictionary<string, Entry> sec) && sec != null)
			{
				if (sec.TryGetValue(key, out Entry entry) && entry != null)
				{
					result = entry.Value;
				}
			}

			return result;
		}

		public T Get<T>(string section, string key)
		{
			string returnValue = InternalGet(section, key);
			if (string.IsNullOrEmpty(returnValue))
			{
				Utility.WriteConsole(ConsoleColor.Red, $"[Settings] Failed to get {key} value in {section} section");
			}
			return ConvertValue<T>(InternalGet(section, key));
		}

		public T Get<T>(string section, string key, T defaultValue)
		{
			if (string.IsNullOrEmpty(InternalGet(section, key)))
			{
				return defaultValue;
			}
			return ConvertValue<T>(InternalGet(section, key));
		}

		private static T ConvertValue<T>(string value)
		{
			return (T)Convert.ChangeType(value, typeof(T), CultureInfo.InvariantCulture);
		}
	}
}
