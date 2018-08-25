using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using YahurrFramework.Enums;

namespace YahurrFramework.Structs
{
    internal class ClientConfig
    {
		[JsonProperty]
		public LogLevel MinLogLevel { get; private set; } = LogLevel.Message;

		[JsonProperty]
		public bool ThrowExceptions { get; private set; }

		[JsonProperty]
		public string TokenDirectory { get; private set; }

		[JsonProperty]
		public int DefaultTokenIndex { get; private set; } = -1;
	}
}
