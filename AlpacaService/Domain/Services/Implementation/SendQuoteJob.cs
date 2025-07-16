namespace BN.PROJECT.AlpacaService;

[PersistJobDataAfterExecution]
[DisallowConcurrentExecution]
public class SendQuoteJob : IJob
{
    private readonly ILogger<SendQuoteJob> _logger;
    private readonly IConfiguration _configuration;
    private readonly IAlpacaDataService _alpacaDataService;
    private readonly IAlpacaRepository _alpacaRepository;
    private readonly IKafkaProducerService _kafkaProducer;
    private readonly IHubContext<AlpacaHub> _hubContext;
    private readonly IDatabase _redisDatabase;

    public SendQuoteJob(
        ILogger<SendQuoteJob> logger,
        IConfiguration configuration,
        IAlpacaDataService alpacaDataService,
        IAlpacaRepository alpacaRepository,
        IKafkaProducerService kafkaProducer,
        IHubContext<AlpacaHub> hubContext,
        IConnectionMultiplexer redis)
    {
        _logger = logger;
        _configuration = configuration;
        _alpacaDataService = alpacaDataService;
        _alpacaRepository = alpacaRepository;
        _kafkaProducer = kafkaProducer;
        _hubContext = hubContext;
        _redisDatabase = redis.GetDatabase();

    }

    public async Task Execute(IJobExecutionContext context)
    {
        JobKey key = context.JobDetail.Key;
        // TODO use Redis
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
                //MessageType = MessageTypeEnum.Quotes,
                Quotes = [q]
            };

            await _kafkaProducer.SendMessageAsync("strategy", message.ToJson());

            var quoteMessage = new QuoteMessage
            {
                UserId = exec.UserId,
                Symbol = quote.Symbol,
                AskPrice = quote.AskPrice,
                BidPrice = quote.BidPrice,
                TimestampUtc = quote.TimestampUtc
            };

            var connectionId = await _redisDatabase.StringGetAsync(exec.UserId.ToString());
            if (!string.IsNullOrEmpty(connectionId.ToString()))
            {
                await _hubContext.Clients.Client(connectionId).SendAsync("ReceiveQuote", message);
            }
        }

        _logger.LogInformation("Instance " + key + " Send Quote Job end");
    }
}
