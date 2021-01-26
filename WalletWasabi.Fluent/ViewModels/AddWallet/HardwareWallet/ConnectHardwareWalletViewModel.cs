using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using ReactiveUI;
using WalletWasabi.Fluent.ViewModels.NavBar;
using WalletWasabi.Fluent.ViewModels.Navigation;
using WalletWasabi.Fluent.ViewModels.Wallets;
using WalletWasabi.Hwi.Models;
using WalletWasabi.Logging;
using WalletWasabi.Nito.AsyncEx;
using WalletWasabi.Wallets;

namespace WalletWasabi.Fluent.ViewModels.AddWallet.HardwareWallet
{
	public partial class ConnectHardwareWalletViewModel : RoutableViewModel
	{
		[AutoNotify] private string _message;
		[AutoNotify] private bool _isSearching;
		[AutoNotify] private bool _existingWalletFound;
		[AutoNotify] private bool _nextButtonEnable;

		public ConnectHardwareWalletViewModel(string walletName, WalletManager walletManager, ObservableCollection<WalletViewModelBase> wallets)
		{
			Title = "Hardware Wallet";
			_message = "";
			WalletName = walletName;
			WalletManager = walletManager;
			Wallets = wallets;
			AbandonedTasks = new AbandonedTasks();
			HardwareWalletOperations = new HardwareWalletOperations(WalletManager.Network);

			NextCommand = ReactiveCommand.Create(() => StartDetection(HardwareWalletOperations));

			OpenBrowserCommand = ReactiveCommand.CreateFromTask(async () =>
				await IoHelpers.OpenBrowserAsync("https://docs.wasabiwallet.io/using-wasabi/ColdWasabi.html#using-hardware-wallet-step-by-step"));

			NavigateToExistingWalletLoginCommand = ReactiveCommand.Create(() =>
			{
				var navBar = NavigationManager.Get<NavBarViewModel>();

				if (ExistingWallet is { } && navBar is { })
				{
					navBar.SelectedItem = ExistingWallet;
					Navigate().Clear();
					ExistingWallet.OpenCommand.Execute(default);
				}
			});

			this.WhenAnyValue(x => x.Message)
				.ObserveOn(RxApp.MainThreadScheduler)
				.Subscribe(message => NextButtonEnable = !string.IsNullOrEmpty(message));
		}

		private AbandonedTasks AbandonedTasks { get; }

		public string WalletName { get; }

		public WalletManager WalletManager { get; }

		public ObservableCollection<WalletViewModelBase> Wallets { get; }

		public WalletViewModelBase? ExistingWallet { get; set; }

		public HardwareWalletOperations? HardwareWalletOperations { get; set; }

		public ICommand OpenBrowserCommand { get; }

		public ICommand NavigateToExistingWalletLoginCommand { get; }

		private void StartDetection(HardwareWalletOperations hwo)
		{
			Message = "";

			if (IsSearching)
			{
				return;
			}

			ExistingWalletFound = false;
			AbandonedTasks.AddAndClearCompleted(DetectionAsync(hwo));
		}

		private async Task DetectionAsync(HardwareWalletOperations hwo)
		{
			IsSearching = true;

			try
			{
				CancellationTokenSource cts = new();
				AbandonedTasks.AddAndClearCompleted(CheckForPassphraseAsync(cts.Token));
				var result = await hwo.DetectAsync();
				cts.Cancel();
				EvaluateDetectionResult(result, hwo);
			}
			catch (Exception ex) when (ex is not OperationCanceledException)
			{
				Logger.LogError(ex);
			}
			finally
			{
				IsSearching = false;
			}
		}

		private async Task CheckForPassphraseAsync(CancellationToken cancellationToken)
		{
			try
			{
				await Task.Delay(7000, cancellationToken);
				Message = "Check your device and enter your passphrase.";
			}
			catch (OperationCanceledException)
			{
				// ignored
			}
		}

		private void EvaluateDetectionResult(HwiEnumerateEntry[] devices, HardwareWalletOperations hwo)
		{
			if (devices.Length == 0)
			{
				Message = "Connect your wallet to the USB port on your PC / Enter the PIN on the Wallet.";
				return;
			}

			if (devices.Length > 1)
			{
				Message = "Make sure you have only one hardware wallet connected to the PC.";
				return;
			}

			var device = devices[0];

			if (WalletManager.WalletExists(device.Fingerprint))
			{
				ExistingWallet = Wallets.FirstOrDefault(x => x.Wallet.KeyManager.MasterFingerprint == device.Fingerprint);
				Message = "The connected hardware wallet is already added to the software, click below to open it or click Continue to search again.";
				ExistingWalletFound = true;
				return;
			}

			if (!device.IsInitialized())
			{
				if (device.Model == HardwareWalletModels.Coldcard)
				{
					Message = "Initialize your device first.";
				}
				else
				{
					Message = "Check your device and finish the initialization.";
					AbandonedTasks.AddAndClearCompleted(hwo.InitHardwareWalletAsync(device));
				}

				return;
			}

			if (device.Code is { })
			{
				Message = "Something happened with your device, unlock it with your PIN/Passphrase or reconnect to the PC.";
				return;
			}

			if (device.NeedsPassphraseSent == true)
			{
				Message = "Enter your passphrase on your device.";
				return;
			}

			if (device.NeedsPinSent == true)
			{
				Message = "Enter your PIN on your device.";
				return;
			}

			Navigate().To(new DetectedHardwareWalletViewModel(WalletManager, WalletName, device));
		}

		protected override void OnNavigatedTo(bool inStack, CompositeDisposable disposable)
		{
			base.OnNavigatedTo(inStack, disposable);

			HardwareWalletOperations ??= new HardwareWalletOperations(WalletManager.Network);

			StartDetection(HardwareWalletOperations);

			Disposable.Create(async () =>
			{
				HardwareWalletOperations.Dispose();
				HardwareWalletOperations = null;
				await AbandonedTasks.WhenAllAsync();
			})
			.DisposeWith(disposable);
		}
	}
}
