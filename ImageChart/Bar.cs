namespace ImageChart
{
    /// <summary>
    /// A measurable value represented as a bar.
    /// </summary>
    public class Bar
    {
        /// <summary>The name for this item.</summary>
        public string Name { get; set; } = "";

        /// <summary>The value to represent as a bar.</summary>
        public float Value { get; set; }

        /// <summary>An optional color.</summary>
        public System.Drawing.Color Color { get; set; } = System.Drawing.Color.FromArgb(0, 0, 0, 0);
    }
}
