using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using YahurrFramework.Attributes;

namespace YahurrFramework.Commands
{
	internal class YParameter
	{
		public string Name { get; }

		public string Summary { get; }

		public bool IsOptional { get; }

		public bool HasDefault { get; }

		public bool IsParam { get; }

		public object Default { get; }

		public Type Type { get; }

		public YParameter(ParameterInfo parameter)
		{
			Name name = parameter.GetCustomAttribute<Name>();
			Summary summary = parameter.GetCustomAttribute<Summary>();
			Description description = parameter.GetCustomAttribute<Description>();

			IsOptional = parameter.IsOptional || parameter.HasDefaultValue;
			HasDefault = parameter.HasDefaultValue;
			Default = parameter.DefaultValue;
			Name = name?.Value ?? description?.Name ?? parameter.Name;
			Summary = summary?.Value ?? description?.Summary ?? "Summary not specefied.";
			Type = parameter.ParameterType;

			if (!(parameter.GetCustomAttribute<ParamArrayAttribute>() is null))
				IsParam = true;
		}
	}
}
