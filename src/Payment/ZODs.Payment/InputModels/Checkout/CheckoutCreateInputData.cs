namespace ZODs.Payment.InputModels.Checkout;

public class CheckoutCreateInputData : PaymentInputData<PaymentCheckoutRelationships, PaymentCheckoutAttributesInputModel>
{
    public CheckoutCreateInputData(
        string type, 
        PaymentCheckoutRelationships? relationships, 
        PaymentCheckoutAttributesInputModel attributes) 
        : base(type, relationships, attributes)
    {
    }
}