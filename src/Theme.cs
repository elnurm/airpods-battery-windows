using System;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;

internal static class Theme {
    static readonly string path=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"AirPodsBattery","theme.txt");
    public static bool Dark=Load();
    static bool Load() {try {return File.Exists(path) && File.ReadAllText(path).Trim()=="dark";}catch {return false;}}
    public static Color Text {get {return Dark?Color.FromArgb(240,241,244):Color.FromArgb(31,32,35);}}
    public static Color Border {get {return Dark?Color.FromArgb(66,69,77):Color.FromArgb(225,225,230);}}
    public static Color Shadow {get {return Dark?Color.FromArgb(22,23,27):Color.FromArgb(235,235,239);}}
    public static void Save() {try {Directory.CreateDirectory(Path.GetDirectoryName(path));File.WriteAllText(path,Dark?"dark":"light");}catch {}}
    public static void Apply(Control control,Color oldCard,Color oldMuted) {
        bool card=control.BackColor.ToArgb()==oldCard.ToArgb();
        bool muted=control.ForeColor.ToArgb()==oldMuted.ToArgb();
        foreach(Control child in control.Controls)Apply(child,oldCard,oldMuted);
        control.BackColor=card?Artwork.Card:Artwork.Background;
        control.ForeColor=muted?Artwork.Muted:Text;
        var button=control as Button;if(button!=null)button.FlatAppearance.BorderColor=Border;
        control.Invalidate();
    }
    public static void ApplyMenu(ToolStripItem item) {
        item.BackColor=Artwork.Card;item.ForeColor=Text;
        var menu=item as ToolStripMenuItem;
        if(menu!=null) {menu.DropDown.BackColor=Artwork.Card;menu.DropDown.ForeColor=Text;foreach(ToolStripItem child in menu.DropDownItems)ApplyMenu(child);}
    }
    [DllImport("dwmapi.dll")] static extern int DwmSetWindowAttribute(IntPtr window,int attribute,ref int value,int size);
    public static void ApplyTitle(Form window) {try {int dark=Dark?1:0;DwmSetWindowAttribute(window.Handle,20,ref dark,4);}catch {}}
}

internal sealed class ThemeMenuColors : ProfessionalColorTable {
    public override Color ToolStripDropDownBackground {get {return Artwork.Card;}}
    public override Color ImageMarginGradientBegin {get {return Artwork.Card;}}
    public override Color ImageMarginGradientMiddle {get {return Artwork.Card;}}
    public override Color ImageMarginGradientEnd {get {return Artwork.Card;}}
    public override Color MenuBorder {get {return Theme.Border;}}
    public override Color MenuItemBorder {get {return Theme.Border;}}
    public override Color MenuItemSelected {get {return Theme.Border;}}
}
