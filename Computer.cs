namespace MPService;
using System;
using System.Text.Json;

class Computer
{
    public string Architecture
    {
        get
        {
            if (Environment.Is64BitOperatingSystem)
                return "x86_64";
            return "x86";
        }
    }
    public string OS
    {
        get
        {
            return Environment.OSVersion.VersionString;
        }
    }
    public string Json()
    {
        var options = new JsonSerializerOptions
        {
            IncludeFields = true,
        };
        return JsonSerializer.Serialize(this, options);
    }
    public short Memory
    {
        get
        {
            short val = (short) Math.Round((double) (GC.GetGCMemoryInfo().TotalAvailableMemoryBytes)/1073741824);
            return val;
        }
    }
}
