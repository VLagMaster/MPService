using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
class Verify{
    public string ComputerName{
        get{
            return Environment.GetEnvironmentVariable("COMPUTERNAME");
        }
    }
    public DateTime DateTimeUTC{
        get;
        set;
    }
    public DateOnly DateOnlyUTC{
        get;
        set;
    }
    public string HexHashSecretWithDateOnlyUTC{
        get;
        set;
    }

    public Verify(){
        DateTimeUTC = DateTime.UtcNow;
        DateOnlyUTC = DateOnly.FromDateTime(DateTimeUTC);
        if(CredentialManager.ReadCredential("MP") != null ){
            byte[] bytes = Encoding.UTF8.GetBytes(CredentialManager.ReadCredential("MP").Password + DateOnlyUTC.ToString());
            using (var sha256 = new System.Security.Cryptography.SHA256Managed()){
                byte[] hash = sha256.ComputeHash(bytes);
                HexHashSecretWithDateOnlyUTC = Convert.ToHexString(hash);
            }
        }else{
            byte[] bytes = new byte[0];
            using (var sha256 = new System.Security.Cryptography.SHA256Managed()){
                byte[] hash = sha256.ComputeHash(bytes);
                HexHashSecretWithDateOnlyUTC = Convert.ToHexString(hash);
            }
        }
    }
    public string Json(){
        return JsonSerializer.Serialize(this);
    }
        
}