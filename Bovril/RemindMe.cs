using Bovril.Comparers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;
using YahurrFramework;
using YahurrFramework.Attributes;

namespace Bovril
{
	[Config(typeof(RemindMeConfig))]
    public class RemindMe : Module
    {
		public new RemindMeConfig Config
		{
			get
			{
				return (RemindMeConfig)base.Config;
			}
		}

		SortedSet<Notification> sortedSet;

		public async override Task Init()
		{
			sortedSet = new SortedSet<Notification>(new NotificationComparer());

			await Task.CompletedTask;
		}

		[Command("notify"), Summary("Add a notifaction.")]
		public async Task AddNotifaction(params string[] tokens)
		{
			/*DateTime nDate;
			TimeSpan nTime;

			if (ParseDate(date, out nDate) && ParseTime(time, out nTime))
			{
				sortedSet.Add(new Notification(nDate + nTime, () => Console.WriteLine("done")));
			}*/

			for (int i = 0; i < tokens.Length; i++)
			{
				await CommandContext.Message.Channel.SendMessageAsync(tokens[i]);
				await Task.Delay(500);
			}

			await Task.CompletedTask;
		}

		bool ParseDate(string date, out DateTime dateTime)
		{
			string[] formats = Config.DateFormats.ToArray();
			CultureInfo culture = CultureInfo.InvariantCulture;
			DateTimeStyles styles = DateTimeStyles.None;

			return DateTime.TryParseExact(date, formats, culture, styles, out dateTime);
		}

		bool ParseTime(string time, out TimeSpan timeSpan)
		{
			string[] formats = Config.TimeFormats.ToArray();
			CultureInfo culture = CultureInfo.InvariantCulture;

			return TimeSpan.TryParseExact(time, formats, culture, out timeSpan);
		}
	}
}
