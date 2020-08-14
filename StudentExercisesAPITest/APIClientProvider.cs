using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;
using System.Text;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace StudentExercisesAPITest
{
    class APIClientProvider : IClassFixture<WebApplicationFactory<Startup>>
    {
        public HttpClient Client { get; private set; }
        private readonly WebApplicationFactory<Startup> _factory = new WebApplicationFactory<Startup>();
        public APIClientProvider()
        {
            Client = _factory.CreateClient();
        }
        public void Dispose()
        {
            _factory?.Dispose();
            Client?.Dispose();
        }
    }
}
