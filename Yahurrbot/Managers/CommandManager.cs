using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Discord.WebSocket;
using YahurrFramework.Attributes;

namespace YahurrFramework.Managers
{
	internal class CommandManager : BaseManager
	{
		internal char CommandPrefix { get; }

		Dictionary<string, List<YahurrCommand>> commands;

		public CommandManager(YahurrBot bot, DiscordSocketClient client) : base(bot, client)
		{
			commands = new Dictionary<string, List<YahurrCommand>>();
			CommandPrefix = '!';
		}

		/// <summary>
		/// Add command to list of all commands.
		/// </summary>
		/// <param name="command"></param>
		internal void AddCommand(YahurrCommand command)
		{
			if (commands.TryGetValue(command.Structure[0], out List<YahurrCommand> cmd))
				cmd.Add(command);
			else
				commands.Add(command.Structure[0], new List<YahurrCommand>() { command });
		}

		/// <summary>
		/// Add method from module to list of commands.
		/// </summary>
		/// <param name="module"></param>
		/// <param name="method"></param>
		internal void AddCommand(YahurrModule module, MethodInfo method)
		{
			AddCommand(new YahurrCommand(method, module));
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

			await RunInternalCommand(command, cmd).ConfigureAwait(false);
			await RunCommand(command, cmd).ConfigureAwait(false);
			return true;
		}

		/// <summary>
		/// Run any hardcoded internal commands.
		/// </summary>
		/// <param name="context">Context for command</param>
		/// <param name="command">Command to run</param>
		/// <returns></returns>
		async Task RunInternalCommand(SocketMessage context, List<string> command)
		{
			string output = "";

			// Might improve later
			switch (command[0])
			{
				case "help":
					output = HelpCommand(command);
					break;
			}

			if (!string.IsNullOrEmpty(output))
				await context.Channel.SendMessageAsync(output).ConfigureAwait(false);
		}

		/// <summary>
		/// Find command on module and run it.
		/// </summary>
		/// <param name="context">Context for the command.</param>
		/// <param name="command">Command with parameters split up by spaces.</param>
		/// <returns></returns>
		async Task<bool> RunCommand(SocketMessage context, List<string> command)
		{
			List<YahurrCommand> commands;
			if (!this.commands.TryGetValue(command[0], out commands))
				return false;

			for (int i = 0; i < commands.Count; i++)
			{
				YahurrCommand cmd = commands[i];

				if (cmd.Verify(command))
				{
					command.RemoveRange(0, cmd.Structure.Count - 1);

					await cmd.Invoke(command, context).ConfigureAwait(false);
				}
			}

			return true;
		}

		#region InternalCommands

		/// <summary>
		/// Start help command.
		/// </summary>
		/// <param name="commands"></param>
		/// <returns></returns>
		string HelpCommand(List<string> commands)
		{
			if (commands.Count == 1)
				return HelpAllCommand(0, 3);

			int page;
			bool succsess = int.TryParse(commands[1], out page);

			if (succsess && page > 0)
				return HelpAllCommand(page - 1, 3);
			else
				return HelpDetailCommand(commands);
		}

		/// <summary>
		/// Creats a string showing all commands for all modules
		/// </summary>
		/// <param name="commands"></param>
		/// <returns></returns>
		string HelpAllCommand(int page, int perPage)
		{
			int index = 0;
			string output = "```";
			output += "!help <page> -- View all commands sorted by module.\n";
			output += "!help <command|module> <module> -- To view a command or module in more detail.\n\n";
			output += "List of all commands by module:\n\n";

			foreach (var moduleList in SortCommands())
			{
				if (index/perPage < page)
					continue;

				output += $"{moduleList.Key.Name}:\n";

				for (int i = 0; i < moduleList.Value.Count; i++)
				{
					YahurrCommand cmd = moduleList.Value[i];
					string cmdString = "	";

					cmd.Structure.ForEach(a => cmdString += $"{a} ");
					cmd.Parameters.ForEach(a => cmdString += $"<{TypeToShorthand(a.type.Name).ToLower()}> ");
					cmdString += $"--- {cmd.Summary}\n";

					output += cmdString;
				}

				index++;
			}

			return output + "```";
		}

		/// <summary>
		/// Display detailed view of command or module.
		/// </summary>
		/// <param name="commands"></param>
		/// <returns></returns>
		string HelpDetailCommand(List<string> commands)
		{
			string name = commands[1];

			if (this.commands.TryGetValue(name, out List<YahurrCommand> cmds))
			{
				YahurrCommand cmd;

				commands.RemoveRange(0, 1);
				if (cmds.Count == 1)
					cmd = cmds[0];
				else if (!string.IsNullOrEmpty(commands[1]))
					cmd = cmds.Find(a => a.Verify(commands));
				else
					return "Ambegious name please specify module.";

				return DisplayCommand(cmd);
			}
			else
			{
				YahurrModule module = Bot.ModuleManager.LoadedModules.Find(a => a.Name == commands[1]);
				return DisplayModule(module);
			}
		}

		/// <summary>
		/// Display detailed view of command.
		/// </summary>
		/// <param name="command">Command to display</param>
		/// <returns></returns>
		string DisplayCommand(YahurrCommand command)
		{
			string output = "```";

			output += $"Child command of {command.Module.Name}:\n";
			command.Structure.ForEach(a => output += $"{a} ");
			command.Parameters.ForEach(a => output += $"<{TypeToShorthand(a.type.Name).ToLower()}> ");
			output += $" -- {command.Summary}";
			command.Parameters.ForEach(a => output += $"\n	<{TypeToShorthand(a.type.Name).ToLower()}> -- {a.summary ?? "Not defined."}");

			return output + "```";
		}

		/// <summary>
		/// Display detailed view of module.
		/// </summary>
		/// <param name="module">Module to display</param>
		/// <returns></returns>
		string DisplayModule(YahurrModule module)
		{
			string output = "```";
			output += $"{module.Name}:\n";

			List<YahurrCommand> modules = SortCommands()[module];
			for (int i = 0; i < modules.Count; i++)
			{
				YahurrCommand cmd = modules[i];
				string cmdString = "	";

				cmd.Structure.ForEach(a => cmdString += $"{a} ");
				cmd.Parameters.ForEach(a => cmdString += $"<{TypeToShorthand(a.type.Name).ToLower()}> ");
				cmdString += $"--- {cmd.Summary}\n";

				output += cmdString;
			}

			return output + "```";
		}

		/// <summary>
		/// Sorts all commands from commands dictionary.
		/// </summary>
		/// <returns></returns>
		Dictionary<YahurrModule, List<YahurrCommand>> SortCommands()
		{
			var sortedCommands = new Dictionary<YahurrModule, List<YahurrCommand>>();

			foreach (var cmds in this.commands)
			{
				List<YahurrCommand> cmdList = cmds.Value;

				for (int i = 0; i < cmdList.Count; i++)
				{
					YahurrCommand cmd = cmdList[i];

					if (sortedCommands.TryGetValue(cmd.Module, out List<YahurrCommand> toAdd))
						toAdd.Add(cmd);
					else
						sortedCommands.Add(cmd.Module, new List<YahurrCommand>() { cmd });
				}
			}

			return sortedCommands;
		}

		/// <summary>
		/// Convert System.Type name to a shorthand version.
		/// </summary>
		/// <param name="type">TypeName to convert</param>
		/// <returns></returns>
		string TypeToShorthand(string type)
		{
			switch (type)
			{
				case "Int32":
					return "Int";
				case "Int64":
					return "Int";
				default:
					return type;
			}
		}

		#endregion
	}
}
