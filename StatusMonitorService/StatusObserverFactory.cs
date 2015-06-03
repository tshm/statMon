using Microsoft.Win32;
using NLog;
using System;
using System.Net.NetworkInformation;
using System.Reactive.Linq;
using System.Windows.Forms;

namespace StatusMonitor
{
    static class StatusObserverFactory
    {
        static public IObservable<bool> GetPowerStatusObserver() {
            var obs = Observable.FromEvent<PowerModeChangedEventHandler, PowerModeChangedEventArgs>(
                evc => (sender, evt) => evc(evt),
                h => SystemEvents.PowerModeChanged += h,
                h => SystemEvents.PowerModeChanged -= h
            ).Where(evt => evt.Mode == PowerModes.StatusChange)
            .Select(
              _ => SystemInformation.PowerStatus.PowerLineStatus == PowerLineStatus.Online
            )
            .StartWith(true);
            obs.Subscribe(e => LogManager.GetCurrentClassLogger().Trace("power : {0}", e));

            LogManager.GetCurrentClassLogger().Trace("Subscribe to power event");
            return obs;
        }

        static public IObservable<bool> GetNetworkStatusObserver() {
            var obs = Observable.FromEvent<NetworkAvailabilityChangedEventHandler, NetworkAvailabilityEventArgs>(
                evc => (sender, evt) => evc(evt),
                h => NetworkChange.NetworkAvailabilityChanged += h,
                h => NetworkChange.NetworkAvailabilityChanged -= h
            )
            .Select(evt => evt.IsAvailable)
            .StartWith(true);
            obs.Subscribe(e => LogManager.GetCurrentClassLogger().Trace("network : {0}", e));

            LogManager.GetCurrentClassLogger().Trace("Subscribe to network event");
            return obs;
        }
    }
}
