using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Server
{
	public static class Assembler
	{
		public static Assembly[] Assemblies { get; set; }

		private static readonly Type[] m_SerialTypeArray = new Type[1] { typeof(Serial) };

		private static void VerifyType(Type t)
		{
			if (t.IsSubclassOf(typeof(Item)) || t.IsSubclassOf(typeof(Mobile)))
			{
				StringBuilder warningSb = null;

				try
				{
					if (t.GetConstructor(m_SerialTypeArray) == null)
					{
						if (warningSb == null)
							warningSb = new StringBuilder();

						warningSb.AppendLine("       - No serialization constructor");
					}

					if (t.GetMethod("Serialize", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly) == null)
					{
						if (warningSb == null)
							warningSb = new StringBuilder();

						warningSb.AppendLine("       - No Serialize() method");
					}

					if (t.GetMethod("Deserialize", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly) == null)
					{
						if (warningSb == null)
							warningSb = new StringBuilder();

						warningSb.AppendLine("       - No Deserialize() method");
					}

					if (warningSb != null && warningSb.Length > 0)
					{
						Console.WriteLine("Warning: {0}\n{1}", t, warningSb.ToString());
					}
				}
				catch
				{
					Console.WriteLine("Warning: Exception in serialization verification of type {0}", t);
				}
			}
		}

		public static bool Load()
		{
			List<Assembly> assemblies = new List<Assembly>();

			Console.Write("Loading scripts...");

			assemblies.Add(Assembly.LoadFrom("Scripts.dll"));

			Utility.WriteConsole(ConsoleColor.Green, "done (cached)");

			//Load modules
			if (Directory.Exists("Modules"))
			{
				var scripts = Directory.EnumerateFiles($"{Directory.GetCurrentDirectory()}/Modules").Where(file => file.Contains(".dll")).ToArray();
				foreach (var script in scripts)
				{
					Console.Write($"Loading module: {Path.GetFileName(script)} ");

					assemblies.Add(Assembly.LoadFrom(script));

					Utility.WriteConsole(ConsoleColor.Green, "done (cached)");
				}
			}

			assemblies.Add(typeof(Assembler).Assembly);

			Assemblies = assemblies.ToArray();

			Console.Write("Verifying... ");

			Stopwatch watch = Stopwatch.StartNew();

			foreach (var assembly in assemblies)
			{
				foreach (var t in assembly.GetTypes())
				{
					VerifyType(t);
				}
			}

			watch.Stop();

			Utility.WriteConsole(ConsoleColor.Green, "done. ({0:F2} seconds)", watch.Elapsed.TotalSeconds);

			return true;
		}

		public static void Invoke(string method)
		{
			var types = Assemblies.SelectMany(a => a.GetTypes());

			var methods = types.Select(t => t.GetMethod(method, BindingFlags.Static | BindingFlags.Public));

			foreach (var m in methods.Where(m => m != null).AsParallel().OrderBy(CallPriorityAttribute.GetValue))
			{
				m.Invoke(null, null);
			}
		}

		private static readonly Dictionary<Assembly, TypeCache> m_TypeCaches = new Dictionary<Assembly, TypeCache>();
		private static TypeCache m_NullCache;

		public static TypeCache GetTypeCache(Assembly asm)
		{
			if (asm == null)
			{
				if (m_NullCache == null)
					m_NullCache = new TypeCache(null);

				return m_NullCache;
			}

			m_TypeCaches.TryGetValue(asm, out TypeCache c);

			if (c == null)
				m_TypeCaches[asm] = c = new TypeCache(asm);

			return c;
		}

		public static Type FindTypeByFullName(string fullName)
		{
			return FindTypeByFullName(fullName, true);
		}

		public static Type FindTypeByFullName(string fullName, bool ignoreCase)
		{
			Type type = null;

			if (string.IsNullOrWhiteSpace(fullName))
				return null;

			for (int i = 0; type == null && i < Assemblies.Length; ++i)
				type = GetTypeCache(Assemblies[i]).GetTypeByFullName(fullName, ignoreCase);

			if (type == null)
				type = GetTypeCache(Core.Assembly).GetTypeByFullName(fullName, ignoreCase);

			return type;
		}

		public static Type FindTypeByName(string name)
		{
			return FindTypeByName(name, true);
		}

		public static Type FindTypeByName(string name, bool ignoreCase)
		{
			Type type = null;

			if (string.IsNullOrWhiteSpace(name))
				return null;

			for (int i = 0; type == null && i < Assemblies.Length; ++i)
				type = GetTypeCache(Assemblies[i]).GetTypeByName(name, ignoreCase);

			if (type == null)
				type = GetTypeCache(Core.Assembly).GetTypeByName(name, ignoreCase);

			return type;
		}
	}

	public class TypeCache
	{
		public Type[] Types { get; }
		public TypeTable Names { get; }
		public TypeTable FullNames { get; }

		public Type GetTypeByName(string name, bool ignoreCase)
		{
			return Names.Get(name, ignoreCase);
		}

		public Type GetTypeByFullName(string fullName, bool ignoreCase)
		{
			return FullNames.Get(fullName, ignoreCase);
		}

		public TypeCache(Assembly asm)
		{
			if (asm == null)
				Types = Type.EmptyTypes;
			else
				Types = asm.GetTypes();

			Names = new TypeTable(Types.Length);
			FullNames = new TypeTable(Types.Length);

			for (int i = 0; i < Types.Length; ++i)
			{
				Type type = Types[i];

				Names.Add(type.Name, type);
				FullNames.Add(type.FullName, type);
			}
		}
	}

	public class TypeTable
	{
		private readonly Dictionary<string, Type> m_Sensitive, m_Insensitive;

		public void Add(string key, Type type)
		{
			m_Sensitive[key] = type;
			m_Insensitive[key] = type;
		}

		public Type Get(string key, bool ignoreCase)
		{
			Type t;
			if (ignoreCase)
				m_Insensitive.TryGetValue(key, out t);
			else
				m_Sensitive.TryGetValue(key, out t);

			return t;
		}

		public TypeTable(int capacity)
		{
			m_Sensitive = new Dictionary<string, Type>(capacity);
			m_Insensitive = new Dictionary<string, Type>(capacity, StringComparer.OrdinalIgnoreCase);
		}
	}
}
