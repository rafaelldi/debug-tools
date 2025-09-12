using System.Globalization;
using System.Threading.Channels;
using Microsoft.Diagnostics.Tracing;
using Monitor.Common;
using Monitor.SessionConfigurations;
using MonitorAgent;

namespace Monitor.Counters;

internal sealed class MetricSessionHandler(
    MetricsSessionConfiguration configuration,
    ChannelWriter<CounterValue> writer
) : BaseCounterSessionHandler<MetricsSessionConfiguration>(configuration, writer)
{
    private const string CounterRateValuePublished = "CounterRateValuePublished";
    private const string GaugeValuePublished = "GaugeValuePublished";
    private const string HistogramValuePublished = "HistogramValuePublished";
    private const string UpDownCounterRateValuePublished = "UpDownCounterRateValuePublished";

    protected override void HandleEvent(TraceEvent evt)
    {
        if (evt.ProcessID != Configuration.ProcessId) return;
        if (evt.ProviderName != ProviderNames.SystemDiagnosticsMetrics) return;
        if (evt.EventName != CounterRateValuePublished &&
            evt.EventName != GaugeValuePublished &&
            evt.EventName != HistogramValuePublished &&
            evt.EventName != UpDownCounterRateValuePublished) return;

        var sessionId = (string)evt.PayloadValue(0);
        if (sessionId != Configuration.SessionId) return;

        var meterName = (string)evt.PayloadValue(1);
        var instrumentName = (string)evt.PayloadValue(3);
        if (meterName is null || instrumentName is null)
        {
            return;
        }

        var units = (string)evt.PayloadValue(4);
        var tags = (string)evt.PayloadValue(5);

        switch (evt.EventName)
        {
            case CounterRateValuePublished:
                HandleCounterRateEvent(evt, instrumentName, meterName, units, tags);
                break;
            case GaugeValuePublished:
                HandleGaugeEvent(evt, instrumentName, meterName, units, tags);
                break;
            case HistogramValuePublished:
                HandleHistogramEvent(evt, instrumentName, meterName, units, tags);
                break;
            case UpDownCounterRateValuePublished:
                HandleUpDownCounterRateEvent(evt, instrumentName, meterName, units, tags);
                break;
        }
    }

    private void HandleCounterRateEvent(TraceEvent evt, string instrumentName, string meterName, string? units,
        string? tags)
    {
        var rateValue = (string)evt.PayloadValue(6);
        if (!double.TryParse(rateValue, NumberStyles.Number | NumberStyles.Float, CultureInfo.InvariantCulture,
                out var rate))
            return;

        var counter = MapToRateCounter(
            evt.TimeStamp,
            instrumentName,
            units,
            meterName,
            rate,
            tags,
            Configuration.RefreshInterval);

        Writer.TryWrite(counter);
    }

    private void HandleGaugeEvent(TraceEvent evt, string instrumentName, string meterName, string? units, string? tags)
    {
        var gaugeValue = (string)evt.PayloadValue(6);
        if (!double.TryParse(gaugeValue, NumberStyles.Number | NumberStyles.Float, CultureInfo.InvariantCulture,
                out var lastValue))
            return;

        var counter = MapToMetricCounter(
            evt.TimeStamp,
            instrumentName,
            units,
            meterName,
            lastValue,
            tags);

        Writer.TryWrite(counter);
    }

    private void HandleHistogramEvent(TraceEvent evt, string instrumentName, string meterName, string? units,
        string? tags)
    {
        var quantiles = (string)evt.PayloadValue(6);
        if (string.IsNullOrEmpty(quantiles)) return;
        var quantileValues = ParseQuantiles(quantiles.AsSpan());

        var counter = MapToMetricCounter(evt.TimeStamp, instrumentName, units, meterName, quantileValues.Value50,
            CombineTagsAndQuantiles(tags, "Percentile=50"));
        Writer.TryWrite(counter);

        counter = MapToMetricCounter(evt.TimeStamp, instrumentName, units, meterName, quantileValues.Value95,
            CombineTagsAndQuantiles(tags, "Percentile=95"));
        Writer.TryWrite(counter);

        counter = MapToMetricCounter(evt.TimeStamp, instrumentName, units, meterName, quantileValues.Value99,
            CombineTagsAndQuantiles(tags, "Percentile=99"));
        Writer.TryWrite(counter);
        return;

        static string CombineTagsAndQuantiles(string tagString, string quantileString) =>
            string.IsNullOrEmpty(tagString) ? quantileString : $"{tagString},{quantileString}";
    }

    private void HandleUpDownCounterRateEvent(TraceEvent evt, string instrumentName, string meterName, string? units,
        string? tags)
    {
        var counterValue = (string)evt.PayloadValue(7);
        if (!double.TryParse(counterValue, NumberStyles.Number | NumberStyles.Float, CultureInfo.InvariantCulture,
                out var value))
            return;

        var counter = MapToMetricCounter(
            evt.TimeStamp,
            instrumentName,
            units,
            meterName,
            value,
            tags);

        Writer.TryWrite(counter);
    }

    private static Quantiles ParseQuantiles(ReadOnlySpan<char> quantiles)
    {
        var firstDelimiterIndex = quantiles.IndexOf(';');
        var value50 = ParsePair(quantiles[..firstDelimiterIndex].Trim());

        quantiles = quantiles[(firstDelimiterIndex + 1)..];
        var secondDelimiterIndex = quantiles.IndexOf(';');
        var value95 = ParsePair(quantiles.Slice(0, secondDelimiterIndex).Trim());

        quantiles = quantiles[(secondDelimiterIndex + 1)..];
        var value99 = ParsePair(quantiles);

        return new Quantiles(value50, value95, value99);
    }

    private static double ParsePair(ReadOnlySpan<char> pair)
    {
        var pairDelimiter = pair.IndexOf('=');
        var valueSlice = pair[(pairDelimiter + 1)..].Trim();

        return double.TryParse(valueSlice.ToString(), NumberStyles.Number | NumberStyles.Float,
            CultureInfo.InvariantCulture, out var value)
            ? value
            : 0;
    }

    private readonly record struct Quantiles(double Value50, double Value95, double Value99);
}