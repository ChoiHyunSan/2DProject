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
import groovy.json.JsonSlurper

/*
[Properties 예시]
BASE_URL=http://host.docker.internal:5274
TARGET_TPS=150            // 목표 TPS(에이전트 1대 기준)
RAMP_SEC=90               // 0→TARGET_TPS 램프업(초)
FLOW_WEIGHTS=40,30,20,10  // 분기 가중치(합계 자유)
THINK_MIN_MS=50           // 추가 지연 최소
THINK_MAX_MS=150          // 추가 지연 최대
LOGIN_INTERVAL_MS=0       // 로그인 후 추가 대기(보통 0)
RUN_ID=20250828           // 유저 구분 토큰

[주의] 이 스크립트는 인증이 필요한 엔드포인트를 모두 POST로 호출하며,
      각 요청 바디에 {email, authToken, ...추가필드}를 포함한다.
*/

@RunWith(GrinderRunner)
class UserFlowBodyAuth {
  static HTTPRequest HTTP
  static GTest T_REG, T_LOGIN, T_PROFILE, T_ITEMS, T_UPDATE, T_LOGOUT
  static String BASE
  static NVPair[] JSON_HDR

  static long TARGET_TPS, RAMP_SEC, START_MS
  static int LOCAL_THREADS
  long nextAt = 0L

  static long INTERVAL
  static String RUN_ID

  String email, password, authToken
  boolean registered

  @BeforeProcess
  static void bp() {
    BASE       = System.getProperty("BASE_URL","http://host.docker.internal:5274")
    TARGET_TPS = Long.getLong("TARGET_TPS",120L)
    RAMP_SEC   = Long.getLong("RAMP_SEC",60L)
    INTERVAL   = Long.getLong("LOGIN_INTERVAL_MS",0L)
    RUN_ID     = System.getProperty("RUN_ID","run")
    START_MS   = System.currentTimeMillis()
    LOCAL_THREADS =
      grinder.properties.getInt("grinder.processes",1) *
      grinder.properties.getInt("grinder.threads",1)

    HTTP = new HTTPRequest()
    T_REG    = new GTest(1,"POST /api/register");        T_REG.record(HTTP)
    T_LOGIN  = new GTest(2,"POST /api/login");           T_LOGIN.record(HTTP)
    T_PROFILE= new GTest(3,"POST /api/profile");         T_PROFILE.record(HTTP)
    T_ITEMS  = new GTest(4,"POST /api/items/list");      T_ITEMS.record(HTTP)
    T_UPDATE = new GTest(5,"POST /api/profile/update");  T_UPDATE.record(HTTP)
    T_LOGOUT = new GTest(6,"POST /api/logout");          T_LOGOUT.record(HTTP)

    JSON_HDR = [ new NVPair("Content-Type","application/json") ] as NVPair[]
  }

  @BeforeThread
  void bt() {
    int p = grinder.processNumber, t = grinder.threadNumber
    email = "user-${RUN_ID}-${p}-${t}@example.com"
    password = "pw-${RUN_ID}-${p}-${t}"
    // registerOnce()     // 최초 1회 회원가입 포함하려면 주석 해제
    login()
  }

  /* ===== 공통 유틸 ===== */
  static byte[] toJsonBytes(Map m) { JsonOutput.toJson(m).getBytes("UTF-8") }
  byte[] authBody(Map extra = [:]) {
    Map m = [email: email, authToken: authToken]
    if (extra) m.putAll(extra)
    return toJsonBytes(m)
  }

  void pace() {
    long now = System.currentTimeMillis()
    double ratio = Math.min(1.0, (now - START_MS) / 1000.0 / RAMP_SEC)
    double curTPS = Math.max(1.0, TARGET_TPS * ratio)
    double perThreadTPS = Math.max(0.1, curTPS / Math.max(1, LOCAL_THREADS))
    long interval = (long)Math.max(1, 1000.0 / perThreadTPS)
    if (nextAt == 0L) nextAt = now + interval
    while (nextAt <= now) nextAt += interval  // 지연 발생 시 추월 금지(하드 캡)
    grinder.sleep(nextAt - now)
    nextAt += interval
  }

  static int[] parseWeights(String s){ s.split(",")*.trim()*.toInteger() as int[] }
  static int pick(int[] w){
    int sum = 0; for (int x : w) sum += x
    int r = (int)(Math.random()*sum), acc = 0
    for (int i=0;i<w.length;i++){ acc+=w[i]; if(r<acc) return i }
    return 0
  }

  /* ===== 액션 ===== */
  void registerOnce() {
    if (registered) return
    byte[] body = toJsonBytes([email: email, password: password])
    HTTPResponse r = HTTP.POST(BASE+"/api/register", body, JSON_HDR)
    int sc = r.statusCode
    if (!((sc>=200 && sc<300) || sc==409)) throw new AssertionError("register "+sc+" "+r.getText())
    registered = true
  }

  void login() {
    byte[] body = toJsonBytes([email: email, password: password])
    HTTPResponse r = HTTP.POST(BASE+"/api/login", body, JSON_HDR)
    if (r.statusCode != 200) throw new AssertionError("login "+r.statusCode+" "+r.getText())
    def js = new JsonSlurper().parseText(r.getText())
    authToken = (js.authToken ?: js.token ?: js.access_token ?: "")
    if (!authToken) throw new AssertionError("no authToken in response")
    if (INTERVAL > 0) grinder.sleep(INTERVAL)
  }

  void profile() {
    HTTPResponse r = HTTP.POST(BASE+"/api/profile", authBody(), JSON_HDR)
    if (r.statusCode != 200) throw new AssertionError("profile "+r.statusCode+" "+r.getText())
  }

  void items() {
    HTTPResponse r = HTTP.POST(BASE+"/api/items/list", authBody([page:1, size:20]), JSON_HDR)
    if (r.statusCode != 200) throw new AssertionError("items "+r.statusCode+" "+r.getText())
  }

  void update() {
    HTTPResponse r = HTTP.POST(BASE+"/api/profile/update",
      authBody([nickname:"u-${grinder.threadNumber}"]), JSON_HDR)
    if (r.statusCode != 200) throw new AssertionError("update "+r.statusCode+" "+r.getText())
  }

  void logoutAndRelogin() {
    HTTPResponse r = HTTP.POST(BASE+"/api/logout", authBody(), JSON_HDR)
    if (!(r.statusCode in [200,204])) throw new AssertionError("logout "+r.statusCode+" "+r.getText())
    login()
  }

  /* ===== 시나리오 루프 ===== */
  @Test
  void run() {
    pace()
    int[] w = parseWeights(System.getProperty("FLOW_WEIGHTS","40,30,20,10"))
    switch (pick(w)) {
      case 0: profile(); break
      case 1: items(); break
      case 2: update(); break
      case 3: logoutAndRelogin(); break
    }
    long min = Long.getLong("THINK_MIN_MS",50L), max = Long.getLong("THINK_MAX_MS",150L)
    if (max > min) grinder.sleep(min + (long)(Math.random()*(max-min)))
  }
}
