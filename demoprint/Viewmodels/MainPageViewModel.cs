using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;
using demoprint.Models;
using demoprint.Services;
using PropertyChanged;
using Xamarin.Forms;
using Zebra.Sdk.Comm;
using Zebra.Sdk.Printer;
using Zebra.Sdk.Printer.Discovery;

namespace demoprint.Viewmodels
{

    [AddINotifyPropertyChangedInterface]
    public class MainPageViewModel
    {
        // members...
        INavigation _nav;
		IZebraPrinterConnectionManager connManager;

		// consts...
		const string NORMAL_TAG_ZPL =
			@"^XA
			^MMT
			^PW406
			^LS0
			{{Blank}}
			^FT96,80^A@I,20,20,TT0003M_^FH\^CI17^F8^FD{{Size}}^FS^CI0
			^FT180,80^A@I,20,20,TT0003M_^FH\^CI17^F8^FD{{Ru}}^FS^CI0
			^FT395,80^A@I,20,20,TT0003M_^FH\^CI17^F8^FD{{Upc}}^FS^CI0
			^FT395,103^A@I,20,20,TT0003M_^FH\^CI17^F8^FD{{ItemDescr}}^FS^CI0
			^FT245,143^A@I,56,56,TT0003M_^FH\^CI17^F8^FD{{ItemNmbr}}^FS^CI0
			^BY2,3,52
			^FT346,18
			^BMI,A,,N,N
			^FD{{ItemNmbrAndCheckDigit}}^FS
			^XZ
			";

		const string RETAIL_TAG_ZPL =
			@"^XA
			^MMT
			^PW406
			^LS0
			{{Blank}}
			^FT96,77^A@I,20,20,TT0003M_^FH\^CI17^F8^FD{{Size}}^FS^CI0
			^FT180,77^A@I,20,20,TT0003M_^FH\^CI17^F8^FD{{Ru}}^FS^CI0
			^FT395,125^A@I,20,20,TT0003M_^FH\^CI17^F8^FD{{ItemNmbr}}^FS^CI0
			^FT395,77^A@I,20,20,TT0003M_^FH\^CI17^F8^FD{{Upc}}^FS^CI0
			^FT395,100^A@I,20,20,TT0003M_^FH\^CI17^F8^FD{{ItemDescr}}^FS^CI0
			^FT180,150^A@I,56,56,TT0003M_^FH\^CI17^F8^FD{{Price}}^FS^CI0
			^BY2,3,52
			^FT346,18
			^BMI,A,,N,N
			^FD{{ItemNmbrAndCheckDigit}}^FS
			^XZ
			";

		public ObservableCollection<Tag> AvailableItems { get; set; } = new ObservableCollection<Tag>();
		public ObservableCollection<BluetoothPrinter> AllPrinters { get; set; } = new ObservableCollection<BluetoothPrinter>();

		public bool IsProgressVisible { get; set; } = false;
		public bool ArePrintersAvailable { get; set; } = false;
		public bool CanPrint { get; set; } = false;

		BluetoothPrinter SelectedPrinter { get; set; }
		// Populated values for printers as they are selected from the list
		public string SelectedPrinterName { get; set; }
		public string SelectedPrinterAddress { get; set; }
		public string SelectedPrinterManufacturer { get; set; }
		public string SelectedPrinterModel { get; set; }
		public string SelectedPrinterFirmware { get; set; }
		public string SelectedPrinterBatteryPercent { get; set; }
		public string SelectedPrinterBatteryStatus { get; set; }
		public string SelectedPrinterBatteryHealth { get; set; }
		public bool IsPrinterBatteryOperated { get; set; }

		public string CurrentPrinterName { get; set; } = "NONE SELECTED";
		public string CurrentPrinterAddress { get; set; } = "";

		public string PrintingStatus { get; set; } = "";
		public int TotalItems { get; set; } = 0;
		public int RollingTotalItems { get; set; } = 0;

		public MainPageViewModel()
        {
			BuildTagSet();
        }

		// OnXxxChanged (Fody PropertyChanged)
		public async void OnSelectedPrinterChanged()
		{
			Console.WriteLine($"Selected is: {SelectedPrinter.PrinterName}");
			if (SelectedPrinter != null)
				await ReadSelectedPrinterInfo(SelectedPrinter);
		}

