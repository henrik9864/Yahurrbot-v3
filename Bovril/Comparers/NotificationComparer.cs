using System;
using System.Collections.Generic;
using System.Text;

namespace Bovril.Comparers
{
	public class NotificationComparer : IComparer<Notification>
	{
		public int Compare(Notification x, Notification y)
		{
			if (x.Time > y.Time)
				return 1;
			else if (x.Time < y.Time)
				return -1;
			else
				return 0;
		}
	}
}
