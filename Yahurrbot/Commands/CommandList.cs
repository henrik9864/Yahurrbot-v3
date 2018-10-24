using System;
using System.Collections.Generic;
using System.Text;

namespace YahurrFramework.Commands
{
	internal class CommandList
	{
		public int StructureLength { get; }

		public int ParameterLength { get; }

		List<YCommand> savedCommands;

		public CommandList(int structureLength, int parameterLength)
		{
			savedCommands = new List<YCommand>();
			StructureLength = structureLength;
			ParameterLength = parameterLength;
		}

		public void Add(YCommand command)
		{
			savedCommands.Add(command);

			//Console.WriteLine($"{savedCommands.Count} commands added: {StructureLength}:{ParameterLength}");
		}

		public bool TryGetCommand(List<string> command, out YCommand yCommand)
		{
			for (int i = 0; i < savedCommands.Count; i++)
			{
				yCommand = savedCommands[i];

				if (ValidateCommand(command, yCommand))
					return true;
			}

			yCommand = null;
			return false;
		}

		bool ValidateCommand(List<string> command, YCommand yCommand)
		{
			for (int i = 0; i < yCommand.Structure.Count; i++)
			{
				if (yCommand.Structure[i] != command[i])
					return false;
			}

			return true;
		}
	}
}
