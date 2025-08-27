using Prometheus;

namespace APIServer.Metrics;

public static class MetricRegistry
{
    // ─────────────────────────────────────────────────────────────────────────────
    // 1) 비즈니스 카운터
    // ─────────────────────────────────────────────────────────────────────────────
    // 로그인 시도 수(성공/실패 합산). 필요하면 라벨(result="ok|fail")을 나중에 추가해도 됨.
    public static readonly Counter LoginTotal =
        Prometheus.Metrics.CreateCounter("login_total", "Login attempts");

    // 스테이지 클리어 시도 수
    public static readonly Counter StageClearTotal =
        Prometheus.Metrics.CreateCounter("stage_clear_total", "Stage clear attempts");

    // 활성화된 세션 수
    public static readonly Gauge ActiveSessions =
        Prometheus.Metrics.CreateGauge("active_sessions", "Active API sessions");

    // ─────────────────────────────────────────────────────────────────────────────
    // 2) 예외 카운터 (대시보드용)
    // ─────────────────────────────────────────────────────────────────────────────
    // 예외 타입/엔드포인트/메서드 단위로 카운팅해 상위 10개 엔드포인트를 집계 가능.
    public static readonly Counter ExceptionsTotal =
        Prometheus.Metrics.CreateCounter("exceptions_total", "Unhandled exception count", new CounterConfiguration
        {
            LabelNames = new[] { "type", "endpoint", "method" }
        });

    // ─────────────────────────────────────────────────────────────────────────────
    // 3) API 지연 시간(초) 히스토그램 (경로·메서드·코드 단위)
    // ─────────────────────────────────────────────────────────────────────────────
    // Built-in UseHttpMetrics의 http_request_duration_seconds(표준)도 이미 존재하지만,
    // 경로(path) 라벨을 커스텀으로 더 촘촘히 보려면 아래 히스토그램을 함께 사용.
    public static readonly Histogram ApiRequestDuration =
        Prometheus.Metrics.CreateHistogram("api_request_duration_seconds", "API request duration (seconds)",
            new HistogramConfiguration
            {
                LabelNames = new[] { "path", "method", "code" },
                // 10ms ~ 약 2분까지 지수 버킷. 필요에 따라 줄이거나 늘리세요.
                Buckets = Histogram.ExponentialBuckets(start: 0.01, factor: 2, count: 15)
            });

    // ─────────────────────────────────────────────────────────────────────────────
    // 4) 헬퍼: 세션 증감과 예외 기록을 한 줄로
    // ─────────────────────────────────────────────────────────────────────────────

    public static void RecordException(Exception ex, string endpoint, string method)
    {
        ExceptionsTotal.WithLabels(ex.GetType().Name, endpoint, method).Inc();
    }
}
