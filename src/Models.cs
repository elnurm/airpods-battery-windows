using System.Collections.Generic;

// Only recognized earbud formats are accepted. Unknown products and AirPods Max
// must not be interpreted as a pair of earbuds without hardware validation.
internal static class Models {
    public const int TestedModel = 0x2720;
    public static readonly Dictionary<int,string> Names = new Dictionary<int,string> {
        {0x0220,"AirPods 1"}, {0x0f20,"AirPods 2"}, {0x1320,"AirPods 3"},
        {0x1920,"AirPods 4"}, {0x1b20,"AirPods 4 ANC"}, {0x0e20,"AirPods Pro"},
        {0x1420,"AirPods Pro 2"}, {0x2420,"AirPods Pro 2 USB-C"}, {TestedModel,"AirPods Pro 3"}
    };
    public static string Label(int id) {
        string name;
        if(!Names.TryGetValue(id,out name)) return "Unknown model";
        return name+(id==TestedModel?"":Language.T(" (experimental)"));
    }
}
