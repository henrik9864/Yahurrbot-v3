using System;
using System.Collections.Generic;
using System.Linq;

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
				{
					if (list.TryGetCommand(command, out yCommand))
						return true;
				}
			}

			yCommand = null;
			return false;
		}

		public bool TryGetCommands(List<string> command, out List<YCommand> yCommand)
		{
			yCommand = new List<YCommand>();

			foreach (var item in savedCommands)
			{
				yCommand.AddRange(item.Value.SavedCommands);
			}

			return yCommand.Count > 0;
		}
	}
}
