using Microsoft.AspNetCore.Mvc;

namespace Models
{
    public class RequestHeaders
    {
        [FromHeader(Name = "Authorization")]
        public string Authorization { get; set; }

        [FromHeader(Name = "User-Id")]
        public string UserId { get; set; }
    }
}
