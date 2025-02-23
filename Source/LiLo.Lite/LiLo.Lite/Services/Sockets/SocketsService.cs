﻿//-----------------------------------------------------------------------
// <copyright file="SocketsService.cs" company="InternetWideWorld.com">
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
	using System.Diagnostics;
	using System.Threading.Tasks;
	using LiLo.Lite.Services.Dialog;
	using LiLo.Lite.Services.Markets;
	using WebSocketSharp;
	using Xamarin.Forms;

	/// <summary>Web Sockets Service interface.</summary>
	public class SocketsService : ISocketsService
	{
		private readonly int delayBetweenTries = 3000;

		private IDialogService dialogService;

		/// <summary>Has the service been resumed.</summary>
		private bool isResumed;

		private IMarketsHelperService marketsHelperService;

		private int numberOfTries = 0;

		/// <summary>Web Socket.</summary>
		private WebSocket webSocket;

		/// <summary>Initialises a new instance of the <see cref="SocketsService"/> class.</summary>
		public SocketsService()
		{
		}

		/// <summary>Gets the dialogue service.</summary>
		public IDialogService DialogService => this.dialogService ??= DependencyService.Resolve<DialogService>();

		/// <summary>Gets a value indicating whether the sockets service is connected.</summary>
		private bool IsConnected => this.webSocket.ReadyState == WebSocketState.Open;

		private IMarketsHelperService MarketsHelperService => this.marketsHelperService ??= DependencyService.Resolve<MarketsHelperService>();

		/// <summary>Connects to the Sockets Service.</summary>
		/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
		public async Task Connect()
		{
			string wss = this.MarketsHelperService.GetWss();
			this.webSocket = new WebSocket(wss)
			{
				EmitOnPing = true,
			};
			this.webSocket.SslConfiguration.EnabledSslProtocols = System.Security.Authentication.SslProtocols.Tls12;
			this.webSocket.SslConfiguration.ServerCertificateValidationCallback = (sender, certificate, chain, sslPolicyError) =>
			{
				return true;
			};

			await this.WebSocket_OnResume();
		}

		/// <summary>Handle when the application closes the sockets connection.</summary>
		/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
		public async Task WebSocket_Close()
		{
			_ = await Task.Factory.StartNew(async () =>
			{
				if (!this.IsConnected)
				{
					return;
				}

				await this.DialogService.ShowToastAsync("Closing connection!");
				if (this.webSocket == null)
				{
					_ = await Task.FromResult(true);
					return;
				}

				if (this.webSocket.IsAlive)
				{
					this.webSocket.CloseAsync(CloseStatusCode.Normal);
				}

				_ = await Task.FromResult(true);
			});
		}

		/// <summary>Handle when the application requests a sockets connection.</summary>
		/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
		public async Task WebSocket_OnConnect()
		{
			_ = await Task.Factory.StartNew(async () =>
			{
				if (this.IsConnected)
				{
					return;
				}

				await this.DialogService.ShowToastAsync("Connecting...");
				try
				{
					if (Device.RuntimePlatform != Device.UWP)
					{
						this.webSocket.ConnectAsync();
					}
					else
					{
						this.webSocket.Connect();
					}

					this.numberOfTries = 1;
				}
				catch (System.Net.Sockets.SocketException)
				{
					this.numberOfTries += 1;
					Debug.WriteLine($"Lost connection, awaiting {this.numberOfTries}");
					Task.Delay(this.numberOfTries * this.delayBetweenTries).Wait();
					await this.WebSocket_OnConnect();
				}

				_ = await Task.FromResult(true);
			});
		}

		/// <summary>Handle when the application resumes from sleep.</summary>
		/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
		public async Task WebSocket_OnResume()
		{
			if (!this.isResumed)
			{
				this.webSocket.OnMessage += this.MarketsHelperService.WebSockets_OnMessageAsync;
				this.webSocket.OnMessage += this.WebSocket_OnMessage;
				this.webSocket.OnError += this.WebSocket_OnError;
				this.webSocket.OnClose += this.WebSocket_OnClose;
				await this.WebSocket_OnConnect();
				this.isResumed = true;
			}

			_ = await Task.FromResult(true);
		}

		/// <summary>Handle when the application goes into sleep.</summary>
		/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
		public async Task WebSocket_OnSleep()
		{
			if (this.isResumed)
			{
				this.webSocket.OnClose -= this.WebSocket_OnClose;
				this.webSocket.OnError -= this.WebSocket_OnError;
				this.webSocket.OnMessage -= this.MarketsHelperService.WebSockets_OnMessageAsync;
				this.webSocket.OnMessage -= this.WebSocket_OnMessage;
				this.webSocket.CloseAsync(CloseStatusCode.Normal);
				this.isResumed = false;
			}

			_ = await Task.FromResult(true);
		}

		/// <summary>Handle when the sockets connection closes.</summary>
		/// <param name="sender">Sender object.</param>
		/// <param name="e">Close event arguments.</param>
		private async void WebSocket_OnClose(object sender, CloseEventArgs e)
		{
			_ = await Task.Factory.StartNew(async () =>
			{
				if (this.IsConnected)
				{
					return;
				}

				await this.DialogService.ShowToastAsync("Disconnected!");
				while (!this.webSocket.IsAlive)
				{
					this.numberOfTries += 1;
					Debug.WriteLine($"Lost connection, awaiting {this.numberOfTries}");
					Task.Delay(this.numberOfTries * this.delayBetweenTries).Wait();
					await this.WebSocket_OnConnect();
				}

				this.numberOfTries = 1;
			});
		}

		/// <summary>Handle when the sockets connection errors.</summary>
		/// <param name="sender">Sender object.</param>
		/// <param name="e">Error event arguments.</param>
		private void WebSocket_OnError(object sender, ErrorEventArgs e)
		{
			if (this.IsConnected)
			{
				_ = this.DialogService.ShowToastAsync(e.Message).ConfigureAwait(true);
			}
		}

		/// <summary>Handle when the sockets connection receives a message.</summary>
		/// <param name="sender">Sender object.</param>
		/// <param name="e">Message event arguments.</param>
		private async void WebSocket_OnMessage(object sender, MessageEventArgs e)
		{
			_ = await Task.Factory.StartNew(async () =>
			{
				if (e.IsText)
				{
					this.webSocket.OnMessage -= this.WebSocket_OnMessage;
				}

				_ = await Task.FromResult(true);
			});
		}
	}
}