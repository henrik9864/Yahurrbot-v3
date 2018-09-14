using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Newtonsoft.Json;
using YFramework.Structs;

namespace YFramework
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

				//Console.Clear();
			}

			Console.WriteLine("Program exited with code: " + code);
        }
    }
}
