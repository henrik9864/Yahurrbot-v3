using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Discord.WebSocket;
using YahurrFramework.Attributes;

namespace YahurrFramework.Commands.InternalCommands
{
	internal class ConfigCommand : InternalCommandContainer
	{
		public ConfigCommand(DiscordSocketClient client, YahurrBot bot) : base(client, bot)
		{
		}

		[Command("reload", "config")]
		public async Task ReloadConfig()
		{
			await Bot.ReloadConfig();
			await Bot.ModuleManager.ReloadConfig();

			await Channel.SendMessageAsync("Config files reloaded.");
		}

		[Command("config")]
		public async Task DisplayConfig(string moduleName)
		{
			YModule module = Bot.ModuleManager.LoadedModules.Find(a => a.Name == moduleName);
			if (module is null)
			{
				await Channel.SendMessageAsync($"Module '{moduleName} does not exist'");
				return;
			}

			Config config = module?.GetType().GetCustomAttribute<Config>(true);
			if (config is null)
			{
				await Channel.SendMessageAsync($"Module '{moduleName} does not have a config class'");
				return;
			}

			await Channel.SendMessageAsync($"```{DisplayConfig(module.GetConfig())}```");
		}

		[Command("config")]
		public async Task ModifyConfig(string moduleName, string property, string value, bool save = true)
		{
			YModule module = Bot.ModuleManager.LoadedModules.Find(a => a.Name == moduleName);
			if (module is null)
			{
				await Channel.SendMessageAsync($"Module '{moduleName} does not exist'");
				return;
			}

			Config config = module?.GetType().GetCustomAttribute<Config>(true);
			if (config is null)
			{
				await Channel.SendMessageAsync($"Module '{moduleName} does not have a config class'");
				return;
			}

			Type configType = module.GetConfig().GetType();
			PropertyInfo pInfo = configType.GetProperty(property);
			FieldInfo fInfo = configType.GetField(property);

			if (pInfo != null)
				pInfo.SetValue(module.GetConfig(), ParseParameter(value, pInfo.PropertyType));

			if (fInfo != null)
				fInfo.SetValue(module.GetConfig(), ParseParameter(value, fInfo.FieldType));


			await Channel.SendMessageAsync($"Config updated");
			if (save)
			{
				await Bot.ModuleManager.SaveConfig();
				await Channel.SendMessageAsync($"Config saved");
			}
		}

		/// <summary>
		/// Create a list of all properties and filds in a config file.
		/// </summary>
		/// <param name="config"></param>
		/// <returns></returns>
		string DisplayConfig(object config)
		{
			Type configType = config.GetType();
			PropertyInfo[] properties = configType.GetProperties();
			FieldInfo[] fields = configType.GetFields();

			string output = "";
			foreach (PropertyInfo property in properties)
			{
				output += $"{property.Name} = {property.GetValue(config).ToString()}\n";
			}

			foreach (FieldInfo field in fields)
			{
				output += $"{field.Name} = {field.GetValue(config).ToString()}\n";
			}

			return output;
		}

		/// <summary>
		/// Convert string to any of the supported types.
		/// </summary>
		/// <param name="param"></param>
		/// <param name="paramType"></param>
		/// <returns></returns>
		object ParseParameter(string param, Type paramType)
		{
			if (typeof(string).IsAssignableFrom(paramType))
				return param;

			if (typeof(int).IsAssignableFrom(paramType) && int.TryParse(param, out int result))
				return result;

			if (typeof(ulong).IsAssignableFrom(paramType) && ulong.TryParse(param, out ulong uResult))
				return uResult;

			if (typeof(bool).IsAssignableFrom(paramType) && bool.TryParse(param, out bool boolResult))
				return boolResult;

			if (typeof(Enum).IsAssignableFrom(paramType))
			{
				object enumResult = null;
				Enum.TryParse(paramType, param, out enumResult);

				return enumResult;
			}

			return null;
		}
	}
}
