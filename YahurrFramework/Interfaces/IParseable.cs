﻿namespace YahurrFramework.Interfaces
{
	public interface IParseable
	{
		/// <summary>
		/// Called to initialize the objetc from a string
		/// </summary>
		/// <param name="parameter"></param>
		void Parse(string parameter);
	}
}
