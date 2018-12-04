using Newtonsoft.Json;
using YahurrFramework.Enums;

namespace YahurrFramework.Structs
{
	internal class ClientConfig
    {
		/// <summary>
		/// At what level messages will start to be displayed.
		/// </summary>
		[JsonProperty]
		public LogLevel MinLogLevel { get; private set; } = LogLevel.Message;

		/// <summary>
		/// If the app will display whole exceptions.
		/// </summary>
		[JsonProperty]
		public bool ThrowExceptions { get; private set; } = true;

		/// <summary>
		/// Directory to look for token files.
		/// </summary>
		[JsonProperty]
		public string TokenDirectory { get; private set; } = "Tokens";

		[JsonProperty]
		public char CommandPrefix { get; private set; } = '!';

		/// <summary>
		/// Default token to use if it finds multiple.
		/// </summary>
		[JsonProperty]
		public string DefaultTokenName { get; private set; }
	}
}
