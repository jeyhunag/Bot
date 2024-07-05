using Alpaca.Markets;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

internal static class Program
{
    private const string KEY_ID = "AKW9FCU5WJNHD4T83LX8";
    private const string SECRET_KEY = "qC1Xk81V4UJfp2nC2sgUJuiuTRG4B2Dy2Gv30zV6";

    public static async Task Main()
    {
        var tradingClient = Environments.Live.GetAlpacaTradingClient(new SecretKey(KEY_ID, SECRET_KEY));
        var dataClient = Environments.Live.GetAlpacaDataClient(new SecretKey(KEY_ID, SECRET_KEY));

        var clock = await tradingClient.GetClockAsync();

        if (clock != null)
        {
            Console.WriteLine("Timestamp: {0}, NextOpen: {1}, NextClose: {2}",
                clock.TimestampUtc, clock.NextOpenUtc, clock.NextCloseUtc);
        }

        // Məsələn, 'AAPL' üçün son 100 günün tarixi bar məlumatlarını çəkirik.
        var barRequest = new HistoricalBarsRequest("AAPL", DateTime.UtcNow.AddDays(-100), DateTime.UtcNow, BarTimeFrame.Day);
        var bars = await dataClient.ListHistoricalBarsAsync(barRequest);

        // Moving averages hesablayırıq
        List<decimal> closes = bars.Items.Select(bar => bar.Close).ToList();
        var shortTermMovingAverage = closes.TakeLast(10).Average(); // Son 10 gün
        var longTermMovingAverage = closes.TakeLast(30).Average(); // Son 30 gün

        // Strategiya tətbiqi
        if (shortTermMovingAverage > longTermMovingAverage)
        {
            Console.WriteLine("Al sinyalı!");
            // Al sinyalında alış əmri yaradırıq
            var orderRequest = new NewOrderRequest("AAPL", OrderQuantity.Notional(1000), OrderSide.Buy, OrderType.Market, TimeInForce.Day);
            await tradingClient.PostOrderAsync(orderRequest);
        }
        else if (shortTermMovingAverage < longTermMovingAverage)
        {
            Console.WriteLine("Sat sinyalı!");
            // Sat sinyalında satış əmri yaradırıq
            var orderRequest = new NewOrderRequest("AAPL", OrderQuantity.Notional(1000), OrderSide.Sell, OrderType.Market, TimeInForce.Day);
            await tradingClient.PostOrderAsync(orderRequest);
        }
    }
}
