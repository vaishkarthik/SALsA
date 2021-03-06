﻿using Kusto.Cloud.Platform.Utils;
using SALsA.LivesiteAutomation.Connectors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Drawing;
using Microsoft.IdentityModel.Protocols.WSIdentity;
using SALsA.General;

namespace SALsA.LivesiteAutomation.Kusto
{
    public abstract class KustoBase<MessageLine> where MessageLine : new()
    {
        public static string DefaultStartTime = DateTime.Today.AddDays(-7).ToUniversalTime().ToString("o");
        protected abstract string Cluster { get; }
        protected abstract string DataBase { get; }
        protected abstract string Table { get; }

        virtual protected int KustoBaseLimit { get { return Constants.KustoClientQueryLimit; } }

        protected bool WriteToIcm;
        protected int Icm;
        protected KustoClient kustoClient;

        protected List<object[]> RawResults { get { if (InitTask.IsCompleted == false) { InitTask.Wait(); } return _RawResults; } }
        protected List<object[]> _RawResults;
        public MessageLine[] Results { get { if (InitTask.IsCompleted == false) { InitTask.Wait(); } return _Results; } }
        protected MessageLine[] _Results;
        public string HTMLResults { get { if (InitTask.IsCompleted == false) { InitTask.Wait(); } return _HTMLResults; } }
        protected string _HTMLResults;

        protected string KustoQuery;
        private Task InitTask;

        protected abstract void GenerateKustoQuery();

        public KustoBase(int icm, bool send = false)
        {
            this.Icm = icm;
            this.WriteToIcm = send;
        }

        virtual protected void Init()
        {
            InitTask = new Task(() =>
            {
                try
                {
                    kustoClient = new KustoClient(Cluster, DataBase, Icm);
                    GenerateKustoQuery();
                    _RawResults = kustoClient.Query(Table, ref KustoQuery, this.Icm, null, KustoBaseLimit);
                    BuildResult();
                    GenerateHTMLResult();
                }
                catch (Exception ex)
                {
                    Log.Critical("Failed to query Kusto {0}.{1}.{2}", Cluster, DataBase, Table);
                    Log.Exception(ex);
                }
            });
            InitTask.Start();
            SALsA.GetInstance(this.Icm)?.TaskManager.AddOneTask(this.InitTask);
        }

        protected virtual void GenerateHTMLResult()
        {
            if (_Results.Length == 0)
            {
                Log.Information("Kusto query for {0}.{1}.{2} yielded empty results. Will skip.", Cluster, DataBase, Table);
                _HTMLResults = null;
                return;
            }
            var query = KustoQuery.Replace("|", Environment.NewLine + "|");
            query = String.Format("cluster('{0}').database('{1}').{2}{3}", Cluster, DataBase, query, Environment.NewLine);
            var htmlOut = Utility.List2DToHTML(_RawResults, fromKusto: true);
            var header = String.Format("<p>Execute: [<a href=\"https://dataexplorer.azure.com/clusters/{0}.kusto.windows.net/databases/{1}?query={2}\">Web</a>] [<a href=\"https://{0}.kusto.windows.net/{1}?query={2}&amp;web=0\">Desktop</a>] [<a href=\"https://lens.msftcloudes.com/v2/#/discover/query//results?datasource=(cluster:{0}.kusto.windows.net,database:{1},type:Kusto)&amp;query={2}&amp;runquery=1\">Web (Lens)</a>] [<a href=\"https://{0}.kusto.windows.net/{1}?query={2}&amp;saw=1\">Desktop (SAW)</a>] <a href=\"https://{0}.kusto.windows.net/{1}\">https://{0}.kusto.windows.net/{1}</a></p><pre><code>{3}</code></pre>",
                this.Cluster, this.DataBase, Utility.Base64Encode(Utility.CompressString(query)), Utility.EncodeHtml(query));
            _HTMLResults = String.Format("{0}{1}", header, htmlOut);
            if (WriteToIcm == true)
            {
                SALsA.GetInstance(this.Icm)?.ICM.QueueICMDiscussion(_HTMLResults, htmlfy: false);
            }
        }

        protected virtual void BuildResult()
        {
            MessageLine[] messages = new MessageLine[_RawResults.Count - 1];
            for (int i = 1; i < _RawResults.Count; ++i)
            {
                MessageLine line = new MessageLine();
                for (int j = 0; j < _RawResults[i].Length; ++j)
                {
                    System.Reflection.PropertyInfo pinfo = typeof(MessageLine).GetProperty(((DataColumn)_RawResults[0][j]).Caption);
                    if (pinfo != null)
                    {
                        try
                        {
                            pinfo.SetValue(line, _RawResults[i][j]);
                        }
                        catch (Exception ex)
                        {
                            Log.Warning("While processing {0}.{1}.{2}, failed to assign value : \"{3}\") to field {4}. Exception : {5}"
                                , Cluster, DataBase, Table, _RawResults[i][j], _RawResults[0][j], ex);
                        }
                    }
                }
                messages[i - 1] = line;
            }
            _Results = messages;
        }

        // tools
        public string InitStartTime(string dateTime = null)
        {
            if (dateTime != null)
            {
                return dateTime;
            }

            var _startTime = SALsA.GetInstance(Icm).ICM.GetCustomField(Constants.AnalyzerStartTimeField);
            if (_startTime != null)
            {
                try 
                {
                    return DateTime.Parse(_startTime).AddDays(-1).ToUniversalTime().ToString("o");
                }
                catch { }
            }
            return Kusto.KustoBase<DateTime>.DefaultStartTime;
        }
    }
}
