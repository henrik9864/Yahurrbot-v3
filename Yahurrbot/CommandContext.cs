using Discord;
using Discord.Rest;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace YahurrFramework
{
	public class CommandContext
	{
		public SocketGuild Guild { get; }

		public SocketChannel Channel { get; }

		public SocketMessage Message { get; }

		public CommandContext(SocketMessage context)
		{
			Guild = (context.Channel as SocketGuildChannel)?.Guild;
			Channel = context.Channel as SocketChannel;
			Message = context;
		}
	}
}
