using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace StatusMonitor
{
    public partial class StatusMonitorService : ServiceBase
    {
        Program p;
        public StatusMonitorService()
        {
            InitializeComponent();
            this.CanStop = true;
        }

        protected override void OnStart(string[] args)
        {
            p = new Program();
            p.Start();
        }

        protected override void OnStop()
        {
            p.Stop();
        }
    }
}
