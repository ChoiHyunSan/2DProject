# 테스트 코드 규약 및 템플릿

## 코드 규약

### 1. 네이밍 & 파일 구조
- 프로젝트명: `APIServer.Tests` (src와 1:1 매핑)
- 폴더 구조: `tests/APIServer.Tests/<레이어>/<대상>`  
  예) `Service/AccountServiceTests.cs`
- 클래스명: `<대상클래스명>Tests`
- 메서드명: `Method_Scenario_Expected`  
  예) `RegisterAccountAsync_EmailAlreadyExists_ReturnsDuplicatedEmail`
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

### (A) 단위 테스트 기본 템플릿
```csharp
[Fact]
public async Task Method_Scenario_Expected()
{
    // Given
    // ... 준비 코드 (Mock, 입력 값 세팅)

    // When
    var result = await sut.MethodAsync(...);

    // Then
    result.IsSuccess.Should().BeTrue();
    result.ErrorCode.Should().Be(ErrorCode.None);
}
```

### (B) 파라미터화(Theory) 템플릿
```csharp
[Theory]
[InlineData("input1", "pw1")]
[InlineData("input2", "pw2")]
public async Task Method_Scenario_Expected_ForVariousInputs(string email, string pw)
{
    // Given
    _repo.Setup(...);

    // When
    var result = await sut.MethodAsync(email, pw);

    // Then
    result.IsSuccess.Should().BeTrue();
}
```

### (C) 예외 매핑 템플릿
```csharp
[Fact]
public async Task Method_WhenDependencyThrows_ReturnsErrorCode()
{
    // Given
    _repo.Setup(x => x.SomeMethod(It.IsAny<string>()))
         .ThrowsAsync(new TimeoutException("db timeout"));

    // When
    var result = await sut.MethodAsync("x@y.com", "pw");

    // Then
    result.IsSuccess.Should().BeFalse();
    result.ErrorCode.Should().Be(ErrorCode.FailedRegister);
}
```
