using System.Collections.ObjectModel;

using AngleSharp.Dom;

using ScissorHands.Core.Manifests;

namespace ScissorHands.Plugin.GoogleAnalytics.Tests;

public static class GoogleAnalyticsTestData
{
    public const string Marker = "<plugin:google-analytics></plugin:google-analytics>";
    public const string ConfigurationError =
        "Plugin 'google-analytics' requires option 'MeasurementId' to be a string containing " +
        "'G-' followed by one or more uppercase ASCII letters or digits, with no whitespace or other characters.";

    public static TheoryData<string> InvalidConfigurationCases => new()
    {
        "null-options", "missing-option", "wrong-key-case", "null-value", "integer", "boolean", "array",
        "nested-object", "unstringifiable-object", "empty", "space", "tab", "newline", "prefix-space",
        "suffix-space", "suffix-linefeed", "suffix-carriage-return", "prefix-linefeed", "embedded-whitespace",
        "missing-prefix", "lowercase-prefix", "lowercase-suffix", "missing-suffix", "duplicate-prefix",
        "unicode-letter", "unicode-digit", "null-character", "zero-width-space", "nonbreaking-space",
        "punctuation", "single-quote", "double-quote", "backslash", "backtick", "script-terminator",
        "html", "url-delimiter", "entity",
    };

    public static IReadOnlyDictionary<string, object?>? CreateInvalidOptions(string scenario)
    {
        if (scenario == "null-options")
        {
            return null;
        }

        if (scenario == "missing-option")
        {
            return new ReadOnlyDictionary<string, object?>(new Dictionary<string, object?>());
        }

        if (scenario == "wrong-key-case")
        {
            return new ReadOnlyDictionary<string, object?>(new Dictionary<string, object?> { ["measurementId"] = "G-EXAMPLE" });
        }

        object? value = scenario switch
        {
            "null-value" => null,
            "integer" => 12345,
            "boolean" => false,
            "array" => new[] { "G-EXAMPLE" },
            "nested-object" => new Dictionary<string, object?> { ["MeasurementId"] = "G-EXAMPLE" },
            "unstringifiable-object" => new UnstringifiableValue(),
            "empty" => "",
            "space" => " ",
            "tab" => "\t",
            "newline" => "\r\n",
            "prefix-space" => " G-EXAMPLE",
            "suffix-space" => "G-EXAMPLE ",
            "suffix-linefeed" => "G-EXAMPLE\n",
            "suffix-carriage-return" => "G-EXAMPLE\r",
            "prefix-linefeed" => "\nG-EXAMPLE",
            "embedded-whitespace" => "G-EX AMPLE",
            "missing-prefix" => "EXAMPLE",
            "lowercase-prefix" => "g-EXAMPLE",
            "lowercase-suffix" => "G-example",
            "missing-suffix" => "G-",
            "duplicate-prefix" => "G-G-EXAMPLE",
            "unicode-letter" => "G-É",
            "unicode-digit" => "G-１",
            "null-character" => "G-EXAMPLE\0",
            "zero-width-space" => "G-EXAMPLE\u200B",
            "nonbreaking-space" => "G-EXAMPLE\u00A0",
            "punctuation" => "G-EXAMPLE_",
            "single-quote" => "G-EXAMPLE');sentinel();//",
            "double-quote" => "G-EXAMPLE\" onload=\"sentinel()",
            "backslash" => "G-EXAMPLE\\",
            "backtick" => "G-EXAMPLE`${sentinel()}`",
            "script-terminator" => "G-EXAMPLE</script><script>sentinel()</script>",
            "html" => "<img src=x onerror=sentinel()>",
            "url-delimiter" => "G-EXAMPLE&extra=sentinel#fragment",
            "entity" => "G-EXAMPLE&#39;",
            _ => throw new ArgumentOutOfRangeException(nameof(scenario)),
        };

        return new ReadOnlyDictionary<string, object?>(new Dictionary<string, object?> { ["MeasurementId"] = value });
    }

    public static PluginManifest CreateManifest(string measurementId, string id = "google-analytics", string? name = null) =>
        new()
        {
            Id = id,
            Name = name,
            Options = new ReadOnlyDictionary<string, object?>(new Dictionary<string, object?> { ["MeasurementId"] = measurementId }),
        };

    public static void AssertTag(IEnumerable<IElement> elements, string measurementId)
    {
        var scripts = elements.ToArray();
        scripts.Length.ShouldBe(2);
        scripts[0].GetAttribute("src").ShouldBe($"https://www.googletagmanager.com/gtag/js?id={measurementId}");
        scripts[0].HasAttribute("async").ShouldBeTrue();
        scripts[0].Attributes.Length.ShouldBe(2);
        scripts[1].Attributes.ShouldBeEmpty();
        scripts[1].TextContent.Split('\n', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries).ShouldBe(
            new[]
            {
                "window.dataLayer = window.dataLayer || [];",
                "function gtag(){dataLayer.push(arguments);}",
                "gtag('js', new Date());",
                $"gtag('config', '{measurementId}');",
            });
    }

    public sealed class OptionsSnapshot
    {
        private readonly Dictionary<string, object?> nested = new() { ["Secret"] = "unchanged-synthetic-value" };
        private readonly List<object?> items = ["original", null];
        private readonly IReadOnlyDictionary<string, object?> options;
        private readonly object? measurementId;

        public OptionsSnapshot(bool invalid)
        {
            measurementId = invalid ? nested : "G-EXAMPLE";
            options = new ReadOnlyDictionary<string, object?>(new Dictionary<string, object?>
            {
                ["MeasurementId"] = measurementId,
                ["Nested"] = nested,
                ["Items"] = items,
                ["Enabled"] = false,
            });
            Manifest = new PluginManifest { Id = "google-analytics", Options = options };
        }

        public PluginManifest Manifest { get; }

        public void AssertUnchanged()
        {
            options.Count.ShouldBe(4);
            Manifest.Options.ShouldNotBeNull();
            Manifest.Options.Count.ShouldBe(4);
            options["MeasurementId"].ShouldBeSameAs(measurementId);
            Manifest.Options["MeasurementId"].ShouldBeSameAs(measurementId);
            Manifest.Options["Nested"].ShouldBeSameAs(nested);
            Manifest.Options["Items"].ShouldBeSameAs(items);
            Manifest.Options["Enabled"].ShouldBe(false);
            nested.Count.ShouldBe(1);
            nested["Secret"].ShouldBe("unchanged-synthetic-value");
            items.ShouldBe(new object?[] { "original", null });
        }
    }

    private sealed class UnstringifiableValue
    {
        public override string ToString() => throw new InvalidOperationException("The payload must never be formatted.");
    }
}
