using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ImageChart
{
    /// <summary>
    /// Builds a bar chart.
    /// </summary>
    public partial class BarChartBuilder
    {
        int _width = 500;
        int _height = 500;
        string _title = "";
        Rgba32 _textColor = Rgba32.Black;
        Rgba32 _backgroundColor = Rgba32.White;
        Rgba32 _barColor = Rgba32.LimeGreen;
        float? _specifiedMinValue = null;
        float? _specifiedMaxValue = null;
        List<Bar> _bars = new List<Bar>();

        /// <summary>Sets the image size.</summary>
        /// <param name="width">Width in pixels.</param>
        /// <param name="height">Height in pixels.</param>
        public BarChartBuilder SetSize(int width, int height)
        {
            _width = width;
            _height = height;
            return this;
        }

        /// <summary>Sets the title of the chart, appearing at the top.</summary>
        /// <param name="title">The title.</param>
        public BarChartBuilder SetTitle(string title)
        {
            _title = title;
            return this;
        }

        /// <summary>Sets the color of the labels on the chart.</summary>
        /// <param name="color">The color.</param>
        public BarChartBuilder SetTextColor(System.Drawing.Color color)
        {
            _textColor = new Rgba32(color.R, color.G, color.B, color.A);
            return this;
        }

        /// <summary>Sets the color of the chart background.</summary>
        /// <param name="color">The color.</param>
        public BarChartBuilder SetBackgroundColor(System.Drawing.Color color)
        {
            _backgroundColor = new Rgba32(color.R, color.G, color.B, color.A);
            return this;
        }

        /// <summary>Sets the default color of the bars on the chart.</summary>
        /// <param name="color">The color.</param>
        public BarChartBuilder SetBarColor(System.Drawing.Color color)
        {
            _barColor = new Rgba32(color.R, color.G, color.B, color.A);
            return this;
        }

        /// <summary>Sets the lower bound of the chart.</summary>
        /// <param name="value">The value.</param>
        public BarChartBuilder SetMin(float value)
        {
            _specifiedMinValue = value;
            return this;
        }

        /// <summary>Sets the upper bound of the chart.</summary>
        /// <param name="value">The value.</param>
        public BarChartBuilder SetMax(float value)
        {
            _specifiedMaxValue = value;
            return this;
        }

        /// <summary>Sets the items to display on the chart.</summary>
        /// <param name="bars">The list of bars.</param>
        public BarChartBuilder SetBars(IEnumerable<Bar> bars)
        {
            _bars = bars.ToList();
            return this;
        }

        /// <summary>Adds an item to display on the chart.</summary>
        /// <param name="bar">A bar.</param>
        public BarChartBuilder AddBar(Bar bar)
        {
            _bars.Add(bar);
            return this;
        }

        /// <summary>
        /// Builds the specified chart and writes it to a file in the implied format.
        /// Supported formats: BMP, JPEG, GIF, PNG
        /// </summary>
        /// <param name="outputPath">The output path including file name.</param>
        public void Build(string outputPath)
        {
            // class members
            var opCenterHCenterV = new TextGraphicsOptions(true) { HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
            var opLeftHCenterV = new TextGraphicsOptions(true) { HorizontalAlignment = HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Center };
            var opRightHCenterV = new TextGraphicsOptions(true) { HorizontalAlignment = HorizontalAlignment.Right, VerticalAlignment = VerticalAlignment.Center };

            // Bounds check, if value boundaries specified
            if (_specifiedMinValue.HasValue)
                if (_bars.Where(bar => bar.Value < _specifiedMinValue.Value).Any())
                    throw new ArgumentOutOfRangeException($"A bar value has exceeded the minimum specified threshold.");

            if (_specifiedMaxValue.HasValue)
                if (_bars.Where(bar => bar.Value > _specifiedMaxValue.Value).Any())
                    throw new ArgumentOutOfRangeException($"A bar value has exceeded the maximum specified threshold.");

            // Calculate the size of the chart components based on the image dimensions
            int rowCount = _bars.Count + 1; // Count of rows: Title row and bar rows
            var rowHeight = (float)_height / rowCount; // The size of each row

            // These are adjustable
            var rowContentHeight = rowHeight * .8f;
            var barHeight = rowContentHeight * 1f;
            var titleFontSize = rowContentHeight * 1f;
            var labelFontSize = rowContentHeight * .8f;

            Font titleFont = SystemFonts.CreateFont("Arial", titleFontSize);
            Font labelFont = SystemFonts.CreateFont("Arial", labelFontSize);

            // Calculate widths
            var barNameWidthMax = _bars.Max(bar => TextMeasurer.Measure(bar.Name, new RendererOptions(labelFont)).Width);
            var barValueWidthMax = _bars.Max(bar => TextMeasurer.Measure(bar.Value.ToString(), new RendererOptions(labelFont)).Width);
            var cellOffset = (rowHeight - rowContentHeight) * .5f; // The offset of content from a cell edge
            var barNamePaddedWidthMax = barNameWidthMax + cellOffset * 2;
            var barValuePaddedWidthMax = barValueWidthMax + cellOffset * 2;
            var barGraphicPaddedWidthMax = _width - (barNamePaddedWidthMax + barValuePaddedWidthMax);
            var barGraphicWidthMax = barGraphicPaddedWidthMax - cellOffset * 2;

            // Determine the value boundaries for the graph
            float maxValue;
            if (_specifiedMaxValue.HasValue) maxValue = _specifiedMaxValue.Value;
            else
            {
                maxValue = _bars.Max(bar => bar.Value);
                if (maxValue < 0) maxValue = 0;
            }
            float minValue;
            if (_specifiedMinValue.HasValue) minValue = _specifiedMinValue.Value;
            else
            {
                minValue = _bars.Min(bar => bar.Value);
                if (minValue > 0) minValue = 0;
            }

            // Calculate the factor used to convert a bar value to pixel value
            var valueRange = maxValue - minValue;
            var widthFactor = barGraphicWidthMax / valueRange;

            // Get the value of the y axis for bar value offsetting later
            float yAxisValue = 0;
            if (minValue > 0) yAxisValue = minValue;
            else if (maxValue < 0) yAxisValue = maxValue;

            // Get the pixel offset of the Y (x=0) axis relative to the bar graphic cell content	
            float yAxisOffset = 0;
            if (minValue < 0) yAxisOffset = (minValue - yAxisValue) * -1 * widthFactor; // Move it over to make room for negative bars that go left

            // Draw the image
            using (Image<Rgba32> image = new Image<Rgba32>(_width, _height))
            {
                // Draw the background
                image.Mutate(ctx => ctx.Fill(_backgroundColor));

                // Put vertical drawing cursor at padding offset
                float curYOffset = cellOffset;

                // Draw the title
                DrawText(image, opCenterHCenterV, _title, titleFont, _textColor, new PointF(image.Width * .5f, curYOffset + rowContentHeight * .5f));
                curYOffset += rowHeight;

                // Draw the bars
                foreach (var bar in _bars)
                {
                    float curXOffset = cellOffset;

                    // Draw the bar name
                    DrawText(image, opLeftHCenterV, bar.Name, labelFont, _textColor, new PointF(curXOffset, curYOffset + rowContentHeight * .5f));
                    curXOffset += barNamePaddedWidthMax;

                    curXOffset += yAxisOffset;
                    var barWidth = (bar.Value - yAxisValue) * widthFactor;

                    if (barWidth < 0)
                    {
                        // Handle negative value, by making the width positive and moving left of the y axis
                        barWidth *= -1;
                        curXOffset -= barWidth;
                    }

                    // Draw the bar graphic
                    var barColor = _barColor; // Default to the text color
                    if (bar.Color.A != 0) barColor = new Rgba32(bar.Color.R, bar.Color.G, bar.Color.B, bar.Color.A);
                    Draw3dBar(image, barColor, barWidth, barHeight, curXOffset, curYOffset);
                    curXOffset += barWidth + cellOffset * 2;

                    // Draw the bar value text
                    DrawText(image, opLeftHCenterV, bar.Value.ToString(), labelFont, _textColor, new PointF(curXOffset, curYOffset + rowContentHeight * .5f));

                    curYOffset += rowHeight;
                }

                image.Save(outputPath);
            }
        }

        static void DrawText(Image<Rgba32> image, TextGraphicsOptions options, string text, Font font, Rgba32 color, PointF location)
        {
            try
            {
                image.Mutate(ctx => ctx.DrawText(options, text, font, color, location));
            }
            catch(ImageProcessingException)
            {
                // The text went out side the bounds of the image; default to just not drawing it.
            }
        }

        static void Draw3dBar(Image<Rgba32> image, Rgba32 color, float barWidth, float barHeight, float x, float y)
        {
            float rStep = color.R / barHeight; // Step size from red component value to 0 for each pixel height of the bar.
            float gStep = color.G / barHeight; // Step size from green component value to 0 for each pixel height of the bar.
            float bStep = color.B / barHeight; // Step size from blue component value to 0 for each pixel height of the bar.

            // We dont want to step all the way to 0, so step to maybe half-way
            rStep *= .5f;
            gStep *= .5f;
            bStep *= .5f;

            // Convert the components from byte range (0-255) to float range (0-1)
            rStep /= 255f;
            gStep /= 255f;
            bStep /= 255f;

            // Draw the bar one pixel row at a time, fading at each step
            for (int i = 0; i < barHeight; i++)
            {
                image.Mutate(ctx => ctx.FillPolygon(
                    color,
                    new PointF(x, y + i), new PointF(x, y + i + 1), new PointF(x + barWidth, y + i + 1), new PointF(x + barWidth, y + i)));

                color = new Rgba32(color.ToVector4() - new System.Numerics.Vector4(rStep, gStep, bStep, 0));
            }
        }
    }
}
