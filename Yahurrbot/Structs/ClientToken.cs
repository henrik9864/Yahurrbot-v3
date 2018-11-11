using Newtonsoft.Json;

namespace YahurrFramework.Structs
{
	class ClientToken
    {
		public ulong ID { get; private set; }

		public string Secret { get; private set; }

		public string Token { get; private set; }

		[JsonConstructor]
		public ClientToken(ulong ID, string Secret, string Token)
		{
			this.ID = ID;
			this.Secret = Secret;
			this.Token = Token;
		}
	}
}
