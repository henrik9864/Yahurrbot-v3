using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using YahurrFramework.Interfaces;

namespace YahurrFramework.Commands
{
	public class CommandContainer : ICommandContainer
	{
		public string Name
		{
			get
			{
				return GetType().Name;
			}
		}

		static protected IGuild Guild
		{
			get
			{
				return asyncContext.Value.Guild;
			}
		}

		static protected ISocketMessageChannel Channel
		{
			get
			{
				return asyncContext.Value.Channel;
			}
		}

		static protected IMessage Message
		{
			get
			{
				return asyncContext.Value.Message;
			}
		}

		static AsyncLocal<MethodContext> asyncContext = new AsyncLocal<MethodContext>();

		/// <summary>
		/// Rund a command in this class.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="paramCount"></param>
		/// <param name="param"></param>
		/// <returns></returns>
		public async Task RunCommand(string name, int paramCount, params object[] param)
		{
			BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.IgnoreCase;
			MethodInfo method = GetType()
				.GetMethods(flags)
				.First(a => a.Name == name && a.GetParameters().Length == paramCount);

			if (method != null)
			{
				try
				{
					object output = method.Invoke(this, param ?? new object[0]);

					if (output is Task)
						await(output as Task);
				}
				catch (Exception)
				{
					string userResponse = "```";
					Random rng = new Random();

					if (rng.Next(0, 101) == 69)
						userResponse += $"oopsie Woopsie {name} did a fucky wucky!";
					else
						userResponse += $"Fatal error in method {name} see bot log for more info.";


					Channel?.SendMessageAsync(userResponse + "```");
					throw;
				}
			}
			else
				throw new MissingMethodException($"Method {name} was not found.");
		}

		/// <summary>
		/// Set context for the command to be run
		/// </summary>
		/// <param name="context"></param>
		public void SetContext(MethodContext context)
		{
			asyncContext.Value = context;
		}
	}
}
