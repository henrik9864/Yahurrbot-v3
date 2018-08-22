using Discord.WebSocket;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using YahurrFramework.Attributes;

namespace YahurrFramework
{
    public class YahurrCommand
    {

		public List<string> Structure { get; }

		public List<(string summary, Type type)> Parameters { get; }

		public string Summary { get; }

		public YahurrModule Module { get; }

		MethodInfo method;
		int parameterCount;

		public YahurrCommand(MethodInfo method, YahurrModule module)
		{
			Command cmd = method.GetCustomAttribute<Command>();
			Summary summary = method.GetCustomAttribute<Summary>();

			this.Structure = cmd.CommandStructure;
			this.Summary = summary.Value ?? "Not specefied";
			this.method = method;
			this.Module = module;

			Parameters = LoadParameters(method);
		}

		/// <summary>
		/// Returns a list of summary and types for each parameter in a method.
		/// </summary>
		/// <param name="method">Method to parse</param>
		/// <returns></returns>
		List<(string summary, Type type)> LoadParameters(MethodInfo method)
		{
			var parameters = new List<(string summary, Type type)>();

			ParameterInfo[] methodParameters = method.GetParameters();
			for (int i = 0; i < methodParameters.Length; i++)
			{
				ParameterInfo parameter = methodParameters[i];
				Summary summary = parameter.GetCustomAttribute<Summary>();

				parameters.Add((summary?.Value, parameter.ParameterType));
			}

			parameterCount = methodParameters.Length;
			return parameters;
		}

		/// <summary>
		/// Validates that this list of commands can preform this command.
		/// </summary>
		/// <param name="command"></param>
		/// <returns></returns>
		public bool Verify(List<string> command)
		{
			for (int i = 0; i < Structure.Count; i++)
			{
				if (Structure[i] != command[i])
					return false;
			}

			return true;
		}

		/// <summary>
		/// Run command.
		/// </summary>
		/// <param name="parameters">Parameters to give</param>
		/// <param name="context">Context for this command invokation</param>
		/// <returns></returns>
		public async Task Invoke(List<string> parameters, SocketMessage context)
		{
			Console.WriteLine("hei");

			if (parameters.Count != Parameters.Count)
				return;
			Console.WriteLine("hei");

			object[] objects = new object[Parameters.Count];
			for (int i = 0; i < parameters.Count; i++)
			{
				string value = parameters[i];
				Type type = Parameters[i].type;

				if (type == typeof(string))
					objects[i] = value;
				else
					objects[i] = JsonConvert.DeserializeObject(value, type);
			}

			Console.WriteLine("hei");

			// Wrap in Task.Run?
			Module.SetContext(context);
			await (Task)method.Invoke(Module, objects);
			Module.SetContext(null);
			await Task.CompletedTask;
		}
    }
}
