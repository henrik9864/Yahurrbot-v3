using System;
using System.Collections.Generic;
using YahurrBot.Interfaces;

namespace YahurrFramework.Commands
{
	internal class CommandNode
	{
		public int StructureLength { get; }

		List<YCommand> commands;

		public CommandNode(int structureLength)
		{
			this.StructureLength = structureLength;
			commands = new List<YCommand>();
		}

		public void Add(YCommand command)
		{
			commands.Add(command);
		}

		/// <summary>
		/// Try get the cosest match to the command input.
		/// </summary>
		/// <param name="command"></param>
		/// <param name="yCommand"></param>
		/// <returns></returns>
		public int TryGetCommand(List<string> command, out YCommand yCommand)
		{
			int pLength = command.Count - StructureLength;
			int best = -1;
			YCommand bestCommand = null;

			for (int i = 0; i < commands.Count; i++)
			{
				YCommand cmd = commands[i];
				int match = ValidateCommand(command, true, cmd);

				if (match > best)
				{
					bestCommand = cmd;
					best = match;
				}
			}

			yCommand = bestCommand;
			return best;
		}

		/// <summary>
		/// Get all commands that match this command.
		/// </summary>
		/// <param name="command"></param>
		/// <param name="validateStructure"></param>
		/// <param name="validateParam"></param>
		/// <param name="foundCommands"></param>
		public void TryGetCommands(List<string> command, bool validateStructure, bool validateParam, ref List<YCommand> foundCommands)
		{
			for (int i = 0; i < commands.Count; i++)
			{
				YCommand cmd = commands[i];

				if (ValidateCommand(command, validateParam, cmd) > -1 || !validateStructure)
					foundCommands.Add(cmd);
			}
		}

		/// <summary>
		/// Validate if a YCommand matches a command structure and parameters.
		/// </summary>
		/// <param name="command">Command structure.</param>
		/// <param name="validateParam">If parameter shall be validated too.</param>
		/// <param name="yCommand"></param>
		/// <returns></returns>
		int ValidateCommand(List<string> command, bool validateParam, YCommand yCommand)
		{
			if (yCommand.IsParam && StructureLength != command.Count)
				return -1;

			for (int i = 0; i < StructureLength; i++)
			{
				if (i >= command.Count)
					break;

				if (command[i] != yCommand.Structure[i])
					return -1;
			}

			if (!validateParam)
				return StructureLength * 2;

			int paramMatch = ValidateParams(command, yCommand);
			if (paramMatch > -1)
				return (StructureLength * 2) + paramMatch;
			else
				return -1;
		}

		/// <summary>
		/// Check if parameters is matching the YCommand.
		/// </summary>
		/// <param name="command"></param>
		/// <param name="yCommand"></param>
		/// <returns></returns>
		int ValidateParams(List<string> command, YCommand yCommand)
		{
			for (int i = 0; i < yCommand.Parameters.Count; i++)
			{
				YParameter parameter = yCommand.Parameters[i];
				Type expectedType = parameter.Type;

				if (i + StructureLength >= command.Count)
				{
					// Check the last params parameters here
					if (parameter.IsParam)
						return yCommand.Parameters.Count;

					if (parameter.IsOptional)
						continue;

					return -1;
				}

				if (parameter.IsParam)
					return yCommand.Parameters.Count;
				
				if (!IsOfType(command[StructureLength + i], expectedType))
					return -1;
			}

			return yCommand.Parameters.Count;
		}

		/// <summary>
		/// Check is string is of valid type to parse
		/// </summary>
		/// <param name="param"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		bool IsOfType(string param, Type type)
		{
			if (typeof(string).IsAssignableFrom(type))
				return true;

			if (typeof(int).IsAssignableFrom(type) && int.TryParse(param, out int result))
				return true;

			if (typeof(bool).IsAssignableFrom(type) && bool.TryParse(param, out bool boolResult))
				return true;

			if (typeof(Enum).IsAssignableFrom(type))
				return Enum.TryParse(type, param, out object enumResult);

			if (typeof(IParseable).IsAssignableFrom(type))
				return true;

			return false;
		}
	}
}
