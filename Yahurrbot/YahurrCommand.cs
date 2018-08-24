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

		public List<(string summary, Type type, bool isParam)> Parameters { get; }

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
		List<(string summary, Type type, bool isParam)> LoadParameters(MethodInfo method)
		{
			var parameters = new List<(string summary, Type type, bool isParam)>();

			ParameterInfo[] methodParameters = method.GetParameters();
			for (int i = 0; i < methodParameters.Length; i++)
			{
				ParameterInfo parameter = methodParameters[i];
				Summary summary = parameter.GetCustomAttribute<Summary>();
				var param = parameter.GetCustomAttribute<ParamArrayAttribute>();

				parameters.Add((summary?.Value, parameter.ParameterType, param != null));
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
				if (!string.Equals(Structure[i], command[i], StringComparison.OrdinalIgnoreCase))
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
			object[] objects = new object[Parameters.Count];
			for (int i = 0; i < Parameters.Count; i++)
			{
				string value = parameters[i];
				Type type = Parameters[i].type;

				if (Parameters[i].isParam)
				{
					Array arr = Array.CreateInstance(type.GetElementType(), parameters.Count - Parameters.Count + 1);

					// Adds rest of parameters into an array of last type
					for (int a = 0; a < parameters.Count - i; a++)
					{
						arr.SetValue(parameters[a + i], a);
					}

					objects[i] = arr;
				}
				else if (type == typeof(string))
					objects[i] = value;
				else
					objects[i] = JsonConvert.DeserializeObject(value, type);
			}

			try
			{
				Module.SetContext(context);
				Task command = (Task)method.Invoke(Module, objects);
				command.Wait();
				Module.SetContext(null);
			}
			catch (AggregateException)
			{
				// Logg when i have logger
				await context.Channel.SendMessageAsync("Error command threw an exception").ConfigureAwait(false);
				throw;
			}

			await Task.CompletedTask;
		}

		/// <summary>
		/// Return custom attribute from method.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public T GetMethodAttribute<T>(bool inherit = false) where T : Attribute
		{
			return method.GetCustomAttribute<T>(inherit);
		}
    }
}
