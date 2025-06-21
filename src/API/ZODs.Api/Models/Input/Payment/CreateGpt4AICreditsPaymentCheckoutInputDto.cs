using System.ComponentModel.DataAnnotations;

namespace ZODs.Api.Models.Input.Payment;

public class CreateGpt4AICreditsPaymentCheckoutInputDto
{
    [Range(10_000, 50_000, ErrorMessage = "Credits amount must be between 10,000 and 50,000")]
    public int CreditsAmount { get; set; }
}
