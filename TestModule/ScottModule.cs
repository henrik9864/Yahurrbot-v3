using System;
using System.Collections.Generic;
using System.Text;
using YahurrFramework;
using YahurrFramework.Attributes;
using ScottServer;
using System.Threading.Tasks;
using YahurrFramework.Enums;

namespace TestModule
{
	class ScottModule : YModule
	{
		ScottClient client;

		protected override async Task Init()
		{
			string ip = "158.36.70.56";
			int port = 80;

			client = new ScottClient(ip, port);
			await client.Start();
		}

		[Command("scott")]
		public async Task StudentNummer(string studentnummer)
		{
			ScottProtocol response = await client.GetResponse(1, 2, new StudentNumber(studentnummer));

			string returnString = "```";

			if (response is ScottResponse)
			{
				byte[] filtered = new byte[response.Data.Length - 2];
				Array.Copy(response.Data, 2, filtered, 0, response.Data.Length - 2);
				returnString += Encoding.UTF8.GetString(filtered);
			}
			else
			{
				ScottError error = response as ScottError;
				returnString += error.Error;
			}

			await RespondAsync(returnString + "```");
		}

	}
}
