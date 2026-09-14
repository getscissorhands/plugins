namespace ScissorHands.Plugins.Sample;

public static class SampleArguments
{
    public static string[] ToHostArguments(string[] args)
    {
        ArgumentNullException.ThrowIfNull(args);

        var hostArguments = args.Where(argument => argument != "--use-placeholders").ToArray();
        if (hostArguments.Length == args.Length)
        {
            return hostArguments;
        }

        // A bare engine mode flag can consume the next configuration argument.
        return ["--Sample:UsePlaceholders=true", .. hostArguments];
    }
}
