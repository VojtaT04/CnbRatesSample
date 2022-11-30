using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CnbRates.Services
{
    internal class StatsGen
    {
        public CnbRatesDbContext Context { get; set; }
        public StatsGen(CnbRatesDbContext context)
        {
            Context = context;
        }

        public async Task<List<(string, decimal)>> GetCurrencyAveragesAsync()
        {
            var res = new List<(string, decimal)>();
            var dataEval = await(
                from a in
                (
                    from e in Context.ExchangeRates
                    join c in Context.Currencies on e.CurrencyId equals c.Id
                    select new
                    {
                        Code = c.Code,
                        Rate = e.Rate
                    }
                )
                group a by a.Code into newA
                select new
                {
                    Code = newA.Key,
                    AverageRate = newA.Average(x => x.Rate)
                }).ToListAsync();

            foreach (var d in dataEval)
            {
                res.Add((d.Code, d.AverageRate));
            }
            return res;
        }

        public async Task<Dictionary<string, int>> GetCurrencyJumpsAsync(int jump)
        {
            var res = new Dictionary<string, int>();
            var rates =await (
                from e in Context.ExchangeRates
                join c in Context.Currencies on e.CurrencyId equals c.Id
                orderby c.Code, c.Id
                select new
                {
                    Code = c.Code,
                    Rate = e.Rate
                }).ToListAsync();

            for (int i = 0; i < rates.Count; i++)
            {
                if (i + 1 < rates.Count && rates[i].Code == rates[i + 1].Code)
                {
                    var bigger = (rates[i].Rate > rates[i + 1].Rate) ? rates[i].Rate: rates[i+1].Rate;
                    var smaller = (rates[i].Rate < rates[i + 1].Rate) ? rates[i].Rate : rates[i + 1].Rate;
                    if (bigger > smaller * (1 + jump / decimal.Parse("100"))) // tohle jde jistě i líp
                    {
                        if (!res.ContainsKey(rates[i].Code))
                        {
                            res.Add(rates[i].Code, 1);
                        }
                        else res[rates[i].Code]++;
                    }
                }
            }

            return res;
        }
    }
}
