using NAudio.CoreAudioApi;
using NLog;
using System;
using System.Reactive.Linq;
using System.Runtime.InteropServices;
using System.ServiceProcess;

namespace StatusMonitor
{
    internal class Alarm
    {
        private System.Media.SoundPlayer player;

        public Alarm()
        {
            player = new System.Media.SoundPlayer(Properties.Resources.alarm);
        }

        public void Play()
        {
            player.PlayLooping();
        }

        public void Stop()
        {
            player.Stop();
        }
    }

    internal class Program
    {
        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool LockWorkStation();

        private static void SetMaxVolume()
        {
            try
            {
                (new MMDeviceEnumerator()).GetDefaultAudioEndpoint((DataFlow)0, (Role)1)
                    .AudioEndpointVolume.MasterVolumeLevelScalar = 1.0f;
            }
            catch (Exception)
            {
            }
        }

        /// <summary>
        /// Main Entry point
        /// </summary>
        private static void Main()
        {
            if (!Environment.UserInteractive)
            {
                ServiceBase[] ServicesToRun;
                ServicesToRun = new ServiceBase[]
                {
                    new StatusMonitorService()
                };
                ServiceBase.Run(ServicesToRun);
            }
            else
            {
                var p = new Program();
                p.Start();
                Console.WriteLine("press any key to stop.");
                Console.ReadKey(true);
                Console.WriteLine("terminating...");
                p.Stop();
            }
        }

        public void Start()
        {
            var player = new Alarm();
            //player.Play();
            LogManager.GetCurrentClassLogger().Info("starting to monitor.");

            var timedObserver = Observable.Interval(TimeSpan.FromSeconds(50));
            var netObserver = StatusObserverFactory.GetNetworkStatusObserver();
            var powObserver = StatusObserverFactory.GetPowerStatusObserver();

            timedObserver.CombineLatest(powObserver, netObserver, (i, pow, net) => !(pow || net))
                //.DistinctUntilChanged()
            .Subscribe(s => React(player, s),
                err => LogManager.GetCurrentClassLogger().Error("Error: {0}", err));
        }

        public void React(Alarm player, bool status)
        {
            //SetMaxVolume();
            if (status) player.Play();
            if (!status) player.Stop();
            LogManager.GetCurrentClassLogger().Info("triggerd : {0}", status);
        }

        public void Stop()
        {
            LogManager.GetCurrentClassLogger().Info("terminating monitor.");
        }
    }
}