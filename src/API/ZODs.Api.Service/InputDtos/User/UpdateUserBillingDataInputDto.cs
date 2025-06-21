namespace ZODs.Api.Service.InputDtos.User
{
    public sealed class UpdateUserBillingDataInputDto
    {
        public string CardLast4 { get; set; } = null!;

        public string CardBrand { get; set; } = null!;

        public string UpdatePaymentMethodUrl { get; set; } = null!;

        public UpdateUserBillingDataInputDto(string cardLast4, string cardBrand, string updatePaymentMethodUrl)
        {
            CardLast4 = cardLast4;
            CardBrand = cardBrand;
            UpdatePaymentMethodUrl = updatePaymentMethodUrl;
        }
    }
}