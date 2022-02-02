using System;
using System.Collections.Generic;
using demoprint.iOS.Services;
using demoprint.Services;
using Xamarin.Forms;
using Zebra.Sdk.Comm;
using Zebra.Sdk.Printer.Discovery;

[assembly: Dependency(typeof(iOSZebraPrinterConnectionManager))]
namespace demoprint.iOS.Services
{
	public class iOSZebraPrinterConnectionManager : IZebraPrinterConnectionManager
	{
		public iOSZebraPrinterConnectionManager()
		{
			System.Diagnostics.Debug.WriteLine("iOSZebraPrinterConnectionManager");
		}

		public string BuildBluetoothConnectionChannelsString(string macAddress)
		{
			throw new NotImplementedException();
		}

		public void FindBluetoothPrinters(DiscoveryHandler discoveryHandler)
		{
			BluetoothDiscoverer.FindPrinters(discoveryHandler);
		}

		public Connection GetBluetoothConnection(string macAddress)
		{
			var c = new BluetoothConnection(macAddress);
			// THIS IS IMPORTANT
			// Default is 5000, which introduced a 5s lag every time
			// the connection is closed, which make a bad user
			// experience if following Zebra-recommended best-practice
			// of close a printer conneceion immediately after printing.
			// Android worked fine with a 200ms delay here, but on iOS
			// when printing return receipt + container labels, the receipt
			// printed and then the container labels did not.  
			// Changing this to 500 worked.  Yay.
			c.TimeToWaitBeforeClose = 500;
			return c;
		}

		public StatusConnection GetBluetoothStatusConnection(string macAddress)
		{
			throw new NotImplementedException();
		}

		public MultichannelConnection GetMultichannelBluetoothConnection(string macAddress)
		{
			throw new NotImplementedException();
		}

		public Connection GetUsbConnection(string symbolicName)
		{
			throw new NotImplementedException();
		}

		public void GetZebraUsbDirectPrinters(DiscoveryHandler discoveryHandler)
		{
			throw new NotImplementedException();
		}

		public List<DiscoveredPrinter> GetZebraUsbDriverPrinters()
		{
			throw new NotImplementedException();
		}

	}
}
