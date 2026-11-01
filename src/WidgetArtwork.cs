using System;
using System.Collections.Generic;
using System.Drawing;
using System.Reflection;
using System.Runtime.InteropServices;

internal static class WidgetArtwork {
    static readonly Dictionary<string,Image> images=new Dictionary<string,Image>();
    public static string Style(int model) {
        switch(model) {
            case 0x0220: case 0x0f20:return "classic";
            case 0x1320:return "airpods3";
            case 0x1920: case 0x1b20:return "airpods4";
            case 0x0e20: case 0x1420: case 0x2420: case Models.TestedModel:return "pro";
            default:return null;
        }
    }
    public static Image Get(int model) {
        string style=Style(model);if(style==null)return null;
        Image image;
        if(!images.TryGetValue(style,out image)) {
            using(var stream=Assembly.GetExecutingAssembly().GetManifestResourceStream("Earbuds."+style)) {
                if(stream==null)throw new InvalidOperationException("Missing earbud artwork: "+style);
                using(var source=Image.FromStream(stream))image=new Bitmap(source);
            }
            images.Add(style,image);
        }
        return image;
    }
    [DllImport("user32.dll")] static extern bool SystemParametersInfo(uint action,uint param,out bool value,uint flags);
    public static bool MotionEnabled {
        get {bool enabled;try {return SystemParametersInfo(0x1042,0,out enabled,0) && enabled;}catch {return false;}}
    }
}
