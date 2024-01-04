using System.Collections.Generic;
using System.Linq;
using System;
using Newtonsoft.Json;

/// <summary>
///     Function parameter is a JSON Schema object.
///     https://json-schema.org/understanding-json-schema/reference/object.html
/// </summary>
public class PropertyDefinition
{
    public enum FunctionObjectTypes
    {
        String,
        Integer,
        Number,
        Object,
        Array,
        Boolean,
        Null
    }

    /// <summary>
    ///     Required. Function parameter object type. Default value is "object".
    /// </summary>
    [JsonProperty("type")]
    public string Type { get; set; } = "object";

    /// <summary>
    ///     Optional. List of "function arguments", as a dictionary that maps from argument name
    ///     to an object that describes the type, maybe possible enum values, and so on.
    /// </summary>
    [JsonProperty("properties")]
    public IDictionary<string, PropertyDefinition>? Properties { get; set; }

    /// <summary>
    ///     Optional. List of "function arguments" which are required.
    /// </summary>
    [JsonProperty("required")]
    public IList<string>? Required { get; set; }

    /// <summary>
    ///     Optional. Whether additional properties are allowed. Default value is true.
    /// </summary>
    [JsonProperty("additionalProperties")]
    public bool? AdditionalProperties { get; set; }

    /// <summary>
    ///     Optional. Argument description.
    /// </summary>
    [JsonProperty("description")]
    public string? Description { get; set; }

    /// <summary>
    ///     Optional. List of allowed values for this argument.
    /// </summary>
    [JsonProperty("enum")]
    public IList<string>? Enum { get; set; }

    /// <summary>
    ///     The number of properties on an object can be restricted using the minProperties and maxProperties keywords. Each of
    ///     these must be a non-negative integer.
    /// </summary>
    [JsonProperty("minProperties")]
    public int? MinProperties { get; set; }

    /// <summary>
    ///     The number of properties on an object can be restricted using the minProperties and maxProperties keywords. Each of
    ///     these must be a non-negative integer.
    /// </summary>
    [JsonProperty("maxProperties")]
    public int? MaxProperties { get; set; }

    /// <summary>
    ///     If type is "array", this specifies the element type for all items in the array.
    ///     If type is not "array", this should be null.
    ///     For more details, see https://json-schema.org/understanding-json-schema/reference/array.html
    /// </summary>
    [JsonProperty("items")]
    public PropertyDefinition? Items { get; set; }

    public static PropertyDefinition DefineArray(PropertyDefinition? arrayItems = null)
    {
        return new PropertyDefinition
        {
            Items = arrayItems,
            Type = ConvertTypeToString(FunctionObjectTypes.Array)
        };
    }

    public static PropertyDefinition DefineEnum(List<string> enumList, string? description = null)
    {
        return new PropertyDefinition
        {
            Description = description,
            Enum = enumList,
            Type = ConvertTypeToString(FunctionObjectTypes.String)
        };
    }

    public static PropertyDefinition DefineInteger(string? description = null)
    {
        return new PropertyDefinition
        {
            Description = description,
            Type = ConvertTypeToString(FunctionObjectTypes.Integer)
        };
    }

    public static PropertyDefinition DefineNumber(string? description = null)
    {
        return new PropertyDefinition
        {
            Description = description,
            Type = ConvertTypeToString(FunctionObjectTypes.Number)
        };
    }

    public static PropertyDefinition DefineString(string? description = null)
    {
        return new PropertyDefinition
        {
            Description = description,
            Type = ConvertTypeToString(FunctionObjectTypes.String)
        };
    }

    public static PropertyDefinition DefineBoolean(string? description = null)
    {
        return new PropertyDefinition
        {
            Description = description,
            Type = ConvertTypeToString(FunctionObjectTypes.Boolean)
        };
    }

    public static PropertyDefinition DefineNull(string? description = null)
    {
        return new PropertyDefinition
        {
            Description = description,
            Type = ConvertTypeToString(FunctionObjectTypes.Null)
        };
    }

    public static PropertyDefinition DefineObject(IDictionary<string, PropertyDefinition>? properties, IList<string>? required, bool? additionalProperties, string? description, IList<string>? @enum)
    {
        return new PropertyDefinition
        {
            Properties = properties,
            Required = required,
            AdditionalProperties = additionalProperties,
            Description = description,
            Enum = @enum,
            Type = ConvertTypeToString(FunctionObjectTypes.Object)
        };
    }

