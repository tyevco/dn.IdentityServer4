// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.
//
// Sanitizer implementation adapted from IdentityServer8
// Copyright (c) 2024 HigginsSoft, Alexander Higgins

using System;
using System.Net;
using System.Web;

namespace IdentityServer4.Security
{
    public enum SanitizerType
    {
        Unknown = 0,
        HtmlSanitizer,
        XmlSanitizer,
        JsonSanitizer,
        UrlSanitizer,
        CssSanitizer,
        ScriptSanitizer,
        StyleSanitizer,
        SqlSanitizer,
        LogSanitizer,
    }

    public enum SanitizerMode
    {
        Debug,
        Clean,
        Mask,
        Full
    }

    public interface IInputSanitizer
    {
        string Sanitize(string input, SanitizerMode mode = SanitizerMode.Clean);
    }

    public interface ISanitizerFactory
    {
        TSanitizer Create<TSanitizer>() where TSanitizer : IInputSanitizer;
        IInputSanitizer Create(SanitizerType type);
    }

    public interface IHtmlSanitizer : IInputSanitizer { }
    public interface IXmlSanitizer : IInputSanitizer { }
    public interface IJsonSanitizer : IInputSanitizer { }
    public interface IUrlSanitizer : IInputSanitizer { }
    public interface ICssSanitizer : IInputSanitizer { }
    public interface IScriptSanitizer : IInputSanitizer { }
    public interface IStyleSanitizer : IInputSanitizer { }
    public interface ISqlSanitizer : IInputSanitizer { }
    public interface ILogSanitizer : IInputSanitizer { }

    public abstract class SanitizerBase : IInputSanitizer
    {
        private Func<string, string> _sanitize;

        public SanitizerBase() : this(WebUtility.HtmlEncode)
        {
        }

        public SanitizerBase(Func<string, string> sanitizer)
        {
            _sanitize = sanitizer ?? (x => x);
        }

        public virtual string Sanitize(string input, SanitizerMode mode = SanitizerMode.Debug)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            switch (mode)
            {
                case SanitizerMode.Debug:
                    return input;
                case SanitizerMode.Clean:
                    return Clean(input);
                case SanitizerMode.Mask:
                case SanitizerMode.Full:
                    var result = _sanitize(input) ?? "";
                    switch (mode)
                    {
                        case SanitizerMode.Mask:
                            return Mask(result);
                        case SanitizerMode.Full:
                            return result;
                        default:
                            throw new NotImplementedException();
                    }
                default:
                    throw new NotImplementedException();
            }
        }

        public string Mask(string input, int unmaskedChars = 4, bool unmaskFirst = false)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            if (unmaskedChars == 0)
            {
                return "********";
            }

