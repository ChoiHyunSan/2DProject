import net.grinder.plugin.http.HTTPRequest
import net.grinder.script.GTest
import net.grinder.scriptengine.groovy.junit.GrinderRunner
import net.grinder.scriptengine.groovy.junit.annotation.BeforeProcess
import net.grinder.scriptengine.groovy.junit.annotation.BeforeThread
import org.junit.Test
import org.junit.runner.RunWith
import HTTPClient.HTTPResponse
import HTTPClient.NVPair
import static net.grinder.script.Grinder.grinder
import groovy.json.JsonOutput

/*
[조절 포인트 요약]  (nGrinder 콘솔 → Test Config → Properties 에서 키=값 추가)
- BASE_URL            : 대상 API (기본: http://host.docker.internal:5274)
- TARGET_TPS          : 목표 TPS (에이전트 1대 기준). 여러 대면 "총 목표 / 에이전트수"로 나눠 넣기.
- RAMP_SEC            : 0→TARGET_TPS까지 선형 램프업 시간(초)
- LOGIN_INTERVAL_MS   : 각 로그인 후 추가 대기(think time). 보통 0, 올리면 TPS↓
- RUN_ID              : 계정 고유 토큰(러닝마다 바꾸면 새 유저)

[권장 시작값(120~150 TPS 확인용)]
TARGET_TPS=150, RAMP_SEC=90, LOGIN_INTERVAL_MS=0, RUN_ID=20250828
에이전트=1, Processes×Threads는 적절히(예: 1×150). SLO/에러율 안정 확인.

[회원가입 수행 여부]
아래 @BeforeThread 의 registerOnce() 주석 해제 시, 각 VUser 최초 1회 등록 수행(409도 성공 취급).
*/

@RunWith(GrinderRunner)
class RegLogin {
  static GTest REG, LOG
  static HTTPRequest HTTP
  static String BASE
  static long INTERVAL
  static String RUN_ID
  static NVPair[] JSON_HDR

  // TPS 페이싱(하드 캡: 목표 간격보다 빠르게 못 보냄)
  static long TARGET_TPS, RAMP_SEC, START_MS
  static int LOCAL_THREADS
  long nextAt = 0L

  String email, password
  boolean registered

  @BeforeProcess
  static void bp() {
    BASE       = System.getProperty("BASE_URL","http://host.docker.internal:5274")  // 타겟 API
    INTERVAL   = Long.getLong("LOGIN_INTERVAL_MS",0L)                                // 추가 대기(ms)
    RUN_ID     = System.getProperty("RUN_ID","run")                                  // 유저 구분 토큰
    TARGET_TPS = Long.getLong("TARGET_TPS",120L)                                     // 목표 TPS(에이전트 1대)
    RAMP_SEC   = Long.getLong("RAMP_SEC",60L)                                        // 램프업(초)
    START_MS   = System.currentTimeMillis()
    // 프로퍼티에서 설정된 전체 VUser 수(프로세스×스레드) 기준으로 스레드당 TPS 배분
    LOCAL_THREADS =
      grinder.properties.getInt("grinder.processes",1) *
      grinder.properties.getInt("grinder.threads",1)

    HTTP = new HTTPRequest()
    REG = new GTest(1,"POST /api/register"); REG.record(HTTP)
    LOG = new GTest(2,"POST /api/login");    LOG.record(HTTP)
    JSON_HDR = [ new NVPair("Content-Type","application/json") ] as NVPair[]
  }

  @BeforeThread
  void bt() {
    int p = grinder.processNumber, t = grinder.threadNumber
    email = "user-${RUN_ID}-${p}-${t}@example.com"
    password = "pw-${RUN_ID}-${p}-${t}"
    // registerOnce()   // 등록도 함께 테스트하려면 주석 해제
  }

  void registerOnce() {
    if (registered) return
    String body = JsonOutput.toJson([email:email, password:password])
    HTTPResponse r = HTTP.POST(BASE+"/api/register", body.getBytes("UTF-8"), JSON_HDR)
    int sc = r.statusCode
    if (!((sc>=200 && sc<300) || sc==409)) throw new AssertionError("register "+sc+" "+r.getText())
    registered = true
  }

  // 하드 캡 페이싱: 지연이 생겨도 '추월 전송' 없이 일정 간격 유지
  void pace() {
    long now = System.currentTimeMillis()
    double ratio = Math.min(1.0, (now - START_MS) / 1000.0 / RAMP_SEC)
    double curTPS = Math.max(1.0, TARGET_TPS * ratio)
    double perThreadTPS = Math.max(0.1, curTPS / Math.max(1, LOCAL_THREADS))
    long interval = (long) Math.max(1, 1000.0 / perThreadTPS) // 스레드당 목표 간격(ms)

    if (nextAt == 0L) nextAt = now + interval
    while (nextAt <= now) nextAt += interval   // 지연 시에도 절대 속도 초과 금지
    grinder.sleep(nextAt - now)
    nextAt += interval
  }

  @Test
  void run() {
    pace()
    String body = JsonOutput.toJson([email:email, password:password])
    HTTPResponse r = HTTP.POST(BASE+"/api/login", body.getBytes("UTF-8"), JSON_HDR)
    if (r.statusCode != 200) throw new AssertionError("login "+r.statusCode+" "+r.getText())
    if (INTERVAL > 0) grinder.sleep(INTERVAL)  // 추가 think time(선택)
  }
}
