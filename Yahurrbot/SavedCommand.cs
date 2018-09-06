using System;
using System.Collections.Generic;
using System.Text;

namespace YahurrFramework
{
    public class SavedCommand
    {
		public string Token { get; }

		Dictionary<string, SavedCommand> subCommands;
		List<YahurrCommand> commands;

		public SavedCommand(YahurrCommand command) : this(command, 0)
		{

		}

		private SavedCommand(YahurrCommand command, int depth)
		{
			this.Token = command.Structure[depth];
			this.subCommands = new Dictionary<string, SavedCommand>();
			this.commands = new List<YahurrCommand>();

			AddCommand(command, depth);
		}

		public void AddCommand(YahurrCommand command)
		{
			AddCommand(command, 0);
		}

		public bool Validate(List<string> command)
		{
			return Validate(command, 0);
		}

		public YahurrCommand GetCommand(List<string> command)
		{
			(SavedCommand cmd, int depth) = GetSavedCommand(command, 0);
			int index = cmd.MatchCommand(command, depth + 1);

			return cmd.commands[index];
		}

		public List<YahurrCommand> GetCommands(List<string> command)
		{
			(SavedCommand cmd, int depth) = GetSavedCommand(command, 0);
			return cmd?.commands;
		}

		/// <summary>
		/// Add a command to this saved command.
		/// </summary>
		/// <param name="command"></param>
		void AddCommand(YahurrCommand command, int depth)
		{
			string token = command.Structure[depth];
			if (token != Token)
				return;

			if (depth >= command.Structure.Count - 1)
			{
				commands.Add(command);
				return;
			}

			string nextToken = command.Structure[depth + 1];
			if (subCommands.TryGetValue(nextToken, out SavedCommand subCommand))
				subCommand.AddCommand(command, depth + 1);
			else
				subCommands.Add(nextToken, new SavedCommand(command, depth + 1));
		}

		/// <summary>
		/// Check if command exists.
		/// </summary>
		/// <param name="command"></param>
		/// <returns></returns>
		bool Validate(List<string> command, int depth)
		{
			string token = command[depth];
			if (token != Token)
				return false;

			string nextToken = "";
			if (command.Count - 1 != depth)
				nextToken = command[depth + 1];

			if (subCommands.TryGetValue(nextToken, out SavedCommand subCommand))
				return subCommand.Validate(command, depth + 1);
			else
				return MatchCommand(command, depth + 1) != -1;
		}

		YahurrCommand GetCommand(List<string> command, int depth)
		{
			string token = command[depth];
			if (token != Token)
				return null;

			string nextToken = "";
			if (command.Count - 1 != depth)
				nextToken = command[depth + 1];

			if (subCommands.TryGetValue(nextToken, out SavedCommand subCommand))
				return subCommand.GetCommand(command, depth + 1);
			else
			{
				int index = MatchCommand(command, depth + 1);
				Console.WriteLine(index);
				return commands[index];
			}
		}

		(SavedCommand command, int depth) GetSavedCommand(List<string> command, int depth)
		{
			string token = command[depth];
			if (token != Token)
				return (null, -1);

			string nextToken = "";
			if (command.Count - 1 != depth)
				nextToken = command[depth + 1];

			if (subCommands.TryGetValue(nextToken, out SavedCommand subCommand))
				return subCommand.GetSavedCommand(command, depth + 1);
			else
				return (this, depth);
		}

		int MatchCommand(List<string> command, int paramStart)
		{
			Console.WriteLine("Matching params...");

			if (commands.Count == 0)
				return -1;

			for (int i = 0; i < commands.Count; i++)
			{
				if (commands[i].VerifyParameters(command))
					return i;
			}

			return -1;
		}
	}
}
