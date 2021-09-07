using Server.Commands;
using Server.Targeting;
using System;
using System.Reflection;

namespace Server.Items
{
	public class FlipCommandHandlers
	{
		public static void Initialize()
		{
			CommandSystem.Register("Flip", AccessLevel.GameMaster, new CommandEventHandler(Flip_OnCommand));
		}

		[Usage("Flip")]
		[Description("Turns an item.")]
		public static void Flip_OnCommand(CommandEventArgs e)
		{
			e.Mobile.Target = new FlipTarget();
		}

		private class FlipTarget : Target
		{
			public FlipTarget()
				: base(-1, false, TargetFlags.None)
			{
			}

			protected override void OnTarget(Mobile from, object targeted)
			{
				if (targeted is Item)
				{
					Item item = (Item)targeted;

					if (item.Movable == false && from.AccessLevel == AccessLevel.Player)
						return;

					Type type = targeted.GetType();

					FlipableAttribute[] AttributeArray = (FlipableAttribute[])type.GetCustomAttributes(typeof(FlipableAttribute), false);

					if (AttributeArray.Length == 0)
					{
						return;
					}

					FlipableAttribute fa = AttributeArray[0];

					fa.Flip((Item)targeted);
				}
			}
		}
	}

	[AttributeUsage(AttributeTargets.Class)]
	public class DynamicFlipingAttribute : Attribute
	{
		public DynamicFlipingAttribute()
		{
		}
	}

	[AttributeUsage(AttributeTargets.Class)]
	public class FlipableAttribute : Attribute
	{
		public int[] ItemIDs { get; }

		public FlipableAttribute()
			: this(null)
		{
		}

		public FlipableAttribute(params int[] itemIDs)
		{
			ItemIDs = itemIDs;
		}

		public virtual void Flip(Item item)
		{
			if (ItemIDs == null)
			{
				try
				{
					MethodInfo flipMethod = item.GetType().GetMethod("Flip", Type.EmptyTypes);
					if (flipMethod != null)
						flipMethod.Invoke(item, new object[0]);
				}
				catch
				{
				}

			}
			else
			{
				int index = 0;
				for (int i = 0; i < ItemIDs.Length; i++)
				{
					if (item.ItemID == ItemIDs[i])
					{
						index = i + 1;
						break;
					}
				}

				if (index > ItemIDs.Length - 1)
					index = 0;

				item.ItemID = ItemIDs[index];
			}
		}
	}
}
