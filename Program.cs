using IdentityModel.Client;
using RefreshClient;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((hostContext, services) =>
    {
        // default cache
        services.AddDistributedMemoryCache();
        
        services.AddClientCredentialsTokenManagement();
        services.AddSingleton(new DiscoveryCache(hostContext.Configuration["Sts:Authority"]));

        // Configure OAuth access Token management
        services.AddClientCredentialsTokenManagement()
            .AddClient("sts", client =>
            {
                var sp = services.BuildServiceProvider();

                var _cache = sp.GetService<DiscoveryCache>();

                client.TokenEndpoint = _cache.GetAsync().GetAwaiter().GetResult().TokenEndpoint;
                client.ClientId = hostContext.Configuration["Sts:ClientId"];
                client.ClientSecret = hostContext.Configuration["Sts:ClientSecret"];

                client.Scope = "calc:double";

                client.Parameters = new Parameters
                {
                    new("audience", hostContext.Configuration["Api:Audience"])
                };
            });

        // Configure http client
        services.AddClientCredentialsHttpClient("client", "sts",
            client => { client.BaseAddress = new Uri(hostContext.Configuration["Api:BaseAddress"]); });

        services.AddHostedService<Worker>();
    })
    .Build();

await host.RunAsync();