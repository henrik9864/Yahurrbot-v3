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

		MethodInfo method;

		int parameterCount;

		public YahurrCommand(List<string> structure, string summary, MethodInfo method)
		{
			this.Structure = structure;
			this.Summary = summary;
			this.method = method;

			Parameters = LoadParameters(method);
		}

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

		public bool Verify(List<string> command)
		{
			if (command.Count != Structure.Count + parameterCount)
				return false;

			for (int i = 0; i < Structure.Count; i++)
			{
				if (Structure[i] != command[i])
					return false;
			}

			return true;
		}

		public async Task Invoke(object obj, List<string> parameters)
		{
			if (parameters.Count != Parameters.Count)
				return;

			object[] objects = new object[Parameters.Count];
			for (int i = 0; i < parameters.Count; i++)
			{
				string value = parameters[i];
				Type type = Parameters[i].type;

				objects[i] = JsonConvert.DeserializeObject(value, type);
			}


			// Wrap in Task.Run?
			await (Task)method.Invoke(obj, objects);
			await Task.CompletedTask;
		}
    }
}
