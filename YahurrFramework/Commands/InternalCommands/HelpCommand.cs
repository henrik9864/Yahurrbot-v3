using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.WebSocket;
using YahurrFramework.Attributes;
using YahurrFramework.Interfaces;

namespace YahurrFramework.Commands.InternalCommands
{
	internal class HelpCommand : InternalCommandContainer
	{
		public HelpCommand(DiscordSocketClient client, YahurrBot bot) : base(client, bot)
		{
		}

		[IgnoreHelp]
		[Command("help")]
		public async Task Help(params string[] command)
		{
			await HelpList(new List<string>(command), 20);
		}

		/// <summary>
		/// Run help command
		/// </summary>
		/// <param name="command"></param>
		/// <param name="perPage"></param>
		/// <param name="output"></param>
		/// <returns></returns>
		async Task HelpList(List<string> command, int perPage)
		{
			// Get all commands with matching structure
			Bot.CommandManager.GetCommands(command, false, false, out List<YCommand> savedCommands);
			savedCommands = FilterCommands(savedCommands);

			int page = 1; // Starts with one cus user prefernce
			int maxPages = (int)Math.Ceiling(savedCommands.Count / (decimal)perPage);

			if (command.Count == 0 || int.TryParse(command[0], out page))
			{
				if (page < 1)
					page = 1;

				if (page > maxPages)
					page = maxPages;

				// Lets just not touch this anymore
				List<YCommand> selectedCommands = savedCommands
					.Where((_, i) => i < page * perPage && i >= (page - 1) * perPage)
					.ToList();

				if (selectedCommands.Count == 0)
					return;

				string output = "```";
				output += "!help <page> -- To change page.\n";
				output += "!help <command> -- To view a command or module in more detail.\n\n";
				output += $"Page {page}/{maxPages}:\n";

				foreach (YCommand cmd in selectedCommands)
				{
					string name = string.Join(' ', cmd.Structure);

					output += $"	!{name} -- {cmd.Summary ?? "No description."}\n";
				}

				await Message?.Channel?.SendMessageAsync(output + "```");
				return;
			}

			await HelpSpecificCommand(command);
		}

		/// <summary>
		/// Find specefied command and display detail view of that command.
		/// </summary>
		/// <param name="command"></param>
		/// <param name="output"></param>
		/// <returns></returns>
		async Task HelpSpecificCommand(List<string> command)
		{
			int selector = -1;
			if (int.TryParse(command[command.Count - 1], out int parsed))
			{
				selector = parsed;
				command.RemoveAt(command.Count - 1);
			}

			Bot.CommandManager.GetCommands(command, true, false, out List<YCommand> savedCommands);
			savedCommands = FilterCommands(savedCommands);

			if (savedCommands.Count == 1 || (savedCommands.Count > 1 && selector > 0 && selector <= savedCommands.Count))
			{
				await DisplayCommand(savedCommands[selector == -1 ? 0 : (selector - 1)]);
			}
			else if (savedCommands.Count > 1)
			{
				string output = "```";
				output += $"{savedCommands.Count} commands found, please be more spesific or add an index to the end of the help command\n";

				for (int i = 0; i < savedCommands.Count; i++)
				{
					YCommand cmd = savedCommands[i];
					string name = string.Join(' ', cmd.Structure);

					output += $"	{i + 1} !{name}\n";
				}

				await Message?.Channel?.SendMessageAsync(output + "```");
			}
		}

		/// <summary>
		/// Display detailed view of command.
		/// </summary>
		/// <param name="command"></param>
		/// <param name="output"></param>
		/// <returns></returns>
		async Task DisplayCommand(YCommand command)
		{
			Example[] examples = command.GetAttributes<Example>(true);
			string joinedParams = ConcatParameters(command);
			string joinedName = string.Join(' ', command.Structure);
			string commandSummary = !string.IsNullOrWhiteSpace(command.Summary) ? command.Summary + "\n" : "No description.\n";

			string output = "```";
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

			await Message?.Channel?.SendMessageAsync(output + "```");
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
		List<YCommand> FilterCommands(List<YCommand> commands)
		{
			List<YCommand> approved = new List<YCommand>();

			for (int i = 0; i < commands.Count; i++)
			{
				YCommand command = commands[i];
				IgnoreHelp ignoreHelp = command.GetAttribute<IgnoreHelp>(true);

				if (ignoreHelp == null && Bot.PermissionManager.CanRun(command, Message as SocketMessage))
					approved.Add(command);

			}

			return approved;
		}
	}
}
