using System.Collections.Generic;

namespace TripApi.DTOs
{
    public class PaginatedResult<T>
    {
        public int PageNum { get; set; }
        public int PageSize { get; set; }
        public int AllPages { get; set; }
        public List<T> Items { get; set; }
    }
}