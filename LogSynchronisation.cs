using System.Numerics;
using System.Text.Json;

namespace MPService{
    class LogSynchronisation{
        private SortedList<long, ServerLog> logQueue = new SortedList<long, ServerLog>();
        private long followingLog = 0;
        public long CreateLog(string Type, string? RequestedBy, long? SwID, string? ExitStatus, DateTime? dateTime){
            if(dateTime == null){
                dateTime = DateTime.UtcNow;
            }
            logQueue.Add(followingLog, new ServerLog(null, Type, RequestedBy, SwID, ExitStatus, dateTime));
            return followingLog++;
        }
        public void UpdateLog(long id, string? ExitStatus){
            lock(logQueue){
                if(logQueue.ContainsKey(id)){
                    logQueue[id].ExitStatus = ExitStatus;
                }
            }
                
        }
        private Communication communication;
        public LogSynchronisation(){
            communication = new Communication();
        }
        public async Task<bool> Synchronise(){
            foreach(KeyValuePair<long, ServerLog> kvp in logQueue){
                    string sentExitStatus = kvp.Value.ExitStatus.ToString();
                    long? id = await communication.WriteLog(kvp.Value);
                    if(id != null){
                        if(sentExitStatus != "Ongoing"){
                            logQueue.Remove(kvp.Key);
                        }else{
                            kvp.Value.IdEvent = id;
                        }
                    }
                    else{
                        return false;
                    }
            }
            return true;
        }
    }
}
