using Microsoft.AspNetCore.Identity;
using System.Text;

namespace ZODs.Common.Extensions
{
    public static class IdentityResultExtensions
    {
        public static string FormatErrors(this IdentityResult result)
        {
            StringBuilder sb = new();

            foreach (var error in result.Errors)
            {
                sb.AppendLine($"Code: {error.Code}, Description: {error.Description}");
            }

            return sb.ToString();
        }
    }
}