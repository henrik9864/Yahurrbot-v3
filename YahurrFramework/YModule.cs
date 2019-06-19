using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Http;
using YahurrFramework.Enums;
using YahurrFramework.Attributes;
using YahurrFramework.Interfaces;
using YahurrFramework.Commands;

namespace YahurrFramework
{
	public class YModule : CommandContainer
    {
		/// <summary>
		/// Name of module.
		/// </summary>
		public new string Name
		{
			get
			{
				return GetType().GetCustomAttribute<Name>()?.Value ?? GetType().Name;
			}
		}

		public string ID
		{
			get
			{
				return GetType().Name;
			}
		}

		internal DiscordSocketClient Client { get; private set; }

		protected HttpClient WebClient
		{
			get
			{
				return Bot.HttpClient;
			}
		}

		protected object Config { get; private set; }

		YahurrBot Bot;

		internal void LoadModule(DiscordSocketClient client, YahurrBot bot, object config)
		{
			this.Client = client;
			this.Bot = bot;
			this.Config = config;
		}

		internal async Task InitModule()
		{
			try
			{
				await Init().ConfigureAwait(false);
			}
			catch (Exception e)
			{
				await Bot.LoggingManager.LogMessage(LogLevel.Error, $"Unable to initialize module {Name}:", "YahurrModule").ConfigureAwait(false);
				await Bot.LoggingManager.LogMessage(e, "ModuleManager").ConfigureAwait(false);
			}
		}

		internal async Task ModuleDone()
		{
			try
			{
				await Done().ConfigureAwait(false);
			}
			catch (Exception e)
			{
				await Bot.LoggingManager.LogMessage(LogLevel.Error, $"Unable to initialize module {Name}:", "YahurrModule").ConfigureAwait(false);
				await Bot.LoggingManager.LogMessage(e, "ModuleManager").ConfigureAwait(false);
			}
		}

		internal void ChangeConfig(object config)
		{
			this.Config = config;
		}

		internal object GetConfig()
		{
			return Config;
		}

		#region Helper functions

		/// <summary>
		/// Tries 5 times to respond to the currently sent message.
		/// </summary>
		/// <param name="message"></param>
		/// <param name="isTTS"></param>
		/// <returns></returns>
		public async Task<IUserMessage> TryRespondAsync(string message, bool dm = false, bool isTTS = false)
		{
			for (int i = 0; i < 5; i++)
			{
				try
				{
					return await RespondAsync(message, dm, isTTS);
				}
				catch (Exception)
				{
					if (i == 4)
						throw;
				}
			}

			throw new Exception("Error sending message to discord.");
		}

		/// <summary>
		/// Responds to currently sent message
		/// </summary>
		/// <param name="message"></param>
		/// <param name="dm"></param>
		/// <param name="isTTS"></param>
		/// <returns></returns>
		public async Task<IUserMessage> RespondAsync(string message, bool dm, bool isTTS)
		{
			if (dm)
				return await Message?.Author?.SendMessageAsync(message, isTTS);
			else
				return await Channel?.SendMessageAsync(message, isTTS);
		}

		/// <summary>
		/// Get response from the same user.
		/// </summary>
		/// <param name="identical">If the responder must be the same user.</param>
		/// <returns></returns>
		protected async Task<SocketMessage> GetResponseAsync(bool identical)
		{
			var tcs = new TaskCompletionSource<SocketMessage>();
			Bot.EventManager.ResponseEvents.Add(tcs);

			SocketMessage message = await tcs.Task;

			if (message.Author == Message.Author || !identical)
				return message;
			return
				null;
		}

		#region User

		/// <summary>
		/// Get guild user by id.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="partial"></param>
		/// <returns></returns>
		public SocketGuildUser GetUser(string name, bool partial)
		{
			List<SocketGuildUser> Users = (Guild as SocketGuild)?.Users.ToList();
			name = name.ToLower();

			if (TryParseIdentefier(name, out ulong id))
				return (Guild as SocketGuild).GetUser(id);

			if (!partial)
				return Users.Find(a => a.Nickname.ToLower() == name || a.Username.ToLower() == name);
			else
				return Users.Find(a => a?.Nickname?.ToLower()?.Contains(name) ?? false || a.Username.ToLower().Contains(name));
		}

