using Discord;
using Discord.Rest;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace YahurrFramework
{
	public class MethodContext
	{
		public IGuild Guild { get; }

		public ISocketMessageChannel Channel { get; }

		public IMessage Message { get; }

		public MethodContext(IGuild guild, ISocketMessageChannel channel, IMessage message)
		{
			Guild = guild;
			Channel = channel;
			Message = message;
		}

		public MethodContext(SocketMessage context)
		{
			Guild = (context.Channel as SocketGuildChannel)?.Guild;
			Channel = context.Channel;
			Message = context;
		}
	}
}
