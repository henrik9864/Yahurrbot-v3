using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace YahurrFramework
{
    public class YahurrLoadedModule
    {
		public Module Module { get; }

		public YahurrLoadedModule(Module module)
		{
			this.Module = module;
		}
	}
}
