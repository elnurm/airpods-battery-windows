using System;
using Windows.Devices.Enumeration;

internal sealed class Reading {
    public int[] Level = {-1,-1,-1};
    public bool[] Charging = new bool[3];
    public DateTime[] Updated = new DateTime[3];
    public ulong Address;
    public short Rssi;
    public int ModelId=Models.TestedModel;
    public override string ToString() { return Models.Label(ModelId)+" · "+Address.ToString("X12"); }
}
internal sealed class PairedChoice {
    public DeviceInformation Device;
    public string Name { get {return Device.Name;} }
    public override string ToString() {return Name;}
}
internal static class Decoder {
    public static int Level(int n) { return n <= 10 ? n * 10 : -1; }
    public static Reading Parse(byte[] b) {
        // Apple manufacturer bytes, excluding company ID. Reject encrypted case frames.
        if(b.Length != 27 || b[0] != 7 || b[1] != 25 || b[2] == 0) return null;
        int model=(b[3]<<8)|b[4];
        if(!Models.Names.ContainsKey(model)) return null;
        bool left = (b[5] & 32) != 0;
        var r = new Reading {ModelId=model};
        r.Level[0] = Level(left ? b[6]&15 : b[6]>>4);
        r.Level[1] = Level(left ? b[6]>>4 : b[6]&15);
        r.Level[2] = Level(b[7]&15);
        r.Charging[0] = (b[7] & (left ? 16 : 32)) != 0;
        r.Charging[1] = (b[7] & (left ? 32 : 16)) != 0;
        r.Charging[2] = (b[7] & 64) != 0;
        return r;
    }
    public static void Test() {
        byte[] b = new byte[27]; b[0]=7; b[1]=25; b[2]=1; b[3]=0x27; b[4]=0x20; b[5]=32; b[6]=0x87; b[7]=0x59;
        var r=Parse(b);
        Check(r.Level[0]==70 && r.Level[1]==80 && r.Level[2]==90 && r.Charging[0] && !r.Charging[1] && r.Charging[2]);
        b[5]=0; r=Parse(b); Check(r.Level[0]==80 && r.Level[1]==70 && !r.Charging[0] && r.Charging[1]);
        b[6]=0xff; b[7]=0xfb; r=Parse(b); Check(r.Level[0]==-1 && r.Level[1]==-1 && r.Level[2]==-1);
        b[2]=0; Check(Parse(b)==null); b[2]=1; b[3]=0x99; Check(Parse(b)==null);
        foreach(int model in Models.Names.Keys) {b[3]=(byte)(model>>8);b[4]=(byte)model;Check(Parse(b).ModelId==model);}
        b[3]=0x0a;b[4]=0x20;Check(Parse(b)==null); // Max is not an earbud pair.
        b[3]=0x27;b[1]=17;Check(Parse(b)==null);b[1]=25;
        Check(Parse(new byte[19])==null); Check(Parse(new byte[0])==null);
        for(int i=0;i<16;i++) Check(Level(i)==(i<=10?i*10:-1));
    }
    static void Check(bool ok) { if(!ok) throw new Exception("Decoder test failed"); }
}
