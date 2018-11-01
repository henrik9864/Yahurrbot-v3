using System;
using System.Collections.Generic;
using System.Text;

namespace YahurrFramework.Commands
{
	internal class CommandList
	{
		public int StructureLength { get; }

		public int ParameterLength { get; }

		public List<YCommand> SavedCommands { get; }

		public CommandList(int structureLength, int parameterLength)
		{
			SavedCommands = new List<YCommand>();
			StructureLength = structureLength;
			ParameterLength = parameterLength;
		}

		public void Add(YCommand command)
		{
			SavedCommands.Add(command);

			// Dont remove, this has come in handy way to many times
			//Console.WriteLine($"Command {command.Name} added with structure {StructureLength} and pLength {ParameterLength}");
		}

		public int TryGetCommand(List<string> command, out YCommand yCommand)
		{
			int best = -1;
			YCommand bestCommand = null;

			for (int i = 0; i < SavedCommands.Count; i++)
			{
				YCommand cmd = SavedCommands[i];

				int match = ValidateCommand(command, cmd);
				if (match > best)
				{
					bestCommand = cmd;
					best = match;
				}
			}

			yCommand = bestCommand;
			return best;
		}

		public void TryGetCommands(List<string> command, bool validate, ref List<YCommand> commands)
		{
			for (int i = 0; i < SavedCommands.Count; i++)
			{
				YCommand cmd = SavedCommands[i];

				int match = ValidateCommand(command, cmd);
				if (!validate || match > 0)
					commands.Add(cmd);
			}
		}

		int ValidateCommand(List<string> command, YCommand yCommand)
		{
			int i = 0;
			for (i = 0; i < command.Count; i++)
			{
				if (i >= StructureLength)
					return ValidateParams(command, yCommand) ? StructureLength : -1;

				if (command[i] != yCommand.Structure[i])
					return -1;
			}

			return i;
		}

		bool ValidateParams(List<string> command, YCommand yCommand)
		{
			for (int i = 0; i < command.Count - ParameterLength - 1; i++)
			{
				if (i >= ParameterLength)
				{
					if (yCommand.IsParam)
						return true;
					else
						return false;
				}

				// type check here
			}

			return true;
		}
	}
}
