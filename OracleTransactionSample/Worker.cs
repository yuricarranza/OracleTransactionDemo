using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;

namespace OracleTransactionSample
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IConfiguration _configuration;

        public Worker(ILogger<Worker> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);

                using (OracleConnection oracleConnection = new OracleConnection(_configuration.GetConnectionString("MyDatabase")))
                {
                    OracleTransaction oracleTransaction;
                    await oracleConnection.OpenAsync();
                    oracleTransaction = oracleConnection.BeginTransaction();
                    try
                    {
                        OracleCommand oracleCommand = new OracleCommand("delete from pruebaYuri", oracleConnection);
                        await oracleCommand.ExecuteNonQueryAsync();
                        oracleCommand.CommandText = "insert into pruebaYuri (nombre, edad) values (:nombre, :edad)";
                        oracleCommand.Parameters.Clear();
                        oracleCommand.Parameters.Add(":nombre", "Yuri");
                        oracleCommand.Parameters.Add(":edad", 28);
                        await oracleCommand.ExecuteNonQueryAsync();
                        oracleTransaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex.ToString());
                        oracleTransaction.Rollback();
                    }                    
                }                

                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}
