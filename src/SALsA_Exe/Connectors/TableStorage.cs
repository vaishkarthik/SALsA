using Microsoft.Azure.Cosmos.Table;
using System;
using System.Collections.Generic;
using System.IO; 
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SALsA.General;

namespace SALsA.LivesiteAutomation
{
    public static class TableStorage
    {
        public class SALsAEntity : TableEntity
        {
            public SALsAEntity(string icm)
            {
                this.PartitionKey = icm;
                this.RowKey = icm;
            }

            public SALsAEntity() { }
            public string SALsALog { get; set; }
            public string Log { get; set; }
            public string SALsAState { get; set; }
        }

        public static List<SALsAEntity> ListAllEntity()
        {
            try
            {
                TableQuery<SALsAEntity> query = new TableQuery<SALsAEntity>();
                return Authentication.Instance.TableStorageClient.ExecuteQuery(query).ToList();
            }
            catch /*(Exception ex)*/
            {
                return null;
            }
        }

        public static void AppendEntity(string icm, SALsAState state, string salsaLog = "", string log = "")
        {
            int icmId;
            if (int.TryParse(icm, out icmId))
            {
                AppendEntity(icmId, state, salsaLog, log);
            }
        }

        public static void RemoveEntity(SALsAEntity entity)
        {
            TableOperation insertOperation = TableOperation.Delete(entity);
            Authentication.Instance.TableStorageClient.Execute(insertOperation);
        }

        public static void AppendEntity(int icm, SALsAState state, string salsaLog = "", string log = "")
        {
            try
            {
                SALsAEntity incident = new SALsAEntity(icm.ToString());
                incident.SALsALog = salsaLog;
                incident.Log = log;
                incident.SALsAState = state.ToString();
                TableOperation insertOperation = TableOperation.InsertOrReplace(incident);
                Authentication.Instance.TableStorageClient.Execute(insertOperation);
            }
            catch (Exception ex)
            {
                Log.Error("Failed to get all Table entity.");
                Log.Exception(ex);
            }
        }

        // Get all entity and remvoe older ones.
        public static List<SALsAEntity> CleanRecentEntity()
        {
            var allEntity = ListAllEntity();
            var icmEntity = ICM.GetIncidentsWithId(allEntity.Select(x => x.PartitionKey).ToList());

            foreach (SALsAEntity entity in allEntity)
            {
                var status = icmEntity[entity.PartitionKey].Status;
                if (status.Equals("Resolved", StringComparison.InvariantCultureIgnoreCase) && DateTime.Now.AddDays(Constants.TableStorageRecentDays) > entity.Timestamp)
                {
                    TableOperation deleteOperation = TableOperation.Delete(entity);
                    Authentication.Instance.TableStorageClient.Execute(deleteOperation);
                }
            }
            allEntity.RemoveAll(x => DateTime.Now.AddDays(Constants.TableStorageRecentDays) > x.Timestamp);

            return allEntity;
        }

        // Get all entity and remvoe older ones.
        public static SALsAEntity GetEntity(int icm)
        {
            try
            {
                TableQuery<SALsAEntity> query = new TableQuery<SALsAEntity>().Where($"PartitionKey eq '{icm}'").Take(1);
                return Authentication.Instance.TableStorageClient.ExecuteQuery(query).FirstOrDefault();
            }
            catch (Exception ex)
            {
                Log.Error("Failed to get Entity.");
                Log.Exception(ex);
                return null;
            }
        }
    }
}
