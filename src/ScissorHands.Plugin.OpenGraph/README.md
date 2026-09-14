# ScissorHands.NET: Open Graph Plugin

This plugin renders the [Open Graph](https://ogp.me/) tags.

## Getting Started

1. Assuming that you've got a running [ScissorHands.NET](https://github.com/getscissorhands/Scissorhands.NET) app.
1. Update the plugin section of `appsettings.json` to add options. `TwitterSiteId` is the Twitter handle for the website, and `TwitterCreatorId` is the default Twitter handle for the content authors.

    ```jsonc
    {
      ...
      "Plugins": [
        {
          "Id": "open-graph",
          "Name": "Open Graph",
          "Options": {
            "TwitterSiteId": "@your_twitter_handle_site",
            "TwitterCreatorId": "@your_twitter_handle_creator"
          }
        }
      ]
    }
    ```

   > **NOTE**: If you don't have any of both, you can omit the property. For example, you can omit both properties like `"Options": {}`.

1. Add a NuGet package.

    ```bash
    dotnet add package ScissorHands.Plugin.OpenGraph --prerelease
    ```

1. Add a UI component, `<OpenGraphComponent />` with parameters, to `MainLayout.razor`.

    ```razor
    <OpenGraphComponent Id="open-graph" />
    ```

   > **NOTE**: The configured plugin manifest is resolved by its exact, case-sensitive `Id`. `Name` is optional display metadata. The current documents, document, theme and site are received from the inherited cascading values.

1. Alternatively, use the paired placeholder, `<plugin:open-graph></plugin:open-graph>` instead of the `<OpenGraphComponent />` component.

    ```html
    <html>
    <head>
        ...
        <plugin:open-graph></plugin:open-graph>
        ...
    </head>
    ```

   Use paired markers for the hook path; self-closing hook markers are not part of the supported contract. Choose one insertion path per intended output to avoid duplicates.