		public void BuildTagSet()
        {
			AvailableItems.Add(new Tag
			{
				ItemNumber="7207988",
				ItemDescription="KELL P/TART FR BLUEBERRY",
				Quantity=6,
				OriginalQuantity=5,
				FormattedCustomer="330-999989-476",
				Upc="038000222559",
				RetailUnit="1",
				Size="13.5 OZ",
				RetailPrice="$4.79",
				ItemNumberAndCheckDigit="72079882",
				IsRetailPrinting=false
			});
			AvailableItems.Add(new Tag
			{
				ItemNumber = "5926902",
				ItemDescription = "CHOBANI BLUEBERRY GREEK YOGURT",
				Quantity = 8,
				OriginalQuantity = 6,
				FormattedCustomer = "330-999989-476",
				Upc = "894700010052",
				RetailUnit = "12",
				Size = "5.3 OZ",
				RetailPrice = "$1.89",
				ItemNumberAndCheckDigit = "59269027",
				IsRetailPrinting = false
			});
			AvailableItems.Add(new Tag
			{
				ItemNumber = "1804111",
				ItemDescription = "BLUE DIA BLUEBERRY ALMONDS",
				Quantity = 7,
				OriginalQuantity = 6,
				FormattedCustomer = "330-999989-476",
				Upc = "041570094754",
				RetailUnit = "12",
				Size = "1.5 OZ",
				RetailPrice = "$1.59",
				ItemNumberAndCheckDigit = "18041111",
				IsRetailPrinting = false
			});
			AvailableItems.Add(new Tag
			{
				ItemNumber = "1889807",
				ItemDescription = "HOSTESS BLUEBERRY MUFFIN FOA",
				Quantity = 12,
				OriginalQuantity = 11,
				FormattedCustomer = "330-999989-476",
				Upc = "888109010546",
				RetailUnit = "3",
				Size = "5.50 OZ",
				RetailPrice = "$2.09",
				ItemNumberAndCheckDigit = "18898072",
				IsRetailPrinting = false
			});
		}

		void PrintItemTag(Connection conn, Tag item)
        {
			var tagQuantity = item.Quantity;
			for (int x = 0; x < tagQuantity; x++)
			{
				var status = PrinterUtil.GetCurrentStatus(conn, PrinterLanguage.ZPL);
				var startPause = DateTime.Now;
				while (status.isReceiveBufferFull && DateTime.Now.Subtract(startPause).TotalSeconds < 30)
				{
					App.Current.Hud.Show("ReceiveBufferFull");
					// haven't see this actually happen yet...just being safe
					System.Diagnostics.Debug.WriteLine("pausing up to 30s for printer...receive buffer is full ({0} formatsInReceiveBuffer", status.numberOfFormatsInReceiveBuffer);
					System.Threading.Thread.Sleep(1000);
				}
				RollingTotalItems++;
				string zpl = NORMAL_TAG_ZPL;
				zpl = zpl.Replace("{{ItemNmbr}}", $"{item.ItemNumber:g0}");
				zpl = zpl.Replace("{{ItemDescr}}", $"{RollingTotalItems}.{item.ItemDescription}");
				zpl = zpl.Replace("{{Upc}}", item.Upc);
				zpl = zpl.Replace("{{Ru}}", $"{item.RetailUnit:g0}");
				zpl = zpl.Replace("{{Size}}", item.Size);
				zpl = zpl.Replace("{{Price}}", item.RetailPrice);
				zpl = zpl.Replace("{{ItemNmbrAndCheckDigit}}", $"{item.ItemNumberAndCheckDigit}");
				App.Current.Hud.Show($"Printing {RollingTotalItems}/{TotalItems}");
				var bytes = Encoding.UTF8.GetBytes(zpl);
				conn.Write(bytes);
			}
		}

