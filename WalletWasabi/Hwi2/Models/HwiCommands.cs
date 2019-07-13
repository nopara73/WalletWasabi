using System;
using System.Collections.Generic;
using System.Text;

namespace WalletWasabi.Hwi2.Models
{
	public enum HwiCommands
	{
		Enumerate,
		GetMasterXpub,
		SignTx,
		GetXpub,
		SignBessage,
		GetKeypool,
		DisplayAddress,
		Setup,
		Wipe,
		Restore,
		Backup,
		PromptPin,
		SendPin
	}
}