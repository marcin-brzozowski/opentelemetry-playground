using OpenTelemetry.Exporter;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OtelGrpc.Services;

var builder = WebApplication.CreateBuilder(args);

var resourceBuilder = ResourceBuilder.CreateDefault().AddService("OtelGrpc-Demo-Service")
    .AddTelemetrySdk()
    .AddEnvironmentVariableDetector();


builder.Services.AddOpenTelemetry()
    .WithMetrics(metricsBuilder =>
    {
        metricsBuilder.SetResourceBuilder(resourceBuilder);
        
        
        // Use default OTLP protocol
        // metricsBuilder.AddOtlpExporter();
        
        // Use HTTP/Protobuf OTLP exporter to push directly to Prometheus 
        metricsBuilder.AddOtlpExporter((exporterOptions, metricReaderOptions) =>
        {
            exporterOptions.Endpoint = new Uri("http://localhost:9090/api/v1/otlp/v1/metrics");
            exporterOptions.Protocol = OtlpExportProtocol.HttpProtobuf;
            metricReaderOptions.PeriodicExportingMetricReaderOptions.ExportIntervalMilliseconds = 1000;
        });

        // !!! THIS DOES NOT WORK !!!
        // c.AddMeter("Grpc.Net.Client")
        //  .AddMeter("Grpc.AspNetCore.Server");
        
        // Add GRPC instrumentation
        metricsBuilder.AddEventCountersInstrumentation(e =>
        {
            e.RefreshIntervalSecs = 1;
            // Note that you typically won't have both Client and Server in the same application
            // We're doing it here just for demo purposes, in prod you'd use one or the other
            e.AddEventSources("Grpc.Net.Client", "Grpc.AspNetCore.Server");
        })
            // Rename instruments to follow convention
        .AddView(instrumentName: "ec.Grpc.AspNetCore.Server.calls-deadline-exceeded", name: "grpc.server.calls_deadline_exceeded")
        .AddView(instrumentName: "ec.Grpc.AspNetCore.Server.calls-failed", name: "grpc.server.calls_failed")
        .AddView(instrumentName: "ec.Grpc.AspNetCore.Server.calls-unimplemented", name: "grpc.server.calls_unimplemented")
        .AddView(instrumentName: "ec.Grpc.AspNetCore.Server.current-calls", name: "grpc.server.current_calls")
        .AddView(instrumentName: "ec.Grpc.AspNetCore.Server.messages-received", name: "grpc.server.messages_received")
        .AddView(instrumentName: "ec.Grpc.AspNetCore.Server.messages-sent", name: "grpc.server.messages_sent")
        .AddView(instrumentName: "ec.Grpc.AspNetCore.Server.total-calls", name: "grpc.server.total_calls")
        .AddView(instrumentName: "ec.Grpc.Net.Client.calls-deadline-exceeded", name: "grpc.client.calls_deadline_exceeded")
        .AddView(instrumentName: "ec.Grpc.Net.Client.calls-failed", name: "grpc.client.calls_failed")
        .AddView(instrumentName: "ec.Grpc.Net.Client.current-calls", name: "grpc.client.current_calls")
        .AddView(instrumentName: "ec.Grpc.Net.Client.messages-received", name: "grpc.client.messages_received")
        .AddView(instrumentName: "ec.Grpc.Net.Client.messages-sent", name: "grpc.client.messages_sent")
        .AddView(instrumentName: "ec.Grpc.Net.Client.total-calls", name: "grpc.client.total_calls");
        
        
        // Add runtime instrumentation
        metricsBuilder.AddMeter("System.Runtime");

        // Add HTTP Client instrumentation
        metricsBuilder.AddMeter("System.Net.Http")
            .AddMeter("System.Net.NameResolution");

        // Add ASP.NET Core instrumentation
        metricsBuilder.AddMeter("Microsoft.AspNetCore.Hosting")
            .AddMeter("Microsoft.AspNetCore.Server.Kestrel")
            .AddView("http-server-request-duration",
                new ExplicitBucketHistogramConfiguration
                {
                    Boundaries = [ 0, 0.005, 0.01, 0.025, 0.05,0.075, 0.1, 0.25, 0.5, 0.75, 1, 2.5, 5, 7.5, 10 ]
                })
            .AddMeter("Microsoft.AspNetCore.Http.Connections")
            .AddMeter("Microsoft.AspNetCore.Routing")
            .AddMeter("Microsoft.AspNetCore.Diagnostics")
            .AddMeter("Microsoft.AspNetCore.RateLimiting");
    });

// Add services to the container.
builder.Services.AddGrpc();

builder.Services.AddHostedService<Client>();

var app = builder.Build();


// Configure the HTTP request pipeline.
app.MapGrpcService<GreeterService>();
app.MapGet("/",
    () =>
        "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.Run();