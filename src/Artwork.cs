using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;

internal static class Artwork {
    public static readonly Color Background=Color.FromArgb(17,20,26);
    public static readonly Color Card=Color.FromArgb(27,32,40);
    public static readonly Color Muted=Color.FromArgb(145,155,169);
    public static readonly Color Mint=Color.FromArgb(136,235,197);
    public static GraphicsPath Round(float x,float y,float w,float h,float r) {
        var p=new GraphicsPath(); float d=r*2;
        p.AddArc(x,y,d,d,180,90); p.AddArc(x+w-d,y,d,d,270,90);
        p.AddArc(x+w-d,y+h-d,d,d,0,90); p.AddArc(x,y+h-d,d,d,90,90); p.CloseFigure(); return p;
    }
    public static void Box(Graphics g,Color c,float x,float y,float w,float h,float r) {
        using(var p=Round(x,y,w,h,r)) using(var b=new SolidBrush(c)) g.FillPath(b,p);
    }
    // Original vector illustration, rendered at the target resolution.
    public static void Earbud(Graphics g,float x,float y,float size,bool mirrored) {
        var state=g.Save(); g.TranslateTransform(x+(mirrored?size:0),y); g.ScaleTransform((mirrored?-1:1)*size/100,size/100);
        using(var shadow=new SolidBrush(Color.FromArgb(60,0,0,0))) g.FillEllipse(shadow,24,104,57,9);
        using(var stem=new LinearGradientBrush(new Point(43,0),new Point(66,0),Color.FromArgb(211,218,225),Color.White)) {
            using(var path=Round(43,30,23,74,11)) g.FillPath(stem,path);
        }
        using(var body=new LinearGradientBrush(new Point(0,0),new Point(65,60),Color.White,Color.FromArgb(185,196,208))) g.FillEllipse(body,18,7,61,48);
        using(var tip=new SolidBrush(Color.FromArgb(225,231,236))) g.FillEllipse(tip,7,16,26,29);
        using(var ink=new SolidBrush(Color.FromArgb(36,42,50))) {g.FillEllipse(ink,11,22,9,17); g.FillEllipse(ink,55,18,12,7);}
        Box(g,Color.FromArgb(120,139,153),49,91,11,4,2);
        g.Restore(state);
    }
    public static Bitmap IconBitmap(int size) {
        var b=new Bitmap(size,size); using(var g=Graphics.FromImage(b)) {
            g.SmoothingMode=SmoothingMode.AntiAlias; g.Clear(Color.Transparent);
            g.ScaleTransform(size/64f,size/64f);
            using(var p=Round(1,1,62,62,15))
            using(var gradient=new LinearGradientBrush(new Point(0,0),new Point(64,64),Color.FromArgb(146,244,208),Color.FromArgb(35,159,130))) g.FillPath(gradient,p);
            // Bold silhouettes stay legible in the small notification-area icon.
            Box(g,Color.FromArgb(55,17,80,65),17,18,12,34,6);
            Box(g,Color.FromArgb(55,17,80,65),36,18,12,34,6);
            Box(g,Color.White,17,22,9,29,4.5f); Box(g,Color.White,38,22,9,29,4.5f);
            using(var white=new SolidBrush(Color.White)) {g.FillEllipse(white,9,13,22,18);g.FillEllipse(white,33,13,22,18);}
            using(var dark=new SolidBrush(Color.FromArgb(24,76,65))) {g.FillEllipse(dark,11,18,4,8);g.FillEllipse(dark,49,18,4,8);}
        } return b;
    }
    public static void SaveIcon(string path) {
        int[] sizes={16,24,32,48,64,128,256}; var frames=new byte[sizes.Length][];
        for(int i=0;i<sizes.Length;i++) {
            using(var b=IconBitmap(sizes[i])) using(var ms=new MemoryStream()) {
                b.Save(ms,ImageFormat.Png); frames[i]=ms.ToArray();
                if(sizes[i]==256) b.Save(Path.Combine(Path.GetDirectoryName(path),"AppIcon.png"),ImageFormat.Png);
            }
        }
        using(var writer=new BinaryWriter(File.Create(path))) {
            writer.Write((short)0);writer.Write((short)1);writer.Write((short)sizes.Length);
            int offset=6+16*sizes.Length;
            for(int i=0;i<sizes.Length;i++) {
                writer.Write((byte)(sizes[i]==256?0:sizes[i]));writer.Write((byte)(sizes[i]==256?0:sizes[i]));writer.Write((byte)0);writer.Write((byte)0);
                writer.Write((short)1);writer.Write((short)32);writer.Write(frames[i].Length);writer.Write(offset);offset+=frames[i].Length;
            }
            foreach(var frame in frames) writer.Write(frame);
        }
    }
}
