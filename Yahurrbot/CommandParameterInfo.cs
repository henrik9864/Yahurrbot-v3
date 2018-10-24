using System;
using System.Reflection;
using YahurrFramework.Attributes;

namespace YahurrFramework
{
	public class CommandParameterInfo
	{
		public string Name { get; }

		public string Summary { get; }

		public bool IsParam { get; }

		public bool IsOptional
		{
			get
			{
				return parameterInfo.IsOptional;
			}
		}

		public bool HasDefaultValue
		{
			get 
			{
				return parameterInfo.HasDefaultValue;
			}
		}

		public object DefaultValue
		{
			get
			{
				return parameterInfo.DefaultValue;
			}
		}

		public Type Type { get; }

		ParameterInfo parameterInfo;

		public CommandParameterInfo(ParameterInfo param)
		{
			Description summary = param.GetCustomAttribute<Description>();
			Name name = param.GetCustomAttribute<Name>();
			var isParams = param.GetCustomAttribute<ParamArrayAttribute>();

			this.parameterInfo = param;
			Name = name?.Value ?? param.Name;
			Type = param.ParameterType;
			Summary = summary?.Summary;
			IsParam = isParams != null;
		}
	}
}
