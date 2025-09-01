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
import java.util.concurrent.ThreadLocalRandom as TLR

@RunWith(GrinderRunner)
class GameFlow {
  // endpoints
  static String BASE
  static String PATH_LOGIN            = "/api/login"
  static String PATH_ATTEND           = "/api/Atten"
  static String PATH_INV_CHAR         = "/api/GetInventoryCharacter"
  static String PATH_INV_ITEMS        = "/api/GetInventoryItem"
  static String PATH_INV_RUNES        = "/api/GetInventoryRune"
  static String PATH_EQUIP_ITEM       = "/api/Equipment/item"
  static String PATH_UNEQUIP_ITEM     = "/api/UnEquip/item"
  static String PATH_EQUIP_RUNE       = "/api/Equipment/rune"
  static String PATH_UNEQUIP_RUNE     = "/api/UnEquip/rune"
  static String PATH_MAIL_LIST        = "/api/GetMail"
  static String PATH_QUEST_LIST       = "/api/GetProgressQuest"

  // http
  static HTTPRequest HTTP
  static NVPair[] JSON_HDR
  static GTest T_LOGIN, T_FLOW

  // pacing
  static long TARGET_TPS, RAMP_SEC, START_MS
  static int LOCAL_THREADS
  long nextAt = 0L

  // 스레드 별 변수
  String email, password, authToken

  // 스레드 별 상태 변수
  List<Long> characterIds = []
  Set<Long> ownedItemIds = [] as Set
  Set<Long> ownedRuneIds = [] as Set
  Map<Long, Set<Long>> charEquippedItems = [:].withDefault { [] as Set }
  Map<Long, Set<Long>> charEquippedRunes = [:].withDefault { [] as Set }

  // 가중치
  static int W_ATTEND, W_INV, W_MAIL, W_QUEST, W_EQ_ITEM, W_UNEQ_ITEM, W_EQ_RUNE, W_UNEQ_RUNE

  // 페이지 옵션
  static int PAGE_SIZE

  @BeforeProcess
  static void bp() {
    // 환경 변수 로드
    BASE       = System.getProperty("BASE_URL", "http://host.docker.internal:5274")
    TARGET_TPS = Long.getLong("TARGET_TPS", 120L)
    RAMP_SEC   = Long.getLong("RAMP_SEC", 60L)
    PAGE_SIZE  = Integer.getInteger("PAGE_SIZE", 20)

    // API 가중치 기본값
    W_ATTEND     = Integer.getInteger("W_ATTEND",     0)
    W_INV        = Integer.getInteger("W_INV",        6)
    W_MAIL       = Integer.getInteger("W_MAIL",       3)
    W_QUEST      = Integer.getInteger("W_QUEST",      3)
    W_EQ_ITEM    = Integer.getInteger("W_EQ_ITEM",    2)
    W_UNEQ_ITEM  = Integer.getInteger("W_UNEQ_ITEM",  1)
    W_EQ_RUNE    = Integer.getInteger("W_EQ_RUNE",    2)
    W_UNEQ_RUNE  = Integer.getInteger("W_UNEQ_RUNE",  1)

    // 로컬 스레드 수 계산
    START_MS = System.currentTimeMillis()
    LOCAL_THREADS =
      grinder.properties.getInt("grinder.processes", 1) *
      grinder.properties.getInt("grinder.threads", 1)

    // HTTP 초기화 및 테스트 레코드
    HTTP = new HTTPRequest()
    JSON_HDR = [ new NVPair("Content-Type", "application/json") ] as NVPair[]
    T_LOGIN = new GTest(1, "POST /api/login"); T_LOGIN.record(HTTP)
    T_FLOW  = new GTest(2, "mixed flow");      T_FLOW.record(HTTP)
  }

  @BeforeThread
  void bt() {
    // 스레드별 계정 구성
    int p = grinder.processNumber
    int t = grinder.threadNumber
    String runId = System.getProperty("RUN_ID", "run")
    email = "user-${runId}-${p}-${t}@example.com"
    password = "pw-${runId}-${p}-${t}"

    // 로그인 및 상태 초기화
    loginOnceAndPrime()
  }

  // 바디 생성 메서드
  byte[] bodyAuth() {
    return JsonOutput.toJson([email: email, authToken: authToken]).getBytes("UTF-8")
  }

  byte[] bodyAuthPage(int page, int size) {
    return JsonOutput.toJson([email: email, authToken: authToken, pageable: [page: page, size: size]]).getBytes("UTF-8")
  }

  byte[] bodyEquipItem(long characterId, long itemId) {
    return JsonOutput.toJson([email: email, authToken: authToken, characterId: characterId, itemId: itemId]).getBytes("UTF-8")
  }

  byte[] bodyEquipRune(long characterId, long runeId) {
    return JsonOutput.toJson([email: email, authToken: authToken, characterId: characterId, runeId: runeId]).getBytes("UTF-8")
  }

