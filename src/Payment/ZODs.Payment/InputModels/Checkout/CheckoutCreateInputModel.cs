using ZODs.Payment.Constants;
using ZODs.Payment.Models.Product;

namespace ZODs.Payment.InputModels.Checkout
{
    public sealed class CheckoutCreateInputModel(CheckoutCreateInputData data)
        : PaymentInputPayload<CheckoutCreateInputData>(data)
    {
        public static CheckoutCreateInputModel Create(
           string storeId,
           int variantId,
           string email,
           ICollection<string> customData,
           decimal? customPrice = null,
           string? discountCode = null,
           PaymentProductOptions? productOptions = null,
           PaymentCheckoutOptions? checkoutOptions = null,
           bool testMode = false)
        {
            var checkoutData = new PaymentCheckoutData(
                email,
                custom: customData,
                discountCode);

            var attributes = new PaymentCheckoutAttributesInputModel(
                checkoutData,
                productOptions,
                checkoutOptions,
                customPrice,
                testMode);

            var relationships = new PaymentCheckoutRelationships(
                store: new Relationship(LemonSqueezyObjects.Stores, storeId),
                variant: new Relationship(LemonSqueezyObjects.Variants, variantId.ToString()));

            var data = new CheckoutCreateInputData(
                LemonSqueezyObjects.Checkouts,
                relationships,
                attributes);

            return new CheckoutCreateInputModel(data);
        }
    }
}