﻿using System;
using System.Collections.Generic;
using System.Text;

namespace YahurrFramework.Attributes
{
	[AttributeUsage(AttributeTargets.Class)]
    public class Config : Attribute
    {
		public Type Type { get; }

		public Config(Type type)
		{
			Type = type;
		}
	}
}
