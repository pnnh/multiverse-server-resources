
namespace Gliese.Models
{

    public class CommonResult<T>
    {
        public int Code { get; set; } = 200;
        public string? Message { get; set; } = "";
        public T? Data { get; set; } = default(T);
    }
}