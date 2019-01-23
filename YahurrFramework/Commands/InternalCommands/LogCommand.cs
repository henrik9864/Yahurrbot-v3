using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.WebSocket;
using YahurrFramework.Attributes;
using YahurrFramework.Enums;

namespace YahurrFramework.Commands.InternalCommands
{
	internal class LogCommand : InternalCommandContainer
	{
		public LogCommand(DiscordSocketClient client, YahurrBot bot) : base(client, bot)
		{
		}

		[Command("log", "redirect")]
		public async Task LogRedirectUser(ulong id, bool onlyException = false)
		{
			SocketUser user = Client.GetUser(id);

			if (user is null)
			{
				await Channel.SendMessageAsync($"User {id} not found.");
				return;
			}

			Bot.LoggingManager.RedirectUser = id;
			Bot.LoggingManager.OnlyException = onlyException;
			await Channel.SendMessageAsync($"Logs redirected to {user.Username}");
		}

		[Command("log", "redirect", "off")]
		public async Task LogRedirectOff()
		{
			Bot.LoggingManager.RedirectUser = 0;
			await Task.CompletedTask;
		}

		[Command("log", "download")]
		public async Task LogDownload(string fileName)
		{
			await Channel.SendFileAsync($"Logs/{fileName}.txt");
		}

		[Command("log")]
		public async Task Log(LogLevel logLevel, string message)
		{
			await Bot.LoggingManager.LogMessage(logLevel, message, Message.Author.Username);
			await Channel.SendMessageAsync($"Message logged.");
		}
	}
}
