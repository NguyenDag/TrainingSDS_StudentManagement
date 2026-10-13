using StudentManagement.Domain.Common;

namespace StudentManagement.Domain.Tests;

public class EntityTests
{
    private sealed class Foo : Entity
    {
        public Foo(int id = 0) => Id = id;
    }

    private sealed class Bar : Entity
    {
        public Bar(int id) => Id = id;
    }

    [Fact]
    public void Unsaved_entities_are_equal_only_to_themselves()
    {
        var foo = new Foo();
        foo.ShouldBe(foo);
        foo.ShouldNotBe(new Foo());
    }

    [Fact]
    public void Saved_entities_with_same_type_and_id_are_equal()
    {
        new Foo(7).ShouldBe(new Foo(7));
        new Foo(7).GetHashCode().ShouldBe(new Foo(7).GetHashCode());
        new Foo(7).ShouldNotBe(new Foo(8));
    }

    [Fact]
    public void Same_id_of_different_types_is_not_equal()
    {
        new Foo(7).Equals(new Bar(7)).ShouldBeFalse();
    }
}
