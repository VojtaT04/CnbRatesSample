using CnbRates.Model;
using Microsoft.Data.SqlClient;

namespace CnbRates.Services
{
	public class DbPersister
	{
		

		public async Task<int> GetCurrencyIdAsync(Currency currency)
        {
			using var conn = new SqlConnection("Server=(localdb)\\mssqllocaldb;Database=ExchangeRates;");

			await conn.OpenAsync();

			var cmd = new SqlCommand("SELECT [Id] FROM [Currencies] WHERE [Code] = @Code");
			cmd.Connection = conn;
			cmd.Parameters.AddWithValue("@Code", currency.Code);

			using var reader = await cmd.ExecuteReaderAsync();
			if (reader.Read())
			{
				var id = int.Parse(reader[0].ToString());
				await conn.CloseAsync();
				return id;
			}
			return -1;
		}

		public async Task<string> GetCurrencyCodeAsync(int currencyId)
        {
			using var conn = new SqlConnection("Server=(localdb)\\mssqllocaldb;Database=ExchangeRates;");

			await conn.OpenAsync();

			var cmd = new SqlCommand("SELECT [Code] FROM [Currencies] WHERE [Id] = @Id");
			cmd.Connection = conn;
			cmd.Parameters.AddWithValue("@Id", currencyId);

			using var reader = await cmd.ExecuteReaderAsync();
			if (reader.Read())
			{
				var name = reader[0].ToString();
				await conn.CloseAsync();
				return name;
			}
			return "???";
		}

		public async Task<int> GetExchangeRateIdAsync(ExchangeRate exchangeRate)
		{
			using var conn = new SqlConnection("Server=(localdb)\\mssqllocaldb;Database=ExchangeRates;");

			await conn.OpenAsync();

			var cmd = new SqlCommand("SELECT [Id] FROM [ExchangeRates] WHERE [Day] = @Day AND [CurrencyId] = @CurrencyId");
			cmd.Connection = conn;
			cmd.Parameters.AddWithValue("@Day", exchangeRate.Day);
			cmd.Parameters.AddWithValue("@CurrencyId", await GetCurrencyIdAsync(exchangeRate.Currency));

			using var reader = await cmd.ExecuteReaderAsync();
			if (reader.Read())
			{
				var id = int.Parse(reader[0].ToString());
				await conn.CloseAsync();
				return id;
			}
			return -1;
		}

		public async Task<int> GetCurrencyCountAsync()
		{
			using var conn = new SqlConnection("Server=(localdb)\\mssqllocaldb;Database=ExchangeRates;");

			await conn.OpenAsync();

			var cmd = new SqlCommand("SELECT Count(*) FROM [Currencies]");
			cmd.Connection = conn;

			using var reader = await cmd.ExecuteReaderAsync();
			if (reader.Read())
			{
				var count = int.Parse(reader[0].ToString());
				await conn.CloseAsync();
				return count;
			}
			return -1;
		}

		public async Task<int> GetExchangeRateCountAsync()
		{
			using var conn = new SqlConnection("Server=(localdb)\\mssqllocaldb;Database=ExchangeRates;");

			await conn.OpenAsync();

			var cmd = new SqlCommand("SELECT Count(*) FROM [ExchangeRates]");
			cmd.Connection = conn;

			using var reader = await cmd.ExecuteReaderAsync();
			if (reader.Read())
			{
				var count = int.Parse(reader[0].ToString());
				await conn.CloseAsync();
				return count;
			}
			return -1;
		}

		public async Task<int> GetLowestFreeCurrencyIdAsync()
		{
			using var conn = new SqlConnection("Server=(localdb)\\mssqllocaldb;Database=ExchangeRates;");

			await conn.OpenAsync();

			var i = await GetCurrencyCountAsync();
			while (true)
			{
				var cmd = new SqlCommand("SELECT [Id] FROM [Currencies] WHERE [Id] = @Id");
				cmd.Connection = conn;
				cmd.Parameters.AddWithValue("@Id", i);
				using var reader = await cmd.ExecuteReaderAsync();
				if (!reader.Read()) { return i; }
					i++;
			}
		}

		public async Task<int> GetLowestFreeExchangeRateIdAsync()
		{
			using var conn = new SqlConnection("Server=(localdb)\\mssqllocaldb;Database=ExchangeRates;");

			await conn.OpenAsync();

			var i = await GetExchangeRateCountAsync();
			while (true)
			{
				var cmd = new SqlCommand("SELECT [Id] FROM [ExchangeRates] WHERE [Id] = @Id");
				cmd.Connection = conn;
				cmd.Parameters.AddWithValue("@Id", i);
				using var reader = await cmd.ExecuteReaderAsync();
				if (!reader.Read()) { return i; }
				i++;
			}
		}

