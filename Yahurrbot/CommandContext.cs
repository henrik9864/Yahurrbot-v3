using Discord;
using Discord.Rest;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace YFramework
{
	public class CommandContext
	{
		public SocketGuild Guild { get; }

		public ISocketMessageChannel Channel { get; }

		public SocketMessage Message { get; }

		public CommandContext(SocketMessage context)
		{
			Guild = (context.Channel as SocketGuildChannel)?.Guild;
			Channel = context.Channel;
			Message = context;
		}
	}
}
