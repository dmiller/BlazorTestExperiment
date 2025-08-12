using System.Diagnostics.CodeAnalysis;

namespace Bte.MediatR;

// The original code had only Result and Result<TValue> classes.
// With Result<TValue> deriving from Result.
// 
// For both, the type of the error data was Logan.MediatR.Error.
//
// I decide I might need more detail information on Error, so I added Result<TValue, TError>.
//
// There is no good way to derive Result<TValue, TError> from Result.  (Error type is fixed in Result.)
//
// There is no good way to derive Result from Result<TValue, TError>:  We can put in Error for TError, but there is nothing for TValue.
// And the way some of the code is written, with a supplied value of null meaning failure,
//  I really couldn't figure out how even to derive Result<TValue> from Result<TValue, TError> by specialing TError to Error.
//
// So I just bailed and created Result<TValue, TError> independently.
//  
// Sigh.


public class Result
{
    public Result(bool isSuccess, Error error)
    {
        if (isSuccess && error != Error.None ||
            !isSuccess && error == Error.None)
        {
            throw new ArgumentException("Invalid error", nameof(error));
        }

        IsSuccess = isSuccess;
        Error = error;
    }

    public bool IsSuccess { get; }

    public bool IsFailure => !IsSuccess;

    public Error Error { get; }

    public static Result Success() => new(true, Error.None);

    public static Result<TValue> Success<TValue>(TValue value) =>
        new(value, true, Error.None);

    public static Result Failure(Error error) => new(false, error);

    public static Result<TValue> Failure<TValue>(Error error) =>
        new(default, false, error);
}

public class Result<TValue>(TValue? value, bool isSuccess, Error error) : Result(isSuccess, error)
{
    [NotNull]
    public TValue Value => IsSuccess
        ? value!
        : throw new InvalidOperationException("The value of a failure result can't be accessed.");

    public static implicit operator Result<TValue>(TValue? value) =>
        value is not null ? Success(value) : Failure<TValue>(Error.NullValue);

    public static Result<TValue> ValidationFailure(Error error) =>
        new(default, false, error);
}




public class Result<TValue, TError>
{
    internal TValue? _value;
    internal TError? _error;
    internal bool _isSuccess;

    private Result()
    {
        _isSuccess = false;
        _value = default;
        _error = default;
    }

    public TError Error => IsFailure
        ? _error!
        : throw new InvalidOperationException("The error of a success result can't be accessed.");

    public TValue Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("The value of a failure result can't be accessed.");

    public bool IsSuccess => _isSuccess;
    public bool IsFailure => !_isSuccess;

    public static Result<TValue, TError> Success(TValue resultValue)
    {
        Result<TValue, TError> result = new()
        {
            _isSuccess = true,
            _value = resultValue
        };
        return result;
    }

    public static Result<TValue, TError> Failure(TError error)
    {
        Result<TValue, TError> result = new()
        {
            _isSuccess = false,
            _error = error
        };
        return result;
    }

    public static implicit operator Result<TValue, TError>(TValue value) => Success(value);

    public static Result<TValue, TError> ValidationFailure(TError error) => Failure(error);

}