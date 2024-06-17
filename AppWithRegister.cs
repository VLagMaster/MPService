namespace MPService;

class AppWithRegistryPath
{
    public string Name;
    public string Version;
    public string RegistryPath;
    public AppWithRegistryPath(string Name, string Version, string RegistryPath){
        this.Name = Name;
        this.Version = Version;
        this.RegistryPath = RegistryPath;
    }
}