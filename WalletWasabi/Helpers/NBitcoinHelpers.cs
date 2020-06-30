using NBitcoin;
using NBitcoin.Crypto;
using NBitcoin.Protocol;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WalletWasabi.Helpers
{
	public static class NBitcoinHelpers
	{
		private static readonly Money[] ReasonableFees = new[]
		{
			Money.Coins(0.002m),
			Money.Coins(0.001m),
			Money.Coins(0.0005m),
			Money.Coins(0.0002m),
			Money.Coins(0.0001m),
			Money.Coins(0.00005m),
			Money.Coins(0.00002m),
			Money.Coins(0.00001m)
		};

		public static string HashOutpoints(IEnumerable<OutPoint> outPoints)
		{
			var sb = new StringBuilder();
			foreach (OutPoint input in outPoints.OrderBy(x => x.Hash.ToString()).ThenBy(x => x.N))
			{
				sb.Append(ByteHelpers.ToHex(input.ToBytes()));
			}

			return HashHelpers.GenerateSha256Hash(sb.ToString());
		}

		public static Money TakeAReasonableFee(Money inputValue)
		{
			Money half = inputValue / 2;

			foreach (Money fee in ReasonableFees)
			{
				Money diff = inputValue - fee;
				if (diff > half)
				{
					return diff;
				}
			}

			return half;
		}

		public static int CalculateVsizeAssumeSegwit(int inNum, int outNum)
		{
			var origTxSize = (inNum * Constants.P2pkhInputSizeInBytes) + (outNum * Constants.OutputSizeInBytes) + 10;
			var newTxSize = (inNum * Constants.P2wpkhInputSizeInBytes) + (outNum * Constants.OutputSizeInBytes) + 10; // BEWARE: This assumes segwit only inputs!
			var vSize = (int)Math.Ceiling(((3 * newTxSize) + origTxSize) / 4m);
			return vSize;
		}

		public static ExtPubKey BetterParseExtPubKey(string extPubKeyString)
		{
			extPubKeyString = Guard.NotNullOrEmptyOrWhitespace(nameof(extPubKeyString), extPubKeyString, trim: true);

			ExtPubKey epk;
			try
			{
				epk = ExtPubKey.Parse(extPubKeyString, NBitcoin.Altcoins.Litecoin.Instance.Mainnet); // Starts with "ExtPubKey": "xpub...
			}
			catch
			{
				try
				{
					epk = ExtPubKey.Parse(extPubKeyString, NBitcoin.Altcoins.Litecoin.Instance.Testnet); // Starts with "ExtPubKey": "xpub...
				}
				catch
				{
					try
					{
						epk = ExtPubKey.Parse(extPubKeyString, NBitcoin.Altcoins.Litecoin.Instance.Regtest); // Starts with "ExtPubKey": "xpub...
					}
					catch
					{
						// Try hex, Old wallet format was like this.
						// Doesn't make sense for LTC Litecoin because there's no legacy wallet format to support :
						epk = new ExtPubKey(ByteHelpers.FromHex(extPubKeyString)); // Starts with "ExtPubKey": "hexbytes...
					}
				}
			}

			return epk;
		}

		public static BitcoinAddress BetterParseBitcoinAddress(string bitcoinAddressString)
		{
			bitcoinAddressString = Guard.NotNullOrEmptyOrWhitespace(nameof(bitcoinAddressString), bitcoinAddressString, trim: true);

			BitcoinAddress ba;
			try
			{
				ba = BitcoinAddress.Create(bitcoinAddressString, NBitcoin.Altcoins.Litecoin.Instance.Mainnet);
			}
			catch
			{
				try
				{
					ba = BitcoinAddress.Create(bitcoinAddressString, NBitcoin.Altcoins.Litecoin.Instance.Testnet);
				}
				catch
				{
					ba = BitcoinAddress.Create(bitcoinAddressString, NBitcoin.Altcoins.Litecoin.Instance.Regtest);
				}
			}

			return ba;
		}

		public static Key BetterParseKey(string keyString)
		{
			keyString = Guard.NotNullOrEmptyOrWhitespace(nameof(keyString), keyString, trim: true);

			Key k;
			try
			{
				k = Key.Parse(keyString, Network.Main);
			}
			catch
			{
				try
				{
					k = Key.Parse(keyString, Network.TestNet);
				}
				catch
				{
					k = Key.Parse(keyString, Network.RegTest);
				}
			}

			return k;
		}

		public static async Task<AddressManager> LoadAddressManagerFromPeerFileAsync(string filePath, Network expectedNetwork = null)
		{
			byte[] data, hash;
			using (var fs = File.Open(filePath, FileMode.Open, FileAccess.Read))
			{
				data = new byte[fs.Length - 32];
				await fs.ReadAsync(data, 0, data.Length);
				hash = new byte[32];
				await fs.ReadAsync(hash, 0, 32);
			}
			var actual = Hashes.Hash256(data);
			var expected = new uint256(hash);
			if (expected != actual)
			{
				throw new FormatException("Invalid address manager file");
			}

			BitcoinStream stream = new BitcoinStream(data) { Type = SerializationType.Disk };
			uint magic = 0;
			stream.ReadWrite(ref magic);
			if (expectedNetwork != null && expectedNetwork.Magic != magic)
			{
				throw new FormatException("This file is not for the expected network");
			}

			var addrman = new AddressManager();
			addrman.ReadWrite(stream);
			return addrman;
		}
	}
}