		/// <summary>
		/// Get all user that match name.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="partial"></param>
		/// <returns></returns>
		public List<SocketGuildUser> GetUsers(string name, bool partial)
		{
			List<SocketGuildUser> Users = (Guild as SocketGuild)?.Users.ToList();
			name = name.ToLower();

			if (!partial)
				return Users.FindAll(a => a.Nickname.ToLower() == name || a.Username.ToLower() == name);
			else
				return Users.FindAll(a => a?.Nickname?.ToLower()?.Contains(name) ?? false || a.Username.ToLower().Contains(name));
		}

		#endregion

		#region Role

		/// <summary>
		/// Get role by name.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="partial"></param>
		/// <returns></returns>
		public SocketRole GetRole(string name, bool partial)
		{
			List<SocketRole> Users = (Guild as SocketGuild)?.Roles.ToList();
			name = name.ToLower();

			if (TryParseIdentefier(name, out ulong id))
				return (Guild as SocketGuild).GetRole(id);

			if (!partial)
				return Users.Find(a => a.Name.ToLower() == name);
			else
				return Users.Find(a => a?.Name?.ToLower()?.Contains(name) ?? false);
		}

		/// <summary>
		/// Get all roles that match name
		/// </summary>
		/// <param name="name"></param>
		/// <param name="partial"></param>
		/// <returns></returns>
		public List<SocketRole> GetRoles(string name, bool partial)
		{
			List<SocketRole> Users = (Guild as SocketGuild)?.Roles.ToList();
			name = name.ToLower();

			if (!partial)
				return Users.FindAll(a => a.Name.ToLower() == name);
			else
				return Users.FindAll(a => a?.Name?.ToLower()?.Contains(name) ?? false);
		}

		#endregion

		#region Channel

		/// <summary>
		/// Get first channel of type.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public T GetChannel<T>() where T : SocketGuildChannel
		{
			List<SocketGuildChannel> Users = (Guild as SocketGuild)?.Channels.ToList();
			return Users.Find(a => typeof(T).IsAssignableFrom(a.GetType())) as T;
		}

		/// <summary>
		/// Get channel from id of type.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="id"></param>
		/// <returns></returns>
		public T GetChannel<T>(ulong id) where T : SocketGuildChannel
		{
			return (Guild as SocketGuild)?.GetChannel(id) as T;
		}

		/// <summary>
		/// Get channel by name of type.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="name"></param>
		/// <param name="partial"></param>
		/// <returns></returns>
		public T GetChannel<T>(string name, bool partial) where T : SocketGuildChannel
		{
			if (TryParseIdentefier(name, out ulong id))
				return GetChannel<T>(id);

			List<SocketGuildChannel> Users = (Guild as SocketGuild)?.Channels.ToList();

			if (!partial)
				return Users.Find(a => a.Name == name && typeof(T).IsAssignableFrom(a.GetType())) as T;
			else
				return Users.Find(a => a?.Name?.Contains(name) ?? false && typeof(T).IsAssignableFrom(a.GetType())) as T;
		}

		#endregion

		bool TryParseIdentefier(string identefier, out ulong id)
		{
			if (identefier[0] == '<' && identefier[identefier.Length - 1] == '>')
			{
				char second = identefier[2];

				if (!char.IsDigit(second))
					identefier = identefier.Substring(3, identefier.Length - 4);
				else
					identefier = identefier.Substring(2, identefier.Length - 3);

				return ulong.TryParse(identefier, out id);
			}

			id = 0;
			return false;
		}

		#endregion

		#region ModuleManagement

		/// <summary>
		/// Get loaded module of type.
		/// </summary>
		/// <typeparam name="T">Module type to find.</typeparam>
		/// <returns></returns>
		protected async Task<T> GetModuleAsync<T>() where T : YModule
		{
			return (T) await GetModuleAsync(typeof(T)).ConfigureAwait(false);
		}

		/// <summary>
		/// Get loaded module with name.
		/// </summary>
		/// <param name="name">Name of module.</param>
		/// <returns></returns>
		protected Task<YModule> GetModuleAsync(Type type)
		{
			return Task.Run(() =>
			{
				return Bot.ModuleManager.LoadedModules.Find(a =>
				{
					return type.IsAssignableFrom(a.GetType());
				});
			});
		}

