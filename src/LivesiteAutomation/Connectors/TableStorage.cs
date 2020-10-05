﻿using Microsoft.Azure.Cosmos.Table;
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
            public SALsAEntity(string icm, string state)
            {
                this.PartitionKey = icm;
                this.RowKey = state;
            }

            public SALsAEntity() { }
            public string Log { get; set; }
        }

        private static List<SALsAEntity> ListAllEntity()
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
        public static void AppendEntity(int icm, SALsA.State state, string log = "")
        {
            try
            {
                SALsAEntity incident = new SALsAEntity(icm.ToString(), state.ToString());
                incident.Log = log;
                incident.Log = Guid.NewGuid().ToString();
                TableOperation insertOperation = TableOperation.InsertOrReplace(incident);
                Authentication.Instance.TableStorageClient.Execute(insertOperation);
            }
            catch (Exception ex)
            {
                SALsA.GetInstance(icm)?.Log.Error("Failed to get all Table entity.");
                SALsA.GetInstance(icm)?.Log.Exception(ex);
            }
        }

        // Get all entity and remvoe older ones.
        public static List<SALsAEntity> GetRecentEntity()
        {
            var allEntity = ListAllEntity();

            try
            { 
                TableBatchOperation op = new TableBatchOperation();
                foreach (SALsAEntity entity in allEntity)
                {
                    if(DateTime.Now.AddDays(Constants.TableStorageRecentDays) > entity.Timestamp)
                    {
                        op.Delete(entity);
                        allEntity.Remove(entity);
                    }
                }
                if(op.Count > 0)
                {
                    Authentication.Instance.TableStorageClient.ExecuteBatchAsync(op);
                }
            }
            catch /*(Exception ex)*/
            {
                return null;
            }
            return allEntity;
        }

        // Get all entity and remvoe older ones.
        public static SALsAEntity GetEntity(int icm)
        {
            try
            {
                TableQuery<SALsAEntity> query = new TableQuery<SALsAEntity>().Where($"PartitionKey eq {icm}").Take(1);
                return Authentication.Instance.TableStorageClient.ExecuteQuery(query).FirstOrDefault();
            }
            catch (Exception ex)
            {
                SALsA.GetInstance(icm)?.Log.Error("Failed to get Entity.");
                SALsA.GetInstance(icm)?.Log.Exception(ex);
                return null;
            }
        }
    }
}
