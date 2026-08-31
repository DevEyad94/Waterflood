using System;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http;
using BackEndWaterFloodApp.Attributes;

namespace BackEndWaterFloodApp.Converters
{
    public class AdminOnlyJsonConverter<T> : JsonConverter<T>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly JsonSerializerOptions _options;

        public AdminOnlyJsonConverter(
            IHttpContextAccessor httpContextAccessor,
            JsonSerializerOptions options
        )
        {
            _httpContextAccessor = httpContextAccessor;
            _options = new JsonSerializerOptions(options);

            // Remove this converter to avoid infinite recursion
            var converter = _options.Converters.FirstOrDefault(c => c is AdminOnlyJsonConverter<T>);
            if (converter != null)
                _options.Converters.Remove(converter);
        }

        public override bool CanConvert(Type typeToConvert) =>
            typeToConvert
                .GetProperties()
                .Any(p => p.GetCustomAttribute<RoleAccessAttribute>() != null);

        public override T Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options
        ) => JsonSerializer.Deserialize<T>(ref reader, _options)!;

        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
        {
            if (value == null)
            {
                writer.WriteNullValue();
                return;
            }

            var currentUser = _httpContextAccessor.HttpContext?.User;
            writer.WriteStartObject();

            foreach (var prop in typeof(T).GetProperties())
            {
                var attr = prop.GetCustomAttribute<RoleAccessAttribute>();

                // Skip properties with RoleAccess attribute if user doesn't have required role
                if (
                    attr != null
                    && (
                        currentUser == null
                        || !attr.AllowedRoles.Any(role => currentUser.IsInRole(role))
                    )
                )
                    continue;

                var propValue = prop.GetValue(value);
                if (
                    propValue == null
                    && options.DefaultIgnoreCondition == JsonIgnoreCondition.WhenWritingNull
                )
                    continue;

                writer.WritePropertyName(GetPropertyName(prop, options));
                JsonSerializer.Serialize(writer, propValue, options);
            }

            writer.WriteEndObject();
        }

        private string GetPropertyName(PropertyInfo prop, JsonSerializerOptions options)
        {
            var attribute = prop.GetCustomAttribute<JsonPropertyNameAttribute>();
            if (attribute != null)
                return attribute.Name;

            return options.PropertyNamingPolicy?.ConvertName(prop.Name) ?? prop.Name;
        }
    }
}
