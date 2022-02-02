using System;
using demoprint.Services;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace demoprint
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            MainPage = new NavigationPage(new MainPage());
        }

        protected override void OnStart()
        {
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }

        // convenience properties...
        public static new App Current
        {
            get
            {
                return (App)Application.Current;
            }
        }
        IHudService _hud;
        public IHudService Hud
        {
            get
            {
                if (_hud == null)
                    _hud = DependencyService.Get<IHudService>();
                return _hud;
            }
        }
    }
}
