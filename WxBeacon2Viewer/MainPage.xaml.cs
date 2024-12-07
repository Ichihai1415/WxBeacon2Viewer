using System;
using System.Threading.Tasks;
using Weathernews.Sensor;
using Windows.Foundation;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace WxBeacon2Viewer
{
    public sealed partial class MainPage : Page
    {
        private WxBeacon2Watcher wxBeacon2Watcher = new WxBeacon2Watcher();

        public MainPage()
        {
            InitializeComponent();
            wxBeacon2Watcher.Received += WxBeacon2Watcher_Found;
            ApplicationView.PreferredLaunchWindowingMode = ApplicationViewWindowingMode.PreferredLaunchViewSize;
            ApplicationView.PreferredLaunchViewSize = new Size { Width = 600, Height = 320 };
            Timer.Interval = TimeSpan.FromMinutes(10);
            Timer.Tick += Timer_Tick;
            Timer.Start();
        }

        private async void WxBeacon2Watcher_Found(object sender, WxBeacon2 beacon)
        {
            try
            {
                var latest = await beacon.GetLatestDataAsync() ?? throw new Exception("Get failed.");
                var getTime = DateTime.Now;//本来の取得時間できないかな
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    T_info.Text = "";
                    T_dateTime.Text = "WxBeacon2       " + getTime.ToString();
                    T_tem.Text = latest.Temperature.ToString("F1");
                    T_hum.Text = latest.Humidity.ToString("F1");
                    T_pre.Text = latest.Pressure.ToString("F1");
                    T_bat.Text = latest.BatteryVoltage.ToString("F2");
                });
                await LogHelper.CreateLogAsync(latest.ToString(), getTime);

                beacon.Dispose();
                wxBeacon2Watcher.Dispose();//受信時切断
                wxBeacon2Watcher = null;
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    T_info.Text = "disconnected";
                });
            }
            catch (Exception ex)
            {
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    T_info.Text = "get failed! -> " + ex.Message;
                });
            }
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            T_info.Text = "searching...";
            /*bool result = */
            ApplicationView.GetForCurrentView().TryResizeView(new Size { Width = 600, Height = 320 });
            wxBeacon2Watcher.Start();
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            T_info.Text = "exiting...";
            wxBeacon2Watcher.Stop();
        }

        private readonly DispatcherTimer Timer = new DispatcherTimer();

        private async void Timer_Tick(object sender, object e)
        {
            GC.Collect();//これないと上手くいかない？
            if (wxBeacon2Watcher == null)
            {
                wxBeacon2Watcher = new WxBeacon2Watcher();
                wxBeacon2Watcher.Received += WxBeacon2Watcher_Found;
            }
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                T_info.Text = "searching...";
            });
            wxBeacon2Watcher.Start();
            //wxBeacon2Watcher.Stop();//受信時切断するため
        }

        private void C_AutoGet_Checked(object sender, RoutedEventArgs e)
        {
            Timer.Start();
        }

        private void C_AutoGet_Unchecked(object sender, RoutedEventArgs e)
        {
            Timer.Stop();
        }
    }

    public class LogHelper
    {
        public static async Task CreateLogAsync(string content, DateTime dirDateTime)
        {
            var localFolder = ApplicationData.Current.LocalFolder;//C:\Users\[user name]\AppData\Local\Packages\...\LocalState\
            await localFolder.CreateFileAsync("WxBeacon2Viewer_Log", CreationCollisionOption.ReplaceExisting);//ファイル名WxBeacon2Viewer_Logで検索すれば出る
            var logFolder = await localFolder.CreateFolderAsync("log", CreationCollisionOption.OpenIfExists);
            var monthFolder = await logFolder.CreateFolderAsync(dirDateTime.ToString("yyyyMM"), CreationCollisionOption.OpenIfExists);
            var dayFolder = await monthFolder.CreateFolderAsync(dirDateTime.ToString("dd"), CreationCollisionOption.OpenIfExists);
            var logFile = await dayFolder.CreateFileAsync($"{dirDateTime:yyyyMMddHHmmss}.txt", CreationCollisionOption.ReplaceExisting);
            await FileIO.WriteTextAsync(logFile, content);
        }
    }
}
