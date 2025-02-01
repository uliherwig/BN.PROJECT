using BN.PROJECT.Core;
using Microsoft.CodeAnalysis.Elfie.Model;

namespace BN.PROJECT.AlpacaService;

[PersistJobDataAfterExecution]
[DisallowConcurrentExecution]
public class SendQuoteJob : IJob
{
    private readonly ILogger<SendQuoteJob> _logger;
    private readonly IConfiguration _configuration;
    private readonly IAlpacaDataService _alpacaDataService;
    private readonly IAlpacaTradingService _alpacaTradingService;
    private readonly IAlpacaRepository _alpacaRepository;
    private readonly IKafkaProducerService _kafkaProducer;

    public SendQuoteJob(
        ILogger<SendQuoteJob> logger,
        IConfiguration configuration,
        IAlpacaDataService alpacaDataService,
        IAlpacaTradingService alpacaTradingService,
        IAlpacaRepository alpacaRepository,
        IKafkaProducerService kafkaProducer)
    {
        _logger = logger;
        _configuration = configuration;
        _alpacaDataService = alpacaDataService;
        _alpacaRepository = alpacaRepository;
        _alpacaTradingService = alpacaTradingService;
        _kafkaProducer = kafkaProducer;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        JobKey key = context.JobDetail.Key;

        var executions = await _alpacaRepository.GetActiveAlpacaExecutionsAsync();
        if (executions == null || executions?.Count == 0)
        {
            return;
        }

        foreach (var exec in executions)
        {
            var quote = await _alpacaDataService.GetLatestQuoteBySymbol(exec.Assets);

            if (quote == null)
            {
                _logger.LogError("Quote not found for asset: " + exec.Assets);
                continue;
            }

            var q = new Quote
            {
                Symbol = quote.Symbol,
                AskPrice = quote.AskPrice,
                BidPrice = quote.BidPrice,
                TimestampUtc = quote.TimestampUtc
            };

            var message = new StrategyMessage
            {
                IsBacktest = false,
                UserId = exec.UserId,
                StrategyId = exec.StrategyId,
                Strategy = exec.StrategyType,
                MessageType = MessageTypeEnum.Quotes,
                Quotes = [q]
            };

            await _kafkaProducer.SendMessageAsync("strategy", message.ToJson());
        }

        _logger.LogInformation("Instance " + key + " Send Quote Job end");
    }
}