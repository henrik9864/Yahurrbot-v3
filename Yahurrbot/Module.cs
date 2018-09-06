using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using YahurrBot.Enums;
using YahurrFramework.Attributes;
using YahurrFramework.Enums;

namespace YahurrFramework
{
    public class Module
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

		protected DiscordSocketClient Client { get; private set; }

		protected CommandContext CommandContext { get; private set; }

		protected object Config { get; private set; }

		internal YahurrBot Bot { get; private set; }

		internal async Task InitModule(DiscordSocketClient client, YahurrBot bot, object config)
		{
			this.Client = client;
			this.Bot = bot;
			this.Config = config;

			try
			{
				await Init().ConfigureAwait(false);
			}
			catch (Exception e)
			{
				await Bot.LoggingManager.LogMessage(LogLevel.Error, $"Unable to initialize module {Name}:", "YahurrModule").ConfigureAwait(false);
				await Bot.LoggingManager.LogMessage(e?.InnerException?.InnerException, "ModuleManager").ConfigureAwait(false);
			}
		}

		internal void SetContext(CommandContext context)
		{
			CommandContext = context;
		}

		#region Save/Load

		/// <summary>
		/// Save object to file.
		/// </summary>
		/// <param name="name">Identefier for this object.</param>
		/// <param name="obj">Object to save.</param>
		/// <param name="override">If you want to override previous saves</param>
		/// <returns></returns>
		protected async Task Save(string name, object obj, bool @override = true, bool append = false)
		{
			await Bot.FileManager.Save(obj, name, this, @override, append).ConfigureAwait(false);
		}

		/// <summary>
		/// Save object to file.
		/// </summary>
		/// <param name="name">Identefier for this object.</param>
		/// <param name="obj">Object to save.</param>
		/// <param name="type">Srialization method to use,</param>
		/// <param name="override">If you want to override previous saves</param>
		/// <returns></returns>
		protected async Task Save(string name, object obj, SerializationType type, bool @override = true, bool append = false)
		{
			await Bot.FileManager.Save(obj, name, type, this, @override, append).ConfigureAwait(false);
		}

		protected async Task Save(string name, object obj, string extension, Func<object, string> serializer, bool @override = true, bool append = false)
		{
			await Bot.FileManager.Save(obj, name, extension, serializer, this, @override, append).ConfigureAwait(false);
		}

		/// <summary>
		/// Load object from file.
		/// </summary>
		/// <typeparam name="T">Type item was saved as.</typeparam>
		/// <param name="name">Identefier for the saved item.</param>
		/// <returns></returns>
		protected Task<T> Load<T>(string name)
		{
			return Bot.FileManager.Load<T>(name, this);
		}

		protected Task<T> Load<T>(string name, Func<string, T> deserializer)
		{
			return Bot.FileManager.Load<T>(name, deserializer, this);
		}

		/// <summary>
		/// Check if save exists.
		/// </summary>
		/// <param name="name">Identefier.</param>
		/// <returns></returns>
		protected Task<bool> Exists(string name)
		{
			return Bot.FileManager.Exists(name, this);
		}

		/// <summary>
		/// Check if saved item is of type.
		/// </summary>
		/// <param name="name">Identefier.</param>
		/// <param name="type">Type to check for.</param>
		/// <returns></returns>
		protected Task<bool> IsValid(string name, Type type)
		{
			return Bot.FileManager.IsValid(name, type, this);
		}

		/// <summary>
		/// Check if saved item is of type.
		/// </summary>
		/// <typeparam name="T">Type to check for.</typeparam>
		/// <param name="name">Identefier.</param>
		/// <returns></returns>
		protected Task<bool> IsValid<T>(string name)
		{
			return IsValid(name, typeof(T));
		}

		#endregion

		#region EventMethods

		public async virtual Task Init()
		{
			await Task.CompletedTask;
		}

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
