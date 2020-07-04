using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.Azure.Cosmos.Table;
using Serilog;
using JobMonitoramentoSites.Entities;

namespace JobMonitoramentoSites
{
    class Program
    {
        static void Main()
        {
            var logger = new LoggerConfiguration()
                .WriteTo.Console()
                .CreateLogger();

            var builder = new ConfigurationBuilder()
             .SetBasePath(Directory.GetCurrentDirectory())
             .AddJsonFile($"appsettings.json")
             .AddEnvironmentVariables();
            var config = builder.Build();

            try
            {
                var serviceConfigurations = new ServiceConfigurations();
                new ConfigureFromConfigurationOptions<ServiceConfigurations>(
                    config.GetSection("ServiceConfigurations"))
                        .Configure(serviceConfigurations);

                var jsonOptions = new JsonSerializerOptions()
                {
                    IgnoreNullValues = true
                };

                var storageAccount = CloudStorageAccount
                    .Parse(config["BaseMonitoramento"]);
                var monitoramentoTable = storageAccount
                    .CreateCloudTableClient().GetTableReference("Monitoramento");
                if (monitoramentoTable.CreateIfNotExistsAsync().Result)
                    logger.Information("Criando a tabela de log...");

                foreach (string host in serviceConfigurations.Hosts)
                {
                    logger.Information(
                        $"Verificando a disponibilidade do host {host}");

                    var resultado = new ResultadoMonitoramento();
                    resultado.Horario =
                        DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    resultado.Host = host;

                    using (var client = new HttpClient())
                    {
                        client.BaseAddress = new Uri(host);
                        client.DefaultRequestHeaders.Accept.Clear();

                        try
                        {
                            // Envio da requisicao a fim de determinar se
                            // o site esta no ar
                            HttpResponseMessage response =
                                client.GetAsync("").Result;

                            resultado.Status = (int)response.StatusCode + " " +
                                response.StatusCode;
                            if (response.StatusCode != HttpStatusCode.OK)
                                resultado.Exception = response.ReasonPhrase;
                        }
                        catch (Exception ex)
                        {
                            resultado.Status = "Exception";
                            resultado.Exception = ex.Message;
                        }
                    }

                    // Imprimindo o resultado do teste
                    string jsonResultado =
                        JsonSerializer.Serialize(resultado, jsonOptions);

                    if (resultado.Exception == null)
                        logger.Information(jsonResultado);
                    else
                        logger.Error(jsonResultado);

                    // Gravando o resultado utilizando Azure Table Storage
                    MonitoramentoEntity dadosMonitoramento =
                        new MonitoramentoEntity(
                            "JobMonitoramentoSites", resultado.Horario);
                    dadosMonitoramento.Local = Environment.MachineName;
                    dadosMonitoramento.DadosLog = jsonResultado;

                    var insertOperation = TableOperation.Insert(dadosMonitoramento);
                    var resultInsert = monitoramentoTable.ExecuteAsync(insertOperation).Result;
                    logger.Information(JsonSerializer.Serialize(resultInsert));

                    Thread.Sleep(3000);
                }

                Environment.Exit(0);
            }
            catch (Exception ex)
            {
                logger.Error(ex.GetType().FullName + " - " + ex.Message);
                Environment.Exit(1);
            }
        }
    }
}