    /// <summary>
    ///     Converts a FunctionObjectTypes enumeration value to its corresponding string representation.
    /// </summary>
    /// <param name="type">The type to convert</param>
    /// <returns>The string representation of the given type</returns>
    public static string ConvertTypeToString(FunctionObjectTypes type)
    {
        return type switch
        {
            FunctionObjectTypes.String => "string",
            FunctionObjectTypes.Integer => "integer",
            FunctionObjectTypes.Number => "number",
            FunctionObjectTypes.Object => "object",
            FunctionObjectTypes.Array => "array",
            FunctionObjectTypes.Boolean => "boolean",
            FunctionObjectTypes.Null => "null",
            _ => throw new ArgumentOutOfRangeException(nameof(type), $"Unknown type: {type}")
        };
    }
}

/// <summary>
///     Definition of a valid function call.
/// </summary>
public class FunctionDefinition
{
    /// <summary>
    ///     The name of the function to be called. Must be a-z, A-Z, 0-9,
    ///     or contain underscores and dashes, with a maximum length of 64.
    /// </summary>
    [JsonProperty("name")]
    public string Name { get; set; }

    /// <summary>
    ///     A description of what the function does, used by the model to choose when and how to call the function.
    /// </summary>
    [JsonProperty("description")]
    public string? Description { get; set; }

    /// <summary>
    ///     Optional. The parameters the functions accepts, described as a JSON Schema object.
    ///     See the <a href="https://platform.openai.com/docs/guides/gpt/function-calling">guide</a> for examples,
    ///     and the <a href="https://json-schema.org/understanding-json-schema/">JSON Schema reference</a> for
    ///     documentation about the format.
    /// </summary>
    [JsonProperty("parameters")]
    public PropertyDefinition Parameters { get; set; }
}

/// <summary>
///     FunctionDefinitionBuilder is used to build and validate a FunctionDefinition object.
/// </summary>
public class FunctionDefinitionBuilder
        {
            /// <summary>
            ///     String constant for validation of function name.
            /// </summary>
            private const string ValidNameChars =
                "abcdefghijklmnopqrstuvwxyz" +
                "ABCDEFGHIJKLMNOPQRSTUVWXYZ" +
                "0123456789-_";

            private readonly FunctionDefinition _definition;

            /// <summary>
            ///     Initializes a new instance of FunctionDefinitionBuilder.
            /// </summary>
            /// <param name="name">The name of the function</param>
            /// <param name="description">The optional description of the function</param>
            public FunctionDefinitionBuilder(string name, string? description = null)
            {
                _definition = new FunctionDefinition
                {
                    Name = name,
                    Description = description,
                    Parameters = new PropertyDefinition
                    {
                        Properties = new Dictionary<string, PropertyDefinition>()
                    }
                };
            }

            public FunctionDefinitionBuilder AddParameter(string name, PropertyDefinition value, bool required = true)
            {
                var pars = _definition.Parameters!;
                pars.Properties![name] = value;

                if (required)
                {
                    pars.Required ??= new List<string>();
                    pars.Required.Add(name);
                }

                return this;
            }

            /// <summary>
            ///     Validates the function definition.
            /// </summary>
            /// <returns>The FunctionDefinitionBuilder instance for fluent configuration</returns>
            public FunctionDefinitionBuilder Validate()
            {
                ValidateName(_definition.Name);
                return this;
            }

            /// <summary>
            ///     Builds the FunctionDefinition object.
            /// </summary>
            /// <returns>The built FunctionDefinition object</returns>
            public FunctionDefinition Build()
            {
                return _definition;
            }

            /// <summary>
            ///     Validates the name of the function.
            /// </summary>
            /// <param name="functionName">The name of the function to validate</param>
            public static void ValidateName(string functionName)
            {
                var invalidChars = functionName.Where(ch => !ValidNameChars.Contains(ch)).ToList();
                if (functionName.Length > 64 || invalidChars.Count > 0)
                {
                    var message = "The name of the function must be a-z, A-Z, 0-9, or contain underscores and dashes, with a maximum length of 64.";
                    if (functionName.Length > 64)
                    {
                        message = "Function name is too long. " + message;
                    }

                    if (invalidChars.Count > 0)
                    {
                        message = $"Function name contains invalid characters: {string.Join(",", invalidChars)}. " + message;
                    }

                    throw new ArgumentOutOfRangeException(nameof(functionName), message);
                }
            }
        }
    