using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord.WebSocket;
using YahurrFramework.Enums;
using YahurrFramework.Structs;

namespace YahurrFramework.Managers
{
	internal class LoggingManager : BaseManager
	{
		public event Func<LogMessage, ClientConfig, Task> Log;

		public event Func<ClientConfig, Task<string>> Read;

		List<LogMessage> loggedMessages = new List<LogMessage>();

		public LoggingManager(YahurrBot bot, DiscordSocketClient client) : base(bot, client)
		{
		}

		/// <summary>
		/// Log a message.
		/// </summary>
		/// <param name="logLevel"></param>
		/// <param name="message"></param>
		/// <param name="source"></param>
		/// <returns></returns>
		public async Task LogMessage(LogLevel logLevel, string message, string source)
		{
			LogMessage msg = new LogMessage(logLevel, message, source);

			await LogMessage(msg);
		}

		/// <summary>
		/// Log an exception.
		/// </summary>
		/// <param name="exception"></param>
		/// <param name="source"></param>
		/// <returns></returns>
		public async Task LogMessage(Exception exception, string source)
		{
			LogMessage msg = new LogMessage(exception, source);

			await LogMessage(msg);
		}

		/// <summary>
		/// Get user input from logger.
		/// </summary>
		/// <returns></returns>
		public Task<string> GetInput()
		{
			return Task.Run(() => Read?.Invoke(Bot.Config) ?? Task.FromResult<string>(null));
		}

		async Task LogMessage(LogMessage message)
		{
			loggedMessages.Add(message);
			await Log.Invoke(message, Bot.Config).ConfigureAwait(false);
		}
	}
}
