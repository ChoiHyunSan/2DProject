using Prometheus;

namespace APIServer.Log.Metrics;

public class MetricsRegistry
{
    /// 활성 세션 수 ///
    public static readonly Gauge ActiveSessions = Prometheus.Metrics.CreateGauge(
        "active_sessions", "Active API sessions");

    /// 로그인 시도/결과 ///
    public static readonly Counter LoginTotal = Prometheus.Metrics.CreateCounter(
        "login_total", "Login attempts",
        new CounterConfiguration { LabelNames = ["result"] }); // success|fail
    
    /// 스테이지 클리어 시도/결과 ///
    public static readonly Counter StageClearTotal = Prometheus.Metrics.CreateCounter(
        "stage_clear_total", "Stage clear attempts",
        new CounterConfiguration { LabelNames = ["stage", "result"] });

    /// API 처리 시간 (초) - 경로/메서드/상태코드로 구분 ///
    public static readonly Histogram ApiRequestDuration = Prometheus.Metrics.CreateHistogram(
        "api_request_duration_seconds", "API request duration (seconds)",
        new HistogramConfiguration
        {
            LabelNames = ["path", "method", "code"],
            // 10ms ~ 약 40s 커버 (0.01 * 2^12 ≈ 40.96)
            Buckets = Histogram.ExponentialBuckets(start: 0.01, factor: 2, count: 12)
        });
}