		/// <summary>
		/// Check if module of type is loaded.
		/// </summary>
		/// <typeparam name="T">Module type.</typeparam>
		/// <returns></returns>
		protected Task<bool> HasModuleAsync<T>()
		{
			return HasModuleAsync(typeof(T));
		}

		/// <summary>
		/// Check if module of type is loaded.
		/// </summary>
		/// <param name="name">Module name</param>
		/// <returns></returns>
		protected Task<bool> HasModuleAsync(Type type)
		{
			return Task.Run(() =>
			{
				return Bot.ModuleManager.LoadedModules.Exists(a =>
				{
					return type.IsAssignableFrom(a.GetType());
				});
			});
		}

		#endregion

		#region Logging

		/// <summary>
		/// Log a message to console.
		/// </summary>
		/// <param name="logLevel"></param>
		/// <param name="msg"></param>
		/// <returns></returns>
		protected async Task LogAsync(LogLevel logLevel, object msg)
		{
			await Bot.LoggingManager.LogMessage(logLevel, msg?.ToString(), Name).ConfigureAwait(false);
		}

		/// <summary>
		/// Log an exception to the console.
		/// </summary>
		/// <param name="exception"></param>
		/// <returns></returns>
		protected async Task LogAsync(Exception exception)
		{
			await Bot.LoggingManager.LogMessage(exception, Name).ConfigureAwait(false);
		}

		#endregion

		#region Save/Load

		/// <summary>
		/// Save object to file.
		/// </summary>
		/// <param name="name">Identefier for this object.</param>
		/// <param name="obj">Object to save.</param>
		/// <param name="override">If you want to override previous saves</param>
		/// <returns></returns>
		protected Task SaveAsync(string name, object obj, bool @override = true, bool append = false)
		{
			return Bot.FileManager.Save(obj, name, this, @override, append);
		}

		/// <summary>
		/// Save object to file.
		/// </summary>
		/// <param name="name">Identefier for this object.</param>
		/// <param name="obj">Object to save.</param>
		/// <param name="extension">File extension.</param>
		/// <param name="serializer">Method to convert from object to string.</param>
		/// <param name="override"></param>
		/// <param name="append">If you want to override previous saves</param>
		/// <returns></returns>
		protected Task SaveAsync(string name, object obj, string extension, Func<object, string> serializer, bool @override = true, bool append = false)
		{
			return Bot.FileManager.Save(obj, name, extension, serializer, this, @override, append);
		}

		/// <summary>
		/// Load object from file.
		/// </summary>
		/// <typeparam name="T">Type item was saved as.</typeparam>
		/// <param name="name">Identefier for the saved item.</param>
		/// <returns></returns>
		protected Task<T> LoadAsync<T>(string name)
		{
			return Bot.FileManager.Load<T>(name, this);
		}

		/// <summary>
		/// Load object from file.
		/// </summary>
		/// <typeparam name="T">Type item was saved as.</typeparam>
		/// <param name="name">Identefier for the saved item.</param>
		/// <param name="deserializer"></param>
		/// <returns></returns>
		protected Task<T> LoadAsync<T>(string name, Func<string, T> deserializer)
		{
			return Bot.FileManager.Load(name, deserializer, this);
		}

		/// <summary>
		/// Check if save exists.
		/// </summary>
		/// <param name="name">Identefier.</param>
		/// <returns></returns>
		protected Task<bool> ExistsAsync(string name)
		{
			return Bot.FileManager.Exists(name, this);
		}

		/// <summary>
		/// Check if saved item is of type.
		/// </summary>
		/// <param name="name">Identefier.</param>
		/// <param name="type">Type to check for.</param>
		/// <returns></returns>
		protected bool IsValidAsync(string name, Type type)
		{
			return Bot.FileManager.IsValid(name, type, this);
		}

		/// <summary>
		/// Check if saved item is of type.
		/// </summary>
		/// <typeparam name="T">Type to check for.</typeparam>
		/// <param name="name">Identefier.</param>
		/// <returns></returns>
		protected bool IsValidAsync<T>(string name)
		{
			return IsValidAsync(name, typeof(T));
		}

		#endregion

		#region EventMethods

		protected async virtual Task Init()
		{
			await Task.CompletedTask;
		}

