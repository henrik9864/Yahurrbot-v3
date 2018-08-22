using System;
using System.Collections.Generic;
using System.Text;

namespace YahurrFramework.Attributes
{
	[AttributeUsage(AttributeTargets.Method)]
    public class Command : Attribute
    {
		public string Value { get; }

		public List<string> CommandStructure { get; }

		public Command(params string[] command)
		{
			CommandStructure = new List<string>();
			CommandStructure.AddRange(command);

			Value += $"!{command[0]}";
			for (int i = 1; i < command.Length; i++)
				Value += $" {Value[i]}";
		}
    }
}
