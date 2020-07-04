using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using MonitoramentoSites.Documents;

namespace MonitoramentoSites
{
    public class Worker : BackgroundService
    {
        private const string IDENTIFICACAO_WORKER = "MonitoramentoSites";
        private readonly ILogger<Worker> _logger;
        private readonly IConfiguration _configuration;

        public Worker(ILogger<Worker> logger,
            IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation(
                $"{IDENTIFICACAO_WORKER} - iniciando execução em: {DateTime.Now}");

            var mongoClient = new MongoClient(
                _configuration.GetConnectionString("BaseMonitoramentoSites"));
            var db = mongoClient.GetDatabase("DBMonitoramento");
            var disponibilidadeCollection =
                db.GetCollection<ResultadoMonitoramento>("Disponibilidade");

            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);

                var sites = _configuration["Sites"]
                    .Split("|", StringSplitOptions.RemoveEmptyEntries);
                foreach (string site in sites)
                {
                    var dadosLog = new ResultadoMonitoramento()
                    {
                        Site = site,
                        Horario =
                            DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                    };

                    using (var client = new HttpClient())
                    {
                        client.BaseAddress = new Uri(site);
                        client.DefaultRequestHeaders.Accept.Clear();

                        try
                        {
                            // Envio da requisicao a fim de determinar se
                            // o site esta no ar
                            HttpResponseMessage response =
                                client.GetAsync("").Result;

                            dadosLog.Status = (int)response.StatusCode + " " +
                                response.StatusCode;
                            if (response.StatusCode != System.Net.HttpStatusCode.OK)
                                dadosLog.DescricaoErro = response.ReasonPhrase;
                        }
                        catch (Exception ex)
                        {
                            dadosLog.Status = "Exception";
                            dadosLog.DescricaoErro = ex.Message;
                        }
                    }

                    string jsonResultado =
                        JsonSerializer.Serialize(dadosLog);

                    if (dadosLog.DescricaoErro == null)
                        _logger.LogInformation(jsonResultado);
                    else
                        _logger.LogError(jsonResultado);

                    disponibilidadeCollection.InsertOne(dadosLog);
                }

                await Task.Delay(Convert.ToInt32(
                    _configuration["IntervaloExecucao"]), stoppingToken);
            }
        }
    }
}