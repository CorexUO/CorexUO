using System;

namespace Server
{
	[AttributeUsage(AttributeTargets.Method)]
	public class UsageAttribute : Attribute
	{
		public string Usage { get; }

		public UsageAttribute(string usage)
		{
			Usage = usage;
		}
	}

	[AttributeUsage(AttributeTargets.Method)]
	public class DescriptionAttribute : Attribute
	{
		public string Description { get; }

		public DescriptionAttribute(string description)
		{
			Description = description;
		}
	}

	[AttributeUsage(AttributeTargets.Method)]
	public class AliasesAttribute : Attribute
	{
		public string[] Aliases { get; }

		public AliasesAttribute(params string[] aliases)
		{
			Aliases = aliases;
		}
	}
}
