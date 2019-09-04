using System;
using System.Collections.Generic;
using System.Data;
using System.Timers;
using DBSizeChecker.ConfigModel;
using DBSizeChecker.DB;
using DBSizeChecker.GoogleDocs;

namespace DBSizeChecker
{
    internal class Program
    {
        private static Configuration _cfg;
        private static GoogleClient _google;

        private static Timer _timer;

        private static void Main(string[] args)
        {
            var currentPath = Environment.CurrentDirectory;
            var file = currentPath + "/settings.json";
            try
                {
                    _cfg = Configuration.LoadFromPath(file);
                } catch(DataException e)
                {
                    Console.WriteLine(e);
                    Console.WriteLine("Cant countinue!!!");
                    WaitLoop();
                }

            _google = new GoogleClient(_cfg.Google);

            _timer = new Timer();
            _timer.Interval = _cfg.RetryInterval * 1000;
            _timer.AutoReset = false;
            _timer.Enabled = true;
            _timer.Elapsed += Timer_Elapsed;
            _timer.Start();
            WaitLoop();
        }

        private static void WaitLoop()
        {
            Console.WriteLine("Press SPACE to exit...");

            while(Console.ReadKey().Key != ConsoleKey.Spacebar) { }
        }

        private static void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            _timer.Stop();
            try
                {
                    Update();
                    
                } catch(Exception exception)
                {
                    Console.WriteLine($"Possible one ore more network errors... Retry through {_cfg.RetryInterval*1000} seconds");
                   
                }
            _timer.Start();
        }

        private static void Update()
        {
            Console.WriteLine("Try to update data");
            var hosts = new List<HostInfo>();
            foreach(var host in _cfg.Hosts)
                {
                    var client = new DBClient(host.Connection);
                    var dbList = client.GetInfo();
                    hosts.Add(new HostInfo(host.ServerID, dbList, host.DiskSpace));
                }

            _google.Update(hosts);
        }
    }
}
