﻿//-----------------------------------------------------------------------
// <copyright file="ISocketsService.cs" company="InternetWideWorld.com">
// Copyright (c) George Leithead, InternetWideWorld.  All rights reserved.
//   THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY
//   OF ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT
//   LIMITED TO THE IMPLIED WARRANTIES OF MERCHANTABILITY AND
//   FITNESS FOR A PARTICULAR PURPOSE.
// </copyright>
// <summary>
//   Web Sockets Service interface.
// </summary>
//-----------------------------------------------------------------------

namespace LiLo.Lite.Services.Sockets
{
	using System.Threading.Tasks;

	/// <summary>Web Sockets Service interface.</summary>
	public interface ISocketsService
	{
		/// <summary>Handle when the application closes the sockets connection.</summary>
		/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
		Task WebSocket_Close();

		/// <summary>Handle when the application requests a sockets connection.</summary>
		/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
		Task WebSocket_OnConnect();

		/// <summary>Handle when the application resumes from sleep.</summary>
		/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
		Task WebSocket_OnResume();

		/// <summary>Handle when the application goes into sleep.</summary>
		/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
		Task WebSocket_OnSleep();

		/// <summary>Connects to the sockets service.</summary>
		/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
		Task Connect();
	}
}