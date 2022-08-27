using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Linq;
using System.Reflection;

namespace Company.Product.WebApi.SwaggerConfig
{
    public class SwaggerDefaultValues : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            if (operation.Parameters == null)
            {
                return;
            }

            var apiDescription = context.ApiDescription;

            if (apiDescription.TryGetMethodInfo(out MethodInfo methodInfo) && methodInfo.CustomAttributes != null)
            {
                operation.Deprecated |= methodInfo.CustomAttributes.OfType<ObsoleteAttribute>().Any();
                foreach (var parameter in operation.Parameters)
                {
                    var description = apiDescription.ParameterDescriptions.First(p => p.Name == parameter.Name);

                    if (parameter.Description == null)
                    {
                        parameter.Description = description.ModelMetadata?.Description;
                    }

                    parameter.Required |= description.IsRequired;
                }
            }
        }
    }
}
