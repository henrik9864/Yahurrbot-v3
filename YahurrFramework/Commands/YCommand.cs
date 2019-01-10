using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using YahurrBot.Interfaces;
using YahurrFramework.Attributes;
using YahurrFramework.Interfaces;

namespace YahurrFramework.Commands
{
	internal class YCommand
	{
		public string Name { get; }

		public string MethodName { get; }

		public string Summary { get; }

		public List<string> Structure { get; }

		public List<YParameter> Parameters { get; }

		public ICommandContainer Parent { get; }

		MemberInfo method;

		internal YCommand(MethodInfo method, ICommandContainer parent) : this(method)
		{
			this.Parent = parent;
		}

		internal YCommand(MethodInfo method)
		{
			Command command = method.GetCustomAttribute<Command>();
			Summary summary = method.GetCustomAttribute<Summary>();
			Name name = method.GetCustomAttribute<Name>();
			ParameterInfo[] parameters = method.GetParameters();

			Structure = command.CommandStructure;
			Parameters = new List<YParameter>();
			Name = name?.Value ?? method.Name;
			MethodName = method.Name;
			Summary = summary?.Value;
			this.method = method;

			for (int i = 0; i < parameters.Length; i++)
			{
				YParameter parameter = new YParameter(parameters[i]);

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

				if (parameter.IsParam)
				{
					if (command.Count - i < 0)
						break;

					Type indexType = parameter.Type.GetElementType();
					object[] param = (object[])Activator.CreateInstance(parameter.Type, new object[] { command.Count - i });

					for (int a = 0; a < command.Count - i; a++)
						param[a] = ParseParameter(command[i + a], indexType);

					formattedParameters[i] = param;
					break;
				}

				// Check and insert optinal parameter if there is one
				if (i >= command.Count || (command[i] == null && parameter.IsOptional))
				{
					formattedParameters[i] = parameter.HasDefault ? parameter.Default : null;
					continue;
				}

				formattedParameters[i] = ParseParameter(command[i], parameter.Type);
			}

			Parent.SetContext(context);
			await Parent.RunCommand(method.Name, formattedParameters.Length, formattedParameters);
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

			if (int.TryParse(param, out int result) && typeof(int).IsAssignableFrom(paramType))
				return result;

			if (bool.TryParse(param, out bool boolResult) && typeof(bool).IsAssignableFrom(paramType))
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

			return null;
		}

		/// <summary>
		/// Get custom attribute from the method info.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="inherit"></param>
		/// <returns></returns>
		internal T GetAttribute<T>(bool inherit) where T : Attribute
		{
			return method.GetCustomAttribute<T>(inherit);
		}

		/// <summary>
		/// Get custom attributes from the method info.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="inherit"></param>
		/// <returns></returns>
		internal T[] GetAttributes<T>(bool inherit) where T : Attribute
		{
			return (T[])method.GetCustomAttributes<T>(inherit);
		}
	}
}
