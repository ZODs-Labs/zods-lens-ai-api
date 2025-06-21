namespace ZODs.Api.Common.Extensions
{
    public static class ValidationMessageExtensions
    {
        public static string EmptyGuidValidationMessage(this string propertyName) => $"{propertyName} should not be empty guid.";

        public static string NotFoundValidationMessage(this Type type, object id) =>
            type == null ? string.Empty : $"{type.Name} with Id {id} not found.";

        public static string NotFoundValidationMessage(this Type type, string paramName, string param) =>
            type == null ? string.Empty : $"{type.Name} with {paramName} {param} not found.";

        public static string NotFoundValidationMessage(this Type type) => $"{type.Name} not found.";

        public static string EntityAlreadyExistsMessage(this Type type, object id) =>
            type == null ? string.Empty : $"{type.Name} with Id {id} already exists.";

        public static string EntityAlreadyExistsMessage(this Type type, string paramName, string param) =>
            $"{type.Name} with {paramName} {param} already exists.";

        public static string ForbiddenAccessMessage(this Type type) => $"Member does not have access rights to this {type.Name}.";

        public static string ForbiddenAccessMessage(this Type type, object id) => $"Member does not have access rights to {type.Name} with Id {id}.";
    }
}
