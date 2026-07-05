using FastCharts.Core.DataBinding;
using FluentAssertions;
using Xunit;

namespace FastCharts.Core.Tests.DataBinding
{
    /// <summary>
    /// Tests for the compiled-delegate property path resolver
    /// </summary>
    public class CachedPropertyPathResolverTests
    {
        [Fact]
        public void GetValue_SimplePath_ReturnsValue()
        {
            var resolver = CachedPropertyPathResolver.Instance;
            var node = new ResolverNode { Name = "abc" };

            resolver.GetValue(node, "Name").Should().Be("abc");
        }

        [Fact]
        public void GetValue_NestedPath_ReturnsValue()
        {
            var resolver = CachedPropertyPathResolver.Instance;
            var node = new ResolverNode { Child = new ResolverLeaf { Value = 3.5 } };

            resolver.GetValue(node, "Child.Value").Should().Be(3.5);
        }

        [Fact]
        public void GetValue_NullIntermediate_ReturnsNull()
        {
            var resolver = CachedPropertyPathResolver.Instance;
            var node = new ResolverNode { Child = null };

            resolver.GetValue(node, "Child.Value").Should().BeNull();
        }

        [Fact]
        public void GetValue_UnknownPath_ReturnsNull()
        {
            var resolver = CachedPropertyPathResolver.Instance;

            resolver.GetValue(new ResolverNode(), "DoesNotExist").Should().BeNull();
            resolver.GetValue(new ResolverNode(), "Name.Missing.Deep").Should().BeNull();
        }

        [Fact]
        public void GetValue_NullSourceOrPath_ReturnsNull()
        {
            var resolver = CachedPropertyPathResolver.Instance;

            resolver.GetValue(null, "Name").Should().BeNull();
            resolver.GetValue(new ResolverNode(), null).Should().BeNull();
            resolver.GetValue(new ResolverNode(), string.Empty).Should().BeNull();
        }

        [Fact]
        public void GetValue_RepeatedCalls_UseCachedDelegate()
        {
            var resolver = CachedPropertyPathResolver.Instance;
            var node = new ResolverNode { Child = new ResolverLeaf { Value = 1 } };

            for (var i = 0; i < 1000; i++)
            {
                resolver.GetValue(node, "Child.Value").Should().Be(1.0);
            }
        }

        [Fact]
        public void GetValue_DerivedInstances_ResolveBaseProperties()
        {
            var resolver = CachedPropertyPathResolver.Instance;
            var derived = new ResolverDerivedNode { Name = "derived", Child = new ResolverLeaf { Value = 7 } };

            resolver.GetValue(derived, "Name").Should().Be("derived");
            resolver.GetValue(derived, "Child.Value").Should().Be(7.0);
            resolver.GetValue(derived, "Extra").Should().Be(0);
        }

        [Fact]
        public void IsValidPath_And_PropertyType_DelegateToReflection()
        {
            var resolver = CachedPropertyPathResolver.Instance;

            resolver.IsValidPath(typeof(ResolverNode), "Child.Value").Should().BeTrue();
            resolver.IsValidPath(typeof(ResolverNode), "Nope").Should().BeFalse();
            resolver.GetPropertyType(typeof(ResolverNode), "Child.Value").Should().Be(typeof(double));
        }

        [Fact]
        public void SetValue_WritesProperty()
        {
            var resolver = CachedPropertyPathResolver.Instance;
            var node = new ResolverNode { Child = new ResolverLeaf() };

            resolver.SetValue(node, "Child.Value", 12.0);

            node.Child!.Value.Should().Be(12.0);
        }
    }
}
