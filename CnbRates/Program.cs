using CnbRates;
using CnbRates.Model;
using CnbRates.Services;
using Microsoft.EntityFrameworkCore;

var currenciesLookupService = new CurrencyLookupService();
var downloader = new ExchnageRateDownloader(currenciesLookupService);
var data = new List<ExchangeRate>();

const int Year = 2022;
DateTime day = new DateTime(Year, 1, 1);

while (day<DateTime.Today)
{
	if ((day.DayOfWeek != DayOfWeek.Saturday)
		&& (day.DayOfWeek != DayOfWeek.Sunday)
		&& (day <= DateTime.Today))
	{
		Console.WriteLine(day.ToString());
		var result = await downloader.DownloadDayAsync(day);
		data.AddRange(result);
	}
	day = day.AddDays(1);
}

var context = new CnbRatesDbContext();
await context.Database.MigrateAsync();

// Ĺoad Currencies
await context.Currencies.AddRangeAsync(currenciesLookupService.GetCurrencies());

// Load Rates
await context.ExchangeRates.AddRangeAsync(data);

// Statistics
var statsGen = new StatsGen(context);
const int jumpPercent = 5;
var averages = await statsGen.GetCurrencyAveragesAsync();
Console.WriteLine("Averages:");
foreach (var a in averages)
{
    Console.WriteLine($"{a.Item1}: {a.Item2}");
}

var jumps = await statsGen.GetCurrencyJumpsAsync(jumpPercent);
Console.WriteLine($"\nJumps bigger than {jumpPercent}%:");
foreach (var j in jumps)
{
	Console.WriteLine($"{j.Key}: {j.Value}");
}

await context.SaveChangesAsync();