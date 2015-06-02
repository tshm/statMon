using NAudio.CoreAudioApi;
using NLog;
using System;
using System.Reactive.Linq;
using System.Runtime.InteropServices;
using System.ServiceProcess;

namespace WindowsService1
{
    class Alarm
    {
        System.Media.SoundPlayer player;

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

    class Program
    {
        [DllImport("user32.dll", SetLastError = true)]
        static extern bool LockWorkStation();

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
        static void Main()
        {
            if (!Environment.UserInteractive)
            {
                ServiceBase[] ServicesToRun;
                ServicesToRun = new ServiceBase[] 
                { 
                    new Service1() 
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
            LogManager.GetCurrentClassLogger().Info("starting to monitor.");

            var timedObserver = Observable.Interval(TimeSpan.FromSeconds(5));
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
