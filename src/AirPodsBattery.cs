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
    readonly CheckBox showAllModels = new CheckBox();
    readonly System.Windows.Forms.Panel content = new System.Windows.Forms.Panel();
    bool layoutReady;
    bool arranging;
    bool settingsExpanded=true;
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
    bool checkingUpdates;
    DateTime nextUpdateCheck=DateTime.MinValue;
    string updateBalloonUrl;
    bool installedUpdateBalloon;
    readonly ToolStripMenuItem englishLanguage=new ToolStripMenuItem("English");
    readonly ToolStripMenuItem russianLanguage=new ToolStripMenuItem("Русский");
    public BatteryWindow(bool diag) {
        diagnostic=diag;
        Text="AirPods Battery"; ClientSize=new Size(400,656); FormBorderStyle=FormBorderStyle.Sizable; MaximizeBox=true;
        AutoScaleDimensions=new SizeF(96,96); AutoScaleMode=AutoScaleMode.Dpi;
        DoubleBuffered=true; StartPosition=FormStartPosition.CenterScreen;
        BackColor=Artwork.Background; ForeColor=Color.FromArgb(31,32,35); Font=new Font("Segoe UI",10);
        deviceTitle.Text="AirPods Battery";deviceTitle.Font=new Font("Segoe UI",22,FontStyle.Bold);
        deviceTitle.SetBounds(20,24,360,42);deviceTitle.TextAlign=ContentAlignment.MiddleCenter;Controls.Add(deviceTitle);
        status.SetBounds(24,72,352,38);status.Font=new Font("Segoe UI",9);status.ForeColor=Artwork.Muted;status.TextAlign=ContentAlignment.TopCenter;Controls.Add(status);
        string[] names={Language.T("Left earbud"),Language.T("Right earbud")};
        for(int i=0;i<2;i++) {
            int x=24+i*184;
            Controls.Add(new Label {Text=names[i],ForeColor=Artwork.Muted,BackColor=Artwork.Card,Font=new Font("Segoe UI",9),Location=new Point(x+20,266),AutoSize=true});
            values[i]=new BatteryGauge {Text="—",Location=new Point(x+20,292),Size=new Size(116,50),AccessibleName=names[i]+Language.T(" battery")};Controls.Add(values[i]);
            ages[i]=new Label {Text=Language.T("Waiting for data"),ForeColor=Artwork.Muted,BackColor=Artwork.Card,Font=new Font("Segoe UI",8),Location=new Point(x+20,357),Size=new Size(136,25)};Controls.Add(ages[i]);
        }
        Controls.Add(new Label {Text=Language.T("Approximate battery levels"),ForeColor=Artwork.Muted,Font=new Font("Segoe UI",8),Location=new Point(24,418),AutoSize=true});
        var rescan=new Button {Text=Language.T("Rescan"),FlatStyle=FlatStyle.Flat,ForeColor=ForeColor,BackColor=Artwork.Card,Location=new Point(304,408),Size=new Size(72,30)};
        rescan.FlatAppearance.BorderColor=Color.FromArgb(222,223,228);rescan.Click+=delegate {Restart();};Controls.Add(rescan);
        settingsToggle.Text=Language.T("Hide device settings");settingsToggle.SetBounds(24,456,352,30);settingsToggle.FlatStyle=FlatStyle.Flat;
        settingsToggle.FlatAppearance.BorderSize=0;settingsToggle.TextAlign=ContentAlignment.MiddleLeft;settingsToggle.ForeColor=Artwork.Muted;
        settingsToggle.Click+=delegate {SetSettingsVisible(!settings.Visible);};Controls.Add(settingsToggle);
        settings.SetBounds(24,496,352,168);settings.BackColor=BackColor;Controls.Add(settings);
        showAllModels.Text=Language.T("Show all AirPods models");showAllModels.SetBounds(0,140,352,24);
        showAllModels.Font=new Font("Segoe UI",8);showAllModels.ForeColor=Artwork.Muted;
        showAllModels.CheckedChanged+=delegate {FilterBroadcasts();};settings.Controls.Add(showAllModels);
        ClientSize=new Size(400,680);
        settings.Controls.Add(new Label {Text=Language.T("Windows Bluetooth device"),ForeColor=Artwork.Muted,Font=new Font("Segoe UI",8),Location=new Point(0,0),AutoSize=true});
        pairedPicker.SetBounds(0,22,352,28);pairedPicker.DropDownStyle=ComboBoxStyle.DropDownList;pairedPicker.DisplayMember="Name";
        pairedPicker.BackColor=Artwork.Card;pairedPicker.ForeColor=ForeColor;pairedPicker.AccessibleName=Language.T("Windows Bluetooth device to monitor");
        pairedPicker.SelectedIndexChanged+=delegate {BindPairedDevice();};settings.Controls.Add(pairedPicker);
        settings.Controls.Add(new Label {Text=Language.T("Nearby battery broadcast"),ForeColor=Artwork.Muted,Font=new Font("Segoe UI",8),Location=new Point(0,65),AutoSize=true});
        picker.SetBounds(0,87,352,28);picker.DropDownStyle=ComboBoxStyle.DropDownList;picker.BackColor=Artwork.Card;picker.ForeColor=ForeColor;picker.AccessibleName=Language.T("Select your AirPods");
        picker.DrawMode=DrawMode.OwnerDrawFixed;picker.ItemHeight=42;picker.DropDownHeight=230;picker.IntegralHeight=false;
        picker.DrawItem+=delegate(object sender,DrawItemEventArgs e) {
            bool selected=(e.State&DrawItemState.Selected)!=0;
            using(var brush=new SolidBrush(selected?Color.FromArgb(226,239,232):Artwork.Card)) e.Graphics.FillRectangle(brush,e.Bounds);
            var item=e.Index<0?null:picker.Items[e.Index] as Reading;
            int half=e.Bounds.Height/2;
            TextRenderer.DrawText(e.Graphics,item==null?Language.T("Choose your AirPods"):item.IdentityLabel,Font,new Rectangle(e.Bounds.X+6,e.Bounds.Y,e.Bounds.Width-12,half),ForeColor,TextFormatFlags.VerticalCenter|TextFormatFlags.EndEllipsis);
            TextRenderer.DrawText(e.Graphics,item==null?Language.T("Inactive entries expire after 2 minutes"):item.SeenLabel(DateTime.UtcNow)+" · "+item.SignalLabel,Font,new Rectangle(e.Bounds.X+6,e.Bounds.Y+half,e.Bounds.Width-12,e.Bounds.Height-half),Artwork.Muted,TextFormatFlags.VerticalCenter|TextFormatFlags.EndEllipsis);
            if((e.State&DrawItemState.Focus)!=0)e.DrawFocusRectangle();
        };
        picker.SelectedIndexChanged+=delegate {if(!pruning) {if(picker.SelectedItem!=null)broadcastExpired=false;RefreshReadings();}};
        picker.SelectionChangeCommitted+=delegate {
            var chosen=picker.SelectedItem as Reading;
            if(chosen!=null) {preferredModel=chosen.ModelId;automaticSelection=false;automaticCandidate=null;showAllModels.Checked=false;FilterBroadcasts();SetSettingsVisible(false);}
        };settings.Controls.Add(picker);
        string iconPath=Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"AirPods.ico");
        if(File.Exists(iconPath)) Icon=new Icon(iconPath);
        tray.Icon=Icon; tray.Text=Language.T("AirPods: waiting for battery data"); tray.Visible=!diag;
        var menu=new ContextMenuStrip(); menu.Items.Add(Language.T("Show battery"),null,delegate { Show(); WindowState=FormWindowState.Normal; Activate(); });
        menu.Items.Add(Language.T("Restart scan"),null,delegate { Restart(); }); menu.Items.Add(Language.T("Exit"),null,delegate { quitting=true; Close(); });
        menu.Items.Insert(2,new ToolStripMenuItem(Language.T("Check for updates"),null,delegate {CheckForUpdates(true);}));
        menu.Items.Insert(3,new ToolStripMenuItem(Language.T("GitHub releases"),null,delegate {OpenRelease(UpdateChecker.ReleasesUrl);}));
        var languageMenu=new ToolStripMenuItem("Language / Язык");
        languageMenu.DropDownItems.Add(englishLanguage);languageMenu.DropDownItems.Add(russianLanguage);menu.Items.Insert(4,languageMenu);
        englishLanguage.Click+=delegate {ChangeLanguage(false,true);};
        russianLanguage.Click+=delegate {ChangeLanguage(true,true);};
        englishLanguage.Checked=!Language.Russian;russianLanguage.Checked=Language.Russian;
        tray.BalloonTipClicked+=delegate {if(installedUpdateBalloon)CheckForUpdates(true);else if(updateBalloonUrl!=null)OpenRelease(updateBalloonUrl);};
        tray.ContextMenuStrip=menu; tray.DoubleClick += delegate { Show(); Activate(); };
        FormClosing += delegate(object sender,FormClosingEventArgs e) { if(!quitting && !diagnostic && e.CloseReason==CloseReason.UserClosing) { e.Cancel=true; Hide(); } };
        watcher.ScanningMode=BluetoothLEScanningMode.Passive;
        watcher.Received += Received;
        watcher.Stopped += delegate(BluetoothLEAdvertisementWatcher s,BluetoothLEAdvertisementWatcherStoppedEventArgs e) {
            Dispatch(delegate { if(e.Error.ToString()!="Success") error=Language.T("Bluetooth scan stopped: ")+e.Error; });
        };
        timer.Interval=1000; timer.Tick += delegate {
            PruneBroadcasts(DateTime.UtcNow);
            picker.Invalidate();
            if(pairedDevice!=null) { try { SetConnected(pairedDevice.ConnectionStatus==BluetoothConnectionStatus.Connected); } catch { SetConnected(null); } }
            TryAutomaticSelection(DateTime.UtcNow);
            if(!diagnostic && DateTime.UtcNow>=nextUpdateCheck)CheckForUpdates(false);
            RefreshReadings();
            if(diagnostic && (DateTime.UtcNow-started).TotalSeconds>=20) { SaveDiagnostic(); quitting=true; Close(); }
        };
        var children=new List<Control>();foreach(Control child in Controls)children.Add(child);
        content.SetBounds(0,0,400,680);content.BackColor=BackColor;
        foreach(var child in children)content.Controls.Add(child);
        Controls.Add(content);content.Paint+=delegate(object sender,PaintEventArgs e) {DrawArtwork(e);};
        layoutReady=true;Resize+=delegate {ArrangeWindow();};ArrangeWindow();
        Shown += delegate { ArrangeWindow();if(previewMode) return; LoadPairedDevices(); Restart(); timer.Start(); if(diagnostic) Hide(); else CheckForUpdates(false); };
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
            connectionError=list.Count==0?Language.T("No paired Bluetooth device found. Pair your AirPods in Windows."):"";
            if(matches==1) pairedPicker.SelectedItem=onlyAirPods;
        } catch {connectionError=Language.T("Unable to check Bluetooth connection. Try Rescan."); SetConnected(null);}
        finally {loadingPaired=false; if(!IsDisposed) RefreshReadings();}
    }
    void ChangeLanguage(bool russian,bool save) {
        Language.Set(russian,save);Language.Apply(this);
        connectionError=Language.StoredMessage(connectionError);error=Language.StoredMessage(error);
        foreach(ToolStripItem item in tray.ContextMenuStrip.Items)Language.ApplyMenu(item);
        englishLanguage.Checked=!russian;russianLanguage.Checked=russian;
        for(int i=0;i<2;i++)values[i].AccessibleName=Language.T(i==0?"Left earbud":"Right earbud")+Language.T(" battery");
        RefreshReadings();picker.Invalidate();content.Invalidate();
    }
    async void CheckForUpdates(bool manual) {
        if(checkingUpdates || diagnostic || previewMode)return;
        checkingUpdates=true;nextUpdateCheck=DateTime.UtcNow.AddDays(1);
        try {
            if(InstalledUpdates.IsInstalled) {await CheckInstalledUpdates(manual);return;}
            var version=await UpdateChecker.Check();
            if(IsDisposed || Disposing)return;
            if(version==null) {
                if(manual)MessageBox.Show(this,Language.T("You have the latest downloadable version (")+UpdateChecker.Current.ToString(3)+").",Language.T("Check for updates"),MessageBoxButtons.OK,MessageBoxIcon.Information);
                return;
            }
            string url=UpdateChecker.Url(version);
            if(manual) {
                if(MessageBox.Show(this,"AirPods Battery "+version.ToString(3)+Language.T(" is available. Open the release page?"),Language.T("Update available"),MessageBoxButtons.YesNo,MessageBoxIcon.Information)==DialogResult.Yes)OpenRelease(url);
            } else if(UpdateChecker.ShouldNotify(version)) {
                updateBalloonUrl=url;
                tray.ShowBalloonTip(8000,Language.T("AirPods Battery update available"),Language.T("Version ")+version.ToString(3)+Language.T(" is available. Click to open the download page."),ToolTipIcon.Info);
                UpdateChecker.MarkNotified(version);
            }
        } catch {
            if(manual && !IsDisposed && !Disposing)MessageBox.Show(this,Language.T("Could not check GitHub for updates. Check your internet connection and try again later."),Language.T("Check for updates"),MessageBoxButtons.OK,MessageBoxIcon.Information);
        } finally {checkingUpdates=false;}
    }
    async System.Threading.Tasks.Task CheckInstalledUpdates(bool manual) {
        var manager=InstalledUpdates.CreateManager();
        var update=await manager.CheckForUpdatesAsync();
        if(IsDisposed || Disposing)return;
        if(update==null) {
            if(manual)MessageBox.Show(this,Language.T("You have the latest downloadable version (")+UpdateChecker.Current.ToString(3)+").",Language.T("Check for updates"),MessageBoxButtons.OK,MessageBoxIcon.Information);
            return;
        }
        var version=InstalledUpdates.VersionOf(update);
        if(!manual) {
            if(UpdateChecker.ShouldNotify(version)) {
                installedUpdateBalloon=true;updateBalloonUrl=null;
                tray.ShowBalloonTip(8000,Language.T("AirPods Battery update available"),Language.T("Version ")+version.ToString(3)+Language.T(" is available. Click to update in the app."),ToolTipIcon.Info);
                UpdateChecker.MarkNotified(version);
            }
            return;
        }
        Show();if(WindowState==FormWindowState.Minimized)WindowState=FormWindowState.Normal;Activate();
        if(MessageBox.Show(this,Language.T("Download version ")+version.ToString(3)+Language.T("? You can keep using the app until you choose to restart."),Language.T("Update available"),MessageBoxButtons.YesNo,MessageBoxIcon.Information)!=DialogResult.Yes)return;
        try {
            using(var download=new UpdateDownloadWindow())if(!await download.Download(manager,update,this))return;
            if(IsDisposed || Disposing)return;
            if(MessageBox.Show(this,Language.T("Update downloaded and verified. Restart now to install it?"),Language.T("Restart and update"),MessageBoxButtons.YesNo,MessageBoxIcon.Information)!=DialogResult.Yes)return;
            manager.WaitExitThenApplyUpdates(update.TargetFullRelease,false,true);
            quitting=true;Close();
        } catch {
            if(!IsDisposed && !Disposing)MessageBox.Show(this,Language.T("Update could not be completed. Your current version is still available. Try again later or use GitHub releases."),Language.T("Update failed"),MessageBoxButtons.OK,MessageBoxIcon.Warning);
        }
    }
    void OpenRelease(string url) {
        try {System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(url) {UseShellExecute=true});}
        catch {MessageBox.Show(this,Language.T("Could not open your browser. Visit github.com/elnurm/airpods-battery-windows/releases."),Language.T("GitHub releases"),MessageBoxButtons.OK,MessageBoxIcon.Information);}
    }
    async void BindPairedDevice() {
        int generation=++pairingGeneration;
        preferredModel=null;automaticCandidate=null;automaticSelection=false;broadcastExpired=false;
        picker.SelectedIndex=-1;
        FilterBroadcasts();
        if(pairedDevice!=null) {pairedDevice.ConnectionStatusChanged-=ConnectionChanged; pairedDevice.Dispose(); pairedDevice=null;}
        connection.Reset(); ClearReadings(); RefreshReadings();
        var choice=pairedPicker.SelectedItem as PairedChoice; if(choice==null) return;
        var chosen=choice.Device;
        try {
            var device=await AwaitOperation(BluetoothDevice.FromIdAsync(chosen.Id));
            if(IsDisposed || generation!=pairingGeneration) {if(device!=null) device.Dispose(); return;}
            if(device==null) {connectionError=Language.T("Cannot read this device's connection. Try Rescan."); SetConnected(null);return;}
            pairedDevice=device; connectionError=""; device.ConnectionStatusChanged+=ConnectionChanged;
            SetConnected(device.ConnectionStatus==BluetoothConnectionStatus.Connected);
        } catch {if(!IsDisposed && generation==pairingGeneration) {connectionError=Language.T("Connection check unavailable. Try Rescan.");SetConnected(null);}}
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
        if(notify && !diagnostic) {updateBalloonUrl=null;installedUpdateBalloon=false;tray.ShowBalloonTip(5000,Language.T("AirPods disconnected"),Language.T("Your AirPods disconnected from this PC. Battery readings have been cleared."),ToolTipIcon.Info);}
        RefreshReadings();
    }
    void SetSettingsVisible(bool visible) {
        float scale=content.Width/400f;
        int oldHeight=(int)((settingsExpanded?680:500)*scale);
        bool compact=WindowState==FormWindowState.Normal && ClientSize.Height<=oldHeight+1;
        settingsExpanded=visible;
        settings.Visible=visible;settingsToggle.Text=visible?Language.T("Hide device settings"):Language.T("Device settings");
        ArrangeWindow();
        if(compact)ClientSize=new Size(ClientSize.Width,(int)((visible?680:500)*scale));
        ArrangeWindow();content.Invalidate();
    }
    void ArrangeWindow() {
        if(!layoutReady || arranging || WindowState==FormWindowState.Minimized)return;
        arranging=true;
        try {
            int height=(int)((settingsExpanded?680:500)*(content.Width/400f));
            content.Height=height;
            MinimumSize=SizeFromClientSize(new Size(content.Width,height));
            content.Location=new Point(Math.Max(0,(ClientSize.Width-content.Width)/2),Math.Max(0,(ClientSize.Height-height)/2));
        } finally {arranging=false;}
    }
    void DrawArtwork(PaintEventArgs e) {
        var g=e.Graphics;g.SmoothingMode=System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        float scale=content.Width/400f;g.ScaleTransform(scale,scale);
        Artwork.Earbud(g,94,121,102,false);Artwork.Earbud(g,204,121,102,true);
        for(int i=0;i<2;i++) {
            int x=24+i*184;
            Artwork.Box(g,Color.FromArgb(235,235,239),x,250,168,148,20);
            Artwork.Box(g,Artwork.Card,x,248,168,148,20);
        }
        using(var pen=new Pen(Color.FromArgb(225,225,230)))g.DrawLine(pen,24,450,376,450);
    }
    void Dispatch(Action action) { if(!IsDisposed && IsHandleCreated) { try { BeginInvoke(action); } catch(InvalidOperationException) {} } }
    bool VisibleBroadcast(Reading reading) {
        return !preferredModel.HasValue || showAllModels.Checked || reading.ModelId==preferredModel.Value;
    }
    void FilterBroadcasts() {
        var selected=picker.SelectedItem as Reading;
        pruning=true;picker.BeginUpdate();
        try {
            picker.Items.Clear();
            foreach(var reading in devices.Values)if(VisibleBroadcast(reading))picker.Items.Add(reading);
            picker.SelectedItem=selected!=null && VisibleBroadcast(selected)?selected:null;
        } finally {picker.EndUpdate();pruning=false;}
        RefreshReadings();
    }
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
                    previous=new Reading {Address=r.Address,ModelId=r.ModelId,LastSeen=e.Timestamp.UtcDateTime,Rssi=r.Rssi}; devices.Add(r.Address,previous); if(VisibleBroadcast(previous))picker.Items.Add(previous);
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
        try { watcher.Stop(); error=""; watcher.Start(); } catch(Exception e) {error=Language.T("Unable to scan: ")+e.Message;}
        RefreshReadings();
    }
    void RefreshReadings() {
        var r=picker.SelectedItem as Reading;
        if(connection.Connected!=true) r=null;
        deviceTitle.Text=r==null?"AirPods Battery":Models.Names[r.ModelId];
        string[] tips={Language.T("L —"),Language.T("R —")};
        for(int i=0;i<2;i++) {
            levels[i]=r==null?-1:r.Level[i]; fresh[i]=false;
            values[i].Level=levels[i];values[i].Fresh=false;
            if(r==null || r.Level[i]<0) {values[i].Text="—"; values[i].ForeColor=ForeColor; ages[i].Text=connection.Connected==false?Language.T("Disconnected"):connection.Connected==null?Language.T("Checking connection"):Language.T("Waiting for data");values[i].Detail=ages[i].Text;values[i].AccessibleDescription=ages[i].Text;values[i].Invalidate();continue;}
            int age=(int)(DateTime.UtcNow-r.Updated[i]).TotalSeconds;
            bool stale=age>60;
            fresh[i]=!stale;
            values[i].Text=r.Level[i]+"%";values[i].Fresh=!stale;
            values[i].ForeColor=stale?Artwork.Muted:ForeColor;
            ages[i].Text=stale?Language.T("Last known"):r.Charging[i]?Language.T("Charging"):"";
            values[i].Detail=Language.T("Approximately ")+r.Level[i]+"%"+(r.Charging[i]?Language.T(" · charging"):"")+Language.T("\nUpdated ")+(age<60?age+Language.T(" seconds"):(age/60)+Language.T(" minutes"))+Language.T(" ago")+(stale?Language.T(" (last known)"):"");
            values[i].AccessibleDescription=values[i].Detail;values[i].Invalidate();
            tips[i]=(Language.Russian?"ЛП":"LR")[i]+" ~"+r.Level[i]+"%"+(stale?Language.T(" old"):"");
        }
        tray.Text=r==null?Language.T("AirPods: select a detected pair"):string.Join(" | ",tips);
        status.Text=error.Length>0?error:(r==null?(devices.Count==0?Language.T("Searching for your AirPods…"):Language.T("Select your nearby pair above")):(fresh[0]||fresh[1]?Language.T("●  Connected to this PC"):Language.T("Waiting for updates · rescan or select your pair again")));
        if(broadcastExpired && connection.Connected==true && error.Length==0)status.Text=Language.T("Searching for a unique matching broadcast… Multiple matches? Choose in Device settings.");
        if(connection.Connected!=true) {
            status.Text=connectionError.Length>0?connectionError:connection.Connected==false?Language.T("AirPods disconnected · reconnect in Windows"):pairedPicker.SelectedItem==null?Language.T("Select your Windows AirPods device in the top list"):Language.T("Checking Bluetooth connection…");
            tray.Text=connection.Connected==false?Language.T("AirPods disconnected"):Language.T("AirPods: connection not verified");
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
        Velopack.VelopackApp.Build().SetAutoApplyOnStartup(false).Run();
        if(args.Length>0 && (args[0]=="--test" || args[0]=="--test-ui"))Language.Set(false,false);
        if(args.Length>0 && args[0]=="--assets") { Artwork.SaveIcon(Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"AirPods.ico")); return 0; }
        if(args.Length>0 && args[0]=="--test") {try {Decoder.Test(); ConnectionState.Test(); UpdateChecker.Test(); File.WriteAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"tests.txt"),"Decoder, connection-transition and update selection tests passed."); return 0;} catch {return 1;}}
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
                w.FilterBroadcasts();
                if(w.picker.Items.Contains(other) || !w.picker.Items.Contains(current))throw new Exception("Model filter failed");
                w.showAllModels.Checked=true;
                if(!w.picker.Items.Contains(other) || w.picker.SelectedItem!=null)throw new Exception("Show all models changed selection");
                w.showAllModels.Checked=false;
                w.TryAutomaticSelection(now.AddSeconds(8));w.TryAutomaticSelection(now.AddSeconds(14));
                if(w.picker.SelectedItem!=current)throw new Exception("Different model blocked unique match");
                w.FilterBroadcasts();
                if(w.picker.SelectedItem!=current)throw new Exception("Filtering lost current selection");
                w.picker.SelectedIndex=-1;w.automaticSelection=false;w.automaticCandidate=null;current.LastSeen=now.AddSeconds(-60);
                w.TryAutomaticSelection(now);w.TryAutomaticSelection(now.AddSeconds(6));
                if(w.picker.SelectedItem!=null)throw new Exception("Stale candidate selected automatically");
                w.SetSettingsVisible(true);
                w.ClientSize=new Size(800,800);Application.DoEvents();
                if(w.content.Left!=200 || w.content.Top!=60 || w.content.Right>w.ClientSize.Width || w.content.Bottom>w.ClientSize.Height)throw new Exception("Resized layout not centered");
                w.SetSettingsVisible(false);
                if(w.ClientSize.Height!=800 || w.content.Height!=500 || w.content.Top!=150)throw new Exception("Settings toggle lost user window size");
                w.SetSettingsVisible(true);
                using(var b=new Bitmap(w.Width,w.Height)) {w.DrawToBitmap(b,new Rectangle(0,0,b.Width,b.Height));b.Save(Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"resized-preview.png"));}
                w.WindowState=FormWindowState.Maximized;Application.DoEvents();w.SetSettingsVisible(false);
                if(w.WindowState!=FormWindowState.Maximized || w.content.Right>w.ClientSize.Width || w.content.Bottom>w.ClientSize.Height)throw new Exception("Maximized settings layout failed");
                w.WindowState=FormWindowState.Normal;w.SetSettingsVisible(true);w.ClientSize=new Size(400,680);Application.DoEvents();
                current.Level[0]=70;current.LastSeen=current.Updated[0]=DateTime.UtcNow;w.picker.SelectedItem=current;
                w.ChangeLanguage(true,false);
                if(w.picker.SelectedItem!=current || w.values[0].Text!="70%" || !w.values[0].Detail.Contains("Обновлено") || w.settingsToggle.Text!="Скрыть настройки устройств" || w.tray.ContextMenuStrip.Items[0].Text!="Показать заряд")throw new Exception("Russian language switch failed");
                if(!current.SeenLabel(DateTime.UtcNow).StartsWith("Сигнал") || !Models.Label(0x0220).Contains("экспериментально"))throw new Exception("Russian broadcast labels failed");
                using(var b=new Bitmap(w.Width,w.Height)) {w.DrawToBitmap(b,new Rectangle(0,0,b.Width,b.Height));b.Save(Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"russian-preview.png"));}
                w.SetConnected(false);
                if(w.ages[0].Text!="Отключено" || w.tray.Text!="AirPods отключены")throw new Exception("Russian disconnect state failed");
                w.ChangeLanguage(false,false);
                if(w.settingsToggle.Text!="Hide device settings" || w.tray.ContextMenuStrip.Items[0].Text!="Show battery" || w.ages[0].Text!="Disconnected")throw new Exception("English language restore failed");
                foreach(float factor in new float[] {1.25f,1.2f}) {
                    w.Scale(new SizeF(factor,factor));w.ArrangeWindow();Application.DoEvents();
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
        try {
            // Diagnostic/test runs remain independent of the user's normal app.
            if(args.Length>0 && args[0]=="--diagnose") {using(var window=new BatteryWindow(true))Application.Run(window);return 0;}
            using(var instance=new SingleInstance("AirPodsBatteryWindows")) {
                if(!instance.IsPrimary) {instance.RequestActivation();return 0;}
                using(var window=new BatteryWindow(false))
                using(var activationTimer=new Timer {Interval=250}) {
                    activationTimer.Tick+=delegate {
                        if(!instance.TakeActivation())return;
                        window.Show();
                        if(window.WindowState==FormWindowState.Minimized)window.WindowState=FormWindowState.Normal;
                        window.BringToFront();window.Activate();
                    };
                    activationTimer.Start();Application.Run(window);
                }
            }
            return 0;
        }
        catch(Exception e) {File.WriteAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"error.txt"),e.ToString()); return 1;}
    }
}
