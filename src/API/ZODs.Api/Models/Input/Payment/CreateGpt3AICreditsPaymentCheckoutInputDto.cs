using System.ComponentModel.DataAnnotations;

namespace ZODs.Api.Models.Input.Payment;

public sealed class CreateGpt3AICreditsPaymentCheckoutInputDto
{
    [Range(200_000, 1_000_000, ErrorMessage = "Credits amount must be between 200,000 and 1,000,000")]
    public int CreditsAmount { get; set; }
}
