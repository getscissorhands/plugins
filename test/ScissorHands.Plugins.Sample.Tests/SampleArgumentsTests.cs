using Microsoft.Extensions.Configuration;

namespace ScissorHands.Plugins.Sample.Tests;

public class SampleArgumentsTests
{
    [Theory]
    [InlineData("--preview", false)]
    [InlineData("--preview", true)]
    [InlineData("--build", false)]
    [InlineData("--build", true)]
    public void Given_PlaceholderFlag_When_Translated_Then_It_Should_Enable_Hooks_InEitherPosition(
        string mode,
        bool flagFirst)
    {
        // Arrange
        var args = flagFirst ? new[] { "--use-placeholders", mode } : new[] { mode, "--use-placeholders" };
        var original = args.ToArray();

        // Act
        var translated = SampleArguments.ToHostArguments(args);
        var configuration = new ConfigurationBuilder().AddCommandLine(translated).Build();

        // Assert
        translated.ShouldBe(new[] { "--Sample:UsePlaceholders=true", mode });
        configuration.GetValue<bool>("Sample:UsePlaceholders").ShouldBeTrue();
        args.ShouldBe(original);
    }

    [Fact]
    public void Given_NoFlag_When_Translated_Then_It_Should_Preserve_ArgumentsAndDefaultToComponents()
    {
        // Arrange
        var args = new[] { "--Site:Title=Sample title", "--Site:BaseUrl=/blog/", "--preview" };

        // Act
        var translated = SampleArguments.ToHostArguments(args);
        var configuration = new ConfigurationBuilder().AddCommandLine(translated).Build();

        // Assert
        translated.ShouldBe(args);
        configuration.GetValue<bool>("Sample:UsePlaceholders").ShouldBeFalse();
        configuration["Site:Title"].ShouldBe("Sample title");
        configuration["Site:BaseUrl"].ShouldBe("/blog/");
    }

    [Fact]
    public void Given_RepeatedFlag_When_Translated_Then_It_Should_Forward_OneSettingAndOtherOptions()
    {
        // Arrange
        var args = new[] { "--use-placeholders", "--urls", "http://localhost:5000", "--use-placeholders", "--build" };

        // Act
        var translated = SampleArguments.ToHostArguments(args);
        var configuration = new ConfigurationBuilder().AddCommandLine(translated).Build();

        // Assert
        translated.ShouldBe(new[] { "--Sample:UsePlaceholders=true", "--urls", "http://localhost:5000", "--build" });
        configuration.GetValue<bool>("Sample:UsePlaceholders").ShouldBeTrue();
        configuration["urls"].ShouldBe("http://localhost:5000");
    }
}
