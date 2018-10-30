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

		public bool TryGetCommand(List<string> command, out YCommand yCommand)
		{
			for (int i = 0; i < SavedCommands.Count; i++)
			{
				yCommand = SavedCommands[i];

				if (ValidateCommand(command, yCommand))
					return true;
			}

			yCommand = null;
			return false;
		}

		public void TryGetCommands(List<string> command, bool validate, ref List<YCommand> commands)
		{
			for (int i = 0; i < SavedCommands.Count; i++)
			{
				YCommand cmd = SavedCommands[i];

				if (!validate || ValidateCommand(command, cmd))
					commands.Add(cmd);
			}
		}

		bool ValidateCommand(List<string> command, YCommand yCommand)
		{
			for (int i = 0; i < command.Count; i++)
			{
				if (i >= StructureLength)
					return ValidateParams(command, yCommand);

				if (command[i] != yCommand.Structure[i])
					return false;
			}

			return true;
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