  // HTTP 상태 헬퍼
  static boolean ok(HTTPResponse r) {
    return r.statusCode >= 200 && r.statusCode < 300
  }

  static boolean conflict(HTTPResponse r) {
    return r.statusCode == 409
  }

  void loginOnceAndPrime() {
    // 1 요청 바디 준비
    String body = JsonOutput.toJson([email: email, password: password])

    // 2 로그인 요청
    HTTPResponse r = HTTP.POST(BASE + PATH_LOGIN, body.getBytes("UTF-8"), JSON_HDR)

    // 3 상태 확인
    if (!ok(r)) {
      throw new AssertionError("login " + r.statusCode + " " + r.getText())
    }

    // 4 응답 파싱
    def json = new JsonSlurper().parseText(r.getText())
    authToken = (json.authToken ?: "").toString()
    if (!authToken) {
      throw new AssertionError("missing authToken")
    }

    // 5 게임 데이터로 로컬 상태 구성
    def gd = json.gameData ?: [:]
    characterIds = (gd.characters ?: []).collect { asLong(it.characterId) }
    ownedItemIds = ((gd.items ?: []).collect { asLong(it.itemId) }) as Set
    ownedRuneIds = ((gd.runes ?: []).collect { asLong(it.runeId) }) as Set

    charEquippedItems.clear()
    charEquippedRunes.clear()

    (gd.characters ?: []).each { c ->
      long cid = asLong(c.characterId)

      def ei = (c.equipItems ?: []).collect { asLong(it) } as Set
      def er = (c.equipRunes ?: []).collect { asLong(it) } as Set

      charEquippedItems[cid].addAll(ei)
      charEquippedRunes[cid].addAll(er)
    }
  }

  void pace() {
    // 1 현재 시간 및 램프 비율 계산
    long now = System.currentTimeMillis()
    double ratio = Math.min(1.0, (now - START_MS) / 1000.0 / RAMP_SEC)

    // 2 현재 목표 TPS 및 스레드당 TPS 계산
    double curTPS = Math.max(1.0, TARGET_TPS * ratio)
    double perThreadTPS = Math.max(0.1, curTPS / Math.max(1, LOCAL_THREADS))

    // 3 인터벌 산출
    long interval = (long) Math.max(1, 1000.0 / perThreadTPS)

    // 4 nextAt 초기화
    if (nextAt == 0L) {
      nextAt = now
    }

    // 5 슬립 계산 및 수행
    long sleepMs = nextAt - now
    if (sleepMs > 0) {
      grinder.sleep(sleepMs)
    }

    // 6 다음 실행 시각 갱신
    nextAt += interval
  }

  // 액션 메서드
  void attend() {
    // 1 요청
    HTTPResponse r = HTTP.POST(BASE + PATH_ATTEND, bodyAuth(), JSON_HDR)

    // 2 응답 확인  2xx 또는 409 허용
    if (!(ok(r) || conflict(r))) {
      throw new AssertionError("attend " + r.statusCode)
    }
  }

  void inventoryReads() {
    // 1 캐릭터 목록
    HTTPResponse r1 = HTTP.POST(BASE + PATH_INV_CHAR, bodyAuth(), JSON_HDR)

    // 2 아이템 목록
    HTTPResponse r2 = HTTP.POST(BASE + PATH_INV_ITEMS, bodyAuthPage(1, PAGE_SIZE), JSON_HDR)

    // 3 룬 목록
    HTTPResponse r3 = HTTP.POST(BASE + PATH_INV_RUNES, bodyAuthPage(1, PAGE_SIZE), JSON_HDR)

    // 4 응답 확인
    if (!ok(r1)) {
      throw new AssertionError("inv char " + r1.statusCode)
    }
    if (!ok(r2)) {
      throw new AssertionError("inv items " + r2.statusCode)
    }
    if (!ok(r3)) {
      throw new AssertionError("inv runes " + r3.statusCode)
    }
  }

  void mailList() {
    // 1 요청
    HTTPResponse r = HTTP.POST(BASE + PATH_MAIL_LIST, bodyAuthPage(1, PAGE_SIZE), JSON_HDR)

    // 2 응답 확인
    if (!ok(r)) {
      throw new AssertionError("mail " + r.statusCode)
    }
  }

  void questList() {
    // 1 요청
    HTTPResponse r = HTTP.POST(BASE + PATH_QUEST_LIST, bodyAuthPage(1, PAGE_SIZE), JSON_HDR)

    // 2 응답 확인
    if (!ok(r)) {
      throw new AssertionError("quest " + r.statusCode)
    }
  }

