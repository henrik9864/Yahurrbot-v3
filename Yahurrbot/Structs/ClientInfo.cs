using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace YahurrFramework.Structs
{
    class ClientInfo
    {
		public string ID { get; }

		public string Secret { get; }

		public string Token { get; }

		[JsonConstructor]
		public ClientInfo(string ID, string Secret, string Token)
		{
			this.ID = ID;
			this.Secret = Secret;
			this.Token = Token;
		}
	}
}
