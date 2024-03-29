using Server.Network;

namespace Server.Menus
{
	public class QuestionMenu : IMenu
	{
		private readonly int m_Serial;
		private static int m_NextSerial;

		int IMenu.Serial => m_Serial;

		int IMenu.EntryLength => Answers.Length;

		public string Question { get; set; }

		public string[] Answers { get; }

		public QuestionMenu(string question, string[] answers)
		{
			Question = question;
			Answers = answers;

			do
			{
				m_Serial = ++m_NextSerial;
				m_Serial &= 0x7FFFFFFF;
			} while (m_Serial == 0);
		}

		public virtual void OnCancel(NetState state)
		{
		}

		public virtual void OnResponse(NetState state, int index)
		{
		}

		public void SendTo(NetState state)
		{
			state.AddMenu(this);
			state.Send(new DisplayQuestionMenu(this));
		}
	}
}
