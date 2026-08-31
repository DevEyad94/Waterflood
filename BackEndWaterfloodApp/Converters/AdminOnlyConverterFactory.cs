using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http;

namespace BackEndWaterFloodApp.Converters
{
    public class AdminOnlyConverterFactory : JsonConverterFactory
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AdminOnlyConverterFactory(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public override bool CanConvert(Type typeToConvert)
        {
            return true;
        }

        public override JsonConverter CreateConverter(
            Type typeToConvert,
            JsonSerializerOptions options
        )
        {
            var converterType = typeof(AdminOnlyJsonConverter<>).MakeGenericType(typeToConvert);
            return (JsonConverter)
                Activator.CreateInstance(
                    converterType,
                    new object[] { _httpContextAccessor, options }
                )!;
        }
    }
}
