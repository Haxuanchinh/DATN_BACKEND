namespace SportManager.Application.Common.Models;

public class ResultModel
{
    public ResultModel()
    {
    }

    public ResultModel(Dictionary<string, List<string>> errors)
    {
        Error = errors;
    }

    public Dictionary<string, List<string>> Error { get; set; }

    public static ResultModel Failure(Dictionary<string, List<string>> errors)
    {
        return new ResultModel(errors);
    }

    public static ResultModel Failure(string errorCode)
    {
        return new ResultModel(new Dictionary<string, List<string>>
            { { "errorCode", new List<string> { errorCode } } });
    }

    public string GetErrorCode()
    {
        return Error["errorCode"].FirstOrDefault() ?? string.Empty;
    }
}
