using System;
using System.Reflection;
using YFramework.Attributes;

namespace YFramework
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
			Summary summary = param.GetCustomAttribute<Summary>();
			Name name = param.GetCustomAttribute<Name>();
			var isParams = param.GetCustomAttribute<ParamArrayAttribute>();

			this.parameterInfo = param;
			Name = name?.Value ?? param.Name;
			Type = param.ParameterType;
			Summary = summary?.Value;
			IsParam = isParams != null;
		}
	}
}
