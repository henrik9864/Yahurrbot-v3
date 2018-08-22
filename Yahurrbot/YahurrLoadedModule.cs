using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace YahurrFramework
{
    public class YahurrLoadedModule
    {
		public YahurrModule Module { get; }

		public YahurrLoadedModule(YahurrModule module)
		{
			this.Module = module;
		}
	}
}
