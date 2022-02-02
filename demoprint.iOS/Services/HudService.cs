using System;
using BigTed;
using demoprint.iOS.Services;
using demoprint.Services;
using Xamarin.Forms;

[assembly: Dependency(typeof(HudService))]
namespace demoprint.iOS.Services
{
    public class HudService : IHudService
    {
        public HudService()
        {
            
        }

        public void Dismiss()
        {
            BTProgressHUD.Dismiss();
        }

        public bool IsVisible()
        {
            if (BTProgressHUD.IsVisible)
                return true;

            return false;
        }

        public void SetStatus(string message)
        {
            BTProgressHUD.SetStatus(message);
        }

        public void Show(string message)
        {
            BTProgressHUD.Show(message, maskType: MaskType.Gradient);
        }

        public void Toast(string message, int seconds = 2)
        {
            BTProgressHUD.ShowToast(message, showToastCentered: true, timeoutMs: TimeSpan.FromSeconds(seconds).TotalMilliseconds);
        }
    }
}
