// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace IdentityServer.IntegrationTests.Common
{
    /// <summary>
    /// Helper methods for handling System.Text.Json deserialization in tests
    /// In .NET 8/10, Dictionary&lt;string, object&gt; wraps primitives as JsonElement
    /// </summary>
    public static class JsonTestHelpers
    {
        /// <summary>
        /// Unwrap JsonElement or return the original value
        /// </summary>
        public static object GetValue(object value)
        {
            if (value is JsonElement element)
            {
                return element.ValueKind switch
                {
                    JsonValueKind.String => element.GetString(),
                    JsonValueKind.Number => element.TryGetInt64(out var l) ? l : element.GetDouble(),
                    JsonValueKind.True => true,
                    JsonValueKind.False => false,
                    JsonValueKind.Null => null,
                    JsonValueKind.Array => element.EnumerateArray().Select(e => GetValue(e)).ToList(),
                    JsonValueKind.Object => element.EnumerateObject()
                        .ToDictionary(p => p.Name, p => GetValue(p.Value)),
                    _ => value
                };
            }
            return value;
        }

        /// <summary>
        /// Get string value, unwrapping JsonElement if needed
        /// </summary>
        public static string GetString(object value)
        {
            var unwrapped = GetValue(value);
            return unwrapped?.ToString();
        }

        /// <summary>
        /// Get long value, unwrapping JsonElement if needed
        /// </summary>
        public static long? GetInt64(object value)
        {
            var unwrapped = GetValue(value);
            if (unwrapped is long l) return l;
            if (unwrapped is int i) return i;
            if (unwrapped is double d) return (long)d;
            return null;
        }

        /// <summary>
        /// Get boolean value, unwrapping JsonElement if needed
        /// </summary>
        public static bool? GetBoolean(object value)
        {
            var unwrapped = GetValue(value);
            if (unwrapped is bool b) return b;
            return null;
        }

        /// <summary>
        /// Get JsonArray (list), unwrapping JsonElement if needed
        /// </summary>
        public static List<object> GetJsonArray(object value)
        {
            var unwrapped = GetValue(value);
            if (unwrapped is List<object> list) return list;
            if (unwrapped is IEnumerable<object> enumerable) return enumerable.ToList();
            return null;
        }

        /// <summary>
        /// Get JsonObject (dictionary), unwrapping JsonElement if needed
        /// </summary>
        public static Dictionary<string, object> GetJsonObject(object value)
        {
            var unwrapped = GetValue(value);
            if (unwrapped is Dictionary<string, object> dict) return dict;
            return null;
        }

        /// <summary>
        /// Get type name for comparison, handling JsonElement wrapping
        /// For numeric values, returns "Int64" if the value is a whole number
        /// </summary>
        public static string GetTypeName(object value)
        {
            var unwrapped = GetValue(value);
            if (unwrapped == null) return "null";

            // Handle Double values that are actually integers
            if (unwrapped is double d && d == Math.Floor(d))
            {
                return "Int64";
            }

            return unwrapped.GetType().Name;
        }
    }
}
