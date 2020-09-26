/***************************************************************************
 *                             ScriptCompiler.cs
 *                            -------------------
 *   begin                : May 1, 2002
 *   copyright            : (C) The RunUO Software Team
 *   email                : info@runuo.com
 *
 *   $Id$
 *
 ***************************************************************************/

/***************************************************************************
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Server
{
	public static class ScriptCompiler
	{
		private static Assembly[] m_Assemblies;

		public static Assembly[] Assemblies
		{
			get
			{
				return m_Assemblies;
			}
			set
			{
				m_Assemblies = value;
			}
		}

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

		public static bool Compile(bool debug, bool cache)
		{
			List<Assembly> assemblies = new List<Assembly>();

			Console.Write("Scripts: Compiling C# scripts...");

			assemblies.Add(Assembly.LoadFrom("Scripts.dll"));
			assemblies.Add(typeof(ScriptCompiler).Assembly);

			Utility.WriteConsole(ConsoleColor.Green, "done (cached)");

			m_Assemblies = assemblies.ToArray();

			Console.Write("Scripts: Verifying...");

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
			var types = m_Assemblies.SelectMany(a => a.GetTypes());

			var methods = types.Select(t => t.GetMethod(method, BindingFlags.Static | BindingFlags.Public));

			foreach (var m in methods.Where(m => m != null).AsParallel().OrderBy(CallPriorityAttribute.GetValue))
			{
				m.Invoke(null, null);
			}
		}

		private static Dictionary<Assembly, TypeCache> m_TypeCaches = new Dictionary<Assembly, TypeCache>();
		private static TypeCache m_NullCache;

		public static TypeCache GetTypeCache(Assembly asm)
		{
			if (asm == null)
			{
				if (m_NullCache == null)
					m_NullCache = new TypeCache(null);

				return m_NullCache;
			}

			TypeCache c = null;
			m_TypeCaches.TryGetValue(asm, out c);

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

			for (int i = 0; type == null && i < m_Assemblies.Length; ++i)
				type = GetTypeCache(m_Assemblies[i]).GetTypeByFullName(fullName, ignoreCase);

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

			for (int i = 0; type == null && i < m_Assemblies.Length; ++i)
				type = GetTypeCache(m_Assemblies[i]).GetTypeByName(name, ignoreCase);

			if (type == null)
				type = GetTypeCache(Core.Assembly).GetTypeByName(name, ignoreCase);

			return type;
		}
	}

	public class TypeCache
	{
		private Type[] m_Types;
		private TypeTable m_Names, m_FullNames;

		public Type[] Types { get { return m_Types; } }
		public TypeTable Names { get { return m_Names; } }
		public TypeTable FullNames { get { return m_FullNames; } }

		public Type GetTypeByName(string name, bool ignoreCase)
		{
			return m_Names.Get(name, ignoreCase);
		}

		public Type GetTypeByFullName(string fullName, bool ignoreCase)
		{
			return m_FullNames.Get(fullName, ignoreCase);
		}

		public TypeCache(Assembly asm)
		{
			if (asm == null)
				m_Types = Type.EmptyTypes;
			else
				m_Types = asm.GetTypes();

			m_Names = new TypeTable(m_Types.Length);
			m_FullNames = new TypeTable(m_Types.Length);

			for (int i = 0; i < m_Types.Length; ++i)
			{
				Type type = m_Types[i];

				m_Names.Add(type.Name, type);
				m_FullNames.Add(type.FullName, type);
			}
		}
	}

	public class TypeTable
	{
		private Dictionary<string, Type> m_Sensitive, m_Insensitive;

		public void Add(string key, Type type)
		{
			m_Sensitive[key] = type;
			m_Insensitive[key] = type;
		}

		public Type Get(string key, bool ignoreCase)
		{
			Type t = null;

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
