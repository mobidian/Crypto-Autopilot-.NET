using Application.Exceptions;

using CryptoExchange.Net.Objects;

namespace Infrastructure.Extensions;

internal static class CallResultExtensions
{
    internal static void ThrowIfHasError(this CallResult callResult) => callResult.ThrowIfHasError_private();
    internal static void ThrowIfHasError<T>(this CallResult<T> callResult) => callResult.ThrowIfHasError_private();

    internal static void ThrowIfHasError(this CallResult callResult, string additionalMessage) => callResult.ThrowIfHasError_private(additionalMessage);
    internal static void ThrowIfHasError<T>(this CallResult<T> callResult, string additionalMessage) => callResult.ThrowIfHasError_private(additionalMessage);

    internal static void ThrowIfHasError(this CallResult callResult, Exception exception) => callResult.ThrowIfHasError_private(exception);
    internal static void ThrowIfHasError<T>(this CallResult<T> callResult, Exception exception) => callResult.ThrowIfHasError_private(exception);




    private static void ThrowIfHasError_private(this CallResult callResult)
    {
        if (callResult.Success)
            return;

        throw new InternalTradingServiceException(callResult.Error!.ToString().Trim());
    }
    private static void ThrowIfHasError_private(this CallResult callResult, string additionalMessage)
    {
        if (callResult.Success)
            return;

        var errorMessage = callResult.Error!.ToString().Trim();
        throw new InternalTradingServiceException($"{additionalMessage!.Trim()} | Error: {errorMessage}");
    }
    private static void ThrowIfHasError_private(this CallResult callResult, Exception exception)
    {
        if (callResult.Success)
            return;

        throw exception;
    }
}
