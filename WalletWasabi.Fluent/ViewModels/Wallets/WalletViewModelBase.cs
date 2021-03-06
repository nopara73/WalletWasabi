using ReactiveUI;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using WalletWasabi.Fluent.ViewModels.NavBar;
using WalletWasabi.Fluent.ViewModels.Navigation;
using WalletWasabi.Helpers;
using WalletWasabi.Wallets;

namespace WalletWasabi.Fluent.ViewModels.Wallets
{
	public abstract partial class WalletViewModelBase : NavBarItemViewModel, IComparable<WalletViewModelBase>, IDisposable
	{
		[AutoNotify] private string _titleTip;
		[AutoNotify(SetterModifier = AccessModifier.Private)] private WalletState _walletState;
		private CompositeDisposable? _disposables;
		private bool _disposedValue;
		private string _title;

		protected WalletViewModelBase(Wallet wallet)
		{
			Wallet = Guard.NotNull(nameof(wallet), wallet);

			_disposables = new CompositeDisposable();

			_title = WalletName;
			var isHardware = Wallet.KeyManager.IsHardwareWallet;
			var isWatch = Wallet.KeyManager.IsWatchOnly;
			_titleTip = isHardware ? "Hardware Wallet" : isWatch ? "Watch Only Wallet" : "Hot Wallet";

			WalletState = wallet.State;

			Observable.FromEventPattern<WalletState>(wallet, nameof(wallet.StateChanged))
				.ObserveOn(RxApp.MainThreadScheduler)
				.Subscribe(x => WalletState = x.EventArgs)
				.DisposeWith(_disposables);

			OpenCommand = ReactiveCommand.Create(() => Navigate().To(this,  NavigationMode.Clear));
		}

		public override string Title
		{
			get => _title;
			protected set => this.RaiseAndSetIfChanged(ref _title, value);
		}

		public Wallet Wallet { get; }

		public string WalletName => Wallet.WalletName;

		public bool IsLoggedIn => Wallet.IsLoggedIn;

		public int CompareTo([AllowNull] WalletViewModelBase other)
		{
			if (WalletState != other!.WalletState)
			{
				if (WalletState == WalletState.Started || other.WalletState == WalletState.Started)
				{
					return other.WalletState.CompareTo(WalletState);
				}
			}

			return Title.CompareTo(other!.Title);
		}

		public override string ToString() => WalletName;

		#region IDisposable Support

		protected virtual void Dispose(bool disposing)
		{
			if (!_disposedValue)
			{
				if (disposing)
				{
					_disposables?.Dispose();
					_disposables = null;
				}

				_disposedValue = true;
			}
		}

		// This code added to correctly implement the disposable pattern.
		public void Dispose()
		{
			// Do not change this code. Put cleanup code in Dispose(bool disposing) above.
			Dispose(true);
		}

		#endregion IDisposable Support
	}
}
