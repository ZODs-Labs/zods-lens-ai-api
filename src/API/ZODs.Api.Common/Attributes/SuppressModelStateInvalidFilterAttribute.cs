using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace ZODs.Api.Common.Attributes;

/// <summary>
/// Suppresses the default ApiController behaviour of automatically creating error 400 responses
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class SuppressModelStateInvalidFilterAttribute : Attribute, IActionModelConvention
{
    private static readonly Type ModelStateInvalidFilterFactory = typeof(ModelStateInvalidFilter).Assembly.GetType("Microsoft.AspNetCore.Mvc.Infrastructure.ModelStateInvalidFilterFactory");

    public void Apply(ActionModel action)
    {
        for (var i = 0; i < action.Filters.Count; i++)
        {
            if (action.Filters[i] is ModelStateInvalidFilter || action.Filters[i].GetType() == ModelStateInvalidFilterFactory)
            {
                action.Filters.RemoveAt(i);
                break;
            }
        }
    }
}