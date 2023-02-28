
namespace Gliese.Models
{

    public class CommonResult<T>
    {
        public int Code { get; set; } = 200;
        public string? Message { get; set; } = "";
        public T? Data { get; set; } = default(T);
    }

    public class Codes {
        public const int Success = 200;
        public const int Error = 500;
        public const int NotFound = 404;
        public const int BadRequest = 400;
    }
}