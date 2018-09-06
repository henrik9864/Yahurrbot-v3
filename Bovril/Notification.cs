using System;
using System.Collections.Generic;
using System.Text;

namespace Bovril
{
    public class Notification
    {
		/// <summary>
		/// When this notification finishes
		/// </summary>
		public DateTime Finish { get; }

		/// <summary>
		/// Time untill this noticatiof will finish
		/// </summary>
		public TimeSpan Time
		{
			get
			{
				if (Finish > DateTime.Now)
					return Finish - DateTime.Now;
				else
					return new TimeSpan();
			}
		}

		Action finishMethod;

		public Notification(DateTime finish, Action func)
		{
			this.Finish = finish;
			this.finishMethod = func;
		}

		/// <summary>
		/// Update this notifcation.
		/// </summary>
		/// <returns></returns>
		public bool Update()
		{
			if (Time.TotalSeconds > 0)
			{
				finishMethod.Invoke();
				return true;
			}

			return false;
		}
    }
}
