# Prometheus Configuration Options

## Remote Write Receiver
`--enable-feature=remote-write-receiver`
Enables Prometheus to receive metrics via the remote write protocol, allowing other systems to push metrics directly to Prometheus.

## Exemplar Storage
`--enable-feature=exemplar-storage`
Enables storage of exemplars, which are example data points that represent specific traces within your metrics data.

## Native Histograms
`--enable-feature=native-histograms`
Enables support for native histogram format, providing more efficient storage and querying of histogram data.

## Config File Location
`--config.file=/etc/prometheus/prometheus.yml`
Specifies the path to the main Prometheus configuration file.

## Storage Path
`--storage.tsdb.path=/prometheus`
Defines the location where Prometheus will store its time-series database files.

## Data Retention Time
`--storage.tsdb.retention.time=7d`
Sets how long metrics data will be kept before being automatically removed (7 days in this case).

## Storage Size Limit
`--storage.tsdb.retention.size=16GB`
Limits the total size of stored metrics data to 16GB. Older data will be removed when this limit is reached.

## OTLP Receiver
`--web.enable-otlp-receiver`
Enables the OpenTelemetry protocol receiver endpoint in Prometheus.

### OTLP Client Configuration
To configure a client application to send metrics to the OTLP endpoint, set these environment variables:

See for more details: https://prometheus.io/docs/guides/opentelemetry/#send-opentelemetry-metrics-to-the-prometheus-server

````bash
OTEL_EXPORTER_OTLP_ENDPOINT=http://localhost:9090
OTEL_EXPORTER_OTLP_PROTOCOL=http/protobuf
OTEL_EXPORTER_OTLP_METRICS_ENDPOINT=http://localhost:9090/api/v1/otlp/v1/metrics
````

Note: The default port for Prometheus in this configuration is mapped to 9099 on the host, so adjust the endpoint accordingly if accessing from outside Docker.