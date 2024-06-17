namespace MPService;

using System.Numerics;
using System.Text.Json;
class ServerLog {
    public long? IdEvent{
        get;
        set;
    }
    public string? Type{
        get;
        set;
    }
    public string? Computer{
        get{
            return Environment.GetEnvironmentVariable("COMPUTERNAME");
        }
    }
    public string? RequestedByHex{
        get;
        set;
    }
    public long? SwID{
        get;
        set;
    }
    public string? ExitStatus{
        get;
        set;
    }
    public DateTime? DateTime{
        get;
        set;
    }
    public string Json()
        {
            var options = new JsonSerializerOptions
            {
                IncludeFields = true,
            };
            return JsonSerializer.Serialize(this, options);
        }
    public ServerLog(long? IdEvent, string Type, string? RequestedBy, long? SwID, string? ExitStatus, DateTime? DateTime){
        this.IdEvent = IdEvent;
        this.Type = Type;
        this.RequestedByHex = RequestedBy;
        this.SwID = SwID;
        this.ExitStatus = ExitStatus;
        this.DateTime = DateTime;
    }
}