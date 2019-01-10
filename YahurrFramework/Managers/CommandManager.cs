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
using YahurrFramework.Interfaces;

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

		/// <summary>
		/// Create a new command from module and add it.
		/// </summary>
		/// <param name="container">Command container.</param>
		/// <param name="method">Command method.</param>
		internal void AddCommand(ICommandContainer container, MethodInfo method)
		{
			AddCommand(new YCommand(method, container));
		}

		/// <summary>
		/// Add all commands in a command container
		/// </summary>
		/// <param name="container"></param>
		internal void AddCommands(ICommandContainer container)
		{
			MethodInfo[] methods = container.GetType().GetMethods();
			for (int i = 0; i < methods.Length; i++)
			{
				MethodInfo method = methods[i];

				if (method.GetCustomAttribute<Command>() != null)
					AddCommand(container, method);
			}
		}

		internal void LoadInternalCommands(string commandNamespace)
		{
			Type[] internalCommands = GetTypesInNamespace(Assembly.GetExecutingAssembly(), commandNamespace);

			for (int i = 0; i < internalCommands.Length; i++)
			{
				Type command = internalCommands[i];

				if (typeof(InternalCommandContainer).IsAssignableFrom(command))
					AddCommands(Activator.CreateInstance(command, Client, Bot) as InternalCommandContainer);

			}
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

			//if (!await RunInternalCommand(command, cmd).ConfigureAwait(false))
				await RunCommand(command, cmd, silent).ConfigureAwait(false);

			return true;
		}


		/// <summary>
		/// Get YCommand from a list of string that represents that command
		/// </summary>
		/// <param name="command"></param>
		/// <param name="savedCommand"></param>
		/// <returns></returns>
		internal bool GetCommand(List<string> command, out YCommand savedCommand)
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
		internal bool GetCommands(List<string> command, bool validateStructure, bool validateParam, out List<YCommand> savedCommands)
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

		bool TryFindCommand(string message, out string command, out bool silent)
		{
			if (!string.IsNullOrEmpty(message) && message[0] == CommandPrefix)
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

		Type[] GetTypesInNamespace(Assembly assembly, string nameSpace)
		{
			return assembly
				.GetTypes()
				.Where(t => string.Equals(t.Namespace, nameSpace, StringComparison.Ordinal))
				.ToArray();
		}
    }
}
