using System;
using System.IO;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Newtonsoft.Json;
using YahurrFramework.Structs;

namespace YahurrFramework
{
    class Program
    {
        static void Main(string[] args)
        {
			new Program().MainAsync().GetAwaiter().GetResult();
        }

		public async Task MainAsync()
		{
			YahurrBot yahurrBot = new YahurrBot();

			await yahurrBot.StartAsync();
		}
    }
}
