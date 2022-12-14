using System.ComponentModel.DataAnnotations;

namespace Company.Product.WebApi
{
    public class AppSettings
    {

        [Required]
        public ApiSettings Api { get; set; }
    }

    public class ApiSettings
    {
        [Required]
        public string Title { get; set; }

        public string Description { get; set; }

        public ApiContact Contact { get; set; }

        public string TermsOfServiceUrl { get; set; }

        public ApiLicense License { get; set; }

        [Required]
        public Swagger Swagger { get; set; }
    }

    public class ApiContact
    {
        public string Name { get; set; }

        public string Email { get; set; }

        public string Url { get; set; }
    }

    public class ApiLicense
    {
        public string Name { get; set; }

        public string Url { get; set; }
    }

    public class Swagger
    {
        [Required]
        public bool Enabled { get; set; }
    }
}
