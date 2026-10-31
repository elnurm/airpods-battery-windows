using System;
using System.Security.Principal;
using System.Threading;

internal sealed class SingleInstance : IDisposable {
    readonly Mutex mutex;
    readonly EventWaitHandle activation;
    public readonly bool IsPrimary;
    public SingleInstance(string applicationId) {
        string name="Local\\"+applicationId+"."+WindowsIdentity.GetCurrent().User.Value;
        activation=new EventWaitHandle(false,EventResetMode.AutoReset,name+".Activate");
        mutex=new Mutex(false,name+".Mutex");
        try {IsPrimary=mutex.WaitOne(0);}
        catch(AbandonedMutexException) {IsPrimary=true;}
    }
    public void RequestActivation() {activation.Set();}
    public bool TakeActivation() {return activation.WaitOne(0);}
    public void Dispose() {
        if(IsPrimary)mutex.ReleaseMutex();
        mutex.Dispose();activation.Dispose();
    }
}
