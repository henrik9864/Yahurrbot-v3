using System;
using System.Collections.Generic;
using System.Text;

namespace YahurrFramework.Structs
{
	public struct CommandLayer
	{
		public List<string> Names { get; }

		public CommandLayer(List<string> names)
		{
			this.Names = names;
		}

		public static implicit operator CommandLayer(string str)
		{
			return new CommandLayer(new List<string>() { str });
		}

		public static implicit operator string(CommandLayer clayer)
		{
			return "s123";
		}

		#region Casting boilerplate

		public static implicit operator CommandLayer((string n1, string n2) tuple)
		{
			return new CommandLayer(new List<string>() { tuple.n1, tuple.n2 });
		}
		
		public static implicit operator CommandLayer((string n1, string n2, string n3) tuple)
		{
			return new CommandLayer(new List<string>() { tuple.n1, tuple.n2, tuple.n3 });
		}

		public static implicit operator CommandLayer((string n1, string n2, string n3, string n4) tuple)
		{
			return new CommandLayer(new List<string>() { tuple.n1, tuple.n2, tuple.n3, tuple.n4 });
		}

		public static implicit operator CommandLayer((string n1, string n2, string n3, string n4, string n5) tuple)
		{
			return new CommandLayer(new List<string>() { tuple.n1, tuple.n2, tuple.n3, tuple.n4, tuple.n5 });
		}

		public static implicit operator CommandLayer((string n1, string n2, string n3, string n4, string n5, string n6) tuple)
		{
			return new CommandLayer(new List<string>() { tuple.n1, tuple.n2, tuple.n3, tuple.n4, tuple.n5, tuple.n6 });
		}

		public static implicit operator CommandLayer((string n1, string n2, string n3, string n4, string n5, string n6, string n7) tuple)
		{
			return new CommandLayer(new List<string>() { tuple.n1, tuple.n2, tuple.n3, tuple.n4, tuple.n5, tuple.n6, tuple.n7 });
		}

		public static implicit operator CommandLayer((string n1, string n2, string n3, string n4, string n5, string n6, string n7, string n8) tuple)
		{
			return new CommandLayer(new List<string>() { tuple.n1, tuple.n2, tuple.n3, tuple.n4, tuple.n5, tuple.n6, tuple.n7, tuple.n8 });
		}

		#endregion
	}
}
