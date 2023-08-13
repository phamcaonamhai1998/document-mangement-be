namespace WebApi.Models;

public class BaseReponse<T>
{
    private bool IsSuccess { get; set; }
    private T Data { get; set; }
    private string ErrorMessage { get; set; }
    BaseReponse(bool isSuccess, T data, string error)
    {
        this.IsSuccess = isSuccess;
        this.Data = data;
        this.ErrorMessage = error;
    }


}
