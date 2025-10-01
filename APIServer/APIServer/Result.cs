namespace APIServer;

using System.Diagnostics.CodeAnalysis;

public readonly record struct Result(ErrorCode ErrorCode)
{
    public static readonly Result Ok = new(ErrorCode.None);
    public bool IsSuccess => ErrorCode == ErrorCode.None;
    public bool IsFailed => ErrorCode != ErrorCode.None;
    
    public static Result Success() => Ok;
    public static Result Failure(ErrorCode error) => new(error);
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
}
