using System;
using System.Collections.Generic;

namespace Server.Engines.MLQuests
{
	[AttributeUsage(AttributeTargets.Class)]
	public class QuesterNameAttribute : Attribute
	{
		public string QuesterName { get; }

		public QuesterNameAttribute(string questerName)
		{
			QuesterName = questerName;
		}

		private static readonly Type m_Type = typeof(QuesterNameAttribute);
		private static readonly Dictionary<Type, string> m_Cache = new();

		public static string GetQuesterNameFor(Type t)
		{
			if (t == null)
				return "";


			if (m_Cache.TryGetValue(t, out string result))
				return result;

			object[] attributes = t.GetCustomAttributes(m_Type, false);

			if (attributes.Length != 0)
				result = ((QuesterNameAttribute)attributes[0]).QuesterName;
			else
				result = t.Name;

			return m_Cache[t] = result;
		}
	}
}
