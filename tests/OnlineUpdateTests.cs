using System;
using System.IO;
using System.Net;
using System.Reflection;
using Velopack;
using Velopack.Locators;
class OnlineUpdateTests {
    static int Main() {
        try {
            string cache=Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"online-update-test-"+Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(cache);
            ServicePointManager.SecurityProtocol=SecurityProtocolType.Tls;
            var app=Assembly.LoadFrom(Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"AirPods.exe"));
            var factory=app.GetType("InstalledUpdates").GetMethod("CreateManager");
            var manager=(UpdateManager)factory.Invoke(null,new object[] {new TestVelopackLocator("AirPodsBatteryDesktop","0.0.1",cache)});
            if(ServicePointManager.SecurityProtocol!=SecurityProtocolType.Tls12)throw new Exception("Legacy TLS defaults not replaced");
            var update=manager.CheckForUpdatesAsync().GetAwaiter().GetResult();
            if(update==null)throw new Exception("No published installer update found");
            manager.DownloadUpdatesAsync(update).GetAwaiter().GetResult();
            Console.WriteLine("Live GitHub check and verified download passed: "+update.TargetFullRelease.Version);
            return 0;
        } catch(Exception e) {Console.Error.WriteLine(e);return 1;}
    }
}
