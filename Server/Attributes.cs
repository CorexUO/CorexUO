using System;
using System.Collections.Generic;
using System.Reflection;

namespace Server
{
	[AttributeUsage(AttributeTargets.Property)]
	public class HueAttribute : Attribute
	{
		public HueAttribute()
		{
		}
	}

	[AttributeUsage(AttributeTargets.Property)]
	public class BodyAttribute : Attribute
	{
		public BodyAttribute()
		{
		}
	}

	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
	public class PropertyObjectAttribute : Attribute
	{
		public PropertyObjectAttribute()
		{
		}
	}

	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
	public class NoSortAttribute : Attribute
	{
		public NoSortAttribute()
		{
		}
	}

	[AttributeUsage(AttributeTargets.Method)]
	public class CallPriorityAttribute : Attribute
	{
		public static int GetValue(MethodInfo m)
		{
			if (m == null)
				return 0;

			var a = m.GetCustomAttribute<CallPriorityAttribute>(false);

			if (a != null)
				return a.Priority;

			return 0;
		}

		public int Priority { get; set; }

		public CallPriorityAttribute(int priority)
		{
			Priority = priority;
		}
	}

	public class CallPriorityComparer : IComparer<MethodInfo>
	{
		public int Compare(MethodInfo x, MethodInfo y)
		{
			if (x == null && y == null)
				return 0;

			if (x == null)
				return 1;

			if (y == null)
				return -1;

			var xPriority = GetPriority(x);
			var yPriority = GetPriority(y);

			if (xPriority > yPriority)
				return 1;

			if (xPriority < yPriority)
				return -1;

			return 0;
		}

		private static int GetPriority(MethodInfo mi)
		{
			object[] objs = mi.GetCustomAttributes(typeof(CallPriorityAttribute), true);

			if (objs == null)
				return 0;

			if (objs.Length == 0)
				return 0;


			if (objs[0] is not CallPriorityAttribute attr)
				return 0;

			return attr.Priority;
		}
	}

	[AttributeUsage(AttributeTargets.Class)]
	public class TypeAliasAttribute : Attribute
	{
		public string[] Aliases { get; }

		public TypeAliasAttribute(params string[] aliases)
		{
			Aliases = aliases;
		}
	}

	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
	public class ParsableAttribute : Attribute
	{
		public ParsableAttribute()
		{
		}
	}

	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum)]
	public class CustomEnumAttribute : Attribute
	{
		public string[] Names { get; }

		public CustomEnumAttribute(string[] names)
		{
			Names = names;
		}
	}

	[AttributeUsage(AttributeTargets.Constructor)]
	public class ConstructableAttribute : Attribute
	{
		public AccessLevel AccessLevel { get; set; }

		public ConstructableAttribute() : this(AccessLevel.Player)  //Lowest accesslevel for current functionality (Level determined by access to [add)
		{
		}

		public ConstructableAttribute(AccessLevel accessLevel)
		{
			AccessLevel = accessLevel;
		}
	}

	[AttributeUsage(AttributeTargets.Property)]
	public class CommandPropertyAttribute : Attribute
	{
		public AccessLevel ReadLevel { get; }
		public AccessLevel WriteLevel { get; }
		public bool ReadOnly { get; }

		public CommandPropertyAttribute(AccessLevel level, bool readOnly)
		{
			ReadLevel = level;
			ReadOnly = readOnly;
		}

		public CommandPropertyAttribute(AccessLevel level) : this(level, level)
		{
		}

		public CommandPropertyAttribute(AccessLevel readLevel, AccessLevel writeLevel)
		{
			ReadLevel = readLevel;
			WriteLevel = writeLevel;
		}
	}
}
