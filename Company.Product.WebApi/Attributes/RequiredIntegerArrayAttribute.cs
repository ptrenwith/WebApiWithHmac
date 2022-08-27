using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Company.Product.WebApi.Attributes
{
    public class RequiredIntegerArrayAttribute : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            return value is int[] @int && @int.Length > 0 && !@int.Any(i => i < 0);
        }
    }
}
