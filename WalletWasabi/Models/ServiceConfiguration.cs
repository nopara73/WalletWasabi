using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using NBitcoin;
using WalletWasabi.Helpers;

namespace WalletWasabi.Models
{
	public class ServiceConfiguration
	{
		public int MixUntilAnonymitySet { get; set; }
		public int PrivacyLevelSome { get; set; }
		public int PrivacyLevelFine { get; set; }
		public int PrivacyLevelStrong { get; set; }
		public EndPoint BitcoinCoreEndPoint { get; set; }
		public Money DustThreshold { get; set; }
		public decimal CoordinatorFeePercentSanity { get; set; }
		public Money CoinJoinFeePerInputsSanity { get; set; }
		public Money CoinJoinFeePerOutputsSanity { get; set; }

		public ServiceConfiguration(
			int mixUntilAnonymitySet,
			int privacyLevelSome,
			int privacyLevelFine,
			int privacyLevelStrong,
			EndPoint bitcoinCoreEndPoint,
			Money dustThreshold,
			decimal coordinatorFeePercentSanity,
			Money coinJoinFeePerInputsSanity,
			Money coinJoinFeePerOutputsSanity)
		{
			MixUntilAnonymitySet = Guard.NotNull(nameof(mixUntilAnonymitySet), mixUntilAnonymitySet);
			PrivacyLevelSome = Guard.NotNull(nameof(privacyLevelSome), privacyLevelSome);
			PrivacyLevelFine = Guard.NotNull(nameof(privacyLevelFine), privacyLevelFine);
			PrivacyLevelStrong = Guard.NotNull(nameof(privacyLevelStrong), privacyLevelStrong);
			BitcoinCoreEndPoint = Guard.NotNull(nameof(bitcoinCoreEndPoint), bitcoinCoreEndPoint);
			DustThreshold = Guard.NotNull(nameof(dustThreshold), dustThreshold);
			CoordinatorFeePercentSanity = Guard.NotNull(nameof(coordinatorFeePercentSanity), coordinatorFeePercentSanity);
			CoinJoinFeePerInputsSanity = Guard.NotNull(nameof(coinJoinFeePerInputsSanity), coinJoinFeePerInputsSanity);
			CoinJoinFeePerOutputsSanity = Guard.NotNull(nameof(coinJoinFeePerOutputsSanity), coinJoinFeePerOutputsSanity);
		}
	}
}
