namespace MPService;
using System.Threading.Tasks;
using Microsoft.Win32.SafeHandles;
using System.Diagnostics;
using Microsoft.Win32;
using System.Text.RegularExpressions;
using System.Text.Json;

class Maintenance //Třída pro Správu aplikací
{
    public async Task<bool> Install (AppInstaller appInstaller)
    {
        Communication communication = new Communication();
        if(appInstaller != null){
            string? result = await communication.DownloadFile(appInstaller.Path, appInstaller.HexSHA256);
            return result != null && await installMSI(result);
        }
        return false;
    }
    public async Task<bool> Remove(AppInstaller app)
    {
        Communication communication = new Communication();
        RegistryKey? rkey = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall");
        if (rkey != null)
        {
            string[] names = rkey.GetSubKeyNames();
            List<AppWithRegistryPath> apps = new List<AppWithRegistryPath>();
            foreach (string s in names)
            {
                RegistryKey? rk = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall" + "\\" + s);
                if (rk != null)
                {
                    if ((string?)rk.GetValue("DisplayVersion", "#####################") != "#####################" && rk.GetValue("DisplayVersion", "#####################") != null)
                    {
                        apps.Add(new AppWithRegistryPath((string)rk.GetValue("DisplayName"), (string)rk.GetValue("DisplayVersion", "0"), "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall" + "\\" + s));
                    }
                    else
                    {
                        apps.Add(new AppWithRegistryPath((string)s, (string)rk.GetValue("DisplayVersion", "0"), "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall" + "\\" + s));
                    }
                }
            }
            app.Name = app.Name.Replace('_', '.');
            app.Name = Regex.Replace(app.Name, @"%", ".*");
            int i = 0;
            while(i < apps.Count && !Regex.IsMatch(apps[i].Name, app.Name)){
                i++;
            }
            if(i < apps.Count){
                RegistryKey? rk = Registry.LocalMachine.OpenSubKey(apps[i].RegistryPath);
                if(rk != null){
                    string? uninstallString = (string?) rk.GetValue("QuietUninstallString", "#####################");
                    if(uninstallString == "#####################" || uninstallString == null){
                        uninstallString = (string?) rk.GetValue("UninstallString", "#####################");
                    }
                    if(uninstallString != "#####################" && uninstallString != null){
                        Process Uninstallation = new Process();
                        Uninstallation.StartInfo.FileName = "cmd.exe";
                        if(Regex.IsMatch(uninstallString.ToLower(), "msiexec")){
                            uninstallString = Regex.Replace(uninstallString, @" \/[Ii] ", " /X ");
                            uninstallString = Regex.Replace(uninstallString, @" \/[Ii]{", " /X{");
                            if(!Regex.IsMatch(uninstallString.ToLower() + " ", @" \/qn[ {]")){
                                uninstallString = uninstallString + " /qn";
                            }
                        }
                        Uninstallation.StartInfo.Arguments = "/C " + uninstallString;
                        Uninstallation.Start();
                        await Uninstallation.WaitForExitAsync();
                        if(Uninstallation.ExitCode == 0){
                            return true;
                        }
                    }
                }
            }

            
        }
        return false;
    }
    private async Task<bool> installMSI(string path){
        Process installation = new Process();
        installation.StartInfo.FileName = "msiexec";
        installation.StartInfo.Arguments = "/i " + path + " /qn /promptrestart";
        if(installation.Start()){
            await installation.WaitForExitAsync();
            if(installation.ExitCode == 0){
                return true;
            }
            return false;
        }
        return false;
    }
}