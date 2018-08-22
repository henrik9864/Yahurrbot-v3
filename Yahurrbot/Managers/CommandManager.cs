using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord.WebSocket;

namespace YahurrFramework.Managers
{
	internal class CommandManager : BaseManager
	{
		internal char CommandPrefix { get; }

		public CommandManager(YahurrBot bot, DiscordSocketClient client) : base(bot, client)
		{
			CommandPrefix = '!';
		}

		/// <summary>
		/// Validate and run command from SocketMessage
		/// </summary>
		/// <param name="command"></param>
		/// <returns></returns>
		internal async Task<bool> RunCommand(SocketMessage command)
		{
			string msg = command.Content;
			if (msg[0] != CommandPrefix)
				return false;
			else
				msg = msg.Substring(1);

			List<string> cmd = new List<string>();
			cmd.AddRange(msg.Split(' '));

			await RunCommand(command, cmd).ConfigureAwait(false);
			return true;
		}

		/// <summary>
		/// Find command on module and run it.
		/// </summary>
		/// <param name="context">Context for the command.</param>
		/// <param name="command">Command with parameters split up by spaces.</param>
		/// <returns></returns>
		async Task<bool> RunCommand(SocketMessage context, List<string> command)
		{
			for (int i = 0; i < Bot.ModuleManager.LoadedModules.Count; i++)
			{
				YahurrLoadedModule loadedModule = Bot.ModuleManager.LoadedModules[i];

				if (loadedModule.VerifyCommand(command, out YahurrCommand cmd))
				{
					command.RemoveRange(0, (command.Count - cmd.Parameters.Count));

					loadedModule.Module.SetContext(context);
					await cmd.Invoke(loadedModule.Module, command).ConfigureAwait(false);
					loadedModule.Module.SetContext(null);

					return true;
				}
			}

			return false;
		}
	}
}
