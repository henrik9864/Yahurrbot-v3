using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Discord.WebSocket;
using YahurrFramework.Attributes;

namespace YahurrFramework.Commands.InternalCommands
{
	internal class FileCommand : InternalCommandContainer
	{
		public FileCommand(DiscordSocketClient client, YahurrBot bot) : base(client, bot)
		{
		}

		[Command("file", "list")]
		public async Task ListFiles(string root)
		{
			if (!Directory.Exists(root))
			{
				await Channel.SendMessageAsync($"Directory '{root}' does not exist.");
				return;
			}

			await Channel.SendMessageAsync($"```{CreateFileTree(root, false)}```");
		}

		[Command("file", "inspect")]
		public async Task InspectFile(string filePath)
		{
			if (!File.Exists(filePath))
			{
				await Channel.SendMessageAsync($"File '{filePath}' does not exist.");
				return;
			}

			using (StreamReader reader = new StreamReader(filePath))
			{
				string content = await reader.ReadToEndAsync();

				if (content.Length <= 2000)
					await Channel.SendMessageAsync($"```{content}```");
				else
					await Channel.SendMessageAsync($"File too long.");
			}
		}

		[Command("file", "download")]
		public async Task DownloadFile(string filePath)
		{
			if (!File.Exists(filePath))
			{
				await Channel.SendMessageAsync($"File '{filePath}' does not exist.");
				return;
			}

			await Channel.SendFileAsync(filePath);
		}

		[Command("file", "upload")]
		public async Task UploadFile(string filePath)
		{
			var file = Message.Attachments.FirstOrDefault();

			using (WebClient webClient = new WebClient())
			{
				await webClient.DownloadFileTaskAsync(new Uri(file.Url), filePath);
			}
		}

		[Command("file", "modify")]
		public async Task AppendFile(string filePath, string content, FileMode mode, FileAccess access)
		{
			using (FileStream fileStream = new FileStream(filePath, mode, access))
			{
				byte[] buffer = Encoding.UTF8.GetBytes(content);
				await fileStream.WriteAsync(buffer);
			}
		}

		/// <summary>
		/// Creates a file tree from root.
		/// </summary>
		/// <param name="root">File tree start directory.</param>
		/// <param name="first"></param>
		/// <param name="indent"></param>
		/// <returns></returns>
		string CreateFileTree(string root, bool first = false, string indent = "")
		{
			string lastFolderName = Path.GetFileName(Path.GetDirectoryName(root+"\\"));
			string output = $"{lastFolderName}\n";

			string[] files = Directory.GetFiles(root);
			string[] directories = Directory.GetDirectories(root);
			for (int i = 0; i < directories.Length; i++)
			{
				string directory = directories[i];
				string prefix = i == directories.Length - 1 && files.Length == 0 ? "└" : "├";
				string indentPrefix = files.Length == 0 ? "" : "│";
				string fileTree = CreateFileTree(directory, true, $"{indent}{indentPrefix}  ");

				output += $"{indent}{prefix}─{fileTree}";
			}

			for (int i = 0; i < files.Length; i++)
			{
				string file = files[i];
				string fileName = Path.GetFileName(file);
				string prefix = i == files.Length - 1 ? "└" : "├";

				output += $"{indent}{prefix}─{fileName}\n";
			}

			return output;
		}
	}
}