  void equipItem() {
    // 1 캐릭터 보유 확인
    if (characterIds.isEmpty()) {
      return
    }

    // 2 대상 캐릭터 선택
    long cid = pick(characterIds)

    // 3 현재 장착 중인 전체 아이템 집합 생성
    Set<Long> equippedAll = charEquippedItems.values().flatten() as Set<Long>

    // 4 미장착 보유 아이템 후보 산출
    List<Long> candidates = ownedItemIds.findAll { !equippedAll.contains(it) }
    if (candidates.isEmpty()) {
      return
    }

    // 5 아이템 선택 및 요청
    long itemId = pick(candidates)
    HTTPResponse r = HTTP.POST(BASE + PATH_EQUIP_ITEM, bodyEquipItem(cid, itemId), JSON_HDR)

    // 6 응답 처리 및 로컬 상태 갱신
    if (ok(r)) {
      charEquippedItems[cid].add(itemId)
    } else if (!conflict(r)) {
      throw new AssertionError("equipItem " + r.statusCode + " " + r.getText())
    }
  }

  void unequipItem() {
    // 1 캐릭터 보유 확인
    if (characterIds.isEmpty()) {
      return
    }

    // 2 대상 캐릭터 선택
    long cid = pick(characterIds)

    // 3 캐릭터 장착 목록 확인
    Set<Long> eq = charEquippedItems[cid]
    if (eq.isEmpty()) {
      return
    }

    // 4 해제 대상 선택 및 요청
    long itemId = pick(eq as List<Long>)
    HTTPResponse r = HTTP.POST(BASE + PATH_UNEQUIP_ITEM, bodyEquipItem(cid, itemId), JSON_HDR)

    // 5 응답 처리 및 로컬 상태 갱신
    if (ok(r)) {
      eq.remove(itemId)
    } else if (!conflict(r)) {
      throw new AssertionError("unequipItem " + r.statusCode + " " + r.getText())
    }
  }

  void equipRune() {
    // 1 캐릭터 보유 확인
    if (characterIds.isEmpty()) {
      return
    }

    // 2 대상 캐릭터 선택
    long cid = pick(characterIds)

    // 3 현재 장착 중인 전체 룬 집합 생성
    Set<Long> equippedAll = charEquippedRunes.values().flatten() as Set<Long>

    // 4 미장착 보유 룬 후보 산출
    List<Long> candidates = ownedRuneIds.findAll { !equippedAll.contains(it) }
    if (candidates.isEmpty()) {
      return
    }

    // 5 룬 선택 및 요청
    long runeId = pick(candidates)
    HTTPResponse r = HTTP.POST(BASE + PATH_EQUIP_RUNE, bodyEquipRune(cid, runeId), JSON_HDR)

    // 6 응답 처리 및 로컬 상태 갱신
    if (ok(r)) {
      charEquippedRunes[cid].add(runeId)
    } else if (!conflict(r)) {
      throw new AssertionError("equipRune " + r.statusCode + " " + r.getText())
    }
  }

  void unequipRune() {
    // 1 캐릭터 보유 확인
    if (characterIds.isEmpty()) {
      return
    }

    // 2 대상 캐릭터 선택
    long cid = pick(characterIds)

    // 3 캐릭터 장착 룬 목록 확인
    Set<Long> eq = charEquippedRunes[cid]
    if (eq.isEmpty()) {
      return
    }

    // 4 해제 대상 선택 및 요청
    long runeId = pick(eq as List<Long>)
    HTTPResponse r = HTTP.POST(BASE + PATH_UNEQUIP_RUNE, bodyEquipRune(cid, runeId), JSON_HDR)

    // 5 응답 처리 및 로컬 상태 갱신
    if (ok(r)) {
      eq.remove(runeId)
    } else if (!conflict(r)) {
      throw new AssertionError("unequipRune " + r.statusCode + " " + r.getText())
    }
  }

  void doOneStep() {
    // 1 총 가중치 계산
    int total =
      W_ATTEND + W_INV + W_MAIL + W_QUEST +
      W_EQ_ITEM + W_UNEQ_ITEM + W_EQ_RUNE + W_UNEQ_RUNE

    // 2 난수 선택
    int r = TLR.current().nextInt(total)

    // 3 가중치 분기
    if ((r -= W_ATTEND) < 0) {
      attend()
      return
    }
    if ((r -= W_INV) < 0) {
      inventoryReads()
      return
    }
    if ((r -= W_MAIL) < 0) {
      mailList()
      return
    }
    if ((r -= W_QUEST) < 0) {
      questList()
      return
    }
    if ((r -= W_EQ_ITEM) < 0) {
      equipItem()
      return
    }
    if ((r -= W_UNEQ_ITEM) < 0) {
      unequipItem()
      return
    }
    if ((r -= W_EQ_RUNE) < 0) {
      equipRune()
      return
    }

    unequipRune()
    return
  }

  @Test
  void run() {
    pace()
    doOneStep()
  }

  // 유틸
  static long asLong(def v) {
    if (v instanceof Number) {
      return v.longValue()
    }
    return Long.parseLong(v.toString())
  }

  static <T> T pick(List<T> xs) {
    if (xs == null || xs.isEmpty()) {
      return null
    }
    return xs[TLR.current().nextInt(xs.size())]
  }
}
