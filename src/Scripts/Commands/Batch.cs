using Server.Commands.Generic;
using Server.Gumps;
using Server.Network;
using System;
using System.Collections;
using System.Reflection;

namespace Server.Commands
{
	public class Batch : BaseCommand
	{
		public BaseCommandImplementor Scope { get; set; }
		public string Condition { get; set; }
		public ArrayList BatchCommands { get; }

		public Batch()
		{
			Commands = new string[] { "Batch" };
			ListOptimized = true;

			BatchCommands = new ArrayList();
			Condition = "";
		}

		public override void ExecuteList(CommandEventArgs e, ArrayList list)
		{
			if (list.Count == 0)
			{
				LogFailure("Nothing was found to use this command on.");
				return;
			}

			try
			{
				BaseCommand[] commands = new BaseCommand[BatchCommands.Count];
				CommandEventArgs[] eventArgs = new CommandEventArgs[BatchCommands.Count];

				for (int i = 0; i < BatchCommands.Count; ++i)
				{
					BatchCommand bc = (BatchCommand)BatchCommands[i];


					bc.GetDetails(out string commandString, out string argString, out string[] args);

					BaseCommand command = Scope.Commands[commandString];

					commands[i] = command;
					eventArgs[i] = new CommandEventArgs(e.Mobile, commandString, argString, args);

					if (command == null)
					{
						e.Mobile.SendMessage("That is either an invalid command name or one that does not support this modifier: {0}.", commandString);
						return;
					}
					else if (e.Mobile.AccessLevel < command.AccessLevel)
					{
						e.Mobile.SendMessage("You do not have access to that command: {0}.", commandString);
						return;
					}
					else if (!command.ValidateArgs(Scope, eventArgs[i]))
					{
						return;
					}
				}

				for (int i = 0; i < commands.Length; ++i)
				{
					BaseCommand command = commands[i];
					BatchCommand bc = (BatchCommand)BatchCommands[i];

					if (list.Count > 20)
						CommandLogging.Enabled = false;

					ArrayList usedList;

					if (Utility.InsensitiveCompare(bc.Object, "Current") == 0)
					{
						usedList = list;
					}
					else
					{
						Hashtable propertyChains = new();

						usedList = new ArrayList(list.Count);

						for (int j = 0; j < list.Count; ++j)
						{
							object obj = list[j];

							if (obj == null)
								continue;

							Type type = obj.GetType();

							PropertyInfo[] chain = (PropertyInfo[])propertyChains[type];

							string failReason = "";

							if (chain == null && !propertyChains.Contains(type))
								propertyChains[type] = chain = Properties.GetPropertyInfoChain(e.Mobile, type, bc.Object, PropertyAccess.Read, ref failReason);

							if (chain == null)
								continue;

							PropertyInfo endProp = Properties.GetPropertyInfo(ref obj, chain, ref failReason);

							if (endProp == null)
								continue;

							try
							{
								obj = endProp.GetValue(obj, null);

								if (obj != null)
									usedList.Add(obj);
							}
							catch
							{
							}
						}
					}

					command.ExecuteList(eventArgs[i], usedList);

					if (list.Count > 20)
						CommandLogging.Enabled = true;

					command.Flush(e.Mobile, list.Count > 20);
				}
			}
			catch (Exception ex)
			{
				e.Mobile.SendMessage(ex.Message);
			}
		}

		public bool Run(Mobile from)
		{
			if (Scope == null)
			{
				from.SendMessage("You must select the batch command scope.");
				return false;
			}
			else if (Condition.Length > 0 && !Scope.SupportsConditionals)
			{
				from.SendMessage("This command scope does not support conditionals.");
				return false;
			}
			else if (Condition.Length > 0 && !Utility.InsensitiveStartsWith(Condition, "where"))
			{
				from.SendMessage("The condition field must start with \"where\".");
				return false;
			}

			string[] args = CommandSystem.Split(Condition);

			Scope.Process(from, this, args);

			return true;
		}

		public static void Initialize()
		{
			CommandSystem.Register("Batch", AccessLevel.Counselor, new CommandEventHandler(Batch_OnCommand));
		}

		[Usage("Batch")]
		[Description("Allows multiple commands to be run at the same time.")]
		public static void Batch_OnCommand(CommandEventArgs e)
		{
			Batch batch = new();

			e.Mobile.SendGump(new BatchGump(e.Mobile, batch));
		}
	}

	public class BatchCommand
	{
		public string Command { get; set; }
		public string Object { get; set; }

		public void GetDetails(out string command, out string argString, out string[] args)
		{
			int indexOf = Command.IndexOf(' ');

			if (indexOf >= 0)
			{
				argString = Command.Substring(indexOf + 1);

				command = Command.Substring(0, indexOf);
				args = CommandSystem.Split(argString);
			}
			else
			{
				argString = "";
				command = Command.ToLower();
				args = Array.Empty<string>();
			}
		}

		public BatchCommand(string command, string obj)
		{
			Command = command;
			Object = obj;
		}
	}

	public class BatchGump : BaseGridGump
	{
		private readonly Mobile m_From;
		private readonly Batch m_Batch;

		public BatchGump(Mobile from, Batch batch) : base(30, 30)
		{
			m_From = from;
			m_Batch = batch;

			Render();
		}

