using System;
using System.Collections.Generic;

namespace YahurrFramework.Commands
{
	internal class CommandNode
	{
		public int StructureLength { get; }

		Dictionary<int, CommandList> savedCommands;

		public CommandNode(int structureLength)
		{
			this.StructureLength = structureLength;
			savedCommands = new Dictionary<int, CommandList>();
		}

		public void Add(YCommand command)
		{
			int parameterLength = command.Parameters.Count;

			if (savedCommands.TryGetValue(parameterLength, out CommandList list))
				list.Add(command);
			else
			{
				list = new CommandList(StructureLength, parameterLength);
				list.Add(command);

				savedCommands.Add(parameterLength, list);
			}
		}

		public bool TryGetCommand(List<string> command, out YCommand yCommand)
		{
			int pLength = command.Count - StructureLength;

			for (int i = 0; i <= pLength; i++)
			{
				if (savedCommands.TryGetValue(i, out CommandList list))
					return list.TryGetCommand(command, out yCommand);
			}

			yCommand = null;
			return false;
		}
	}
}
