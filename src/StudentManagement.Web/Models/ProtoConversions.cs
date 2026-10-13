using AntDesign.TableModels;
using AntSortDirection = AntDesign.SortDirection;
using Proto = StudentManagement.Contracts.V1;

namespace StudentManagement.Web.Models;

public static class ProtoConversions
{
    public static DateTime ToDateTime(this Proto.Date date) => new(date.Year, date.Month, date.Day);

    public static Proto.Date ToProto(this DateTime date) => new() { Year = date.Year, Month = date.Month, Day = date.Day };

    public static string Display(this Proto.Date? date) => date is null ? "" : $"{date.Day:00}/{date.Month:00}/{date.Year}";

    public static int AgeToday(this Proto.Date date)
    {
        var today = DateTime.Today;
        var age = today.Year - date.Year;
        return date.ToDateTime().AddYears(age) > today ? age - 1 : age;
    }

    /// <summary>The column the user sorted by (property name) and its direction; null when unsorted.</summary>
    public static (string Field, Proto.SortDirection Direction)? ActiveSort<T>(this QueryModel<T> query)
    {
        var sort = query.SortModel.FirstOrDefault(s => s.SortDirection != AntSortDirection.None);
        return sort is null
            ? null
            : (sort.FieldName, sort.SortDirection == AntSortDirection.Descending ? Proto.SortDirection.Desc : Proto.SortDirection.Asc);
    }
}
