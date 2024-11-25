using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Weathernews.Sensor;
using Windows.UI.Core;

namespace WxBeacon2Viewer
{
    public sealed partial class MainPage : Page
    {
        private WxBeacon2Watcher wxBeacon2Watcher = new WxBeacon2Watcher();

        public MainPage()
        {
            InitializeComponent();
            wxBeacon2Watcher.Received += WxBeacon2Watcher_Found;
        }

        private async void WxBeacon2Watcher_Found(object sender, WxBeacon2 beacon)
        {
            var latest = await beacon.GetLatestDataAsync();
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                textBlock.Text = latest.ToString();
                beacon.Dispose();
            });
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            textBlock.Text = "searching...";
            wxBeacon2Watcher.Start();
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            textBlock.Text = "exiting...";
            wxBeacon2Watcher.Stop();
        }
    }
}
