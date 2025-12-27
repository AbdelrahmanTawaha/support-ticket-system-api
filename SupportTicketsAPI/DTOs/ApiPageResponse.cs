namespace SupportTicketsAPI
{
    public class ApiResponse<T>
    {
        public int ErrorCode { get; set; }


        public string? MsgError { get; set; }


        public T? Data { get; set; }
    }



    public class ApiPageResponse<T> : ApiResponse<T>
    {




        public int TotalCount { get; set; }
    }
}
