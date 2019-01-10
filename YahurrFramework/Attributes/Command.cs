using System;
using System.Collections.Generic;

namespace YahurrFramework.Attributes
{
	[AttributeUsage(AttributeTargets.Method)]
    public class Command : Attribute
    {
		public string Value { get; private set; }

		public List<string> CommandStructure { get; private set; }

		public Command(params string[] command)
		{
			Init(command);
		}

		void Init(string[] command)
		{
			CommandStructure = new List<string>();
			CommandStructure.AddRange(command);

			Value += $"!{command[0]}";
			for (int i = 1; i < command.Length; i++)
				Value += $" {Value[i]}";
		}
    }
}
