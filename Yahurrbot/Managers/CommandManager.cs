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
using YahurrFramework.Commands;

namespace YahurrFramework.Managers
{
	internal class CommandManager : BaseManager
	{
		internal char CommandPrefix { get; }

		int maxLength = 0;
		Dictionary<int, CommandNode> savedCommands;

		public CommandManager(YahurrBot bot, DiscordSocketClient client) : base(bot, client)
		{
			savedCommands = new Dictionary<int, CommandNode>();
			CommandPrefix = '!';
		}

		/// <summary>
		/// Add command to list of all commands.
		/// </summary>
		/// <param name="command"></param>
		internal void AddCommand(YCommand command)
		{
			int structureLength = command.Structure.Count;

			if (structureLength > maxLength)
				maxLength = structureLength;

			if (savedCommands.TryGetValue(structureLength, out CommandNode node))
				node.Add(command);
			else
			{
				node = new CommandNode(structureLength);
				node.Add(command);

				savedCommands.Add(structureLength, node);
			}
		}

		internal void AddCommand(YModule module, MethodInfo method)
		{
			AddCommand(new YCommand(method, module));
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

			if (!await RunInternalCommand(command, cmd).ConfigureAwait(false))
				await RunCommand(command, cmd).ConfigureAwait(false);

			return true;
		}

		/// <summary>
		/// Run any hardcoded internal commands.
		/// </summary>
		/// <param name="context">Context for command</param>
		/// <param name="command">Command to run</param>
		/// <returns></returns>
		async Task<bool> RunInternalCommand(SocketMessage context, List<string> command)
		{
			// En kommando på villspor
			// Validere kommando navn
			string output = "";
			int succsess = -1;

			// Might improve later
			switch (command[0])
			{
				case "help":
					succsess = HelpCommand(command, ref output) ? 1 : 0;
					break;
			}

			if (succsess == 1)
				await context.Channel.SendMessageAsync(output).ConfigureAwait(false);
			else if (succsess == 0)
				await context.Channel.SendMessageAsync("```" +
														"Oppsie woopsie our code made a do do, our code monekys are working hard to fix it.\n" +
														"In the mean time have a laugh at this hilarious joke.\n" +
														"Why did the monky fall out of the tree? Because it was DEAD.\n" +
														"```").ConfigureAwait(false);

			return succsess != -1;
		}

		/// <summary>
		/// Find command on module and run it.
		/// </summary>
		/// <param name="context">Context for the command.</param>
		/// <param name="command">Command with parameters split up by spaces.</param>
		/// <returns></returns>
		async Task<bool> RunCommand(SocketMessage context, List<string> command)
		{
			if (!GetCommand(command, out YCommand savedCommand))
				await context.Channel.SendMessageAsync("Command not found.").ConfigureAwait(false);

			// Check if user can run command
			string reason = "";
			if (!ValidateCommand(context, savedCommand, ref reason))
			{
				await context.Channel.SendMessageAsync(reason).ConfigureAwait(false);
				return false;
			}

			try
			{
				await savedCommand.Invoke(command, new MethodContext(context)).ConfigureAwait(false);
			}
			catch (Exception ex)
			{
				await Bot.LoggingManager.LogMessage(LogLevel.Error, $"Unable to run command {savedCommand.Name}:", "ModuleManager").ConfigureAwait(false);
				await Bot.LoggingManager.LogMessage(ex, "ModuleManager").ConfigureAwait(false);
			}

			return true;
		}

		/// <summary>
		/// Validates if a command can be excecuted by person.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="command"></param>
		/// <returns></returns>
		bool ValidateCommand(SocketMessage context, YCommand command, ref string reason)
		{
			SocketGuildChannel channel = context.Channel as SocketGuildChannel;
			SocketGuildUser guildUser = context.Author as SocketGuildUser;
			ChannelFilter channelFilter = command.GetAttribute<ChannelFilter>(true);
			RoleFilter roleFilter = command.GetAttribute<RoleFilter>(true);

			if (channelFilter != null && !channelFilter.IsFiltered(channel?.Id ?? 0))
			{
				reason = "This command cannot be run in this channel.";
				return false;
			}

			if (command.IsDM && !(context.Channel is SocketDMChannel))
			{
				reason = "This is a DM only command.";
				return false;
			}

			if (roleFilter != null)
			{
				foreach (SocketRole role in guildUser.Roles)
				{
					if (roleFilter.IsFiltered(role.Id))
						return true;
				}

				reason = "You do not have permission to run this command.";
				return false;
			}

			// Assume valid untill proven othervise
			return true;
		}

		bool GetCommand(List<string> command, out YCommand savedCommand)
		{
			for (int i = 1; i <= command.Count; i++)
			{
				if (savedCommands.TryGetValue(i, out CommandNode node))
				{
					if (node.TryGetCommand(command, out savedCommand))
						return true;
				}
			}

			savedCommand = null;
			return false;
		}

		bool GetCommands(List<string> command, out List<YCommand> savedCommands)
		{
			List<YCommand> foundCommands = new List<YCommand>();
			for (int i = 0; i <= maxLength; i++)
			{
				if (this.savedCommands.TryGetValue(i, out CommandNode node))
					node.TryGetCommands(command, ref foundCommands);
			}

			savedCommands = foundCommands;
			return foundCommands.Count > 0;
		}

		#region Internal Command

		bool HelpCommand(List<string> command, ref string output)
		{
			command.RemoveAt(0);

			if (!GetCommands(command, out List<YCommand> savedCommands))
				return false;

			output = "```";
			output += $"{savedCommands.Count} command{(savedCommands.Count==1?"":"s")} found.\n";

			foreach (var cmd in savedCommands)
			{
				output += $"	{cmd.Name}\n";
			}

			output += "```";
			return true;
		}

		#endregion

		// Need to fix
		/*

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

		*/
	}
}
