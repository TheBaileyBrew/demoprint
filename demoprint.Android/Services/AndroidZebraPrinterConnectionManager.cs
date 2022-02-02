using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using demoprint.Droid.Services;
using demoprint.Services;
using Xamarin.Forms;
using Zebra.Sdk.Comm;
using Zebra.Sdk.Printer.Discovery;

[assembly: Dependency(typeof(AndroidZebraPrinterConnectionManager))]
namespace demoprint.Droid.Services
{
    public class AndroidZebraPrinterConnectionManager : IZebraPrinterConnectionManager
	{
		public string BuildBluetoothConnectionChannelsString(string macAddress)
		{
			var c = new BluetoothConnection(macAddress);
			c.Open();

			try
			{
				var handler = new ServiceDiscoveryHandlerImplementation();
				BluetoothDiscoverer.FindServices(Android.App.Application.Context, macAddress, handler);

				while (!handler.Finished)
					Task.Delay(100);

				var sb = new StringBuilder();
				foreach (var channel in handler.ConnectionChannels)
				{
					sb.AppendLine(channel.ToString());
				}
				return sb.ToString();
			}
			finally
			{
				try
				{
					c?.Close();
				}
				catch (ConnectionException) { }
			}
		}
		public void FindBluetoothPrinters(DiscoveryHandler discoveryHandler)
		{
			try
			{
				Console.WriteLine("Trying To Find Printers");
				BluetoothDiscoverer.FindPrinters(MainActivity.Instance, discoveryHandler);
			}
			catch (Exception ex)
			{
				if (ex is ConnectionException cex)
				{
					Console.WriteLine("Conn Exception: " + cex.StackTrace);
				}
				else
				{
					Console.WriteLine("Exception: " + ex.StackTrace);
				}
			}

		}
		public Connection GetBluetoothConnection(string macAddress)
		{
			var c = new BluetoothConnection(macAddress);
			// THIS IS IMPORTANT
			// Default is 5000, which introduced a 5s lag every time
			// the connection is closed, which make a bad user
			// experience if following Zebra-recommended best-practice
			// of close a printer connection immediately after printing.
			c.TimeToWaitBeforeClose = 250;
			return c;
		}
		public StatusConnection GetBluetoothStatusConnection(string macAddress)
		{
			return new BluetoothStatusConnection(macAddress);
		}
		public MultichannelConnection GetMultichannelBluetoothConnection(string macAddress)
		{
			return new MultichannelBluetoothConnection(macAddress);
		}

		class ServiceDiscoveryHandlerImplementation : ServiceDiscoveryHandler
		{
			public List<ConnectionChannel> ConnectionChannels { get; private set; } = new List<ConnectionChannel>();
			public bool Finished { get; private set; }

			public void DiscoveryFinished()
			{
				Finished = true;
			}

			public void FoundService(ConnectionChannel channel)
			{
				ConnectionChannels.Add(channel);
			}
		}
	}
}
