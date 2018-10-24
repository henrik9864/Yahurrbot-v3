using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using YahurrFramework.Attributes;

namespace YahurrFramework.Commands
{
	internal class YCommand
	{
		public string Name { get; }

		public List<string> Structure { get; }

		public List<YParameter> Parameters { get; }

		public bool IsParam { get; }

		public bool IsDM { get; }

		YModule module;
		MemberInfo method;

		public YCommand(MethodInfo method, YModule module)
		{
			Command command = method.GetCustomAttribute<Command>();
			ParameterInfo[] parameters = method.GetParameters();

			Structure = command.CommandStructure;
			Parameters = new List<YParameter>();
			IsParam = false;
			IsDM = command.IsDM;
			Name = method.Name;
			this.module = module;
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
			command.RemoveRange(0, Structure.Count);
			object[] formattedParameters = new object[Parameters.Count];

			for (int i = 0; i < Parameters.Count; i++)
			{
				YParameter parameter = Parameters[i];

				if (i >= command.Count || command[i] == null && parameter.IsOptional)
					formattedParameters[i] = parameter.HasDefault ? parameter.Default : null;

				if (parameter.IsParam)
				{
					Type indexType = parameter.Type.GetElementType();
					object[] param = (object[])Activator.CreateInstance(parameter.Type, new object[] { command.Count - i });//new string[command.Count - i];

					for (int a = 0; a < command.Count - i; a++)
					{
						param[a] = command[i + a];
						//bool succsess = TryParseParameter(indexType, command[i + a], out param[a]);

						//if (!succsess)
						//	throw new InvalidCastException($"Cannot convert {command[i + a]} to {indexType.Name}");
					}

					formattedParameters[i] = param;
					break;
				}

				formattedParameters[i] = command[i];
			}

			module.SetContext(context);
			await module.RunMethod(Name, formattedParameters);
		}

		bool TryParseParameter(Type type, string param, out object parsed)
		{
			int intParsed = 0;
			object objParsed = null;
			var @switch = new Dictionary<Type, Func<string, bool>>
			{
				{ typeof(int), m => int.TryParse(m, out intParsed) },
			};

			if (@switch.TryGetValue(type, out Func<string, bool> func))
			{
				if (func(param))
				{
					parsed = objParsed ?? intParsed;
					return true;
				}
			}

			parsed = objParsed;
			return false;
		}

		internal T GetAttribute<T>(bool inherit) where T : Attribute
		{
			return method.GetCustomAttribute<T>(inherit);
		}
	}
}
