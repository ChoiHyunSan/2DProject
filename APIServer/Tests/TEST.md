# 테스트 코드 규약 및 템플릿

아래 문서는 기존 형식을 유지하되, **메서드명은 짧게(대상 메서드 + 케이스 번호)** 표기하고, **시나리오는 DisplayName + 상단 주석**으로 기술하는 규약만 보강/수정했습니다.

## 코드 규약

### 1. 네이밍 & 파일 구조
- 프로젝트명: `APIServer.Tests` (src와 1:1 매핑)
- 폴더 구조: `tests/APIServer.Tests/<레이어>/<대상>`  
  예) `Service/AccountServiceTests.cs`
- 클래스명: `<대상클래스명>Tests`
- **메서드명: `<대상메서드명>_CaseNN`**  
  예) `RegisterAccountAsync_Case01`  
  → 시나리오 설명은 **[Fact(DisplayName="...")]** 과 **상단 주석**으로 기술
- **DisplayName 권장**: `[Fact(DisplayName = "[Feature] 시나리오 요약")]`
- **그룹핑(Trait) 권장**: `[Trait("Target", "<대상메서드명>")]`
- 주석 필수: `// Given // When // Then`

### 2. 테스트 스타일
- 단위(Unit): 외부 의존성은 Moq로 Mock. 비즈니스 로직만 검증.
- 정상/비정상/예외 3셋을 기본으로 준비.

### 3. Arrange(준비) 규칙
- Test Data는 명시적으로 (매직 값 금지, 상수/Builder 사용)
- Mock 규칙:
  - 호출될 것만 `Setup`
  - 중요 경로는 `Verify`

### 4. Assert(검증) 규칙
- FluentAssertions 사용 (`.Should().BeTrue()`, `.ThrowAsync<>()` 등)
- 단일 책임: 한 테스트는 한 시나리오/결과만 검증

### 5. 예외/에러 코드
- 예외 기대: `await act.Should().ThrowAsync<ArgumentException>();`
- 에러 코드 기대: `result.ErrorCode.Should().Be(...);`

### 6. 비동기/성능
- 모든 테스트는 `async Task`

### 7. 기타
- 테스트는 빠르고 결정적(Deterministic)이어야 함
- 실패 메시지는 명확히 (FluentAssertions `.Because()` 활용)

---

## 테스트 코드 템플릿

> 각 테스트 메서드 상단에는 아래와 같은 **시나리오 주석**을 권장합니다.
```
/*
 * Target   : <대상메서드명>
 * Scenario : <시나리오 요약>
 * Given    : <사전 조건>
 * When     : <행동>
 * Then     : <기대 결과>
 */
```

### (A) 단위 테스트 기본 템플릿
```csharp
[Fact(DisplayName = "[<Feature>] <자연어 시나리오 요약>")]
[Trait("Target", "<대상메서드명>")]
public async Task <대상메서드명>_CaseNN()
{
    // Given
    // ... 준비 코드 (Mock, 입력 값 세팅)

    // When
    var result = await sut.<대상메서드명>(/* args */);

    // Then
    result.IsSuccess.Should().BeTrue();
    result.ErrorCode.Should().Be(ErrorCode.None);
}
```

### (B) 파라미터화(Theory) 템플릿
```csharp
[Theory(DisplayName = "[<Feature>] 다양한 입력에 대해 동일 시나리오 검증")]
[Trait("Target", "<대상메서드명>")]
[InlineData("input1", "pw1")]
[InlineData("input2", "pw2")]
public async Task <대상메서드명>_CaseNN(string email, string pw)
{
    // Given
    _repo.Setup(...);

    // When
    var result = await sut.<대상메서드명>(email, pw);

    // Then
    result.IsSuccess.Should().BeTrue();
}
```

### (C) 예외 매핑 템플릿
```csharp
[Fact(DisplayName = "[<Feature>] 의존성 예외 발생 시 에러코드 매핑")]
[Trait("Target", "<대상메서드명>")]
public async Task <대상메서드명>_CaseNN()
{
    // Given
    _repo.Setup(x => x.SomeMethod(It.IsAny<string>()))
         .ThrowsAsync(new TimeoutException("db timeout"));

    // When
    var result = await sut.<대상메서드명>("x@y.com", "pw");

    // Then
    result.IsSuccess.Should().BeFalse();
    result.ErrorCode.Should().Be(ErrorCode.FailedRegister);
}
```
