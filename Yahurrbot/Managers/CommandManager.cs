using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using Discord.WebSocket;
using YahurrFramework;
using YahurrFramework.Enums;
using YahurrFramework.Attributes;

namespace YahurrFramework.Managers
{
	internal class CommandManager : BaseManager
	{
		internal char CommandPrefix { get; }

		Dictionary<string, SavedCommand> savedCommands;
		Dictionary<YModule, List<YahurrCommand>> sortedCommands;

		public CommandManager(YahurrBot bot, DiscordSocketClient client) : base(bot, client)
		{
			savedCommands = new Dictionary<string, SavedCommand>();
			sortedCommands = new Dictionary<YModule, List<YahurrCommand>>();
			CommandPrefix = '!';
		}

		/// <summary>
		/// Add command to list of all commands.
		/// </summary>
		/// <param name="command"></param>
		internal void AddCommand(YahurrCommand command)
		{
			if (savedCommands.TryGetValue(command.Structure[0], out SavedCommand cmd))
				cmd.AddCommand(command);
			else
				savedCommands.Add(command.Structure[0], new SavedCommand(command));

			if (sortedCommands.TryGetValue(command.Module, out List<YahurrCommand> commands))
				commands.Add(command);
			else
				sortedCommands.Add(command.Module, new List<YahurrCommand>() { command });
		}

		/// <summary>
		/// Add method from module to list of commands.
		/// </summary>
		/// <param name="module"></param>
		/// <param name="method"></param>
		internal void AddCommand(YModule module, MethodInfo method)
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
			SavedCommand savedCommand;
			if (!this.savedCommands.TryGetValue(command[0], out savedCommand))
				return false;

			if (savedCommand.Validate(command))
			{
				YahurrCommand cmd = savedCommand.GetCommand(command);

				// Check if user can run command
				if (!ValidateCommand(context, cmd))
				{
					await context.Channel.SendMessageAsync("You do not have permission to run that command!").ConfigureAwait(false);
					return false;
				}

				try
				{
					command.RemoveRange(0, cmd.Structure.Count);
					await cmd.Invoke(command, new CommandContext(context)).ConfigureAwait(false);
				}
				catch (Exception ex)
				{
					await Bot.LoggingManager.LogMessage(LogLevel.Error, $"Unable to run command {cmd.Name}:", "ModuleManager").ConfigureAwait(false);
					await Bot.LoggingManager.LogMessage(ex, "ModuleManager").ConfigureAwait(false);
				}
			}

			return true;
		}

		/// <summary>
		/// Validates if a command can be excecuted by person.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="command"></param>
		/// <returns></returns>
		bool ValidateCommand(SocketMessage context, YahurrCommand command)
		{
			SocketGuildChannel channel = context.Channel as SocketGuildChannel;
			SocketGuildUser guildUser = context.Author as SocketGuildUser;
			ChannelFilter channelFilter = command.GetMethodAttribute<ChannelFilter>(true);
			RoleFilter roleFilter = command.GetMethodAttribute<RoleFilter>(true);

			if (channel == null || guildUser == null)
				return true;

			if (channelFilter != null && !channelFilter.IsFiltered(channel.Id))
				return false;

			if (roleFilter != null)
			{
				foreach (SocketRole role in guildUser.Roles)
				{
					if (roleFilter.IsFiltered(role.Id))
						return true;
				}

				return false;
			}

			return true;
		}

		// Commands hardcoded into the bot.
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

			foreach (var moduleList in sortedCommands)
			{
				if (index/perPage < page)
					continue;

				output += $"{moduleList.Key.Name}:\n";

				for (int i = 0; i < moduleList.Value.Count; i++)
				{
					YahurrCommand cmd = moduleList.Value[i];
					string cmdString = "	";

					cmd.Structure.ForEach(a => cmdString += $"{a} ");
					cmd.Parameters.ForEach(a => cmdString += $"<{TypeToShorthand(a.Type.Name).ToLower()}> ");
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

			if (this.savedCommands.TryGetValue(name, out SavedCommand savedCommand))
			{
				commands.RemoveRange(0, 1);
				List<YahurrCommand> cmds = savedCommand.GetCommands(commands);
				int index = 1;

				if (cmds.Count > 1 && !int.TryParse(commands[commands.Count - 1], out index))
				{
					string output = "```";

					output += "Multiple results found please specify by adding an index at the end of the command.\n";
					output += "Commands found:\n";

					for (int i = 0; i < cmds.Count; i++)
					{
						YahurrCommand cmd = cmds[i];
						output += $"{DisplayCommandSmall(cmd)}\n";
					}

					return output + "```";
				}

				// Convert from natural number
				index--;
				if (cmds.Count > 1 && (index < 0 || index >= cmds.Count))
				{
					return "```Invalid index!```";
				}

				return DisplayCommand(cmds[index]);
			}
			else
			{
				YModule module = Bot.ModuleManager.LoadedModules.Find(a => a.Name == commands[1]);
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
			command.Parameters.ForEach(a => output += $"<{TypeToShorthand(a.Type.Name).ToLower()}> ");
			output += $" -- {command.Summary}";
			command.Parameters.ForEach(a => output += $"\n	<{(a.IsParam ? "params " : "")}{(a.IsOptional ? "?" : "")}{TypeToShorthand(a.Type.Name).ToLower()}> -- {a.Summary ?? a.Name ?? "Not defined."}");

			return output + "```";
		}

		/// <summary>
		/// Display one line version of command.
		/// </summary>
		/// <param name="command"></param>
		/// <returns></returns>
		string DisplayCommandSmall(YahurrCommand command)
		{
			string output = "";

			command.Structure.ForEach(a => output += $"{a} ");
			command.Parameters.ForEach(a => output += $"<{TypeToShorthand(a.Type.Name).ToLower()}> ");
			output += $" -- {command.Summary}";

			return output;
		}

		/// <summary>
		/// Display detailed view of module.
		/// </summary>
		/// <param name="module">Module to display</param>
		/// <returns></returns>
		string DisplayModule(YModule module)
		{
			string output = "```";
			output += $"{module.Name}:\n";

			List<YahurrCommand> modules = sortedCommands[module];
			for (int i = 0; i < modules.Count; i++)
			{
				YahurrCommand cmd = modules[i];
				string cmdString = "	";

				cmd.Structure.ForEach(a => cmdString += $"{a} ");
				cmd.Parameters.ForEach(a => cmdString += $"<{TypeToShorthand(a.Type.Name).ToLower()}> ");
				cmdString += $"--- {cmd.Summary}\n";

				output += cmdString;
			}

			return output + "```";
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
