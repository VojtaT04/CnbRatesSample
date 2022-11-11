using System.Globalization;
using CnbRates.Model;

namespace CnbRates.Services
{
	public class ExchnageRateDownloader
	{
		private readonly CurrencyLookupService currencyLookupService;

		public ExchnageRateDownloader(CurrencyLookupService currencyLookupService)
		{
			this.currencyLookupService = currencyLookupService;
		}

		public async Task<IEnumerable<ExchangeRate>> DownloadDayAsync(DateTime day)
		{
			using var httpClient = new HttpClient();
			var url = $"https://www.cnb.cz/cs/financni-trhy/devizovy-trh/kurzy-devizoveho-trhu/kurzy-devizoveho-trhu/denni_kurz.txt;?date={day:g}";
			var content = await httpClient.GetStringAsync(url);

			return ParseContent(content, day);
		}

		private IEnumerable<ExchangeRate> ParseContent(string content, DateTime requestedDate)
		{
			var rows = content.Split('\n');

			// verify header
			if (!DateTime.TryParse(rows[0].Substring(0, rows[0].IndexOf(' ')),
				out var parsedDate) || (parsedDate != requestedDate))
			{
				return Enumerable.Empty<ExchangeRate>();
			}

			var rates = new List<ExchangeRate>(rows.Length - 2);
			for (int i = 2; i < rows.Length; i++)
			{
				var exchangeRate = ParseRow(rows[i], requestedDate);
				if (exchangeRate is not null)
				{
					rates.Add(exchangeRate);
				}
			}
			return rates;
		}

		private ExchangeRate ParseRow(string row, DateTime requestedDate)
		{
			var segments = row.Split('|');
			if (segments.Length != 5)
			{
				return null;
			}
			var rate = new ExchangeRate();
			rate.Currency = currencyLookupService.GetOrAdd(segments[3], segments[0], segments[1]);
			rate.Day = requestedDate;
			var amount = int.Parse(segments[2]);
			var nominalRate = decimal.Parse(segments[4], CultureInfo.GetCultureInfo("cs-cz"));
			rate.Rate = nominalRate / amount;

			return rate;
		}
	}
}
