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

		public List<CommandParameterInfo> Parameters { get; }

		public string Summary { get; }

		public Module Module { get; }

		public string Name
		{
			get
			{
				return method.Name;
			}
		}

		MethodInfo method;
		int parameterCount;

		public YahurrCommand(MethodInfo method, Module module)
		{
			Attributes.Command cmd = method.GetCustomAttribute<Attributes.Command>();
			Summary summary = method.GetCustomAttribute<Summary>();

			this.Structure = cmd.CommandStructure;
			this.Summary = summary?.Value ?? "Not specefied";
			this.method = method;
			this.Module = module;

			Parameters = LoadParameters(method);
		}

		/// <summary>
		/// Returns a list of summary and types for each parameter in a method.
		/// </summary>
		/// <param name="method">Method to parse</param>
		/// <returns></returns>
		List<CommandParameterInfo> LoadParameters(MethodInfo method)
		{
			var parameters = new List<CommandParameterInfo>();

			ParameterInfo[] methodParameters = method.GetParameters();
			for (int i = 0; i < methodParameters.Length; i++)
			{
				ParameterInfo parameter = methodParameters[i];
				parameters.Add(new CommandParameterInfo(parameter));
			}

			parameterCount = methodParameters.Length;
			return parameters;
		}

		/// <summary>
		/// Validates that this list of commands can preform this command.
		/// </summary>
		/// <param name="parameters"></param>
		/// <returns></returns>
		public bool VerifyParameters(List<string> parameters)
		{
			Console.WriteLine(parameters.Count);
			Console.WriteLine(Structure.Count);

			if (parameters.Count - Structure.Count == Parameters.Count)
				return true;

			for (int i = 0; i < Parameters.Count; i++)
			{
				int index = i + Structure.Count;
				CommandParameterInfo param = Parameters[i];

				if (parameters.Count > index)
					continue;
				else if (param.IsOptional || param.HasDefaultValue)
					continue;
				else if (param.IsParam)
					return true;
				else
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
		public async Task Invoke(List<string> parameters, CommandContext context)
		{
			// Remove all invalid parameters
			parameters.RemoveAll(a => string.IsNullOrEmpty(a));

			object[] objects = new object[Parameters.Count];
			for (int i = 0; i < Parameters.Count; i++)
			{
				string value = parameters[i];
				Type type = Parameters[i].Type;

				if (Parameters[i].IsParam)
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
				{
					try
					{
						objects[i] = JsonConvert.DeserializeObject(value, type);
					}
					catch (Exception)
					{
						await context.Message.Channel.SendMessageAsync($"```Error parsing parameter {i + 1}: {value}.```").ConfigureAwait(false);
						throw;
					}
				}
			}

			try
			{
				Module.SetContext(context);
				Task command = (Task)method.Invoke(Module, objects);
				command.Wait();
				Module.SetContext(null);
			}
			catch (AggregateException e)
			{
				await context.Message.Channel.SendMessageAsync($"```Error: Command threw an exception:\n{e.InnerException.Message}```").ConfigureAwait(false);
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