            input = Clean(input);
            if (input.Length <= unmaskedChars)
            {
                return new string('*', input.Length);
            }
            else if (unmaskFirst)
            {
                return input.Substring(0, unmaskedChars) + new string('*', input.Length - unmaskedChars);
            }
            else
            {
                return new string('*', input.Length - unmaskedChars) + input.Substring(input.Length - unmaskedChars);
            }
        }

        public string Clean(string input)
        {
            input = input ?? string.Empty;
            input = input.Replace("\r", " ").Replace("\n", " ");
            var idx = input.IndexOf("  ");
            while (idx > -1)
            {
                input = input.Replace("  ", " ");
                idx = input.IndexOf("  ");
            }
            input = _sanitize(input) ?? "";

            // Unescape common HTML entities
            input = input.Replace("&#39;", "'");
            input = input.Replace("&#34;", "\"");
            input = input.Replace("&#20;", " ");
            input = input.Replace("&apos;", "'");
            input = input.Replace("&quot;", "\"");
            input = input.Replace("&nbsp;", " ");

            return input.Trim() ?? "";
        }
    }

    public class HtmlSanitizer : SanitizerBase, IHtmlSanitizer
    {
        public HtmlSanitizer() : base() { }
    }

    public class XmlSanitizer : SanitizerBase, IXmlSanitizer
    {
        public XmlSanitizer() : base(WebUtility.HtmlEncode) { }
    }

    public class JsonSanitizer : SanitizerBase, IJsonSanitizer
    {
        public JsonSanitizer() : base(System.Text.Encodings.Web.JavaScriptEncoder.Default.Encode) { }
    }

    public class UrlSanitizer : SanitizerBase, IUrlSanitizer
    {
        public UrlSanitizer() : base(x => Uri.EscapeDataString(x ?? "")) { }
    }

    public class CssSanitizer : SanitizerBase, ICssSanitizer
    {
        public CssSanitizer() : base() { }
    }

    public class ScriptSanitizer : SanitizerBase, IScriptSanitizer
    {
        public ScriptSanitizer() : base(x => Uri.EscapeDataString(x ?? "")) { }
    }

    public class StyleSanitizer : SanitizerBase, IStyleSanitizer
    {
        public StyleSanitizer() : base() { }
    }

    public class SqlSanitizer : SanitizerBase, ISqlSanitizer
    {
        public SqlSanitizer() : base() { }
    }

    public class LogSanitizer : SanitizerBase, ILogSanitizer
    {
        public LogSanitizer() : base(WebUtility.HtmlEncode) { }

        public override string Sanitize(string input, SanitizerMode mode)
        {
            if (input is null)
                return input;

            switch (mode)
            {
                case SanitizerMode.Debug:
                    return base.Mask(input, input.Length);
                case SanitizerMode.Clean:
                    return base.Clean(input);
                case SanitizerMode.Mask:
                    return base.Mask(input);
                case SanitizerMode.Full:
                    return base.Mask(input, 0);
                default:
                    throw new NotImplementedException();
            }
        }
    }

    public class SanitizerFactory : ISanitizerFactory
    {
        public TInputSanitizer Create<TInputSanitizer>() where TInputSanitizer : IInputSanitizer
        {
            var typeName = typeof(TInputSanitizer).Name.Substring(1); // Remove 'I' prefix
            var type = Enum.Parse<SanitizerType>(typeName);
            return (TInputSanitizer)Create(type);
        }

        public IInputSanitizer Create(SanitizerType type)
        {
            switch (type)
            {
                case SanitizerType.HtmlSanitizer:
                    return new HtmlSanitizer();
                case SanitizerType.XmlSanitizer:
                    return new XmlSanitizer();
                case SanitizerType.JsonSanitizer:
                    return new JsonSanitizer();
                case SanitizerType.UrlSanitizer:
                    return new UrlSanitizer();
                case SanitizerType.CssSanitizer:
                    return new CssSanitizer();
                case SanitizerType.ScriptSanitizer:
                    return new ScriptSanitizer();
                case SanitizerType.StyleSanitizer:
                    return new StyleSanitizer();
                case SanitizerType.SqlSanitizer:
                    return new SqlSanitizer();
                case SanitizerType.LogSanitizer:
                    return new LogSanitizer();
                default:
                    throw new NotImplementedException();
            }
        }
    }

    public interface ISanitizer
    {
        IHtmlSanitizer Html { get; }
        IXmlSanitizer Xml { get; }
        IJsonSanitizer Json { get; }
        IUrlSanitizer Url { get; }
        ICssSanitizer Css { get; }
        IScriptSanitizer Script { get; }
        IStyleSanitizer Style { get; }
        ISqlSanitizer Sql { get; }
        ILogSanitizer Log { get; }
    }

    public static class Sanitizer
    {
        private static readonly ISanitizerFactory _factory = new SanitizerFactory();

        public static IHtmlSanitizer Html { get; } = _factory.Create<IHtmlSanitizer>();
        public static IXmlSanitizer Xml { get; } = _factory.Create<IXmlSanitizer>();
        public static IJsonSanitizer Json { get; } = _factory.Create<IJsonSanitizer>();
        public static IUrlSanitizer Url { get; } = _factory.Create<IUrlSanitizer>();
        public static ICssSanitizer Css { get; } = _factory.Create<ICssSanitizer>();
        public static IScriptSanitizer Script { get; } = _factory.Create<IScriptSanitizer>();
        public static IStyleSanitizer Style { get; } = _factory.Create<IStyleSanitizer>();
        public static ISqlSanitizer Sql { get; } = _factory.Create<ISqlSanitizer>();
        public static ILogSanitizer Log { get; } = _factory.Create<ILogSanitizer>();
    }
}