		public async Task ReadSelectedPrinterInfo(BluetoothPrinter p)
		{
			IsProgressVisible = true;
			App.Current.Hud.Show("Getting Printer Details");
			connManager = DependencyService.Get<IZebraPrinterConnectionManager>();
			var conn = connManager.GetBluetoothConnection(p.PrinterAddress);
			await Task.Run(async() =>
			{
				Console.WriteLine("Initial Open Attempt");
				try
				{
					conn.Open();
				}
				catch (Exception ex)
				{
					Console.WriteLine("Failed to Open: " + ex.Message);
					IsProgressVisible = false;
					return;
                }

				App.Current.Hud.Dismiss();
				App.Current.Hud.Show("Getting Printer Instance");
				Console.WriteLine("Get Printer Instance");
				try
				{
					var printer = ZebraPrinterFactory.GetInstance(conn);
					var linkOsPrinter = ZebraPrinterFactory.CreateLinkOsPrinter(printer);
					
					var settings = linkOsPrinter.GetAllSettings();

					if (settings.ContainsKey("device.friendly_name"))
						SelectedPrinterName = settings["device.friendly_name"].Value;
					if (settings.ContainsKey("bluetooth.address"))
						SelectedPrinterAddress = settings["bluetooth.address"].Value;
					if (settings.ContainsKey("device.company_name"))
						SelectedPrinterManufacturer = settings["device.company_name"].Value;
					if (settings.ContainsKey("device.product_name"))
						SelectedPrinterModel = settings["device.product_name"].Value;
					if (settings.ContainsKey("appl.name"))
						SelectedPrinterFirmware = settings["appl.name"].Value;
					// Determine Battery Status
					if (settings.ContainsKey("power.percent_full"))
						SelectedPrinterBatteryPercent = settings["power.percent_full"].Value;
					if (settings.ContainsKey("power.status"))
						SelectedPrinterBatteryStatus = settings["power.status"].Value;
					if (settings.ContainsKey("power.health"))
						SelectedPrinterBatteryHealth = settings["power.health"].Value;


					IsPrinterBatteryOperated = SelectedPrinterBatteryPercent != null;
					if (IsPrinterBatteryOperated)
					{
						SelectedPrinterBatteryStatus = "Full Power (AC)";
					}
				}
				catch(Exception ex)
                {
					App.Current.Hud.Dismiss();
					return;
                }
				finally
				{

				}
				App.Current.Hud.Dismiss();
				App.Current.Hud.Show("Sending Commands To Printer");
				Console.WriteLine("Prep Printer Commands");
                try
                {
					var commands = new List<String>();

					// ~CT~ change control command prefix to '~'
					// ~CD, change command delimiter to ','
					// ~CC^ change format command prefix to '^'
					commands.Add("~CT~~CD,~CC^");
					// show printer name on front panel line 2
					//commands.Add($"! U1 setvar \"device.frontpanel.line2\" \"   {SelectedPrinter.PrinterName}\"");
					// show printer bluetooth mac addr on front panel line 4
					//commands.Add($"! U1 setvar \"device.frontpanel.line4\" \" {SelectedPrinter.PrinterAddress}\"");
					// ^MF,C,C re-calibrate when printer powered on or head closed
					// ~JC peforms a recalibration right now
					commands.Add("^XA^MFC,C~JC^JUS^XZ");

					// terminate commands with CR-LF
					commands.Add("");
					var buffer = string.Join("\r\n", commands.ToArray());
					conn.Write(Encoding.UTF8.GetBytes(buffer));
					conn.Close();
					CurrentPrinterName = SelectedPrinterName;
					CurrentPrinterAddress = SelectedPrinterAddress;

					// Force a tiny delay for confirmation
					await Task.Delay(500);
				}
				catch(Exception ex)
                {
					Console.WriteLine("Message is: " + ex.Message);
					Console.WriteLine("Stack is: " + ex.StackTrace);
					Console.WriteLine("Inner Ex is: " + ex.InnerException);
                }
                finally
                {
					SelectedPrinter = null;
					IsProgressVisible = false;
					conn.Close();
				}

				if (conn != null && conn.Connected)
					conn.Close();
				IsProgressVisible = false;
				App.Current.Hud.Dismiss();
			});


		}
		public Task CheckPrinterStatus()
        {
			Connection conn = null;
			return Task.Run(() =>
			{
                try
                {
                    if (string.IsNullOrWhiteSpace(SelectedPrinterName))
                    {
						Console.WriteLine("Printer is not paired - cancel printing");
						return;
                    }
					App.Current.Hud.Show("Checking Printer Status...");

					connManager = DependencyService.Get<IZebraPrinterConnectionManager>();
					conn = connManager.GetBluetoothConnection(SelectedPrinterAddress);
					conn.Open();
					var printer = ZebraPrinterFactory.GetInstance(conn);
					var linkOsPrinter = ZebraPrinterFactory.CreateLinkOsPrinter(printer);
					var status = linkOsPrinter.GetCurrentStatus();
					if (!status.isReadyToPrint)
					{
						var psm = new PrinterStatusMessages(status);
						App.Current.Hud.Dismiss();
						App.Current.Hud.Toast($"Printer is not ready",2);
						return;
					}

				}
				catch (ConnectionException connEx)
				{
					Console.WriteLine(connEx.Message);
					if (conn != null)
						conn.Close();
					return;
				}
				catch (Exception ex)
				{
					Console.WriteLine(ex.Message);
					if (conn != null)
						conn.Close();
					return;
				}
				finally
				{
					CanPrint = true;
					if (conn != null && conn.Connected)
						conn.Close();
				}

				App.Current.Hud.Dismiss();
			});
        }
		public Task PrintAllItems()
        {
			return Task.Run(async () =>
			{
                try
                {
					connManager = DependencyService.Get<IZebraPrinterConnectionManager>();
					var conn = connManager.GetBluetoothConnection(SelectedPrinterAddress);
					conn.Open();
					App.Current.Hud.Show(PrintingStatus);
					TotalItems = 0;
					foreach (var it in AvailableItems)
						TotalItems += it.Quantity;
					foreach(var i in AvailableItems)
						PrintItemTag(conn, i);

					while (true)
					{
						var status = PrinterUtil.GetCurrentStatus(conn, PrinterLanguage.ZPL);
						System.Diagnostics.Debug.WriteLine("labelsRemainingInBatch:{0}, isReceiveBufferFull:{1}, numberOfFormatsInReceiveBuffer:{2}, isReadyToPrint:{3}",
							status.labelsRemainingInBatch,
							status.isReceiveBufferFull,
							status.numberOfFormatsInReceiveBuffer,
							status.isReadyToPrint);
						if (status.numberOfFormatsInReceiveBuffer > 0)
						{
							App.Current.Hud.Show($"{status.numberOfFormatsInReceiveBuffer} formats in receive buffer");
							System.Diagnostics.Debug.WriteLine("printing not complete, numberOfFormatsInReceiveBuffer:{0}", status.numberOfFormatsInReceiveBuffer);
							await Task.Delay(500);
						}
						else
						{
							System.Diagnostics.Debug.WriteLine("printer receive buffer empty");
							break;
						}
					}
					App.Current.Hud.Show("closing printer");
					System.Diagnostics.Debug.WriteLine("closing printer");
					conn.Close();
					await Task.Delay(1000);
				}
				catch (ConnectionException connEx)
				{
					Console.WriteLine(connEx.Message);
					return;
				}
				catch (Exception ex)
				{
					Console.WriteLine(ex.Message);
					return;
				}
				finally
				{
					App.Current.Hud.Dismiss();
				}
			});
        }

