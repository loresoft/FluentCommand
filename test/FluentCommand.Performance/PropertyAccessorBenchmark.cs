using System;
using System.Reflection;

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;

using FluentCommand.Entities;
using FluentCommand.Reflection;

namespace FluentCommand.Performance;

[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net70)]
[SimpleJob(RuntimeMoniker.Net60)]
[BenchmarkCategory("Accessor")]
public class PropertyAccessorBenchmark
{
    private readonly User _user;

    public PropertyAccessorBenchmark()
    {
        _user = new User
        {
            FirstName = "Test",
            LastName = "User",
            DisplayName = "Test User",
            EmailAddress = "test@email.com"
        };
    }

    private PropertyInfo _propertyInfo;
    private IMemberAccessor _propertyAccessor;
    private Func<object, object> _functionAccessor;

    [GlobalSetup]
    public void Setup()
    {
        var type = _user.GetType();
        _propertyInfo = type.GetProperty(nameof(User.DisplayName));

        var typeAccessor = TypeAccessor.GetAccessor<User>();
        _propertyAccessor = typeAccessor.FindProperty(nameof(User.DisplayName));
        _functionAccessor = (p) => (p as User)?.DisplayName;

    }

    [Benchmark(Description = "ProperyRead", Baseline = true)]
    public string ProperyRead()
    {
        var displayName = _user.DisplayName;

        return displayName;
    }

    [Benchmark(Description = "ProperyReflection")]
    public string ProperyReflection()
    {
        var displayName = _propertyInfo.GetValue(_user) as string;

        return displayName;
    }

    [Benchmark(Description = "ProperyFunction")]
    public string ProperyFunction()
    {
        var displayName = _functionAccessor(_user) as string;
        return displayName;
    }

    [Benchmark(Description = "ProperyAccessor")]
    public string ProperyAccessor()
    {
        var displayName = _propertyAccessor.GetValue(_user) as string;
        return displayName;
    }


}
