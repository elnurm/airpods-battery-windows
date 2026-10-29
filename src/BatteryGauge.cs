using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

// A keyboard-focusable battery indicator. The timestamp is available on hover
// and focus; stale/disconnected labels remain visible in the parent window.
internal sealed class BatteryGauge : Control {
    readonly ToolTip tooltip = new ToolTip {InitialDelay=350, ReshowDelay=100, AutoPopDelay=10000};
    public int Level = -1;
    public bool Fresh;
    public string Detail = "Waiting for a reading";
    public BatteryGauge() {
        SetStyle(ControlStyles.UserPaint|ControlStyles.AllPaintingInWmPaint|ControlStyles.OptimizedDoubleBuffer|ControlStyles.Selectable,true);
        TabStop=true; BackColor=Artwork.Card; Font=new Font("Segoe UI",18,FontStyle.Bold);
        AccessibleRole=AccessibleRole.Indicator;
        MouseEnter+=delegate {tooltip.SetToolTip(this,Detail);};
        Enter+=delegate {tooltip.Show(Detail,this,0,Height,10000);Invalidate();};
        Leave+=delegate {tooltip.Hide(this);Invalidate();};
    }
    protected override void OnPaint(PaintEventArgs e) {
        base.OnPaint(e);
        var g=e.Graphics; g.SmoothingMode=SmoothingMode.AntiAlias;
        var saved=g.Save();g.ScaleTransform(Width/116f,Height/50f);
        Artwork.Box(g,Color.FromArgb(224,226,230),0,3,104,44,12);
        Artwork.Box(g,Color.FromArgb(171,175,182),107,18,4,14,2);
        if(Level>=0) {
            using(var clip=Artwork.Round(3,6,98,38,9)) {
                g.SetClip(clip);
                using(var fill=new SolidBrush(!Fresh?Color.FromArgb(193,196,201):Level<=20?Color.FromArgb(246,190,105):Color.FromArgb(91,210,140)))
                    g.FillRectangle(fill,3,6,98*Level/100f,38);
                g.ResetClip();
            }
        }
        g.Restore(saved);
        TextRenderer.DrawText(g,Text,Font,new Rectangle(0,0,(int)(Width*104f/116),Height),Fresh?Color.FromArgb(29,48,37):Artwork.Muted,TextFormatFlags.HorizontalCenter|TextFormatFlags.VerticalCenter|TextFormatFlags.NoPadding);
        if(Focused) ControlPaint.DrawFocusRectangle(g,ClientRectangle);
    }
    protected override void Dispose(bool disposing) {if(disposing) tooltip.Dispose();base.Dispose(disposing);}
}
