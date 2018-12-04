using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Discord.WebSocket;
using YahurrFramework.Enums;
using YahurrFramework.Attributes;
using YahurrFramework.Commands;
using System.Linq;
using YahurrFramework.Structs;

namespace YahurrFramework.Managers
{
	internal class CommandManager : BaseManager
	{
		internal char CommandPrefix { get; set; } = '!';

		int maxLength = 0;
		Dictionary<int, CommandNode> savedCommands;

		public CommandManager(YahurrBot bot, DiscordSocketClient client) : base(bot, client)
		{
			savedCommands = new Dictionary<int, CommandNode>();
		}

		/// <summary>
		/// Add command to list of all commands.
		/// </summary>
		/// <param name="command"></param>
		internal void AddCommand(YCommand command)
		{
			lock (savedCommands)
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
		internal async Task<bool> RunMessageCommand(SocketMessage command)
		{
			if (!TryFindCommand(command.Content, out string msg, out bool silent))
				return false;

			List<string> cmd = new List<string>();
			cmd.AddRange(msg.Split(' '));

			if (!await RunInternalCommand(command, cmd).ConfigureAwait(false))
				await RunCommand(command, cmd, silent).ConfigureAwait(false);

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
					succsess = HelpCommand(command, context, 20, ref output) ? 1 : 0;
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
		async Task<bool> RunCommand(SocketMessage context, List<string> command, bool silent = false)
		{
			if (!GetCommand(command, out YCommand savedCommand))
			{
				if (!silent)
					await context.Channel.SendMessageAsync("Command not found.").ConfigureAwait(false);

				return false;
			}

			// Check if user can run command
			if (!Bot.PermissionManager.CanRun(savedCommand, context))
			{
				await context.Channel.SendMessageAsync("Command not found.").ConfigureAwait(false);
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

		bool GetCommand(List<string> command, out YCommand savedCommand)
		{
			int best = -1;
			YCommand bestCommand = null;

			for (int i = 0; i <= command.Count; i++)
			{
				if (savedCommands.TryGetValue(i, out CommandNode node))
				{
					int match = node.TryGetCommand(command, out YCommand cmd);
					
					if (match > best)
					{
						bestCommand = cmd;
						best = match;
					}
				}
			}

			savedCommand = bestCommand;
			return best != -1;
		}

		/// <summary>
		/// Get all commands that match the commadn structure
		/// </summary>
		/// <param name="command"></param>
		/// <param name="validateStructure">If the command structure should be mactehd</param>
		/// <param name="validateParam">If command parameters is part of the structure</param>
		/// <param name="savedCommands"></param>
		/// <returns></returns>
		bool GetCommands(List<string> command, bool validateStructure, bool validateParam, out List<YCommand> savedCommands)
		{
			List<YCommand> foundCommands = new List<YCommand>();
			for (int i = 0; i <= maxLength; i++)
			{
				if (this.savedCommands.TryGetValue(i, out CommandNode node))
					node.TryGetCommands(command, validateStructure, validateParam, ref foundCommands);
			}

			savedCommands = foundCommands;
			return foundCommands.Count > 0;
		}

		bool TryFindCommand(string message, out string command, out bool silent)
		{
			if (message[0] == CommandPrefix)
			{
				command = message.Substring(1);
				silent = false;
				return true;
			}

			int bracketStart = message.IndexOf('{');
			if (bracketStart > -1)
			{
				int bracketEnd = message.IndexOf('}');

				if (bracketEnd > -1)
				{
					command = message.Substring(bracketStart + 1, bracketEnd - bracketStart - 1);
					if (command[0] == CommandPrefix)
						command = command.Substring(1);

					silent = true;
					return true;
				}
			}

			command = null;
			silent = false;
			return false;
		}

		#region Internal Commands

		/// <summary>
		/// Run help command
		/// </summary>
		/// <param name="command"></param>
		/// <param name="perPage"></param>
		/// <param name="output"></param>
		/// <returns></returns>
		bool HelpCommand(List<string> command, SocketMessage context, int perPage, ref string output)
		{
			int page = 1; // Starts with one cus user prefernce
			int maxPages = (int)Math.Ceiling(savedCommands.Count / (decimal)perPage);

			if (command.Count == 1 || int.TryParse(command[1], out page))
			{
				if (page < 1)
					page = 1;

				if (page > maxPages)
					page = maxPages;

				// Get all commands with matching structure
				GetCommands(command, false, false, out List<YCommand> savedCommands);
				// Filter out all commands user does not have permission to run
				FilterCommands(context, ref savedCommands);

				// Lets just not touch this anymore
				List<YCommand> selectedCommands = savedCommands.Where((_, i) => i < page * perPage && i >= (page - 1) * perPage).ToList();

				output = "```";
				output += "!help <page> -- To change page.\n";
				output += "!help <command> -- To view a command or module in more detail.\n\n";
				output += $"Page {page}/{maxPages}:\n";

				foreach (YCommand cmd in selectedCommands)
				{
					string name = string.Join(' ', cmd.Structure);

					output += $"	!{name} -- {cmd.Summary ?? "No description."}\n";
				}

				output += "```";

				return true;
			}

			return HelpSpecificCommand(command, context, ref output);
		}

		/// <summary>
		/// Find specefied command and display detail view of that command.
		/// </summary>
		/// <param name="command"></param>
		/// <param name="output"></param>
		/// <returns></returns>
		bool HelpSpecificCommand(List<string> command, SocketMessage context, ref string output)
		{
			int selector = -1;
			if (int.TryParse(command[command.Count - 1], out int parsed))
			{
				selector = parsed;
				command.RemoveAt(command.Count - 1);
			}

			command.RemoveAt(0);

			GetCommands(command, true, false, out List<YCommand> savedCommands);
			FilterCommands(context, ref savedCommands);

			if (savedCommands.Count == 1 || (savedCommands.Count > 1 && selector > 0 && selector <= savedCommands.Count))
			{
				return DisplayCommand(savedCommands[selector == -1 ? 0 : (selector - 1)], ref output);
			}
			else if (savedCommands.Count > 1)
			{
				output = "```";
				output += $"{savedCommands.Count} commands found, please be more spesific or add an index to the end of the help command\n";

				for (int i = 0; i < savedCommands.Count; i++)
				{
					YCommand cmd = savedCommands[i];
					string name = string.Join(' ', cmd.Structure);

					output += $"	{i + 1} !{name}\n";
				}

				output += "```";
			}
			else
			{
				output += "Unknown command.";
			}

			return true;
		}

		/// <summary>
		/// Display detailed view of command.
		/// </summary>
		/// <param name="command"></param>
		/// <param name="output"></param>
		/// <returns></returns>
		bool DisplayCommand(YCommand command, ref string output)
		{
			Example[] examples = command.GetAttributes<Example>(true);
			string joinedParams = ConcatParameters(command);
			string joinedName = string.Join(' ', command.Structure);
			string commandSummary = !string.IsNullOrWhiteSpace(command.Summary) ? command.Summary + "\n" : "No description.\n";

			output = "```";
			output += $"{command.Name} command:\n";
			output += commandSummary;
			output += $"	!{joinedName} {joinedParams}\n";

			for (int i = 0; i < command.Parameters.Count; i++)
			{
				YParameter parameter = command.Parameters[i];
				string parameterSummary = parameter.Summary == null ? "No description." : parameter.Summary;

				output += $"	 {i}: {parameter.Name} -- {parameterSummary}\n";
			}

			if (!(examples is null))
			{
				output += "\nExample:";

				foreach (Example example in examples)
				{
					output += $"\n{example.Value}";
				}
			}

			output += "```";

			return true;
		}

		/// <summary>
		/// Converts yCommand parameters into one continious string.
		/// </summary>
		/// <param name="command"></param>
		/// <returns></returns>
		string ConcatParameters(YCommand command)
		{
			string output = "";

			for (int i = 0; i < command.Parameters.Count; i++)
			{
				YParameter parameter = command.Parameters[i];
				string hasParam = parameter.IsParam ? "params " : "";
				string shorthandType = TypeToShorthand(parameter.Type.Name).ToLower();

				output += $"<{hasParam}{shorthandType} {parameter.Name.ToLower()}> ";
			}

			return output;
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

		/// <summary>
		/// Remove all unwanted commands.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="commands"></param>
		void FilterCommands(SocketMessage context, ref List<YCommand> commands)
		{
			//commands = commands.Where(a => Bot.PermissionManager.CanRun(a, context)).ToList();

			for (int i = 0; i < commands.Count; i++)
			{
				YCommand command = commands[i];
				IgnoreHelp ignoreHelp = command.GetAttribute<IgnoreHelp>(true);

				if (ignoreHelp != null || !Bot.PermissionManager.CanRun(command, context))
					commands.RemoveAt(i);
			}
		}

		#endregion
	}
}
