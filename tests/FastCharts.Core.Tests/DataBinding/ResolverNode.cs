namespace FastCharts.Core.Tests.DataBinding
{
    /// <summary>
    /// Test model: node with nested leaf and a name
    /// </summary>
    public class ResolverNode
    {
        public ResolverLeaf? Child { get; set; }

        public string? Name { get; set; }
    }
}
