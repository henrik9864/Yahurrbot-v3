using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using YahurrFramework.Commands;
using YahurrFramework.Commands.InternalCommands;
using YahurrFramework.Enums;
using YahurrFramework.Enums.Permissions;
using YahurrFramework.Interfaces;
using YahurrFramework.Structs;
using YahurrLexer;

namespace YahurrFramework.Managers
{
	internal class PermissionManager : BaseManager
	{
		public Dictionary<string, PermissionClass> Permissions { get; } = new Dictionary<string, PermissionClass>();

		Lexer<PermissionTokenType> lexer = new Lexer<PermissionTokenType>();

		public PermissionManager(YahurrBot bot, DiscordSocketClient client) : base(bot, client)
		{
			lexer.AddRule(new Rule(@"(?<Type>method|group|class)"));
			lexer.AddRule(new Rule(@"(?<TargetType>blacklist|whitelist)"));
			lexer.AddRule(new Rule(@"(?<Group>channel|user|guild|role)"));
			lexer.AddRule(new Rule(@"(?<Bracket><|>)"));
			lexer.AddRule(new Rule(@"(?<Operator>=)"));
			lexer.AddRule(new Rule(@"(?<Colon>:)"));
			lexer.AddRule(new Rule(@"(?<Number>[0-9]+)"));
			lexer.AddRule(new Rule(@"(?<Text>[^ ]+)"));
		}

		/// <summary>
		/// Load permission from files.
		/// </summary>
		/// <returns></returns>
		public async Task LoadPermissions()
		{
			if (!Directory.Exists("Permissions"))
				Directory.CreateDirectory("Permissions");

			int succsess = 0;
			string[] paths = Directory.GetFiles("Permissions", "*.prm", SearchOption.AllDirectories);
			for (int i = 0; i < paths.Length; i++)
			{
				using (StreamReader reader = new StreamReader(paths[i]))
				{
					string name = Path.GetFileNameWithoutExtension(paths[i]);
					string file = await reader.ReadToEndAsync();
					lexer.Lex(file.Split(new[] { Environment.NewLine, " " }, StringSplitOptions.None));

					try
					{
						PermissionClass @class = PermissionClass.Parse(lexer, name);
						Permissions.Add(@class.Name, @class);

						succsess++;
					}
					catch (Exception e)
					{
						await Bot.LoggingManager.LogMessage(LogLevel.Warning, $"Unable to parse {name}.prm", "PermissionManager");
						await Bot.LoggingManager.LogMessage(e, "PermissionManager");
					}
				}
			}

			await Bot.LoggingManager.LogMessage(LogLevel.Message, $"Parsed {succsess}/{paths.Length} permission{(paths.Length == 1 ? "" : "s")} files.", "PermissionManager");
		}

		/// <summary>
		/// Validate if a command can be run from the socket mesage.
		/// </summary>
		/// <param name="command">Selected command</param>
		/// <param name="message">Context for running this command.</param>
		/// <returns></returns>
		public bool CanRun(YCommand command, SocketMessage message)
		{
            if (message.Author.Id == Bot.Config.Maintainer)
                return true;

			bool defaultAllow = true;
			ulong userID = message.Author.Id;
			ulong channelID = message.Channel.Id;
			ulong guildID = (message.Channel as SocketGuildChannel)?.Guild?.Id ?? 1;
			List<SocketRole> roles = new List<SocketRole>();

			if (message.Channel is SocketGuildChannel)
				roles = (message.Author as SocketGuildUser)?.Roles.ToList();

			// Internal commands must be explicitly allowed, except Help
			if (command.Parent.GetType() != typeof(HelpCommand) && typeof(InternalCommandContainer).IsAssignableFrom(command.Parent.GetType()))
				defaultAllow = false;

			if (Permissions.TryGetValue(command.Parent.Name, out PermissionClass @class))
			{
				PermissionGroup group = @class[command.MethodName];

				if (group != null)
                {
                    if (!ValidateProperties(group, message))
                        return false;

					PermissionStatus gStatus = IsFiltered(group, userID, channelID, guildID, roles);
					if (gStatus != PermissionStatus.NotFound)
						return ToBool(gStatus, false);

					if (group.Properties.TryGetValue("IgnoreAbove", out string value) && bool.TryParse(value, out bool result) && result)
						return ToBool(gStatus, defaultAllow);
				}

				PermissionStatus status = IsFiltered(@class, userID, channelID, guildID, roles);
				return ToBool(status, defaultAllow);
			}

			return defaultAllow;
		}

        /// <summary>
        /// Get premission class for this command if there is any
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        public PermissionClass GetPermissionClass(YCommand command)
        {
            if (Permissions.TryGetValue(command.Parent.Name, out PermissionClass @class))
                return @class;

            return null;
        }

		/// <summary>
		/// Check if properties allow this command to be run.
		/// </summary>11
		/// <param name="group"></param>
		/// <param name="message"></param>
		/// <returns></returns>
		bool ValidateProperties(PermissionGroup group, SocketMessage message)
		{
			if (group.Properties.TryGetValue("IsDM", out string value) || group.Class.Properties.TryGetValue("IsDM", out value))
			{
				bool.TryParse(value, out bool result);

				if (result && message.Channel is SocketDMChannel)
					return true;

				return true;
			}

			return true;
		}

		/// <summary>
		/// Check if this command is filtered.
		/// </summary>
		/// <param name="group"></param>
		/// <param name="userID"></param>
		/// <param name="channelID"></param>
		/// <param name="guildID"></param>
		/// <param name="roles"></param>
		/// <returns></returns>
		PermissionStatus IsFiltered(IPermissionGroup group, ulong userID, ulong channelID, ulong guildID, List<SocketRole> roles)
		{
			PermissionStatus guildStatus = group.IsFiltered(guildID, Enums.PermissionTarget.Guild);
			PermissionStatus channelStatus = group.IsFiltered(channelID, Enums.PermissionTarget.Channel);
			PermissionStatus userStatus = group.IsFiltered(userID, Enums.PermissionTarget.User);
			PermissionStatus roleStatus = PermissionStatus.NotFound;

			for (int i = 0; i < roles.Count; i++)
			{
				SocketRole role = roles[i];
				PermissionStatus status = group.IsFiltered(role.Id, Enums.PermissionTarget.Role);

				if (status != PermissionStatus.NotFound)
				{
					roleStatus = status;

					if (roleStatus == PermissionStatus.Approved)
						break;
				}
			}

			if (userStatus != PermissionStatus.NotFound)
				return userStatus;

			if (roleStatus != PermissionStatus.NotFound)
				return roleStatus;

			if (channelStatus != PermissionStatus.NotFound)
				return channelStatus;

			if (guildStatus != PermissionStatus.NotFound)
				return guildStatus;

			return PermissionStatus.NotFound;
		}

		/// <summary>
		/// Converts PermissionStatus to passed/not passed
		/// </summary>
		/// <param name="status"></param>
		/// <param name="defaultAllow"></param>
		/// <returns></returns>
		bool ToBool(PermissionStatus status, bool defaultAllow = true)
		{
			switch (status)
			{
				case PermissionStatus.Approved:
					return true;
				case PermissionStatus.Denied:
					return false;
				case PermissionStatus.NotFound:
					return defaultAllow;
				default:
					throw new Exception("Da fuck?");
			}
		}
	}
}
