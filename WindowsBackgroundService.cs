namespace MPService;
using System;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Dynamic;
using System.Numerics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.VisualBasic;

public class WindowsBackgroundService : BackgroundService
{
    private readonly ILogger<WindowsBackgroundService> _logger;
    private string status = "Starting";
    public string Status{
        get{
            return status;
        }
    }

    public WindowsBackgroundService(ILogger<WindowsBackgroundService> logger)
    {
        _logger = logger;
    }
    Communication communication = new Communication();
    LogSynchronisation logSynchronisation = new LogSynchronisation();
    private async Task checkCredentials(){
        Credential? credential = CredentialManager.ReadCredential("MP");
        if(credential == null ){
            string? newSecret = await communication.Register();
            if(newSecret != "0" && newSecret != "" && newSecret != null){
                CredentialManager.WriteCredential("MP", "MP", newSecret);
                credential = CredentialManager.ReadCredential("MP");
                await communication.Auth(credential.Password);
            }
        }
    }

    private async Task installApps(long[] apps, string task){
        if(apps.Length > 0 && apps[0] != 0){
            Maintenance maintenance = new Maintenance();
            if(task != "SwUninstall"){
                foreach(int app in apps){
                    AppInstaller? appInstaller = await communication.GetAppInstaller(app);
                    if(appInstaller != null)
                    {
                        long logID = logSynchronisation.CreateLog(task, null, appInstaller.ID, "Ongoing", null);
                        if(await maintenance.Install(appInstaller)){
                            _logger.LogInformation(task + " " + appInstaller.ID.ToString() + " was successfull at " + DateTimeOffset.Now);
                            logSynchronisation.UpdateLog(logID, "Success");
                        }else{
                            _logger.LogInformation(task + " " + appInstaller.ID.ToString() + " has ended with error at " + DateTimeOffset.Now);
                            logSynchronisation.UpdateLog(logID, "Failure");
                        }
                    }
                }
                return;
            }
            else{
                foreach(int app in apps){
                    AppInstaller? appInstaller = await communication.GetAppInstaller(app);
                    if(appInstaller != null)
                    {
                        long logID = logSynchronisation.CreateLog(task, null, appInstaller.ID, "Ongoing", null);
                        if(await maintenance.Remove(appInstaller)){
                            _logger.LogInformation(task + " " + appInstaller.ID.ToString() + " was successfull at " + DateTimeOffset.Now);
                            logSynchronisation.UpdateLog(logID, "Success");
                        }else{
                            _logger.LogInformation(task + " " + appInstaller.ID.ToString() + " has ended with error at " + DateTimeOffset.Now);
                            logSynchronisation.UpdateLog(logID, "Failure");
                        }
                    }
                }
                return;
            }
        }
        
    }
    private async Task WriteVerify(){
        Communication communication = new Communication();
        await communication.writeVerify();
    }
    
    private async Task installApps(AppRequest[] apps, string task){
        if(apps.Length > 0){
            Maintenance maintenance = new Maintenance();
            if(task != "SwUninstall"){
                foreach(AppRequest app in apps){
                    AppInstaller? appInstaller = await communication.GetAppInstaller(app.ID);
                    if(appInstaller != null)
                    {
                        long logID = logSynchronisation.CreateLog(task, app.RequestedByHex, appInstaller.ID, "Ongoing", null);
                        if(await maintenance.Install(appInstaller)){
                            _logger.LogInformation(task + " " + appInstaller.ID.ToString() + " was successfull at " + DateTimeOffset.Now);
                            logSynchronisation.UpdateLog(logID, "Success");
                        }else{
                            _logger.LogInformation(task + " " + appInstaller.ID.ToString() + " has ended with error at " + DateTimeOffset.Now);
                            logSynchronisation.UpdateLog(logID, "Failure");
                        }
                    }else{
                    }
                }
                return;
            }
            else{
                foreach(AppRequest app in apps){
                    AppInstaller? appInstaller = await communication.GetAppInstaller(app.ID);
                    if(appInstaller != null)
                    {
                        long logID = logSynchronisation.CreateLog(task, app.RequestedByHex, appInstaller.ID, "Ongoing", null);
                        if(await maintenance.Remove(appInstaller)){
                            _logger.LogInformation(task + " " + appInstaller.ID.ToString() + " was successfull at " + DateTimeOffset.Now);
                            logSynchronisation.UpdateLog(logID, "Success");
                        }else{
                            _logger.LogInformation(task + " " + appInstaller.ID.ToString() + " has ended with error at " + DateTimeOffset.Now);
                            logSynchronisation.UpdateLog(logID, "Failure");
                        }
                    }
                }
                return;
            }
        }
    }
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation(".NET MP Service successfully started");
        DateTime lastSync = DateTime.MinValue;
        DateTime lastVerify = DateTime.MinValue;
        await checkCredentials();
        Task<bool> synchroniseLogs = logSynchronisation.Synchronise();
        ///////
        string[] tasks = {"Synchronisation", "SwUpdate", "SwInstall", "SwUninstall", "Ready"};
        byte currentTask = 0;
        Task<ListOfChanges?>? appCheck = null;
        Task? appUpdate = null;
        //////////////////////////////////
        currentTask = 0;
        ListOfChanges? listOfChanges = null;
        while(!stoppingToken.IsCancellationRequested){
            if((DateTime.UtcNow - lastVerify) > TimeSpan.FromHours(24)){
                await WriteVerify();
                lastVerify = DateTime.UtcNow;
            }
            if(currentTask < 4){
                lastSync = DateTime.UtcNow;
                switch (currentTask){
                    case 0:
                        if(appCheck == null){
                            appCheck = communication.Synchronise();
                        }else if(appCheck.IsCompleted){
                            if(appCheck.Result == null){
                                appCheck = null;
                            }else{
                                listOfChanges = appCheck.Result;
                                currentTask++;
                            }
                        }
                        break;
                    case 1:
                        if(appUpdate == null){
                            appUpdate = installApps(listOfChanges.Updates, "SwUpdate");
                            appCheck = null;
                        }else if(appUpdate.IsCompleted){
                            currentTask++;
                            appUpdate = null;
                        }
                        break;
                    case 2:
                        if(appUpdate == null){
                            appUpdate = installApps(listOfChanges.Installs.ToArray(), "SwInstall");
                            appCheck = null;
                        }else if(appUpdate.IsCompleted){
                            currentTask++;
                            appUpdate = null;
                        }
                        break;
                    case 3:
                        if(appUpdate == null){
                            appUpdate = installApps(listOfChanges.Uninstalls.ToArray(), "SwUninstall");
                            appCheck = null;
                        }else if(appUpdate.IsCompleted){
                            currentTask++;
                            appUpdate = null;
                        }
                        break;
                }
            }else if((DateTime.UtcNow - lastSync) > TimeSpan.FromMinutes(2)){
                currentTask = 0;
            }
            if(synchroniseLogs.IsCompleted){
                synchroniseLogs = logSynchronisation.Synchronise();
            }
            await Task.Delay(1000, stoppingToken);
        }
    }
}
