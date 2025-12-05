namespace Business.SmartAppt.Models;

public class BaseResponse
{
    public BaseResponseStatus Status { get; set; }
}

public class BaseResponse<T> : BaseResponse
{
    public T? Data { get; set; }

    public static BaseResponse<T> Create(BaseResponseStatus status, T? data)
        => new BaseResponse<T> { Status = status, Data = data };
}