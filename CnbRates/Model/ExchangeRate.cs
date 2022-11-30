using System.ComponentModel.DataAnnotations;

namespace CnbRates.Model
{
	public record ExchangeRate
	{
		[Key]
		public int Id { get; set; }

		[Required]
		public DateTime Day { get; set; }

		[Required]
		public Currency Currency { get; set; }
		public int CurrencyId { get; set; }

		[Required]
		public decimal Rate { get; set; }
	}
}
