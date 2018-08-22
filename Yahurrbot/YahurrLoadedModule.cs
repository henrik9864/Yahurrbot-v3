using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace YahurrFramework
{
    public class YahurrLoadedModule
    {
		public YahurrModule Module { get; }

		public List<YahurrCommand> Commands { get; }

		public YahurrLoadedModule(YahurrModule module, List<YahurrCommand> commands)
		{
			this.Module = module;
			this.Commands = commands;
		}

		public bool VerifyCommand(List<string> command, out YahurrCommand yahurrCommand)
		{
			for (int i = 0; i < Commands.Count; i++)
			{
				YahurrCommand cmd = Commands[i];

				if (cmd.Verify(command))
				{
					yahurrCommand = cmd;
					return true;
				}
			}

			yahurrCommand = null;
			return false;
		}
	}
}
