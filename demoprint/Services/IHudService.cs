using System;
namespace demoprint.Services
{
    public interface IHudService
    {
        void Dismiss();
        void SetStatus(string message);
        void Show(string message);
        void Toast(string message, int seconds = 2);

        bool IsVisible();
    }
}