		public void Render()
		{
			AddNewPage();

			/* Header */
			AddEntryHeader(20);
			AddEntryHtml(180, Center("Batch Commands"));
			AddEntryHeader(20);
			AddNewLine();

			AddEntryHeader(9);
			AddEntryLabel(191, "Run Batch");
			AddEntryButton(20, ArrowRightID1, ArrowRightID2, GetButtonID(1, 0, 0), ArrowRightWidth, ArrowRightHeight);
			AddNewLine();

			AddBlankLine();

			/* Scope */
			AddEntryHeader(20);
			AddEntryHtml(180, Center("Scope"));
			AddEntryHeader(20);
			AddNewLine();

			AddEntryHeader(9);
			AddEntryLabel(191, m_Batch.Scope == null ? "Select Scope" : m_Batch.Scope.Accessors[0]);
			AddEntryButton(20, ArrowRightID1, ArrowRightID2, GetButtonID(1, 0, 1), ArrowRightWidth, ArrowRightHeight);
			AddNewLine();

			AddBlankLine();

			/* Condition */
			AddEntryHeader(20);
			AddEntryHtml(180, Center("Condition"));
			AddEntryHeader(20);
			AddNewLine();

			AddEntryHeader(9);
			AddEntryText(202, 0, m_Batch.Condition);
			AddEntryHeader(9);
			AddNewLine();

			AddBlankLine();

			/* Commands */
			AddEntryHeader(20);
			AddEntryHtml(180, Center("Commands"));
			AddEntryHeader(20);

			for (int i = 0; i < m_Batch.BatchCommands.Count; ++i)
			{
				BatchCommand bc = (BatchCommand)m_Batch.BatchCommands[i];

				AddNewLine();

				AddImageTiled(CurrentX, CurrentY, 9, 2, 0x24A8);
				AddImageTiled(CurrentX, CurrentY + 2, 2, EntryHeight + OffsetSize + EntryHeight - 4, 0x24A8);
				AddImageTiled(CurrentX, CurrentY + EntryHeight + OffsetSize + EntryHeight - 2, 9, 2, 0x24A8);
				AddImageTiled(CurrentX + 3, CurrentY + 3, 6, EntryHeight + EntryHeight - 4 - OffsetSize, HeaderGumpID);

				IncreaseX(9);
				AddEntryText(202, 1 + (i * 2), bc.Command);
				AddEntryHeader(9, 2);

				AddNewLine();

				IncreaseX(9);
				AddEntryText(202, 2 + (i * 2), bc.Object);
			}

			AddNewLine();

			AddEntryHeader(9);
			AddEntryLabel(191, "Add New Command");
			AddEntryButton(20, ArrowRightID1, ArrowRightID2, GetButtonID(1, 0, 2), ArrowRightWidth, ArrowRightHeight);

			FinishPage();
		}

		public override void OnResponse(NetState sender, RelayInfo info)
		{

			if (!SplitButtonID(info.ButtonID, 1, out int type, out int index))
				return;

			TextRelay entry = info.GetTextEntry(0);

			if (entry != null)
				m_Batch.Condition = entry.Text;

			for (int i = m_Batch.BatchCommands.Count - 1; i >= 0; --i)
			{
				BatchCommand sc = (BatchCommand)m_Batch.BatchCommands[i];

				entry = info.GetTextEntry(1 + (i * 2));

				if (entry != null)
					sc.Command = entry.Text;

				entry = info.GetTextEntry(2 + (i * 2));

				if (entry != null)
					sc.Object = entry.Text;

				if (sc.Command.Length == 0 && sc.Object.Length == 0)
					m_Batch.BatchCommands.RemoveAt(i);
			}

			switch (type)
			{
				case 0: // main
					{
						switch (index)
						{
							case 0: // run
								{
									m_Batch.Run(m_From);
									break;
								}
							case 1: // set scope
								{
									m_From.SendGump(new BatchScopeGump(m_From, m_Batch));
									return;
								}
							case 2: // add command
								{
									m_Batch.BatchCommands.Add(new BatchCommand("", ""));
									break;
								}
						}

						break;
					}
			}

			m_From.SendGump(new BatchGump(m_From, m_Batch));
		}
	}

	public class BatchScopeGump : BaseGridGump
	{
		private readonly Mobile m_From;
		private readonly Batch m_Batch;

		public BatchScopeGump(Mobile from, Batch batch) : base(30, 30)
		{
			m_From = from;
			m_Batch = batch;

			Render();
		}

		public void Render()
		{
			AddNewPage();

			/* Header */
			AddEntryHeader(20);
			AddEntryHtml(140, Center("Change Scope"));
			AddEntryHeader(20);

			/* Options */
			for (int i = 0; i < BaseCommandImplementor.Implementors.Count; ++i)
			{
				BaseCommandImplementor impl = BaseCommandImplementor.Implementors[i];

				if (m_From.AccessLevel < impl.AccessLevel)
					continue;

				AddNewLine();

				AddEntryLabel(20 + OffsetSize + 140, impl.Accessors[0]);
				AddEntryButton(20, ArrowRightID1, ArrowRightID2, GetButtonID(1, 0, i), ArrowRightWidth, ArrowRightHeight);
			}

			FinishPage();
		}

		public override void OnResponse(NetState sender, RelayInfo info)
		{

			if (SplitButtonID(info.ButtonID, 1, out int type, out int index))
			{
				switch (type)
				{
					case 0:
						{
							if (index < BaseCommandImplementor.Implementors.Count)
							{
								BaseCommandImplementor impl = BaseCommandImplementor.Implementors[index];

								if (m_From.AccessLevel >= impl.AccessLevel)
									m_Batch.Scope = impl;
							}

							break;
						}
				}
			}

			m_From.SendGump(new BatchGump(m_From, m_Batch));
		}
	}
}
