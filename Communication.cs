namespace MPService;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
//////
using System.Net.Http;
using System.Text.Json;
using Microsoft.Win32;
using System.IO;
using System.Security.Cryptography;
using Microsoft.Win32.SafeHandles;
using System.Text.Json.Serialization;
using System.Text;

class Communication //Třída pro kuminikaci s HTTP servery, např. Centrálním serverem
{
    private string computerName{
        get{
            return Environment.GetEnvironmentVariable("COMPUTERNAME");
        }
    }
    
    private string hexSecret{
        get{
            try{
            Credential? credential = CredentialManager.ReadCredential("MP");
            if(CredentialManager.ReadCredential("MP") != null ){
                return credential.Password;
            }
            return "";
            }
            catch{
                return "";
            }
        }
    }
    private HttpClient httpClient = new HttpClient();
    private string address{
        get{
            try{
            RegistryKey? rkey = Registry.LocalMachine.OpenSubKey("SOFTWARE\\.NET MP Service");
            if(rkey != null){
                if(rkey.GetValue("address", "#####################").ToString() != "#####################" && (string?) rkey.GetValue("address", "#####################").ToString() != null){
                        return rkey.GetValue("address").ToString();
                }
            }
            return "";
            }
            catch{
                return "";
            }
        }
    }
    public async Task<string?> DownloadFile(string path, string hash){
        try{
            var response = await httpClient.GetStreamAsync(path);
            File.Delete(Path.GetTempPath() + "installer.msi");
            SafeFileHandle safeFileHandle = NativeMethods.CreateFile(Path.GetTempPath() + "installer.msi", FileAccess.ReadWrite, FileShare.ReadWrite, IntPtr.Zero, FileMode.OpenOrCreate, 0, IntPtr.Zero);
            var fs = new FileStream( safeFileHandle, FileAccess.ReadWrite);
            await response.CopyToAsync(fs);
            SHA256 sha256 = SHA256.Create();
            fs.Position = 0;
            byte[] hashValue = await sha256.ComputeHashAsync(fs);
            safeFileHandle.Close();
            safeFileHandle.Dispose();
            if(hash.ToUpper() == Convert.ToHexString(hashValue).ToUpper()){
                return Path.GetTempPath() + "installer.msi";
            }
        }
        catch{
            return null;
        }
        return null;
    }
    private string verifyJsonPath
    {
        get
        {
            try
            {
                RegistryKey? rkey = Registry.LocalMachine.OpenSubKey("SOFTWARE\\.NET MP Service");
                if (rkey != null)
                {
                    if (rkey.GetValue("verifyJsonPath", "#####################").ToString() != "#####################" && (string?)rkey.GetValue("address", "#####################").ToString() != null)
                    {
                        return rkey.GetValue("verifyJsonPath").ToString();
                    }
                }
                return "";
            }
            catch
            {
                return "";
            }
        }
    }
    public async Task writeVerify(){
        File.Delete(verifyJsonPath);
        Verify verify = new Verify();
        using(StreamWriter outputFile = new StreamWriter(verifyJsonPath)){
            await outputFile.WriteLineAsync(verify.Json());
        }
        return;
    }
    public async Task<ListOfChanges?> Synchronise() //Metoda pro průběžnou synchronizaci se serverem
    {
        var request = new HttpRequestMessage(HttpMethod.Post, address + "api/sync.php");
        Computer computer = new Computer();
        InstalledApps installedApps = new InstalledApps();
        var formData = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("computer", computerName),
            new KeyValuePair<string, string>("hexSecret", hexSecret),
            new KeyValuePair<string, string>("task", "Synchronise"),
            new KeyValuePair<string, string>("computerSpecifications", computer.Json()),
            new KeyValuePair<string, string>("installedApps", installedApps.Json())
        });
        request.Content = formData;
        try
        {
            var response = await httpClient.SendAsync(request);
            if(response.IsSuccessStatusCode){
                string json = response.Content.ReadAsStringAsync().Result;
                ListOfChanges? listOfChanges = JsonSerializer.Deserialize<ListOfChanges>(json);
                return listOfChanges;
            }
            else
            {
                return null;
            }
        }
        catch
        {
            return null;
        }
    }
    public async Task<long[]?> GetListOfUpdates() //Metoda pro získání aktualizací ze serveru
    {
        var request = new HttpRequestMessage(HttpMethod.Post, address + "api/sync.php");
        var FormData = new FormUrlEncodedContent(new[] {
            new KeyValuePair<string, string>("computer", computerName),
            new KeyValuePair<string, string>("hexSecret", hexSecret),
            new KeyValuePair<string, string>("task", "getListOfUpdates")
        });
        request.Content = FormData;
        try
        {
            var response = await httpClient.SendAsync(request);
            if(response.IsSuccessStatusCode){
                var jsonstring = response.Content.ReadAsStringAsync().Result;
                var JSONObj = JsonSerializer.Deserialize<long[]>(jsonstring);
                if(JSONObj == null || JSONObj[0] == 0){
                    return new long[] {};
                }
                return JSONObj;
            }
            return null;
        }
        catch 
        {
            return null;
        }
    }
    public async Task<long[]?> GetListOfInstalls(){
        var request = new HttpRequestMessage(HttpMethod.Post, address + "api/sync.php");
        var FormData = new FormUrlEncodedContent(new[] {
            new KeyValuePair<string, string>("computer", computerName),
            new KeyValuePair<string, string>("hexSecret", hexSecret),
            new KeyValuePair<string, string>("task", "getListOfInstalls")
        });
        request.Content = FormData;
        try
        {
            var response = await httpClient.SendAsync(request);
            if(response.IsSuccessStatusCode){
                var jsonstring = response.Content.ReadAsStringAsync().Result;
                var JSONObj = JsonSerializer.Deserialize<long[]>(jsonstring);
                if(JSONObj == null || JSONObj[0] == 0){
                    return new long[] {};
                }
                return JSONObj;
            }
            return null;
        }
        catch 
        {
            return null;
        }
    }
    public async Task<long[]?> GetListOfUninstalls(){
        var request = new HttpRequestMessage(HttpMethod.Post, address + "api/sync.php");
        var FormData = new FormUrlEncodedContent(new[] {
            new KeyValuePair<string, string>("computer", computerName),
            new KeyValuePair<string, string>("hexSecret", hexSecret),
            new KeyValuePair<string, string>("task", "getListOfUninstalls")
        });
        request.Content = FormData;
        try
        {
            var response = await httpClient.SendAsync(request);
            if(response.IsSuccessStatusCode){
                var jsonstring = response.Content.ReadAsStringAsync().Result;
                var JSONObj = JsonSerializer.Deserialize<long[]>(jsonstring);
                if(JSONObj == null || JSONObj[0] == 0){
                    return new long[] {};
                }
                return JSONObj;
            }
            return null;
        }
        catch 
        {
            return null;
        }
    }
    public async Task<AppInstaller?> GetAppInstaller(long id)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, address + "api/sync.php");
        var formData = new FormUrlEncodedContent(new []{
            new KeyValuePair<string, string>("computer", computerName),
            new KeyValuePair<string, string>("hexSecret", hexSecret),
            new KeyValuePair<string, string>("task", "getAppUpdate"),
            new KeyValuePair<string, string>("SwID", id.ToString())
        });
        request.Content = formData;
        try {
            var response = await httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                var jsonstring = response.Content.ReadAsStringAsync().Result;
                return JsonSerializer.Deserialize<AppInstaller>(jsonstring);

            }
            else
            {
                return null;
            }
        }
        catch{
            return null;
        }
    }
    private string InstalledApps()
    {
        RegistryKey? rkey = Registry.LocalMachine.OpenSubKey("Počítač\\HKEY_LOCAL_MACHINE\\SOFTWARE\\.NET MP Service");
        if(rkey != null){
            return (string) rkey.GetValue("address", "");
        }
        return "";
    }
    public async Task<long?> WriteLog(ServerLog serverLog){
        
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, address + "api/sync.php");
        FormUrlEncodedContent formData = new FormUrlEncodedContent(new[]{
            new KeyValuePair<string, string>("computer", computerName),
            new KeyValuePair<string, string>("hexSecret", hexSecret),
            new KeyValuePair<string, string>("task", "reportEvent"),
            new KeyValuePair<string, string>("log", serverLog.Json())
        });
        request.Content = formData;
        try{
            var response = await httpClient.SendAsync(request);
            if(response.IsSuccessStatusCode){
                string responseString = response.Content.ReadAsStringAsync().Result;
                return long.Parse(responseString);
            }else{
                return null;
            }
        }catch{
            return null;
        }
    }
    public async Task<long?> WriteLog(long? IdEvent, string Type, string? RequestedBy, long? SwID, string? ExitStatus, DateTime? Time){
        ServerLog serverLog = new ServerLog(IdEvent, Type, RequestedBy, SwID, ExitStatus, Time);
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, address + "api/sync.php");
        FormUrlEncodedContent formData = new FormUrlEncodedContent(new[]{
            new KeyValuePair<string, string>("computer", computerName),
            new KeyValuePair<string, string>("hexSecret", hexSecret),
            new KeyValuePair<string, string>("task", "reportEvent"),
            new KeyValuePair<string, string>("log", serverLog.Json())
        });
        request.Content = formData;
        try{
            var response = await httpClient.SendAsync(request);
            if(response.IsSuccessStatusCode){
                string responseString = response.Content.ReadAsStringAsync().Result;
                return long.Parse(responseString);
            }else{
                return null;
            }
        }catch{
            return null;
        }
    }
    public async Task<bool> Auth(string hexSecret){
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, address + "api/auth.php");
        FormUrlEncodedContent formData = new FormUrlEncodedContent(new []{
            new KeyValuePair<string, string>("computer", computerName),
            new KeyValuePair<string, string>("hexSecret", hexSecret),
            new KeyValuePair<string, string>("task", "auth"),
            new KeyValuePair<string, string>("hexSecret", hexSecret)
        });
        request.Content = formData;
        try{
            var response = await httpClient.SendAsync(request);
            if(response.IsSuccessStatusCode){
                string responseString = response.Content.ReadAsStringAsync().Result;
                if(responseString == "1"){
                    return true;
                }
            }
            return false;
        }catch{
            return false;
        }
    }
    public async Task<string?> Register(){
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, address + "api/auth.php");
        FormUrlEncodedContent formData = new FormUrlEncodedContent(new []{
            new KeyValuePair<string, string>("computer", computerName),
            new KeyValuePair<string, string>("hexSecret", hexSecret),
            new KeyValuePair<string, string>("task", "register")
        });
        request.Content = formData;
        try{
            var response = await httpClient.SendAsync(request);
            if(response.IsSuccessStatusCode){
                string responseString = response.Content.ReadAsStringAsync().Result;
                if(responseString != "0"){
                    return responseString;
                }
            }
        }catch{
            return null;
        }
        return null;
    }
}

class NativeMethods
{
    [System.Runtime.InteropServices.DllImport("kernel32.dll", SetLastError = true, CharSet = System.Runtime.InteropServices.CharSet.Auto)]
    public static extern SafeFileHandle CreateFile(string lpFileName, FileAccess dwDesiredAccess, FileShare dwShareMode, IntPtr lpSecurityAttributes, FileMode dwCreationDisposition, int dwFlagsAndAttributes, IntPtr hTemplateFile);
}