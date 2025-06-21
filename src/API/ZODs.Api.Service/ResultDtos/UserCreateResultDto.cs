using ZODs.Common.Extensions;
using Microsoft.AspNetCore.Identity;

namespace ZODs.Api.Service.ResultDtos
{
    public sealed class UserCreateResultDto
    {
        public Guid UserId { get; set; }

        public IdentityResult Result { get; set; } = null!;

        public bool IsSuccess => Result.Succeeded;

        public string ErrorsFormatted => Result?.FormatErrors() ?? string.Empty;

        public static UserCreateResultDto FromIdentityResult(Guid userId, IdentityResult result)
        {
            return new UserCreateResultDto
            {
                UserId = userId,
                Result = result
            };
        }
    }
}