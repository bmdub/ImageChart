using System.Diagnostics;
using System.Drawing;

namespace sandbox
{
    class Program
    {
        static void Main(string[] args)
        {
            // Create a bar chart
            new ImageChart.BarChartBuilder()
                .SetSize(300, 100)
                .SetTextColor(Color.White)
                .SetBackgroundColor(Color.Black)
                .SetBarColor(Color.LimeGreen)
                .SetTitle("Election Results")
                .AddBar(new ImageChart.Bar() {  Name = "Cthulu", Value = 512, Color = Color.Gold })
                .AddBar(new ImageChart.Bar() { Name = "Bob", Value = 112 })
                .AddBar(new ImageChart.Bar() { Name = "Hitler", Value = -22 })
                .Build("test.png");

            // Open the image file
            var startInfo = new ProcessStartInfo("test.png") { UseShellExecute = true };
            Process.Start(startInfo);
        }
    }
}
