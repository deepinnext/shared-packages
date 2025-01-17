using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Deepin.ServiceDefaults.ActionResults;
public class InternalServerErrorObjectResult : ObjectResult
{
    public InternalServerErrorObjectResult(object? value) : base(value)
    {
        StatusCode = StatusCodes.Status500InternalServerError;
    }
}