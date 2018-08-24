using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using YahurrFramework.Attributes;

namespace YahurrFramework
{
    public class YahurrModule
    {
		/// <summary>
		/// Name of module.
		/// </summary>
		public string Name
		{
			get
			{
				return GetType().GetCustomAttribute<Summary>()?.Value ?? GetType().Name;
			}
		}

		protected DiscordSocketClient Client { get; }

		protected CommandContext CommandContext { get; private set; }

		public YahurrModule(DiscordSocketClient client)
		{
			this.Client = client;
		}

		internal void SetContext(CommandContext context)
		{
			CommandContext = context;
		}

		#region Methods

		public async virtual Task GuildUpdated(SocketGuild before, SocketGuild after)
		{
			await Task.CompletedTask;
		}

		public async virtual Task ReactionRemoved(IUserMessage message, ISocketMessageChannel channel, SocketReaction reaction)
		{
			await Task.CompletedTask;
		}

		public async virtual Task ReactionsCleared(IUserMessage message, ISocketMessageChannel channel)
		{
			await Task.CompletedTask;
		}

		public async virtual Task RoleCreated(SocketRole role)
		{
			await Task.CompletedTask;
		}

		public async virtual Task RoleDeleted(SocketRole role)
		{
			await Task.CompletedTask;
		}

		public async virtual Task RoleUpdated(SocketRole before, SocketRole after)
		{
			await Task.CompletedTask;
		}

		public async virtual Task JoinedGuild(SocketGuild guild)
		{
			await Task.CompletedTask;
		}

		public async virtual Task UserIsTyping(SocketUser user, ISocketMessageChannel channel)
		{
			await Task.CompletedTask;
		}

		public async virtual Task CurrentUserUpdated(SocketSelfUser before, SocketSelfUser after)
		{
			await Task.CompletedTask;
		}

		public async virtual Task UserVoiceStateUpdated(SocketUser user, SocketVoiceState before, SocketVoiceState after)
		{
			await Task.CompletedTask;
		}

		public async virtual Task GuildMemberUpdated(SocketGuildUser before, SocketGuildUser after)
		{
			await Task.CompletedTask;
		}

		public async virtual Task UserUpdated(SocketUser before, SocketUser after)
		{
			await Task.CompletedTask;
		}

		public async virtual Task UserUnbanned(SocketUser user, SocketGuild guild)
		{
			await Task.CompletedTask;
		}

		public async virtual Task UserBanned(SocketUser user, SocketGuild guild)
		{
			await Task.CompletedTask;
		}

		public async virtual Task ReactionAdded(IUserMessage message, ISocketMessageChannel channel, SocketReaction reaction)
		{
			await Task.CompletedTask;
		}

		public async virtual Task LeftGuild(SocketGuild guild)
		{
			await Task.CompletedTask;
		}

		public async virtual Task GuildAvailable(SocketGuild guild)
		{
			await Task.CompletedTask;
		}

		public async virtual Task GuildUnavailable(SocketGuild guild)
		{
			await Task.CompletedTask;
		}

		public async virtual Task GuildMembersDownloaded(SocketGuild guild)
		{
			await Task.CompletedTask;
		}

		public async virtual Task UserJoined(SocketGuildUser user)
		{
			await Task.CompletedTask;
		}

		public async virtual Task MessageUpdated(IMessage before, SocketMessage after, ISocketMessageChannel channel)
		{
			await Task.CompletedTask;
		}

		public async virtual Task LatencyUpdated(int before, int after)
		{
			await Task.CompletedTask;
		}

		public async virtual Task MessageReceived(SocketMessage message)
		{
			await Task.CompletedTask;
		}

		public async virtual Task MessageDeleted(IMessage message, ISocketMessageChannel channel)
		{
			await Task.CompletedTask;
		}

		public async virtual Task Connected()
		{
			await Task.CompletedTask;
		}

		public async virtual Task Disconnected(Exception exception)
		{
			await Task.CompletedTask;
		}

		public async virtual Task Ready()
		{
			await Task.CompletedTask;
		}

		public async virtual Task RecipientRemoved(SocketGroupUser user)
		{
			await Task.CompletedTask;
		}

		public async virtual Task ChannelCreated(SocketChannel channel)
		{
			await Task.CompletedTask;
		}

		public async virtual Task ChannelDestroyed(SocketChannel channel)
		{
			await Task.CompletedTask;
		}

		public async virtual Task ChannelUpdated(SocketChannel before, SocketChannel after)
		{
			await Task.CompletedTask;
		}

		public async virtual Task RecipientAdded(SocketGroupUser user)
		{
			await Task.CompletedTask;
		}

		#endregion
	}
}
