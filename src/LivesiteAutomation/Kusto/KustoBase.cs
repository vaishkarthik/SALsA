using Kusto.Cloud.Platform.Utils;
using LivesiteAutomation.Connectors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LivesiteAutomation.Kusto
{
    public abstract class KustoBase
    {
        protected abstract string Cluster { get; }
        protected abstract string DataBase { get; }
        protected abstract string Table { get; }

        protected int Icm;
        protected KustoClient kustoClient;
        protected string ContainerId;

        public KustoBase(int icm, string containerId)
        {
            this.Icm = icm;
            this.ContainerId = containerId;
            kustoClient = new KustoClient(Cluster, DataBase, Icm);
        }
    }
}
