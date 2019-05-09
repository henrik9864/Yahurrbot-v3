using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using YahurrFramework.Attributes;
using YahurrFramework.Enums;

namespace YahurrFramework.Commands.InternalCommands
{
	internal class RoleCommand : InternalCommandContainer
	{
		public RoleCommand(DiscordSocketClient client, YahurrBot bot) : base(client, bot)
		{
		}

		[Command("get", "roles")]
		public async Task LogRedirectUser()
		{
			string output = "";

			IReadOnlyCollection<IRole> roles = Guild.Roles;
			foreach (IRole role in roles)
			{
				output += $"{role.Name}: {role.Id}\n";
			}

			await Channel.SendMessageAsync($"```{output}```");
		}
	}
}
