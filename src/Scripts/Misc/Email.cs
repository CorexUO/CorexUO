using System;
using System.Net.Mail;
using System.Text.RegularExpressions;
using System.Threading;

namespace Server.Misc
{
	public class Email
	{
		/* In order to support emailing, fill in EmailServer and FromAddress:
		 * Example:
		 *  public static readonly string EmailServer = "mail.domain.com";
		 *  public static readonly string FromAddress = "corexuo@domain.com";
		 *
		 * If you want to add crash reporting emailing, fill in CrashAddresses:
		 * Example:
		 *  public static readonly string CrashAddresses = "first@email.here,second@email.here,third@email.here";
		 *
		 * If you want to add speech log page emailing, fill in SpeechLogPageAddresses:
		 * Example:
		 *  public static readonly string SpeechLogPageAddresses = "first@email.here,second@email.here,third@email.here";
		 */

		public static readonly bool Enabled = Settings.Configuration.Get<bool>("Email", "Enabled", false);
		public static readonly string EmailServer = Settings.Configuration.Get<string>("Email", "Server", null);
		public static readonly int EmailPort = Settings.Configuration.Get<int>("Email", "Port", 25);

		public static readonly string FromAddress = Settings.Configuration.Get<string>("Email", "FromAddress", null);
		public static readonly string EmailUsername = Settings.Configuration.Get<string>("Email", "Username", null);
		public static readonly string EmailPassword = Settings.Configuration.Get<string>("Email", "Password", null);

		public static readonly string CrashAddresses = Settings.Configuration.Get<string>("Email", "CrashAddresses", null);
		public static readonly string SpeechLogPageAddresses = Settings.Configuration.Get<string>("Email", "SpeechLogPageAddresses", null);

		private static readonly Regex _pattern = new(@"^[a-z0-9.+_-]+@([a-z0-9-]+\.)+[a-z]+$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

		public static bool IsValid(string address)
		{
			if (address == null || address.Length > 320)
				return false;

			return _pattern.IsMatch(address);
		}

		private static SmtpClient _Client;

		public static void Configure()
		{
			if (Enabled && EmailServer != null)
			{
				_Client = new SmtpClient(EmailServer, EmailPort);
				if (EmailUsername != null)
				{
					_Client.Credentials = new System.Net.NetworkCredential(EmailUsername, EmailPassword);
				}
			}
		}

		public static bool Send(MailMessage message)
		{
			try
			{
				// .NET relies on the MTA to generate Message-ID header. Not all MTAs will add this header.

				DateTime now = DateTime.UtcNow;
				string messageID = string.Format("<{0}.{1}@{2}>", now.ToString("yyyyMMdd"), now.ToString("HHmmssff"), EmailServer);
				message.Headers.Add("Message-ID", messageID);

				message.Headers.Add("X-Mailer", "CorexUO");

				lock (_Client)
				{
					_Client.Send(message);
				}
			}
			catch
			{
				return false;
			}

			return true;
		}

		public static void AsyncSend(MailMessage message)
		{
			ThreadPool.QueueUserWorkItem(new WaitCallback(SendCallback), message);
		}

		private static void SendCallback(object state)
		{
			MailMessage message = (MailMessage)state;

			if (Send(message))
				Console.WriteLine("Sent e-mail '{0}' to '{1}'.", message.Subject, message.To);
			else
				Console.WriteLine("Failure sending e-mail '{0}' to '{1}'.", message.Subject, message.To);
		}
	}
}
