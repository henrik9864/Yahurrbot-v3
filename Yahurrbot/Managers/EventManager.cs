using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using YahurrFramework.Attributes;

namespace YahurrFramework.Managers
{
    internal class EventManager : BaseManager
    {
		public EventManager(YahurrBot bot, DiscordSocketClient client) : base(bot, client)
		{
			BindEvents();
		}

		/// <summary>
		/// Bind all events to their coresponding event in the module loader.
		/// </summary>
		void BindEvents()
		{
			Client.GuildUpdated += (a, b) => RunEvent("GuildUpdated", a, b);
			Client.ReactionRemoved += (a, b, c) => RunEvent("ReactionRemoved", a, b, c);
			Client.ReactionsCleared += (a, b) => RunEvent("ReactionsCleared", a, b);
			Client.RoleCreated += (a) => RunEvent("RoleCreated", a);
			Client.RoleDeleted += (a) => RunEvent("RoleDeleted", a);
			Client.RoleUpdated += (a, b) => RunEvent("RoleUpdated", a, b);
			Client.JoinedGuild += (a) => RunEvent("JoinedGuild", a);
			Client.UserIsTyping += (a, b) => RunEvent("RoleUpdated", a, b);
			Client.CurrentUserUpdated += (a, b) => RunEvent("CurrentUserUpdated", a, b);
			Client.UserVoiceStateUpdated += (a, b, c) => RunEvent("UserVoiceStateUpdated", a, b, c);
			Client.GuildMemberUpdated += (a, b) => RunEvent("GuildMemberUpdated", a, b);
			Client.UserUpdated += (a, b) => RunEvent("UserUpdated", a, b);
			Client.UserUnbanned += (a, b) => RunEvent("UserUnbanned", a, b);
			Client.UserBanned += (a, b) => RunEvent("UserBanned", a, b);
			Client.ReactionAdded += (a, b, c) => RunEvent("ReactionAdded", a, b, c);
			Client.LeftGuild += (a) => RunEvent("LeftGuild", a);
			Client.GuildAvailable += (a) => RunEvent("GuildAvailable", a);
			Client.GuildUnavailable += (a) => RunEvent("GuildUnavailable", a);
			Client.GuildMembersDownloaded += (a) => RunEvent("GuildMembersDownloaded", a);
			Client.UserJoined += (a) => RunEvent("UserJoined", a);
			Client.MessageUpdated += (a, b, c) => RunEvent("MessageUpdated", a, b, c);
			Client.LatencyUpdated += (a, b) => RunEvent("LatencyUpdated", a, b);
			Client.MessageReceived += (a) => RunEvent("MessageReceived", a);
			Client.MessageDeleted += (a, b) => RunEvent("MessageDeleted", a, b);
			Client.Connected += () => RunEvent("Connected");
			Client.Disconnected += (a) => RunEvent("Disconnected", a);
			Client.Ready += () => RunEvent("Ready");
			Client.RecipientRemoved += (a) => RunEvent("RecipientRemoved", a);
			Client.ChannelCreated += (a) => RunEvent("ChannelCreated", a);
			Client.ChannelDestroyed += (a) => RunEvent("ChannelDestroyed", a);
			Client.ChannelUpdated += (a, b) => RunEvent("ChannelUpdated", a, b);
			Client.RecipientAdded += (a) => RunEvent("RecipientAdded", a);
		}

		/// <summary>
		/// Run an event with no parameters on all modules.
		/// </summary>
		/// <param name="name">Name of method.</param>
		/// <returns></returns>
		async Task RunEvent(string name)
		{
			await RunCommand(name);
			await Bot.ModuleManager.RunMethod(name, _ => true).ConfigureAwait(false);
		}

		/// <summary>
		/// Run an event with 1 parameter on all modules.
		/// </summary>
		/// <typeparam name="P">Parameter type</typeparam>
		/// <param name="name">Name of method.</param>
		/// <param name="p">Parameter</param>
		/// <returns></returns>
		async Task RunEvent<P>(string name, P p)
		{
			await RunCommand(name, p);
			await Bot.ModuleManager.RunMethod(name, Validate(p), p).ConfigureAwait(false);
		}

		/// <summary>
		/// Run an event with 2 parameters on all modules.
		/// </summary>
		/// <typeparam name="P">Parameter 1 type</typeparam>
		/// <typeparam name="P1">Parameter 2 type</typeparam>
		/// <param name="name">Name of method.</param>
		/// <param name="p">Parameter 1</param>
		/// <param name="p1">Parameter 2</param>
		/// <returns></returns>
		async Task RunEvent<P, P1>(string name, P p, P1 p1)
		{
			await RunCommand(name, p, p1);
			await Bot.ModuleManager.RunMethod(name, m => Validate(p)(m) && Validate(p1)(m), p, p1).ConfigureAwait(false);
		}

		/// <summary>
		/// Run an event with 3 parameters on all modules.
		/// </summary>
		/// <typeparam name="P">Parameter 1 type</typeparam>
		/// <typeparam name="P1">Parameter 2 type</typeparam>
		/// <typeparam name="P2">Parameter 3 type</typeparam>
		/// <param name="name">Name of method</param>
		/// <param name="p">Parameter 1</param>
		/// <param name="p1">Parameter 2</param>
		/// <param name="p2">Parameter 3</param>
		/// <returns></returns>
		async Task RunEvent<P, P1, P2>(string name, P p, P1 p1, P2 p2)
		{
			await RunCommand(name, p, p1, p2);
			await Bot.ModuleManager.RunMethod(name, m => Validate(p)(m) && Validate(p1)(m) && Validate(p2)(m), p, p1, p2).ConfigureAwait(false);
		}

		/// <summary>
		/// Run corresponding command to input type
		/// </summary>
		/// <param name="name">Name of Event</param>
		/// <param name="paremeters">Event parameters</param>
		/// <returns></returns>
		private async Task RunCommand(string name, params object[] paremeters)
		{
			switch (name)
			{
				case "MessageReceived":
					await Bot.CommandManager.RunCommand(paremeters[0] as SocketMessage).ConfigureAwait(false);
					break;
			}
		}

		/// <summary>
		/// Validates if a module can see event trigger.
		/// </summary>
		/// <typeparam name="T">Parameter type.</typeparam>
		/// <param name="p">Parameter</param>
		/// <returns></returns>
		Func<YahurrModule, bool> Validate<T>(T p)
		{
			// Substitute switch statement
			var @switch = new Dictionary<Type, Func<YahurrModule, bool>>
			{
				{ typeof(SocketGuild), m => ValidateGuild(p as SocketGuild, m) },
				{ typeof(SocketGuildUser), m => ValidateGuild((p as SocketGuildUser)?.Guild, m) },
				{ typeof(SocketGuildChannel), m => ValidateGuild((p as SocketGuildChannel)?.Guild, m) }
			};

			if (@switch.TryGetValue(p.GetType(), out Func<YahurrModule, bool> func))
				return func;
			else
				return _ => true;
		}

		/// <summary>
		/// Validata a module can access a guild.
		/// </summary>
		/// <param name="guild">Guild to access</param>
		/// <param name="module">Module</param>
		/// <returns></returns>
		bool ValidateGuild(SocketGuild guild, YahurrModule module)
		{
			List<ServerFilter> filterAttributes = (List<ServerFilter>)module.GetType().GetCustomAttributes<ServerFilter>();
			for (int i = 0; i < filterAttributes.Count; i++)
			{
				ServerFilter filter = filterAttributes[i];

				if (filter.IsFiltered((long)guild.Id))
					return false;
			}

			return true;
		}
	}
}
