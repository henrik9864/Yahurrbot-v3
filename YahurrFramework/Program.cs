using Newtonsoft.Json;
using System;
using System.Linq.Expressions;
using System.Reflection;

namespace YahurrFramework
{
	class Program
    {
        static void Main(string[] args)
        {
            int code;
			while (true)
			{
				YahurrBot bot = new YahurrBot();
				code = (int)bot.StartAsync().GetAwaiter().GetResult();

				if (code != 2)
					break;
			}

			Console.WriteLine("Program exited with code: " + code);
        }
    }
}
