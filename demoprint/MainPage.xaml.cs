using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using demoprint.Viewmodels;
using Xamarin.Forms;

namespace demoprint
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
            BindingContext = new MainPageViewModel();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            if(BindingContext is MainPageViewModel vm)
            {
                //vm.BuildTagSet();
            }
        }

        async void CollectionView_SelectionChanged(System.Object sender, Xamarin.Forms.SelectionChangedEventArgs e)
        {
            Console.WriteLine("Selection Changed");
            if(sender is CollectionView cv)
            {
                BluetoothPrinter selected = cv.SelectedItem as BluetoothPrinter;
                Console.WriteLine($"Printer Is: {selected.PrinterName} / {selected.PrinterAddress}");

                if(BindingContext is MainPageViewModel vm)
                {
                    await vm.ReadSelectedPrinterInfo(selected);
                }
            }
        }
    }
}
