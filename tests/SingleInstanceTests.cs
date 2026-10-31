using System;
using System.Diagnostics;
using System.Threading;

internal static class SingleInstanceTests {
    static int Main(string[] args) {
        if(args.Length>0) {
            using(var second=new SingleInstance(args[0])) {
                if(second.IsPrimary)return 2;
                second.RequestActivation();return 0;
            }
        }
        string id="AirPodsBatteryTests."+Guid.NewGuid().ToString("N");
        using(var first=new SingleInstance(id)) {
            if(!first.IsPrimary)return 3;
            for(int i=0;i<2;i++) {
                using(var process=Process.Start(new ProcessStartInfo(typeof(SingleInstanceTests).Assembly.Location,id) {UseShellExecute=false,CreateNoWindow=true})) {
                    if(!process.WaitForExit(5000)) {process.Kill();return 4;}
                    if(process.ExitCode!=0)return 5;
                }
                if(!first.TakeActivation() || first.TakeActivation())return 6;
            }
        }
        using(var reopened=new SingleInstance(id))if(!reopened.IsPrimary)return 7;
        Console.WriteLine("Single-instance activation and reopening tests passed.");return 0;
    }
}
