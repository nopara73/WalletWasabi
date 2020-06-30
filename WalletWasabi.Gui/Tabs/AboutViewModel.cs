using Avalonia.Diagnostics.ViewModels;
using System;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text;
using WalletWasabi.Gui.ViewModels;
using System.IO;
using ReactiveUI;
using System.Reactive;
using WalletWasabi.Helpers;
using WalletWasabi.Logging;
using System.Reactive.Linq;

namespace WalletWasabi.Gui.Tabs
{
	internal class AboutViewModel : WasabiDocumentTabViewModel
	{
		public AboutViewModel() : base("About")
		{
			OpenBrowserCommand = ReactiveCommand.CreateFromTask<string>(IoHelpers.OpenBrowserAsync);

			OpenBrowserCommand.ThrownExceptions
				.ObserveOn(RxApp.TaskpoolScheduler)
				.Subscribe(ex => Logger.LogError(ex));
		}

		public ReactiveCommand<string, Unit> OpenBrowserCommand { get; }

		public Version ClientVersion => Constants.ClientVersion;
		public string BackendMajorVersion => Constants.BackendMajorVersion;
		public Version BitcoinCoreVersion => Constants.BitcoinCoreVersion;
		public Version HwiVersion => Constants.HwiVersion;

		public string ClearnetLink => "https://MustardWallet.com/";

		public string TorLink => "http://p5d45yv2wobyqaj7.onion";

		public string SourceCodeLink => "https://github.com/MustardWallet/MustardWalletLTC/";

		public string CustomerSupportLink => "https://www.reddit.com/r/MustardWallet/";

		public string BugReportLink => "https://github.com/MustardWallet/MustardWalletLTC/issues/";

		public string FAQLink => "https://docs.wasabiwallet.io/FAQ/";

		public string DocsLink => "https://docs.wasabiwallet.io/";
	}
}
