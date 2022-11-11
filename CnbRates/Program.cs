using CnbRates.Model;
using CnbRates.Services;

var currenciesLookupService = new CurrencyLookupService();
var downloader = new ExchnageRateDownloader(currenciesLookupService);
var data = new List<ExchangeRate>();

const int Year = 2022;
DateTime day = new(Year, 1, 1);
while (day < DateTime.Today)
{
	if ((day.DayOfWeek != DayOfWeek.Saturday)
		&& (day.DayOfWeek != DayOfWeek.Sunday))
	{
		Console.WriteLine(day.ToString());
		var result = await downloader.DownloadDayAsync(day);
		data.AddRange(result);
	}
	day = day.AddDays(1);
}

var persister = new DbPersister();
await persister.SaveCurrenciesAsync(currenciesLookupService.GetCurrencies());
await persister.SaveRatesAsync(data);
await persister.GetCurrencyAveragesAsync();
Console.WriteLine();
await persister.GetCurrencyJumpsAsync(); // Tohle je nejošklivější, nejzoufalejší, nejneudržitelnější kus kódu, co jsem kdy napsal