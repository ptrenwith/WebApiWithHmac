using Company.Product.AuthenticationService;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace ApiClient
{
    internal class Program
    {
        static void Main(string[] args)
        {
            RunAsync().Wait();
            Console.ReadLine();
        }
        static async Task RunAsync()
        {
            string apiBaseAddress = "https://localhost:44372/";
            HMACDelegatingHandler customDelegatingHandler = new HMACDelegatingHandler();
            HttpClient client = HttpClientFactory.Create(customDelegatingHandler);

            var request = new int[] { 2, 3, 2, 1, 3, 1, 1, 2, 1 };
            HttpResponseMessage response = await client.PostAsJsonAsync(apiBaseAddress + "v1/calculations/dams/capacities", request);
            if (response.IsSuccessStatusCode)
            {
                string responseString = await response.Content.ReadAsStringAsync();
                Console.WriteLine(responseString);
                Console.WriteLine("HTTP Status: {0}, Reason {1}. Press ENTER to exit", response.StatusCode, response.ReasonPhrase);
            }
            else
            {
                Console.WriteLine("Failed to call the API. HTTP Status: {0}, Reason {1}", response.StatusCode, response.ReasonPhrase);
            }
        }
    }
}
