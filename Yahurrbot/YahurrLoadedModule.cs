using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace YahurrFramework
{
    public class YahurrLoadedModule
    {
		public YModule Module { get; }

		public YahurrLoadedModule(YModule module)
		{
			this.Module = module;
		}
	}
}
