using System;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Velopack;
using Velopack.Sources;

internal static class InstalledUpdates {
    public static UpdateManager CreateManager() {
        // Public repository, HTTPS, no credentials. Preview releases are included.
        return new UpdateManager(new GithubSource("https://github.com/elnurm/airpods-battery-windows",null,true),new UpdateOptions {ExplicitChannel="win",AllowVersionDowngrade=false});
    }
    public static bool IsInstalled {get {var manager=CreateManager();return manager.IsInstalled && !manager.IsPortable;}}
    public static Version VersionOf(UpdateInfo info) {
        var v=info.TargetFullRelease.Version;return new Version(v.Major,v.Minor,v.Patch,0);
    }
    public static async Task Download(UpdateManager manager,UpdateInfo update,Action<int> progress,CancellationToken cancellation) {
        cancellation.ThrowIfCancellationRequested();
        await manager.DownloadUpdatesAsync(update,progress,cancellation);
        cancellation.ThrowIfCancellationRequested();
    }
}

internal sealed class UpdateDownloadWindow : Form {
    readonly ProgressBar progress=new ProgressBar();
    readonly Label message=new Label();
    readonly Button cancel=new Button();
    readonly CancellationTokenSource cancellation=new CancellationTokenSource();
    bool complete;
    public UpdateDownloadWindow() {
        Text=Language.T("Downloading update");ClientSize=new Size(380,125);StartPosition=FormStartPosition.CenterParent;
        FormBorderStyle=FormBorderStyle.FixedDialog;MinimizeBox=false;MaximizeBox=false;
        Font=new Font("Segoe UI",9);BackColor=Artwork.Background;
        message.SetBounds(16,12,348,24);message.Text=Language.T("Downloading update");Controls.Add(message);
        progress.SetBounds(16,42,348,20);Controls.Add(progress);
        cancel.Text=Language.T("Cancel");cancel.SetBounds(244,80,120,28);Controls.Add(cancel);
        cancel.Click+=delegate {CancelDownload();};
        FormClosing+=delegate(object sender,FormClosingEventArgs e) {if(!complete) {e.Cancel=true;CancelDownload();}};
    }
    void CancelDownload() {cancellation.Cancel();cancel.Enabled=false;message.Text=Language.T("Cancelling…");}
    public async Task<bool> Download(UpdateManager manager,UpdateInfo update,Form owner) {
        Show(owner);
        try {
            await InstalledUpdates.Download(manager,update,delegate(int value) {
                if(IsDisposed || !IsHandleCreated)return;
                try {BeginInvoke((Action)delegate {if(!IsDisposed)progress.Value=Math.Max(0,Math.Min(100,value));});} catch(InvalidOperationException) {}
            },cancellation.Token);
            cancellation.Token.ThrowIfCancellationRequested();
            return true;
        } catch(OperationCanceledException) {return false;}
        finally {complete=true;Close();}
    }
    protected override void Dispose(bool disposing) {if(disposing)cancellation.Dispose();base.Dispose(disposing);}
}
