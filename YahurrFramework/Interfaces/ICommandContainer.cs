using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace YahurrFramework.Interfaces
{
	/// <summary>
	/// A class that contain commands
	/// </summary>
	public interface ICommandContainer
	{
		string Name { get; }

		Task RunCommand(string name, int paramCount, params object[] param);

		void SetContext(MethodContext context);
	}
}
