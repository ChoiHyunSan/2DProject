namespace APIServer;

using System.Diagnostics.CodeAnalysis;

public readonly record struct Result(ErrorCode ErrorCode)
{
    public static readonly Result Ok = new(ErrorCode.None);
    public bool IsSuccess => ErrorCode == ErrorCode.None;
    public bool IsFailed => ErrorCode != ErrorCode.None;
    
    public static Result Success() => Ok;
    public static Result Failure(ErrorCode error) => new(error);
    
    // 튜플처럼 쓸 수 있게
    public void Deconstruct(out ErrorCode error) => error = ErrorCode;

    // ErrorCode로의 암시 변환 허용(원하면)
    public static implicit operator Result(ErrorCode error) => new(error);
}

public readonly record struct Result<T>(ErrorCode ErrorCode, T? Value)
{
    [MemberNotNullWhen(true, nameof(Value))]
    public bool IsSuccess => ErrorCode == ErrorCode.None;
    public bool IsFailed => ErrorCode != ErrorCode.None;
    
    public static Result<T> Success(T value) => new(ErrorCode.None, value);
    public static Result<T> Failure(ErrorCode error) => new(error, default);
    public static Result<T> Failure(ErrorCode error, T? value) => new(error, value);

    // callsite 가독성 개선
    public void Deconstruct(out ErrorCode error, out T? value)
    { error = ErrorCode; value = Value; }

    // 성공 보장 액세스(디버깅 도움)
    public T Require() => IsSuccess ? Value! :
        throw new InvalidOperationException($"Result not successful: {ErrorCode}");
}
