using System;
using System.IO;
using System.Threading;
using Velopack;
using Velopack.Locators;
using Velopack.Sources;

internal static class InstalledUpdateTests {
    static UpdateManager Manager(string feed,string cache,string version) {
        Directory.CreateDirectory(cache);
        return new UpdateManager(new SimpleFileSource(new DirectoryInfo(feed)),new UpdateOptions {ExplicitChannel="win",AllowVersionDowngrade=false},new TestVelopackLocator("AirPodsBatteryDesktop",version,cache));
    }
    static int Main(string[] args) {
        try {
            string feed=Path.GetFullPath(args[0]),root=Path.GetFullPath(args[1]);
            Directory.CreateDirectory(root);
            var manager=Manager(feed,Path.Combine(root,"valid"),"0.0.1");
            var update=manager.CheckForUpdatesAsync().GetAwaiter().GetResult();
            if(update==null || update.TargetFullRelease.Version.ToString()!=args[2])throw new Exception("New version not detected");
            manager.DownloadUpdatesAsync(update).GetAwaiter().GetResult();
            if(!File.Exists(Path.Combine(root,"valid",update.TargetFullRelease.FileName)))throw new Exception("Package not downloaded");
            if(Manager(feed,Path.Combine(root,"same"),args[2]).CheckForUpdatesAsync().GetAwaiter().GetResult()!=null)throw new Exception("Same version offered");
            if(Manager(feed,Path.Combine(root,"newer"),"9999.0.0").CheckForUpdatesAsync().GetAwaiter().GetResult()!=null)throw new Exception("Downgrade offered");
            string corrupt=Path.Combine(root,"corrupt-feed");Directory.CreateDirectory(corrupt);
            File.Copy(Path.Combine(feed,"releases.win.json"),Path.Combine(corrupt,"releases.win.json"),true);
            File.WriteAllText(Path.Combine(corrupt,update.TargetFullRelease.FileName),"corrupted download");
            var broken=Manager(corrupt,Path.Combine(root,"broken"),"0.0.1");
            bool rejected=false;
            try {broken.DownloadUpdatesAsync(broken.CheckForUpdatesAsync().GetAwaiter().GetResult()).GetAwaiter().GetResult();} catch {rejected=true;}
            if(!rejected)throw new Exception("Corrupt package accepted");
            var cancelled=Manager(feed,Path.Combine(root,"cancelled"),"0.0.1");
            bool stopped=false;
            var app=System.Reflection.Assembly.LoadFrom(Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"AirPods.exe"));
            var download=app.GetType("InstalledUpdates").GetMethod("Download");
            try {((System.Threading.Tasks.Task)download.Invoke(null,new object[] {cancelled,cancelled.CheckForUpdatesAsync().GetAwaiter().GetResult(),null,new CancellationToken(true)})).GetAwaiter().GetResult();}
            catch(OperationCanceledException){stopped=true;}
            if(!stopped)throw new Exception("Cancelled download was not stopped");
            Console.WriteLine("Installer update tests passed: version detection, download/checksum, equal version, downgrade and corrupt-package rejection.");return 0;
        } catch(Exception e) {Console.Error.WriteLine(e);return 1;}
    }
}
