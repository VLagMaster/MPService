namespace MPService;
using Microsoft.Win32;
using System.Collections.Generic;
//////
using System.Text.Json;

class InstalledApps
    {
        public List<App> app = new List<App>();
        public InstalledApps()
        {
                RegistryKey? rkey = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall");
                if(rkey != null){
                    string[] names = rkey.GetSubKeyNames();
                    foreach (string s in names)
                    {
                        RegistryKey? rk = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall" + "\\" + s);
                        if(rk != null){
                            if((string?) rk.GetValue("DisplayVersion", "#####################") != "#####################"){
                            app.Add(new App( (string) rk.GetValue("DisplayName"), (string) rk.GetValue("DisplayVersion", "0")));
                            }else{
                                app.Add(new App( (string) s, (string) rk.GetValue("DisplayVersion", "0")));
                            }
                        }
                    }
                }else{
                    return;
                } 
        }
        public string Json()
        {
            var options = new JsonSerializerOptions
            {
                IncludeFields = true,
            };
            return JsonSerializer.Serialize(app, options);
        }
    }
