﻿//-----------------------------------------------------------------------
// <copyright file="IMarketsHelperService.cs" company="InternetWideWorld.com">
// Copyright (c) George Leithead, InternetWideWorld.  All rights reserved.
//   THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY
//   OF ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT
//   LIMITED TO THE IMPLIED WARRANTIES OF MERCHANTABILITY AND
//   FITNESS FOR A PARTICULAR PURPOSE.
// </copyright>
// <summary>
//   Markets helper service.
// </summary>
//-----------------------------------------------------------------------

namespace LiLo.Lite.Services.Bybit
{
	using System.Collections.ObjectModel;
	using System.ComponentModel;
	using LiLo.Lite.Models.Markets;
	using WebSocketSharp;

	/// <summary>Markets helper service.</summary>
	public interface IMarketsHelperService
	{
		/// <summary>Property changed event</summary>
		event PropertyChangedEventHandler PropertyChanged;

		/// <summary>Gets or sets a list of Markets.</summary>
		ObservableCollection<MarketsModel> MarketsList { get; set; }

		/// <summary>Message received handler.</summary>
		/// <param name="sender">Message sender</param>
		/// <param name="e">Message event arguments</param>
		void WebSockets_OnMessageAsync(object sender, MessageEventArgs e);
	}
}