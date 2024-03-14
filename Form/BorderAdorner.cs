using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows;
using System.Windows.Documents;

namespace Form;

public class BorderAdorner : Adorner
{
    // Be sure to call the base class constructor.
    public BorderAdorner(UIElement adornedElement) : base(adornedElement)
    {
    }

    protected override void OnRender(DrawingContext drawingContext)
    {
        var adornedElementRect = new Rect(this.AdornedElement.DesiredSize);
        var pen = new Pen(Brushes.DarkBlue, 1.5);

        // Inflate the rectangle to create space for the ellipse
        adornedElementRect.Inflate(5, 5);

        // Calculate the center of the ellipse
        var centerX = adornedElementRect.Left + adornedElementRect.Width / 2;
        var centerY = adornedElementRect.Top + adornedElementRect.Height / 2;

        // Calculate the radius of the ellipse
        var radiusX = adornedElementRect.Width / 2;
        var radiusY = adornedElementRect.Height / 2;

        // Draw the ellipse
        drawingContext.DrawEllipse(null, pen, new Point(centerX, centerY), radiusX, radiusY);
    }

}
