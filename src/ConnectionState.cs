using System;

internal sealed class ConnectionState {
    public bool? Connected { get; private set; }
    public DateTime AcceptAfter { get; private set; }
    bool wasConnected;
    public bool Update(bool? connected, DateTime now) {
        bool notify=connected==false && wasConnected;
        if(connected==true && Connected!=true) AcceptAfter=now;
        if(connected.HasValue) wasConnected=connected.Value;
        Connected=connected;
        return notify;
    }
    public void Reset() { Connected=null; wasConnected=false; AcceptAfter=DateTime.MaxValue; }
    public bool Accept(DateTime time) { return Connected==true && time>=AcceptAfter; }
    public static void Test() {
        var s=new ConnectionState(); var now=DateTime.UtcNow;
        Check(!s.Update(false,now)); Check(!s.Accept(now));
        Check(!s.Update(true,now)); Check(s.Accept(now)); Check(!s.Accept(now.AddSeconds(-1)));
        Check(s.Update(false,now)); Check(!s.Accept(now)); Check(!s.Update(false,now));
        s.Update(true,now); s.Update(null,now); Check(!s.Accept(now)); Check(s.Update(false,now));
        s.Update(true,now.AddSeconds(5)); Check(!s.Accept(now)); Check(s.Accept(now.AddSeconds(6)));
        s.Reset(); Check(!s.Update(false,now));
    }
    static void Check(bool ok) { if(!ok) throw new Exception("Connection transition test failed"); }
}
