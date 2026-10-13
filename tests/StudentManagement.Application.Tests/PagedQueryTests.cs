using StudentManagement.Application.Common;
using StudentManagement.Application.Students;

namespace StudentManagement.Application.Tests;

public class PagedQueryTests
{
    [Theory]
    [InlineData(0, 0, 1, PagedQuery.DefaultPageSize)]  // unset proto fields arrive as 0
    [InlineData(-3, -1, 1, PagedQuery.DefaultPageSize)]
    [InlineData(3, 500, 3, PagedQuery.MaxPageSize)]
    [InlineData(2, 10, 2, 10)]
    public void Out_of_range_paging_falls_back_to_defaults(int page, int pageSize, int expectedPage, int expectedPageSize)
    {
        var query = new StudentQuery { Page = page, PageSize = pageSize };

        query.Page.ShouldBe(expectedPage);
        query.PageSize.ShouldBe(expectedPageSize);
    }

    [Fact]
    public void Skip_is_derived_from_page_and_size()
    {
        new StudentQuery { Page = 3, PageSize = 10 }.Skip.ShouldBe(20);
    }
}
