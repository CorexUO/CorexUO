using System;

namespace Server.Commands.Generic
{
	public sealed class WhereExtension : BaseExtension
	{
		public static ExtensionInfo ExtInfo = new ExtensionInfo(20, "Where", -1, delegate () { return new WhereExtension(); });

		public static void Initialize()
		{
			ExtensionInfo.Register(ExtInfo);
		}

		public override ExtensionInfo Info => ExtInfo;

		private ObjectConditional m_Conditional;

		public ObjectConditional Conditional => m_Conditional;

		public WhereExtension()
		{
		}

		public override void Optimize(Mobile from, Type baseType, ref AssemblyEmitter assembly)
		{
			if (baseType == null)
				throw new InvalidOperationException("Insanity.");

			m_Conditional.Compile(ref assembly);
		}

		public override void Parse(Mobile from, string[] arguments, int offset, int size)
		{
			if (size < 1)
				throw new Exception("Invalid condition syntax.");

			m_Conditional = ObjectConditional.ParseDirect(from, arguments, offset, size);
		}

		public override bool IsValid(object obj)
		{
			return m_Conditional.CheckCondition(obj);
		}
	}
}
