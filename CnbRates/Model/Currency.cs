using System.ComponentModel.DataAnnotations;

namespace CnbRates.Model
{
	public class Currency
	{
		[Key]
		public int Id { get; set; }

		[Required, MaxLength(10)]
		public string Code { get; init; }

		[Required, MaxLength(100)]
		public string Country { get; init; }

		[Required, MaxLength(20)]
		public string Name { get; init; }
	}
}