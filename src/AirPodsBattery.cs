using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Windows.Devices.Bluetooth.Advertisement;
using Windows.Storage.Streams;
using Windows.Devices.Bluetooth;
using Windows.Devices.Enumeration;

internal sealed class BatteryWindow : Form {
    static bool previewMode;
    readonly BluetoothLEAdvertisementWatcher watcher = new BluetoothLEAdvertisementWatcher();
    readonly Dictionary<ulong,Reading> devices = new Dictionary<ulong,Reading>();
    readonly ComboBox picker = new ComboBox();
    readonly ComboBox pairedPicker = new ComboBox();
    readonly ConnectionState connection = new ConnectionState();
    BluetoothDevice pairedDevice;
    int pairingGeneration;
    bool loadingPaired;
    string connectionError="";
    readonly Label status = new Label();
    readonly Label[] values = new Label[2];
    readonly Label[] ages = new Label[2];
    readonly int[] levels = {-1,-1};
    readonly bool[] fresh = new bool[2];
    readonly NotifyIcon tray = new NotifyIcon();
    readonly Timer timer = new Timer();
    readonly bool diagnostic;
    DateTime started = DateTime.UtcNow;
    int packets, apple, cases, pro;
    string error = "";
    bool quitting;
    public BatteryWindow(bool diag) {
        diagnostic=diag;
        Text="AirPods Battery"; ClientSize=new Size(520,572); FormBorderStyle=FormBorderStyle.FixedSingle; MaximizeBox=false;
        AutoScaleMode=AutoScaleMode.Dpi; DoubleBuffered=true; StartPosition=FormStartPosition.CenterScreen;
        BackColor=Artwork.Background; ForeColor=Color.FromArgb(242,245,249); Font=new Font("Segoe UI",10);
        Controls.Add(new Label { Text="YOUR EVERYDAY AUDIO", ForeColor=Artwork.Muted, Font=new Font("Segoe UI",8,FontStyle.Bold), Location=new Point(28,26), AutoSize=true });
        Controls.Add(new Label { Text="AirPods Battery", Font=new Font("Segoe UI",25,FontStyle.Bold), Location=new Point(24,46), AutoSize=true });
        pairedPicker.SetBounds(28,100,464,30); pairedPicker.DropDownStyle=ComboBoxStyle.DropDownList;
        pairedPicker.DisplayMember="Name"; pairedPicker.BackColor=Artwork.Card; pairedPicker.ForeColor=ForeColor;
        pairedPicker.AccessibleName="Windows Bluetooth device to monitor";
        pairedPicker.SelectedIndexChanged+=delegate { BindPairedDevice(); }; Controls.Add(pairedPicker);
        picker.SetBounds(28,140,464,32); picker.DropDownStyle=ComboBoxStyle.DropDownList;
        picker.FlatStyle=FlatStyle.Flat; picker.BackColor=Artwork.Card; picker.ForeColor=ForeColor; picker.AccessibleName="Select your AirPods";
        picker.DrawMode=DrawMode.OwnerDrawFixed; picker.ItemHeight=25;
        picker.DrawItem+=delegate(object sender,DrawItemEventArgs e) {
            using(var b=new SolidBrush(Artwork.Card)) e.Graphics.FillRectangle(b,e.Bounds);
            string text=e.Index<0?"Choose your AirPods":picker.Items[e.Index].ToString();
            TextRenderer.DrawText(e.Graphics,text,Font,new Rectangle(e.Bounds.X+8,e.Bounds.Y,e.Bounds.Width-12,e.Bounds.Height),ForeColor,TextFormatFlags.VerticalCenter|TextFormatFlags.EndEllipsis);
            if((e.State & DrawItemState.Focus)!=0) e.DrawFocusRectangle();
        };
        picker.SelectedIndexChanged += delegate { RefreshReadings(); }; Controls.Add(picker);
        string[] names={"LEFT EAR","RIGHT EAR"};
        for(int i=0;i<2;i++) {
            int x=28+i*240;
            Controls.Add(new Label {Text=names[i], ForeColor=Artwork.Muted, BackColor=Artwork.Card,Font=new Font("Segoe UI",9,FontStyle.Bold),Location=new Point(x+20,210),AutoSize=true});
            values[i]=new Label {Text="—", BackColor=Artwork.Card,Font=new Font("Segoe UI",30,FontStyle.Bold),Location=new Point(x+16,344),Size=new Size(185,60)}; Controls.Add(values[i]);
            ages[i]=new Label {Text="Waiting for a reading",ForeColor=Artwork.Muted,BackColor=Artwork.Card,Font=new Font("Segoe UI",9),Location=new Point(x+20,435),Size=new Size(185,22)}; Controls.Add(ages[i]);
        }
        status.SetBounds(28,484,464,25); status.Font=new Font("Segoe UI",9); status.ForeColor=Artwork.Muted; Controls.Add(status);
        Controls.Add(new Label {Text="Battery estimates · updated as your earbuds report",ForeColor=Artwork.Muted,Font=new Font("Segoe UI",8),Location=new Point(28,536),AutoSize=true});
        var rescan=new Button {Text="Rescan",FlatStyle=FlatStyle.Flat,ForeColor=Artwork.Mint,BackColor=Artwork.Background,Location=new Point(420,526),Size=new Size(72,30),TabIndex=1}; rescan.FlatAppearance.BorderSize=0; rescan.Click+=delegate {Restart();}; Controls.Add(rescan);
        string iconPath=Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"AirPods.ico");
        if(File.Exists(iconPath)) Icon=new Icon(iconPath);
        tray.Icon=Icon; tray.Text="AirPods: waiting for battery data"; tray.Visible=!diag;
        var menu=new ContextMenuStrip(); menu.Items.Add("Show battery",null,delegate { Show(); WindowState=FormWindowState.Normal; Activate(); });
        menu.Items.Add("Restart scan",null,delegate { Restart(); }); menu.Items.Add("Exit",null,delegate { quitting=true; Close(); });
        tray.ContextMenuStrip=menu; tray.DoubleClick += delegate { Show(); Activate(); };
        FormClosing += delegate(object sender,FormClosingEventArgs e) { if(!quitting && !diagnostic && e.CloseReason==CloseReason.UserClosing) { e.Cancel=true; Hide(); } };
        watcher.ScanningMode=BluetoothLEScanningMode.Passive;
        watcher.Received += Received;
        watcher.Stopped += delegate(BluetoothLEAdvertisementWatcher s,BluetoothLEAdvertisementWatcherStoppedEventArgs e) {
            Dispatch(delegate { if(e.Error.ToString()!="Success") error="Bluetooth scan stopped: "+e.Error; });
        };
        timer.Interval=1000; timer.Tick += delegate {
            if(pairedDevice!=null) { try { SetConnected(pairedDevice.ConnectionStatus==BluetoothConnectionStatus.Connected); } catch { SetConnected(null); } }
            RefreshReadings();
            if(diagnostic && (DateTime.UtcNow-started).TotalSeconds>=20) { SaveDiagnostic(); quitting=true; Close(); }
        };
        Shown += delegate { if(previewMode) return; LoadPairedDevices(); Restart(); timer.Start(); if(diagnostic) Hide(); };
        RefreshReadings();
    }
    async void LoadPairedDevices() {
        if(loadingPaired || IsDisposed) return;
        loadingPaired=true;
        try {
            var list=await AwaitOperation(DeviceInformation.FindAllAsync(BluetoothDevice.GetDeviceSelectorFromPairingState(true)));
            if(IsDisposed) return;
            pairedPicker.Items.Clear(); PairedChoice onlyAirPods=null; int matches=0;
            foreach(var d in list) { var item=new PairedChoice {Device=d}; pairedPicker.Items.Add(item); if(d.Name.IndexOf("airpods",StringComparison.OrdinalIgnoreCase)>=0) {onlyAirPods=item;matches++;} }
            connectionError=list.Count==0?"No paired Bluetooth device found. Pair your AirPods in Windows.":"";
            if(matches==1) pairedPicker.SelectedItem=onlyAirPods;
        } catch {connectionError="Unable to check Bluetooth connection. Try Rescan."; SetConnected(null);}
        finally {loadingPaired=false; if(!IsDisposed) RefreshReadings();}
    }
    async void BindPairedDevice() {
        int generation=++pairingGeneration;
        if(pairedDevice!=null) {pairedDevice.ConnectionStatusChanged-=ConnectionChanged; pairedDevice.Dispose(); pairedDevice=null;}
        connection.Reset(); ClearReadings(); RefreshReadings();
        var choice=pairedPicker.SelectedItem as PairedChoice; if(choice==null) return;
        var chosen=choice.Device;
        try {
            var device=await AwaitOperation(BluetoothDevice.FromIdAsync(chosen.Id));
            if(IsDisposed || generation!=pairingGeneration) {if(device!=null) device.Dispose(); return;}
            if(device==null) {connectionError="Cannot read this device's connection. Try Rescan."; SetConnected(null);return;}
            pairedDevice=device; connectionError=""; device.ConnectionStatusChanged+=ConnectionChanged;
            SetConnected(device.ConnectionStatus==BluetoothConnectionStatus.Connected);
        } catch {if(!IsDisposed && generation==pairingGeneration) {connectionError="Connection check unavailable. Try Rescan.";SetConnected(null);}}
    }
    void ConnectionChanged(BluetoothDevice sender,object args) {
        Dispatch(delegate {if(sender==pairedDevice) {try {SetConnected(sender.ConnectionStatus==BluetoothConnectionStatus.Connected);} catch {SetConnected(null);}}});
    }
    static System.Threading.Tasks.Task<T> AwaitOperation<T>(Windows.Foundation.IAsyncOperation<T> operation) {
        var completion=new System.Threading.Tasks.TaskCompletionSource<T>();
        operation.Completed=delegate(Windows.Foundation.IAsyncOperation<T> result,Windows.Foundation.AsyncStatus state) {
            try { if(state==Windows.Foundation.AsyncStatus.Canceled) completion.TrySetCanceled(); else if(state==Windows.Foundation.AsyncStatus.Error) completion.TrySetException(result.ErrorCode); else completion.TrySetResult(result.GetResults()); }
            catch(Exception e) {completion.TrySetException(e);}
            finally {result.Close();}
        };
        return completion.Task;
    }
    void ClearReadings() {
        foreach(var r in devices.Values) for(int i=0;i<3;i++) {r.Level[i]=-1;r.Charging[i]=false;r.Updated[i]=DateTime.MinValue;}
    }
    void SetConnected(bool? connected) {
        bool? previous=connection.Connected;
        bool notify=connection.Update(connected,DateTime.UtcNow);
        if(previous!=connected || connected!=true) ClearReadings();
        if(notify && !diagnostic) tray.ShowBalloonTip(5000,"AirPods disconnected","Your AirPods disconnected from this PC. Battery readings have been cleared.",ToolTipIcon.Info);
        RefreshReadings();
    }
    protected override void OnPaint(PaintEventArgs e) {
        base.OnPaint(e); var g=e.Graphics; g.SmoothingMode=System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        g.ScaleTransform(ClientSize.Width/520f,ClientSize.Height/572f);
        Artwork.Earbud(g,434,30,32,false); Artwork.Earbud(g,462,30,32,true);
        for(int i=0;i<2;i++) {
            int x=28+i*240; Artwork.Box(g,Artwork.Card,x,192,224,280,20);
            Artwork.Earbud(g,x+69,243,88,i==1);
            Artwork.Box(g,Color.FromArgb(49,57,68),x+20,414,184,6,3);
            if(levels[i]>0) Artwork.Box(g,fresh[i]?(levels[i]<=20?Color.FromArgb(255,185,111):Artwork.Mint):Artwork.Muted,x+20,414,Math.Max(6,184*levels[i]/100),6,3);
        }
    }
    void Dispatch(Action action) { if(!IsDisposed && IsHandleCreated) { try { BeginInvoke(action); } catch(InvalidOperationException) {} } }
    void Received(BluetoothLEAdvertisementWatcher sender,BluetoothLEAdvertisementReceivedEventArgs e) {
        var decoded=new List<Reading>(); int apples=0, encrypted=0;
        foreach(var m in e.Advertisement.ManufacturerData) {
            if(m.CompanyId!=76) continue;
            apples++; byte[] b=new byte[m.Data.Length]; using(var reader=DataReader.FromBuffer(m.Data)) reader.ReadBytes(b);
            if(b.Length==19 && b[0]==7 && b[1]==17) encrypted++;
            var r=Decoder.Parse(b); if(r!=null) {r.Address=e.BluetoothAddress; r.Rssi=e.RawSignalStrengthInDBm; decoded.Add(r);}
        }
        Dispatch(delegate {
            packets++; apple+=apples; cases+=encrypted;
            if(!connection.Accept(e.Timestamp.UtcDateTime)) return;
            foreach(var r in decoded) {
                pro++; Reading previous;
                if(!devices.TryGetValue(r.Address,out previous)) {
                    // Never auto-select or merge rotating addresses: identity is not authenticated.
                    if(devices.Count>=100) continue;
                    previous=new Reading {Address=r.Address,ModelId=r.ModelId}; devices.Add(r.Address,previous); picker.Items.Add(previous);
                }
                previous.Rssi=r.Rssi;
                for(int i=0;i<3;i++) if(r.Level[i]>=0) {previous.Level[i]=r.Level[i]; previous.Charging[i]=r.Charging[i]; previous.Updated[i]=DateTime.UtcNow;}
            }
            RefreshReadings();
        });
    }
    void Restart() {
        if(!previewMode && pairedDevice==null && !loadingPaired) { if(pairedPicker.SelectedItem!=null) BindPairedDevice(); else LoadPairedDevices(); }
        try { watcher.Stop(); error=""; watcher.Start(); } catch(Exception e) {error="Unable to scan: "+e.Message;}
        RefreshReadings();
    }
    void RefreshReadings() {
        var r=picker.SelectedItem as Reading;
        if(connection.Connected!=true) r=null;
        string[] tips={"L —","R —"};
        for(int i=0;i<2;i++) {
            levels[i]=r==null?-1:r.Level[i]; fresh[i]=false;
            if(r==null || r.Level[i]<0) {values[i].Text="—"; values[i].ForeColor=ForeColor; ages[i].Text=connection.Connected==false?"Disconnected":connection.Connected==null?"Checking connection":"Waiting for a reading"; continue;}
            int age=(int)(DateTime.UtcNow-r.Updated[i]).TotalSeconds;
            bool stale=age>60;
            fresh[i]=!stale;
            values[i].Text="~"+r.Level[i]+"%";
            values[i].ForeColor=stale?Artwork.Muted:ForeColor;
            ages[i].Text=(r.Charging[i]?"Charging · ":"")+(stale?"Last seen ":"")+(age<60?age+"s":(age/60)+"m")+" ago";
            tips[i]="LRC"[i]+" ~"+r.Level[i]+"%"+(stale?" old":"");
        }
        tray.Text=r==null?"AirPods: select a detected pair":string.Join(" | ",tips);
        status.Text=error.Length>0?error:(r==null?(devices.Count==0?"Searching for your AirPods…":"Select your nearby pair above"):(fresh[0]||fresh[1]?"●  Receiving battery updates":"Waiting for updates · rescan or select your pair again"));
        if(connection.Connected!=true) {
            status.Text=connectionError.Length>0?connectionError:connection.Connected==false?"AirPods disconnected · reconnect in Windows":pairedPicker.SelectedItem==null?"Select your Windows AirPods device in the top list":"Checking Bluetooth connection…";
            tray.Text=connection.Connected==false?"AirPods disconnected":"AirPods: connection not verified";
        }
        Invalidate();
    }
    void SaveDiagnostic() {
        File.WriteAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"diagnostic.txt"),"Scan status: "+watcher.Status+"\r\nBLE packets: "+packets+"\r\nApple packets: "+apple+"\r\nEncrypted case packets (any pair): "+cases+"\r\nPro 3 packets: "+pro+"\r\nPro 3 candidates: "+devices.Count+"\r\nError: "+error+"\r\n");
    }
    protected override void Dispose(bool disposing) {
        if(disposing && pairedDevice!=null) {pairedDevice.ConnectionStatusChanged-=ConnectionChanged;pairedDevice.Dispose();pairedDevice=null;}
        if(disposing) {timer.Dispose(); watcher.Received-=Received; try {watcher.Stop();} catch {} tray.Visible=false; tray.Dispose();}
        base.Dispose(disposing);
    }
    [STAThread] static int Main(string[] args) {
        if(args.Length>0 && args[0]=="--assets") { Artwork.SaveIcon(Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"AirPods.ico")); return 0; }
        if(args.Length>0 && args[0]=="--test") {try {Decoder.Test(); ConnectionState.Test(); File.WriteAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"tests.txt"),"Decoder and connection-transition tests passed."); return 0;} catch {return 1;}}
        Application.EnableVisualStyles(); Application.SetCompatibleTextRenderingDefault(false);
        if(args.Length>0 && args[0]=="--test-ui") {
            previewMode=true;
            using(var w=new BatteryWindow(true)) {
                w.Show(); Application.DoEvents(); w.SetConnected(true);
                var r=new Reading {Address=1};r.Level[0]=80;r.Level[1]=70;r.Updated[0]=r.Updated[1]=DateTime.UtcNow;
                w.devices.Add(1,r);w.picker.Items.Add(r);w.picker.SelectedIndex=0;w.RefreshReadings();
                if(w.values[0].Text!="~80%") throw new Exception("Connected display test failed");
                using(var b=new Bitmap(w.Width,w.Height)) {w.DrawToBitmap(b,new Rectangle(0,0,b.Width,b.Height));b.Save(Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"screenshot.png"));}
                w.SetConnected(false);
                if(w.values[0].Text!="—" || w.values[1].Text!="—" || w.levels[0]!=-1 || w.levels[1]!=-1 || r.Level[0]!=-1 || w.tray.Text!="AirPods disconnected") throw new Exception("Disconnect clearing test failed");
                using(var b=new Bitmap(w.Width,w.Height)) {w.DrawToBitmap(b,new Rectangle(0,0,b.Width,b.Height));b.Save(Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"disconnected-preview.png"));}
                w.SetConnected(true); if(w.values[0].Text!="—") throw new Exception("Reconnect reused cached battery");
                w.SetConnected(null); if(w.values[0].Text!="—") throw new Exception("Unknown connection leaked reading");
                File.WriteAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"ui-tests.txt"),"Connected display, disconnect clearing, empty bars and tray, reconnect cache reset, and unknown state passed.");w.Close();
            } return 0;
        }
        if(args.Length>0 && args[0]=="--preview") {
            previewMode=true;
            using(var w=new BatteryWindow(true)) { w.Show(); Application.DoEvents(); using(var b=new Bitmap(w.Width,w.Height)) {w.DrawToBitmap(b,new Rectangle(0,0,b.Width,b.Height)); b.Save(Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"preview.png"));} w.Close(); } return 0;
        }
        try {using(var window=new BatteryWindow(args.Length>0 && args[0]=="--diagnose")) Application.Run(window); return 0;}
        catch(Exception e) {File.WriteAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"error.txt"),e.ToString()); return 1;}
    }
}
