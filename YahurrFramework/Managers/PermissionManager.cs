using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using YahurrFramework.Commands;
using YahurrFramework.Enums;
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

			ulong userID = message.Author.Id;
			ulong channelID = message.Channel.Id;
			ulong guildID = (message.Channel as SocketGuildChannel)?.Guild?.Id ?? 1;
			List<SocketRole> roles = new List<SocketRole>();

			if (message.Channel is SocketGuildChannel)
				roles = (message.Author as SocketGuildUser)?.Roles.ToList();

			if (Permissions.TryGetValue(command.Parent.Name, out PermissionClass @class))
			{
				PermissionGroup group = @class[command.MethodName];

				if (group != null)
                {
                    if (!ValidateProperties(group, message))
                        return false;

                    if (IsFiltered(group, userID, channelID, guildID, roles))
                        return true;
                }

                return IsFiltered(@class, userID, channelID, guildID, roles);
            }

			return true;
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
		bool IsFiltered(IPermissionGroup group, ulong userID, ulong channelID, ulong guildID, List<SocketRole> roles)
		{
			bool guildFound = group.IsFiltered(guildID, Enums.PermissionTarget.Guild, out bool guildResult);
			bool channelFound = group.IsFiltered(channelID, Enums.PermissionTarget.Channel, out bool channelResult);
			bool userFound = group.IsFiltered(userID, Enums.PermissionTarget.User, out bool userResult);

			bool roleFound = false;
			bool roleFiltered = false;

			for (int i = 0; i < roles.Count; i++)
			{
				SocketRole role = roles[i];
				bool roleResult = false;

				roleFound = roleFound || group.IsFiltered(role.Id, Enums.PermissionTarget.Role, out roleResult);

				if (roleResult)
					roleFiltered = true;
			}

			if (userFound)
				return !userResult;

			if (roleFound)
				return !roleFiltered;

			if (channelFound)
				return !channelResult;

			if (guildFound)
				return !guildResult;

			return true;
		}
	}
}
