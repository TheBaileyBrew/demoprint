using System;
using Zebra.Sdk.Comm;
using Zebra.Sdk.Printer.Discovery;

namespace demoprint.Services
{
	public interface IZebraPrinterConnectionManager
	{
		string BuildBluetoothConnectionChannelsString(string macAddress);
		void FindBluetoothPrinters(DiscoveryHandler discoveryHandler);
		Connection GetBluetoothConnection(string macAddress);
		StatusConnection GetBluetoothStatusConnection(string macAddress);
		MultichannelConnection GetMultichannelBluetoothConnection(string macAddress);
	}
}
