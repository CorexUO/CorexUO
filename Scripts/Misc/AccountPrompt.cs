using System;
using Server.Accounting;

namespace Server.Misc
{
	public class AccountPrompt
	{
		public static void Initialize()
		{
			if (Accounts.Count == 0 && !Core.Service)
			{
				Utility.WriteConsole(ConsoleColor.Cyan, "This server has no accounts.");
				Console.Write("Do you want to create the owner account now? (y/n)");

				if (Console.ReadKey(true).Key == ConsoleKey.Y)
				{
					Console.WriteLine();

					Console.Write("Username: ");
					string username = Console.ReadLine();

					Console.Write("Password: ");
					string password = Console.ReadLine();

					Account a = new Account(username, password);
					a.AccessLevel = AccessLevel.Owner;

					Utility.WriteConsole(ConsoleColor.Green, "Account created.");
				}
				else
				{
					Console.WriteLine();

					Utility.WriteConsole(ConsoleColor.Yellow, "Account not created.");
				}
			}
		}
	}
}