		public Command SearchForPrintersCommand => new Command(async () =>
		{
			connManager = DependencyService.Get<IZebraPrinterConnectionManager>();
			try
            {
				IsProgressVisible = true;
				SelectedPrinter = null;
				AllPrinters = new ObservableCollection<BluetoothPrinter>();
				var handler = new PrinterDiscoveryHandlerImplementation(this);
				connManager.FindBluetoothPrinters(handler);
			}
			catch(Exception ex)
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
            finally
            {
				if (AllPrinters.Count == 0)
				{
					Console.WriteLine("Printer Count is 0");
				}
				else
				{
					Console.WriteLine("At least 1 printer");
				}
			}
		});
		public Command PrintAllItemsCommand => new Command(async () =>
		{


            try
            {
				await Task.WhenAll(CheckPrinterStatus());

				if (!CanPrint)
					Console.WriteLine("Printing is not available");
                else
                {
					await PrintAllItems();
                }
            }
            finally
            {

            }
		});

    }


	public class BluetoothPrinter
	{
		public string PrinterName { get; set; }
		public string PrinterAddress { get; set; }
	}

	class PrinterDiscoveryHandlerImplementation : DiscoveryHandler
	{
		MainPageViewModel _vm;
		public PrinterDiscoveryHandlerImplementation(MainPageViewModel vm)
		{
			_vm = vm;
		}


		public void DiscoveryError(string message)
		{
			Console.WriteLine("Failed to discover printer");
			Console.WriteLine("message");
			_vm.IsProgressVisible = false;
		}

		public void DiscoveryFinished()
		{
			Console.WriteLine("Discovery Has Finished");
			_vm.IsProgressVisible = false;
		}

		public void FoundPrinter(DiscoveredPrinter printer)
		{
			_vm.ArePrintersAvailable = true;
			var ptr = new BluetoothPrinter
			{
				PrinterAddress = printer.Address,
				PrinterName = printer.DiscoveryDataMap["FRIENDLY_NAME"]
			};
			Console.WriteLine("Printer is: " + ptr.PrinterName);
			_vm.AllPrinters.Add(ptr);
		}
	}
}