		public async Task SaveCurrenciesAsync(IEnumerable<Currency> currencies)
		{
			using var conn = new SqlConnection("Server=(localdb)\\mssqllocaldb;Database=ExchangeRates;");

			await conn.OpenAsync();

			foreach (var currency in currencies)
			{
				if (await GetCurrencyIdAsync(currency) != -1) { continue; }

				var cmd = new SqlCommand("INSERT INTO [Currencies](Id, Code, Name, Country) VALUES(@Id, @Code, @Name, @Country)");
				cmd.Connection = conn;
				cmd.Parameters.AddWithValue("@Id", await GetLowestFreeCurrencyIdAsync());
				cmd.Parameters.AddWithValue("@Code", currency.Code);
				cmd.Parameters.AddWithValue("@Name", currency.Name);
				cmd.Parameters.AddWithValue("@Country", currency.Country);
				await cmd.ExecuteNonQueryAsync();
			}

			await conn.CloseAsync();
		}
		
		public async Task SaveRatesAsync(IEnumerable<ExchangeRate> eRates)
		{
			using var conn = new SqlConnection("Server=(localdb)\\mssqllocaldb;Database=ExchangeRates;");

			await conn.OpenAsync();

			foreach (var eRate in eRates)
			{
				if (await GetExchangeRateIdAsync(eRate) != -1) { continue; }

				var cmd = new SqlCommand("INSERT INTO [ExchangeRates](Id, Day, Rate, CurrencyId) VALUES(@Id, @Day, @Rate, @CurrencyId)");
				cmd.Connection = conn;
				cmd.Parameters.AddWithValue("@Id", await GetLowestFreeExchangeRateIdAsync());
				cmd.Parameters.AddWithValue("@Day", eRate.Day);
				cmd.Parameters.AddWithValue("@Rate", eRate.Rate);
				cmd.Parameters.AddWithValue("@CurrencyId", await GetCurrencyIdAsync(eRate.Currency));
				await cmd.ExecuteNonQueryAsync();
			}

			await conn.CloseAsync();
		}

		public async Task GetCurrencyAveragesAsync()
        {
			using var conn = new SqlConnection("Server=(localdb)\\mssqllocaldb;Database=ExchangeRates;");

			await conn.OpenAsync();
            Console.WriteLine("CODE    AVERAGE    VOLATILITY\n");
			var cmd = new SqlCommand("SELECT DISTINCT Code AS 'Currency Code', AVG([ExchangeRates].[Rate]) OVER(PARTITION BY[CurrencyId]) AS 'Average Rate', (STDEV([ExchangeRates].[Rate]) OVER(PARTITION BY [CurrencyId]))/(AVG([ExchangeRates].[Rate]) OVER(PARTITION BY [CurrencyId])) * 100 AS 'Volatility %' FROM[ExchangeRates] JOIN[Currencies] ON[ExchangeRates].[CurrencyId] = [Currencies].[Id] ORDER BY[Volatility %]");
			cmd.Connection = conn;

			using var reader = await cmd.ExecuteReaderAsync();
			while (reader.Read())
			{
				var code = reader[0].ToString();
				var avgRate = decimal.Parse(reader[1].ToString());
				var volatility = decimal.Parse(reader[2].ToString());
				Console.WriteLine($"{code}     {decimal.Round(avgRate, avgRate < 10 ? 5 : avgRate < 100 ? 4 : 3)}     {decimal.Round(volatility, 5)}%");
			}
			await conn.CloseAsync();
		}

		public async Task GetCurrencyJumpsAsync()
		{
			using var conn = new SqlConnection("Server=(localdb)\\mssqllocaldb;Database=ExchangeRates;");

			await conn.OpenAsync();
			var cmd = new SqlCommand("SELECT DISTINCT COUNT(*) OVER (ORDER BY currencyId) AS Cnt, currencyId FROM(SELECT Rate, lead(ExchangeRates.Rate) OVER (ORDER BY currencyId) AS nextRate, currencyId FROM ExchangeRates) AS tmp WHERE tmp.nextRate > tmp.Rate * 1.01 OR tmp.nextRate < tmp.Rate * 0.99 ORDER BY currencyid"); // Fakt netuším jak/jestli to funguje, ale po hodině hraní si v SSMS to vypadá, že to něco dělá.
			cmd.Connection = conn;
			Console.WriteLine("CODE    >1% Jumps\n");
			var prevCount = 0;
			using var reader = await cmd.ExecuteReaderAsync();
			while (reader.Read())
			{
				var count = int.Parse(reader[0].ToString());
				var currencyId = int.Parse(reader[1].ToString());
				var currencyCode = await GetCurrencyCodeAsync(currencyId);
				Console.WriteLine($"{currencyCode}        {count-prevCount}");
				prevCount=count;
			}
			await conn.CloseAsync();
		}
	}
}
