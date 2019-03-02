# ImageChart
https://www.nuget.org/packages/ImageChart

ImageChart is a cross-platform C# library allows you to create simple charts as image files.

This library makes use of the ImageSharp drawing library.

![alt text](https://raw.githubusercontent.com/bmdub/ImageChart/master/image/test.png)

### Usage

```CSharp
// Create a bar chart
new ImageChart.BarChartBuilder()
    .SetSize(600, 100)
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
``` 
