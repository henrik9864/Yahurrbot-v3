using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using YahurrBot.Interfaces;
using YahurrFramework.Attributes;

namespace YahurrFramework.Commands
{
	internal class YCommand
	{
		public string Name { get; }

		public string Summary { get; }

		public List<string> Structure { get; }

		public List<YParameter> Parameters { get; }

		public bool IsParam { get; }

		public bool IsDM { get; }

		public YModule Parent { get; }

		MemberInfo method;

		public YCommand(MethodInfo method, YModule module)
		{
			Command command = method.GetCustomAttribute<Command>();
			Summary summary = method.GetCustomAttribute<Summary>();
			Name name = method.GetCustomAttribute<Name>();
			ParameterInfo[] parameters = method.GetParameters();

			Structure = command.CommandStructure;
			Parameters = new List<YParameter>();
			IsParam = false;
			IsDM = command.IsDM;
			Name = name?.Value ?? method.Name;
			Summary = summary?.Value;
			this.Parent = module;
			this.method = method;

			for (int i = 0; i < parameters.Length; i++)
			{
				YParameter parameter = new YParameter(parameters[i]);

				if (parameter.IsParam)
					IsParam = true;

				Parameters.Add(parameter);
			}
		}

		internal async Task Invoke(List<string> command, MethodContext context)
		{
			command.RemoveRange(0, Structure.Count); // Remove uneded structure part of command
			object[] formattedParameters = new object[Parameters.Count];

			for (int i = 0; i < Parameters.Count; i++)
			{
				YParameter parameter = Parameters[i];

				if (i >= command.Count || (command[i] == null && parameter.IsOptional))
					formattedParameters[i] = parameter.HasDefault ? parameter.Default : null;

				if (parameter.IsParam)
				{
					Type indexType = parameter.Type.GetElementType();
					object[] param = (object[])Activator.CreateInstance(parameter.Type, new object[] { command.Count - i });

					for (int a = 0; a < command.Count - i; a++)
						param[a] = ParseParameter(command[i + a], indexType);
						//param[a] = JsonConvert.DeserializeObject(command[i + a], indexType);

					formattedParameters[i] = param;
					break;
				}

				//formattedParameters[i] = JsonConvert.DeserializeObject(command[i], parameter.Type);
				formattedParameters[i] = ParseParameter(command[i], parameter.Type);
			}

			Parent.SetContext(context);
			await Parent.RunMethod(method.Name, formattedParameters);
		}

		object ParseParameter(string param, Type paramType)
		{
			if (int.TryParse(param, out int result))
				return result;

			if (bool.TryParse(param, out bool boolResult))
				return result;

			if (typeof(Enum).IsAssignableFrom(paramType))
			{
				object enumResult = null;
				Enum.TryParse(paramType, param, out enumResult);

				return enumResult;
			}
			else if(typeof(IParseable).IsAssignableFrom(paramType))
			{
				try
				{
					IParseable obj = (IParseable)Activator.CreateInstance(paramType);
					obj.Parse(param);

					return obj;
				}
				catch (Exception)
				{
					return null;
				}
			}

			return param;
		}

		internal T GetAttribute<T>(bool inherit) where T : Attribute
		{
			return method.GetCustomAttribute<T>(inherit);
		}
	}
}
