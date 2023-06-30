using NBitcoin;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WalletWasabi.Bases;
using WalletWasabi.BitcoinCore.Rpc;
using WalletWasabi.WabiSabi.Backend.Rounds.CoinJoinStorage;
using System.Collections.Immutable;
using WalletWasabi.BitcoinCore.Mempool;

namespace WalletWasabi.WabiSabi.Backend;

public class CoinJoinMempoolManager : PeriodicRunner
{
	public CoinJoinMempoolManager(ICoinJoinIdStore coinJoinIdStore, MempoolMirror mempool) : base(TimeSpan.FromMinutes(1))
	{
		CoinJoinIdStore = coinJoinIdStore;
		Mempool = mempool;
	}

	private ICoinJoinIdStore CoinJoinIdStore { get; }
	public MempoolMirror Mempool { get; }
	public ImmutableArray<uint256> CoinJoinIds { get; private set; } = ImmutableArray.Create<uint256>();

	protected override Task ActionAsync(CancellationToken cancel)
	{
		var mempoolHashes = Mempool.GetMempoolHashes();
		var coinJoinsInMempool = mempoolHashes.Where(CoinJoinIdStore.Contains);
		CoinJoinIds = coinJoinsInMempool.ToImmutableArray();

		return Task.CompletedTask;
	}
}
