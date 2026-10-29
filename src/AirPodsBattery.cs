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
    bool pruning;
    bool broadcastExpired;
    int? preferredModel;
    Reading automaticCandidate;
    DateTime candidateSince;
    bool automaticSelection;
    string connectionError="";
    readonly Label status = new Label();
    readonly BatteryGauge[] values = new BatteryGauge[2];
    readonly System.Windows.Forms.Panel settings = new System.Windows.Forms.Panel();
    readonly Button settingsToggle = new Button();
    readonly Label deviceTitle = new Label();
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
        Text="AirPods Battery"; ClientSize=new Size(400,656); FormBorderStyle=FormBorderStyle.FixedSingle; MaximizeBox=false;
        AutoScaleDimensions=new SizeF(96,96); AutoScaleMode=AutoScaleMode.Dpi;
        DoubleBuffered=true; StartPosition=FormStartPosition.CenterScreen;
        BackColor=Artwork.Background; ForeColor=Color.FromArgb(31,32,35); Font=new Font("Segoe UI",10);
        deviceTitle.Text="AirPods Battery";deviceTitle.Font=new Font("Segoe UI",22,FontStyle.Bold);
        deviceTitle.SetBounds(20,24,360,42);deviceTitle.TextAlign=ContentAlignment.MiddleCenter;Controls.Add(deviceTitle);
        status.SetBounds(24,72,352,38);status.Font=new Font("Segoe UI",9);status.ForeColor=Artwork.Muted;status.TextAlign=ContentAlignment.TopCenter;Controls.Add(status);
        string[] names={"Left earbud","Right earbud"};
        for(int i=0;i<2;i++) {
            int x=24+i*184;
            Controls.Add(new Label {Text=names[i],ForeColor=Artwork.Muted,BackColor=Artwork.Card,Font=new Font("Segoe UI",9),Location=new Point(x+20,266),AutoSize=true});
            values[i]=new BatteryGauge {Text="—",Location=new Point(x+20,292),Size=new Size(116,50),AccessibleName=names[i]+" battery"};Controls.Add(values[i]);
            ages[i]=new Label {Text="Waiting for data",ForeColor=Artwork.Muted,BackColor=Artwork.Card,Font=new Font("Segoe UI",8),Location=new Point(x+20,357),Size=new Size(136,25)};Controls.Add(ages[i]);
        }
        Controls.Add(new Label {Text="Approximate battery levels",ForeColor=Artwork.Muted,Font=new Font("Segoe UI",8),Location=new Point(24,418),AutoSize=true});
        var rescan=new Button {Text="Rescan",FlatStyle=FlatStyle.Flat,ForeColor=ForeColor,BackColor=Artwork.Card,Location=new Point(304,408),Size=new Size(72,30)};
        rescan.FlatAppearance.BorderColor=Color.FromArgb(222,223,228);rescan.Click+=delegate {Restart();};Controls.Add(rescan);
        settingsToggle.Text="Hide device settings";settingsToggle.SetBounds(24,456,352,30);settingsToggle.FlatStyle=FlatStyle.Flat;
        settingsToggle.FlatAppearance.BorderSize=0;settingsToggle.TextAlign=ContentAlignment.MiddleLeft;settingsToggle.ForeColor=Artwork.Muted;
        settingsToggle.Click+=delegate {SetSettingsVisible(!settings.Visible);};Controls.Add(settingsToggle);
        settings.SetBounds(24,496,352,144);settings.BackColor=BackColor;Controls.Add(settings);
        settings.Controls.Add(new Label {Text="Windows Bluetooth device",ForeColor=Artwork.Muted,Font=new Font("Segoe UI",8),Location=new Point(0,0),AutoSize=true});
        pairedPicker.SetBounds(0,22,352,28);pairedPicker.DropDownStyle=ComboBoxStyle.DropDownList;pairedPicker.DisplayMember="Name";
        pairedPicker.BackColor=Artwork.Card;pairedPicker.ForeColor=ForeColor;pairedPicker.AccessibleName="Windows Bluetooth device to monitor";
        pairedPicker.SelectedIndexChanged+=delegate {BindPairedDevice();};settings.Controls.Add(pairedPicker);
        settings.Controls.Add(new Label {Text="Nearby battery broadcast",ForeColor=Artwork.Muted,Font=new Font("Segoe UI",8),Location=new Point(0,65),AutoSize=true});
        picker.SetBounds(0,87,352,28);picker.DropDownStyle=ComboBoxStyle.DropDownList;picker.BackColor=Artwork.Card;picker.ForeColor=ForeColor;picker.AccessibleName="Select your AirPods";
        picker.DrawMode=DrawMode.OwnerDrawFixed;picker.ItemHeight=42;picker.DropDownHeight=230;picker.IntegralHeight=false;
        picker.DrawItem+=delegate(object sender,DrawItemEventArgs e) {
            bool selected=(e.State&DrawItemState.Selected)!=0;
            using(var brush=new SolidBrush(selected?Color.FromArgb(226,239,232):Artwork.Card)) e.Graphics.FillRectangle(brush,e.Bounds);
            var item=e.Index<0?null:picker.Items[e.Index] as Reading;
            int half=e.Bounds.Height/2;
            TextRenderer.DrawText(e.Graphics,item==null?"Choose your AirPods":item.IdentityLabel,Font,new Rectangle(e.Bounds.X+6,e.Bounds.Y,e.Bounds.Width-12,half),ForeColor,TextFormatFlags.VerticalCenter|TextFormatFlags.EndEllipsis);
            TextRenderer.DrawText(e.Graphics,item==null?"Inactive entries expire after 2 minutes":item.SeenLabel(DateTime.UtcNow)+" · "+item.SignalLabel,Font,new Rectangle(e.Bounds.X+6,e.Bounds.Y+half,e.Bounds.Width-12,e.Bounds.Height-half),Artwork.Muted,TextFormatFlags.VerticalCenter|TextFormatFlags.EndEllipsis);
            if((e.State&DrawItemState.Focus)!=0)e.DrawFocusRectangle();
        };
        picker.SelectedIndexChanged+=delegate {if(!pruning) {if(picker.SelectedItem!=null)broadcastExpired=false;RefreshReadings();}};
        picker.SelectionChangeCommitted+=delegate {
            var chosen=picker.SelectedItem as Reading;
            if(chosen!=null) {preferredModel=chosen.ModelId;automaticSelection=false;automaticCandidate=null;SetSettingsVisible(false);}
        };settings.Controls.Add(picker);
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
            PruneBroadcasts(DateTime.UtcNow);
            picker.Invalidate();
            if(pairedDevice!=null) { try { SetConnected(pairedDevice.ConnectionStatus==BluetoothConnectionStatus.Connected); } catch { SetConnected(null); } }
            TryAutomaticSelection(DateTime.UtcNow);
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
        preferredModel=null;automaticCandidate=null;automaticSelection=false;broadcastExpired=false;
        picker.SelectedIndex=-1;
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
    void SetSettingsVisible(bool visible) {
        float scale=ClientSize.Width/400f;
        settings.Visible=visible;settingsToggle.Text=visible?"Hide device settings":"Device settings";
        ClientSize=new Size(ClientSize.Width,(int)((visible?656:500)*scale));
        Invalidate();
    }
    protected override void OnPaint(PaintEventArgs e) {
        base.OnPaint(e);var g=e.Graphics;g.SmoothingMode=System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        float scale=ClientSize.Width/400f;g.ScaleTransform(scale,scale);
        Artwork.Earbud(g,94,121,102,false);Artwork.Earbud(g,204,121,102,true);
        for(int i=0;i<2;i++) {
            int x=24+i*184;
            Artwork.Box(g,Color.FromArgb(235,235,239),x,250,168,148,20);
            Artwork.Box(g,Artwork.Card,x,248,168,148,20);
        }
        using(var pen=new Pen(Color.FromArgb(225,225,230)))g.DrawLine(pen,24,450,376,450);
    }
    void Dispatch(Action action) { if(!IsDisposed && IsHandleCreated) { try { BeginInvoke(action); } catch(InvalidOperationException) {} } }
    void PruneBroadcasts(DateTime now) {
        var expired=new List<Reading>();
        foreach(var r in devices.Values) if(r.IsExpired(now)) expired.Add(r);
        if(expired.Count==0)return;
        var selected=picker.SelectedItem as Reading;
        bool lost=selected!=null && expired.Contains(selected);
        pruning=true;picker.BeginUpdate();
        try {
            foreach(var r in expired) {devices.Remove(r.Address);picker.Items.Remove(r);}
            // Removing a selected ComboBox item can select its neighbour; never
            // silently switch the battery source to another person's AirPods.
            picker.SelectedItem=lost?null:selected;
            if(lost) {picker.SelectedIndex=-1;broadcastExpired=true;SetSettingsVisible(true);}
        } finally {picker.EndUpdate();pruning=false;}
        RefreshReadings();
    }
    void TryAutomaticSelection(DateTime now) {
        if(connection.Connected!=true || !preferredModel.HasValue || error.Length>0) {automaticCandidate=null;return;}
        if(picker.SelectedItem!=null && !automaticSelection) return;
        Reading only=null;int matches=0;
        foreach(var r in devices.Values) {
            if(r.ModelId==preferredModel.Value && !r.IsExpired(now)) {only=r;matches++;}
        }
        if(matches!=1) {
            automaticCandidate=null;
            if(automaticSelection) {picker.SelectedIndex=-1;automaticSelection=false;broadcastExpired=true;SetSettingsVisible(true);}
            return;
        }
        if(picker.SelectedItem==only)return;
        if((now-only.LastSeen).TotalSeconds>30 || (only.Level[0]<0 && only.Level[1]<0)) {automaticCandidate=null;return;}
        // Observe a sole matching model for five seconds before choosing it.
        // Model matching is a convenience heuristic, not authenticated identity.
        if(automaticCandidate!=only) {automaticCandidate=only;candidateSince=now;return;}
        if((now-candidateSince).TotalSeconds<5)return;
        automaticSelection=true;picker.SelectedItem=only;SetSettingsVisible(false);
    }
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
            PruneBroadcasts(DateTime.UtcNow);
            foreach(var r in decoded) {
                pro++; Reading previous;
                if(!devices.TryGetValue(r.Address,out previous)) {
                    // Keep addresses separate; automatic selection only uses a sole matching model.
                    if(devices.Count>=100) continue;
                    previous=new Reading {Address=r.Address,ModelId=r.ModelId,LastSeen=e.Timestamp.UtcDateTime,Rssi=r.Rssi}; devices.Add(r.Address,previous); picker.Items.Add(previous);
                }
                previous.Rssi=r.Rssi;
                previous.LastSeen=e.Timestamp.UtcDateTime;
                for(int i=0;i<3;i++) if(r.Level[i]>=0) {previous.Level[i]=r.Level[i]; previous.Charging[i]=r.Charging[i]; previous.Updated[i]=DateTime.UtcNow;}
            }
            RefreshReadings();
        });
    }
    void Restart() {
        PruneBroadcasts(DateTime.UtcNow);
        if(!previewMode && pairedDevice==null && !loadingPaired) { if(pairedPicker.SelectedItem!=null) BindPairedDevice(); else LoadPairedDevices(); }
        try { watcher.Stop(); error=""; watcher.Start(); } catch(Exception e) {error="Unable to scan: "+e.Message;}
        RefreshReadings();
    }
    void RefreshReadings() {
        var r=picker.SelectedItem as Reading;
        if(connection.Connected!=true) r=null;
        deviceTitle.Text=r==null?"AirPods Battery":Models.Names[r.ModelId];
        string[] tips={"L —","R —"};
        for(int i=0;i<2;i++) {
            levels[i]=r==null?-1:r.Level[i]; fresh[i]=false;
            values[i].Level=levels[i];values[i].Fresh=false;
            if(r==null || r.Level[i]<0) {values[i].Text="—"; values[i].ForeColor=ForeColor; ages[i].Text=connection.Connected==false?"Disconnected":connection.Connected==null?"Checking connection":"Waiting for data";values[i].Detail=ages[i].Text;values[i].AccessibleDescription=ages[i].Text;values[i].Invalidate();continue;}
            int age=(int)(DateTime.UtcNow-r.Updated[i]).TotalSeconds;
            bool stale=age>60;
            fresh[i]=!stale;
            values[i].Text=r.Level[i]+"%";values[i].Fresh=!stale;
            values[i].ForeColor=stale?Artwork.Muted:ForeColor;
            ages[i].Text=stale?"Last known":r.Charging[i]?"Charging":"";
            values[i].Detail="Approximately "+r.Level[i]+"%"+(r.Charging[i]?" · charging":"")+"\nUpdated "+(age<60?age+" seconds":(age/60)+" minutes")+" ago"+(stale?" (last known)":"");
            values[i].AccessibleDescription=values[i].Detail;values[i].Invalidate();
            tips[i]="LRC"[i]+" ~"+r.Level[i]+"%"+(stale?" old":"");
        }
        tray.Text=r==null?"AirPods: select a detected pair":string.Join(" | ",tips);
        status.Text=error.Length>0?error:(r==null?(devices.Count==0?"Searching for your AirPods…":"Select your nearby pair above"):(fresh[0]||fresh[1]?"●  Connected to this PC":"Waiting for updates · rescan or select your pair again"));
        if(broadcastExpired && connection.Connected==true && error.Length==0)status.Text="Searching for a unique matching broadcast… Multiple matches? Choose in Device settings.";
        if(connection.Connected!=true) {
            status.Text=connectionError.Length>0?connectionError:connection.Connected==false?"AirPods disconnected · reconnect in Windows":pairedPicker.SelectedItem==null?"Select your Windows AirPods device in the top list":"Checking Bluetooth connection…";
            tray.Text=connection.Connected==false?"AirPods disconnected":"AirPods: connection not verified";
        }
        status.ForeColor=connection.Connected==true && error.Length==0?Artwork.Mint:Artwork.Muted;
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
                var r=new Reading {Address=1,LastSeen=DateTime.UtcNow,Rssi=-48};r.Level[0]=80;r.Level[1]=70;r.Updated[0]=r.Updated[1]=DateTime.UtcNow;
                w.devices.Add(1,r);w.picker.Items.Add(r);w.picker.SelectedIndex=0;w.RefreshReadings();
                w.SetSettingsVisible(false);
                if(w.values[0].Text!="80%" || w.ages[0].Text!="" || !w.values[0].Detail.Contains("Updated")) throw new Exception("Connected display test failed");
                using(var b=new Bitmap(w.Width,w.Height)) {w.DrawToBitmap(b,new Rectangle(0,0,b.Width,b.Height));b.Save(Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"screenshot.png"));}
                r.Updated[0]=DateTime.UtcNow.AddMinutes(-3);w.RefreshReadings();
                if(w.ages[0].Text!="Last known" || w.values[0].Fresh || !w.values[0].Detail.Contains("3 minutes"))throw new Exception("Stale reading visibility test failed");
                using(var b=new Bitmap(w.Width,w.Height)) {w.DrawToBitmap(b,new Rectangle(0,0,b.Width,b.Height));b.Save(Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"stale-preview.png"));}
                w.SetSettingsVisible(true);if(!w.settings.Visible || w.ClientSize.Height<=500)throw new Exception("Settings expansion failed");
                w.SetSettingsVisible(false);
                w.SetConnected(false);
                if(w.values[0].Text!="—" || w.values[1].Text!="—" || w.levels[0]!=-1 || w.levels[1]!=-1 || r.Level[0]!=-1 || w.tray.Text!="AirPods disconnected") throw new Exception("Disconnect clearing test failed");
                using(var b=new Bitmap(w.Width,w.Height)) {w.DrawToBitmap(b,new Rectangle(0,0,b.Width,b.Height));b.Save(Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"disconnected-preview.png"));}
                w.SetConnected(true); if(w.values[0].Text!="—") throw new Exception("Reconnect reused cached battery");
                w.SetConnected(null); if(w.values[0].Text!="—") throw new Exception("Unknown connection leaked reading");
                var now=DateTime.UtcNow;
                r.LastSeen=now;
                var old=new Reading {Address=2,LastSeen=now.AddMinutes(-3),Rssi=-75};
                var current=new Reading {Address=3,LastSeen=now,Rssi=-42};
                w.devices.Add(2,old);w.devices.Add(3,current);w.picker.Items.Add(old);w.picker.Items.Add(current);
                w.PruneBroadcasts(now);
                if(w.devices.ContainsKey(2) || w.picker.SelectedItem!=r || w.picker.Items.Count!=2)throw new Exception("Expiry changed a live selection");
                r.LastSeen=now.AddSeconds(-120);w.PruneBroadcasts(now);
                if(w.picker.SelectedIndex!=-1 || w.values[0].Text!="—" || !w.broadcastExpired || !w.settings.Visible || !w.devices.ContainsKey(3))throw new Exception("Expired selection switched to another pair");
                w.preferredModel=current.ModelId;
                w.TryAutomaticSelection(now);w.TryAutomaticSelection(now.AddSeconds(6));
                if(w.picker.SelectedItem!=null)throw new Exception("Auto selection while connection unknown");
                w.SetConnected(true);current.Level[0]=70;current.Updated[0]=now;
                w.TryAutomaticSelection(now);
                if(w.picker.SelectedItem!=null)throw new Exception("Auto selection skipped observation period");
                w.TryAutomaticSelection(now.AddSeconds(6));
                if(w.picker.SelectedItem!=current || w.values[0].Text!="70%" || w.settings.Visible)throw new Exception("Unique model not selected automatically");
                var other=new Reading {Address=4,LastSeen=now,ModelId=current.ModelId};
                w.devices.Add(4,other);w.picker.Items.Add(other);w.TryAutomaticSelection(now.AddSeconds(7));
                if(w.picker.SelectedItem!=null || w.values[0].Text!="—")throw new Exception("Ambiguous automatic selection retained");
                other.ModelId=0x0220;
                w.TryAutomaticSelection(now.AddSeconds(8));w.TryAutomaticSelection(now.AddSeconds(14));
                if(w.picker.SelectedItem!=current)throw new Exception("Different model blocked unique match");
                w.picker.SelectedIndex=-1;w.automaticSelection=false;w.automaticCandidate=null;current.LastSeen=now.AddSeconds(-60);
                w.TryAutomaticSelection(now);w.TryAutomaticSelection(now.AddSeconds(6));
                if(w.picker.SelectedItem!=null)throw new Exception("Stale candidate selected automatically");
                w.SetSettingsVisible(true);
                foreach(float factor in new float[] {1.25f,1.2f}) {
                    w.Scale(new SizeF(factor,factor));Application.DoEvents();
                    if(w.settings.Bottom>w.ClientSize.Height || w.values[1].Right>w.ClientSize.Width)throw new Exception("Scaled layout clipped");
                }
                using(var b=new Bitmap(w.Width,w.Height)) {w.DrawToBitmap(b,new Rectangle(0,0,b.Width,b.Height));b.Save(Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"scaled-settings-preview.png"));}
                File.WriteAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"ui-tests.txt"),"Battery icon display, fresh hover timestamps, stale labels, settings toggle, disconnect clearing, reconnect cache reset, unknown state, broadcast expiry, and selection preservation passed.");w.Close();
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
