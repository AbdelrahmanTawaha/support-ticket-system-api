

namespace BusinessLayer.Responses
{
    public enum ErrorCode
    {
        Success = 0,
        GeneralError = 1

    }

    public class Response<T>
    {
        public T Data { get; set; }
        public string MsgError { get; set; } = string.Empty;
        public ErrorCode ErrorCode { get; set; } = ErrorCode.Success;
    }

    public class PageResponse<T> : Response<T>
    {
        public int TotalCount { get; set; }
    }
}
