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
			int pLength = command.Parameters.Count;

			if (savedCommands.TryGetValue(pLength, out CommandList list))
				list.Add(command);
			else
			{
				list = new CommandList(StructureLength, pLength);
				list.Add(command);

				savedCommands.Add(pLength, list);
			}
		}

		public int TryGetCommand(List<string> command, out YCommand yCommand)
		{
			int pLength = command.Count - StructureLength;
			int best = -1;
			YCommand bestCommand = null;

			for (int i = 0; i <= pLength; i++)
			{
				if (savedCommands.TryGetValue(i, out CommandList list))
				{
					int match = list.TryGetCommand(command, out YCommand cmd);

					if (match > best)
					{
						bestCommand = cmd;
						best = match;
					}
				}
			}

			yCommand = bestCommand;
			return best;
		}

		public void TryGetCommands(List<string> command, bool validate, bool validateParam, ref List<YCommand> foundCommands)
		{
			foreach (var cmdList in savedCommands)
			{
				cmdList.Value.TryGetCommands(command, validate, validateParam, ref foundCommands);
			}
		}
	}
}