		protected async virtual Task Done()
		{
			await Task.CompletedTask;
		}

		protected async virtual Task Shutdown()
		{
			await Task.CompletedTask;
		}

		protected async virtual Task GuildUpdated(SocketGuild before, SocketGuild after)
		{
			await Task.CompletedTask;
		}

		protected async virtual Task ReactionRemoved(IUserMessage message, ISocketMessageChannel channel, SocketReaction reaction)
		{
			await Task.CompletedTask;
		}

		protected async virtual Task ReactionsCleared(IUserMessage message, ISocketMessageChannel channel)
		{
			await Task.CompletedTask;
		}

		protected async virtual Task RoleCreated(SocketRole role)
		{
			await Task.CompletedTask;
		}

		protected async virtual Task RoleDeleted(SocketRole role)
		{
			await Task.CompletedTask;
		}

		protected async virtual Task RoleUpdated(SocketRole before, SocketRole after)
		{
			await Task.CompletedTask;
		}

		protected async virtual Task JoinedGuild(SocketGuild guild)
		{
			await Task.CompletedTask;
		}

		protected async virtual Task UserIsTyping(SocketUser user, ISocketMessageChannel channel)
		{
			await Task.CompletedTask;
		}

		protected async virtual Task CurrentUserUpdated(SocketSelfUser before, SocketSelfUser after)
		{
			await Task.CompletedTask;
		}

		protected async virtual Task UserVoiceStateUpdated(SocketUser user, SocketVoiceState before, SocketVoiceState after)
		{
			await Task.CompletedTask;
		}

		protected async virtual Task GuildMemberUpdated(SocketGuildUser before, SocketGuildUser after)
		{
			await Task.CompletedTask;
		}

		protected async virtual Task UserUpdated(SocketUser before, SocketUser after)
		{
			await Task.CompletedTask;
		}

		protected async virtual Task UserUnbanned(SocketUser user, SocketGuild guild)
		{
			await Task.CompletedTask;
		}

		protected async virtual Task UserBanned(SocketUser user, SocketGuild guild)
		{
			await Task.CompletedTask;
		}

		protected async virtual Task ReactionAdded(IUserMessage message, ISocketMessageChannel channel, SocketReaction reaction)
		{
			await Task.CompletedTask;
		}

		protected async virtual Task LeftGuild(SocketGuild guild)
		{
			await Task.CompletedTask;
		}

		protected async virtual Task GuildAvailable(SocketGuild guild)
		{
			await Task.CompletedTask;
		}

		protected async virtual Task GuildUnavailable(SocketGuild guild)
		{
			await Task.CompletedTask;
		}

		protected async virtual Task GuildMembersDownloaded(SocketGuild guild)
		{
			await Task.CompletedTask;
		}

		protected async virtual Task UserJoined(SocketGuildUser user)
		{
			await Task.CompletedTask;
		}

		protected async virtual Task MessageUpdated(IMessage before, SocketMessage after, ISocketMessageChannel channel)
		{
			await Task.CompletedTask;
		}

		protected async virtual Task LatencyUpdated(int before, int after)
		{
			await Task.CompletedTask;
		}

		protected async virtual Task MessageReceived(SocketMessage message)
		{
			await Task.CompletedTask;
		}

		protected async virtual Task MessageDeleted(IMessage message, ISocketMessageChannel channel)
		{
			await Task.CompletedTask;
		}

		protected async virtual Task Connected()
		{
			await Task.CompletedTask;
		}

		protected async virtual Task Disconnected(Exception exception)
		{
			await Task.CompletedTask;
		}

		protected async virtual Task Ready()
		{
			await Task.CompletedTask;
		}

		protected async virtual Task RecipientRemoved(SocketGroupUser user)
		{
			await Task.CompletedTask;
		}

		protected async virtual Task ChannelCreated(SocketChannel channel)
		{
			await Task.CompletedTask;
		}

		protected async virtual Task ChannelDestroyed(SocketChannel channel)
		{
			await Task.CompletedTask;
		}

		protected async virtual Task ChannelUpdated(SocketChannel before, SocketChannel after)
		{
			await Task.CompletedTask;
		}

		protected async virtual Task RecipientAdded(SocketGroupUser user)
		{
			await Task.CompletedTask;
		}

		#endregion
	}
}
