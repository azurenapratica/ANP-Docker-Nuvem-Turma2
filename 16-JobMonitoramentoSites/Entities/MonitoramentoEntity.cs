using Microsoft.Azure.Cosmos.Table;

namespace JobMonitoramentoSites.Entities
{
    public class MonitoramentoEntity : TableEntity
    {
        public MonitoramentoEntity(string status, string horario)
        {
            PartitionKey = status;
            RowKey = horario;
        }

        public MonitoramentoEntity() { }

        public string Local { get; set; }
        public string DadosLog { get; set; }
    }
}