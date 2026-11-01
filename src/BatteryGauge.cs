using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

// A keyboard-focusable battery ring. Reading details remain available to screen readers.
internal sealed class BatteryGauge : Control {
    readonly Timer motion=new Timer {Interval=16};
    int level=-1;
    float shown,from,tilt;
    DateTime changed;
    bool hovered,charging;
    public int ModelId;
    public bool Mirrored;
    public bool Fresh;
    public string Detail="Waiting for a reading";
    public int Level {
        get {return level;}
        set {
            if(level==value)return;
            from=level<0?0:shown;level=value;changed=DateTime.UtcNow;
            if(value<0 || !WidgetArtwork.MotionEnabled) {shown=Math.Max(0,value);motion.Stop();}
            else if(Visible)motion.Start();
            Invalidate();
        }
    }
    public bool Charging {get {return charging;}set {if(charging==value)return;charging=value;UpdateMotion();Invalidate();}}
    public BatteryGauge() {
        SetStyle(ControlStyles.UserPaint|ControlStyles.AllPaintingInWmPaint|ControlStyles.OptimizedDoubleBuffer|ControlStyles.Selectable,true);
        TabStop=true;BackColor=Artwork.Background;Font=new Font("Segoe UI",15,FontStyle.Bold);AccessibleRole=AccessibleRole.Indicator;
        MouseEnter+=delegate {hovered=true;UpdateMotion();};
        MouseLeave+=delegate {hovered=false;UpdateMotion();};
        Enter+=delegate {Invalidate();};
        Leave+=delegate {Invalidate();};
        VisibleChanged+=delegate {UpdateMotion();};
        motion.Tick+=delegate {
            if(!Visible || !WidgetArtwork.MotionEnabled) {motion.Stop();shown=Math.Max(0,level);tilt=0;Invalidate();return;}
            double t=Math.Min(1,(DateTime.UtcNow-changed).TotalMilliseconds/650);
            shown=level<0?0:(float)(from+(level-from)*(1-Math.Pow(1-t,3)));
            float target=hovered?(Mirrored?6:-6):0;tilt+=(target-tilt)*.22f;
            Invalidate();
            if(t>=1 && Math.Abs(tilt-target)<.1 && !charging)motion.Stop();
        };
    }
    void UpdateMotion() {
        if(Visible && WidgetArtwork.MotionEnabled && (charging || hovered || Math.Abs(tilt)>.1 || Math.Abs(shown-level)>.1))motion.Start();
        else {motion.Stop();if(!WidgetArtwork.MotionEnabled) {tilt=0;shown=Math.Max(0,level);}}
    }
    public void FinishAnimation() {shown=Math.Max(0,level);tilt=0;motion.Stop();Invalidate();}
    protected override void OnPaint(PaintEventArgs e) {
        base.OnPaint(e);var g=e.Graphics;g.SmoothingMode=SmoothingMode.AntiAlias;
        var state=g.Save();g.ScaleTransform(Width/106f,Height/106f);
        var ring=new RectangleF(19,3,68,68);
        Color track=Theme.Dark?Color.FromArgb(65,69,78):Color.FromArgb(225,228,233);
        using(var pen=new Pen(track,4))g.DrawEllipse(pen,ring);
        if(level>=0 && shown>0)using(var pen=new Pen(!Fresh?Artwork.Muted:level<=20?Color.FromArgb(218,158,64):Artwork.Mint,4)) {
            pen.StartCap=pen.EndCap=LineCap.Round;g.DrawArc(pen,ring,-90,Math.Min(359.9f,shown*3.6f));
        }
        var image=WidgetArtwork.Get(ModelId);
        if(image!=null) {
            var iconState=g.Save();g.TranslateTransform(53,37);g.RotateTransform(tilt);if(Mirrored)g.ScaleTransform(-1,1);
            float height=45,width=image.Width*height/image.Height;
            float rise=level>=0 && WidgetArtwork.MotionEnabled?(float)Math.Max(0,5*(1-Math.Min(1,(DateTime.UtcNow-changed).TotalMilliseconds/650))):0;
            g.InterpolationMode=InterpolationMode.HighQualityBicubic;g.DrawImage(image,-width/2,-height/2+rise,width,height);g.Restore(iconState);
        } else {
            using(var pen=new Pen(Artwork.Muted,2)) {g.DrawArc(pen,36,23,34,28,180,180);g.DrawLine(pen,36,37,36,49);g.DrawLine(pen,70,37,70,49);}
        }
        if(charging && level>=0) {
            double pulse=WidgetArtwork.MotionEnabled?.65+.35*Math.Cos(DateTime.UtcNow.TimeOfDay.TotalSeconds*3.5):1;
            using(var brush=new SolidBrush(Color.FromArgb((int)(255*pulse),Artwork.Mint)))g.FillPolygon(brush,new PointF[] {new PointF(77,53),new PointF(69,63),new PointF(75,63),new PointF(72,71),new PointF(82,60),new PointF(76,60)});
        }
        g.Restore(state);
        TextRenderer.DrawText(g,Text,Font,new Rectangle(0,(int)(Height*75f/106),Width,(int)(Height*30f/106)),Fresh?Theme.Text:Artwork.Muted,TextFormatFlags.HorizontalCenter|TextFormatFlags.VerticalCenter|TextFormatFlags.NoPadding);
        if(Focused)ControlPaint.DrawFocusRectangle(g,ClientRectangle);
    }
    protected override void Dispose(bool disposing) {if(disposing) {motion.Dispose();}base.Dispose(disposing);}
}
