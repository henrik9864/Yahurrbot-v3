using System;
using System.Collections.Generic;
using System.Text;

namespace Bovril
{
    public class RemindMeConfig
    {
		public List<string> DateFormats { get; set; } = new List<string>()
		{
			"dd/MM/yyyy",
			"dd/MM/yy",
		};

		public List<string> TimeFormats { get; set; } = new List<string>()
		{
			"ss:mm:HH",
		};
	}
}
