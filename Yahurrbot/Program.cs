using System;

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

				//Console.Clear();
			}

			Console.WriteLine("Program exited with code: " + code);
        }
    }
}
