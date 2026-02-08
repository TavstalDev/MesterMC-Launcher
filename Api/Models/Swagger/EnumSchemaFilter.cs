using System.Text.Json.Nodes;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Tavstal.MesterMC.Api.Models.Swagger;

// ReSharper disable once ClassNeverInstantiated.Global
public class EnumSchemaFilter : ISchemaFilter
{
    public void Apply(IOpenApiSchema schema, SchemaFilterContext context)
    {
        // Check if the context type is an enum
        if (!context.Type.IsEnum)
            return;
        
        // Clear existing enum values in the schema
        schema.Enum.Clear();
        // Add enum names as string values to the schema
        Enum.GetNames(context.Type)
            .ToList()
            .ForEach(name => schema.Enum.Add(JsonValue.Create(name)));
    }
}