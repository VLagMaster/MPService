using System.Numerics;

namespace MPService;

public class AppInstaller
{
    public long ID { get; set; }
    public string Name { get; set; }
    public string Version { get; set; }
    public string Path { get; set; }
    public string HexSHA256 { get; set; }
    public AppInstaller(long ID, string Name, string Version, string Path, string HexSHA256){
        this.ID = ID;
        this.Name = Name;
        this.Version = Version;
        this.Path = Path;
        this.HexSHA256 = HexSHA256;
    }
}
