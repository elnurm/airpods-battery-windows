using System;
using System.IO;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

internal sealed class ReleaseInfo {
    public string tag_name {get;set;}
    public bool draft {get;set;}
    public ReleaseAsset[] assets {get;set;}
}
internal sealed class ReleaseAsset {
    public string name {get;set;}
    public string state {get;set;}
}
internal static class UpdateChecker {
    public const string ReleasesUrl="https://github.com/elnurm/airpods-battery-windows/releases";
    static readonly string statePath=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"AirPodsBattery","notified-version.txt");
    static Version notified=LoadNotified();
    public static Version Current {get {return Assembly.GetExecutingAssembly().GetName().Version;}}
    sealed class Client : WebClient {
        protected override WebRequest GetWebRequest(Uri address) {
            var request=base.GetWebRequest(address);request.Timeout=15000;
            var http=request as HttpWebRequest;
            if(http!=null) {http.ReadWriteTimeout=15000;http.AllowAutoRedirect=false;}
            return request;
        }
    }
    public static async Task<Version> Check() {
        ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;
        using(var client=new Client()) {
            client.Headers[HttpRequestHeader.UserAgent]="AirPodsBatteryWindows/"+Current;
            client.Headers[HttpRequestHeader.Accept]="application/vnd.github+json";
            var download=client.DownloadStringTaskAsync("https://api.github.com/repos/elnurm/airpods-battery-windows/releases?per_page=100");
            if(await Task.WhenAny(download,Task.Delay(15000))!=download) {
                client.CancelAsync();
                try {await download;} catch(WebException) {} catch(TaskCanceledException) {}
                throw new TimeoutException("Update check timed out");
            }
            var json=await download;
            return FindNewer(json,Current);
        }
    }
    public static Version FindNewer(string json,Version current) {
        var releases=new JavaScriptSerializer {MaxJsonLength=4194304}.Deserialize<ReleaseInfo[]>(json);
        Version best=null;
        foreach(var release in releases ?? new ReleaseInfo[0]) {
            Version version;
            if(release==null || release.draft || !TryVersion(release.tag_name,out version) || version<=current)continue;
            bool downloadable=false;
            foreach(var asset in release.assets ?? new ReleaseAsset[0])
                if(asset!=null && asset.state=="uploaded" && asset.name=="airpods-battery-windows-"+version.ToString(3)+"-win-x64.zip")downloadable=true;
            if(downloadable && (best==null || version>best))best=version;
        }
        return best;
    }
    static bool TryVersion(string text,out Version version) {
        version=null;Version parsed;
        if(string.IsNullOrEmpty(text) || !Version.TryParse(text.TrimStart('v'),out parsed) || parsed.Build<0 || parsed.Revision>0)return false;
        version=new Version(parsed.Major,parsed.Minor,parsed.Build,0);return true;
    }
    public static string Url(Version version) {return ReleasesUrl+"/tag/v"+version.ToString(3);}
    public static bool ShouldNotify(Version version) {return notified==null || version>notified;}
    public static void MarkNotified(Version version) {
        if(!ShouldNotify(version))return;
        notified=version;
        try {Directory.CreateDirectory(Path.GetDirectoryName(statePath));File.WriteAllText(statePath,version.ToString());} catch(IOException) {} catch(UnauthorizedAccessException) {}
    }
    static Version LoadNotified() {
        try {Version version;return TryVersion(File.ReadAllText(statePath).Trim(),out version)?version:null;} catch(IOException) {return null;} catch(UnauthorizedAccessException) {return null;}
    }
    public static void Test() {
        string asset="\"assets\":[{\"name\":\"airpods-battery-windows-0.3.0-win-x64.zip\",\"state\":\"uploaded\"}]";
        var current=new Version(0,2,3,0);
        if(FindNewer("[{\"tag_name\":\"v0.3.0\",\"prerelease\":true,"+asset+"}]",current)!=new Version(0,3,0,0))throw new Exception("Preview update missing");
        foreach(string entry in new [] {"{\"tag_name\":\"v0.3.0\",\"draft\":true,"+asset+"}","{\"tag_name\":\"bad\","+asset+"}","{\"tag_name\":\"v0.3.0\"}","{\"tag_name\":\"v0.2.3\","+asset+"}"})
            if(FindNewer("["+entry+"]",current)!=null)throw new Exception("Invalid update accepted");
        if(FindNewer("[]",current)!=null)throw new Exception("Empty releases failed");
        string release="{\"tag_name\":\"v0.3.0\","+asset+"}";
        if(FindNewer("["+release+"]",new Version(0,3,0,0))!=null || FindNewer("["+release+"]",new Version(0,4,0,0))!=null)throw new Exception("Equal or older release accepted");
        if(FindNewer("["+release+","+release.Replace("0.3.0","0.10.0")+"]",current)!=new Version(0,10,0,0))throw new Exception("Numeric version ordering failed");
        if(FindNewer("["+release.Replace("uploaded","new")+"]",current)!=null)throw new Exception("Incomplete upload accepted");
        var saved=notified;
        try {
            notified=new Version(0,3,0,0);
            if(ShouldNotify(new Version(0,3,0,0)) || ShouldNotify(current) || !ShouldNotify(new Version(0,4,0,0)))throw new Exception("Duplicate notification suppression failed");
        } finally {notified=saved;}
    }
}
