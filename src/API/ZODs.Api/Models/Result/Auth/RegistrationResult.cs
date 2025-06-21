namespace ZODs.Api.Models.Result.Auth
{
    public class RegistrationResult
    {
        public Guid? UserId { get; set; }

        public bool IsSuccess { get; set; }
    }
}
