using ScissorHands.Plugins.Sample;
using ScissorHands.Web;

var app = new ScissorHandsApplicationBuilder(SampleArguments.ToHostArguments(args))
    .AddLayouts<SampleLayout, IndexView, PostView, PageView, NotFoundView, TagListView, TagView>()
    .Build();
await app.RunAsync();
