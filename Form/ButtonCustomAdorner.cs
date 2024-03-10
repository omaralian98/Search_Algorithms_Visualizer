using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows;

namespace Form;

public class ButtonCustomAdorner : Adorner
{
    private Rect[] rectangles;
    private Button adornedButton;
    private Point lastMousePosition;

    //   Top-Left      //  Top-Right     //   Bottom-Left  //  Bottom-Right
    private readonly Cursor[] cornerCursors = [Cursors.SizeNWSE, Cursors.SizeNESW, Cursors.SizeNESW, Cursors.SizeNWSE];


    public ButtonCustomAdorner(UIElement adornedElement) : base(adornedElement)
    {
        if (adornedElement is not Button button)
        {
            throw new ArgumentException($"{nameof(ButtonCustomAdorner)} is meant to be used for buttons only");
        }
        IsHitTestVisible = true;
        MouseDown += ButtonCustomAdorner_MouseDown;
        MouseMove += ButtonCustomAdorner_MouseMove;
        MouseUp += ButtonCustomAdorner_MouseUp;


        adornedButton = button;
    }

    // Renders the Adorners
    protected override void OnRender(DrawingContext drawingContext)
    {
        Rect adornedElementRect = new(AdornedElement.DesiredSize);

        SolidColorBrush renderBrush = new(Colors.Green)
        {
            Opacity = 0.2
        };
        Pen renderPen = new(new SolidColorBrush(Colors.Navy), 1.5);
        double renderSize = 8;

        var topLeft = new Rect(adornedElementRect.TopLeft, new Size(renderSize, renderSize));
        var topRight = new Rect(adornedElementRect.TopRight.X - renderSize, adornedElementRect.TopRight.Y, renderSize, renderSize);
        var bottomLeft = new Rect(adornedElementRect.BottomLeft.X, adornedElementRect.BottomLeft.Y - renderSize, renderSize, renderSize);
        var bottomRight = new Rect(adornedElementRect.BottomRight.X - renderSize, adornedElementRect.BottomRight.Y - renderSize, renderSize, renderSize);

        drawingContext.DrawRectangle(renderBrush, renderPen, topLeft);
        drawingContext.DrawRectangle(renderBrush, renderPen, topRight);
        drawingContext.DrawRectangle(renderBrush, renderPen, bottomLeft);
        drawingContext.DrawRectangle(renderBrush, renderPen, bottomRight);

        // Saves the Adorners in an array
        rectangles = [topLeft, topRight, bottomLeft, bottomRight];
    }

    // When an Adorner is clicked
    private void ButtonCustomAdorner_MouseDown(object sender, MouseButtonEventArgs e)
    {
        //Get the point of the click
        Point mousePosition = e.GetPosition(this);

        //Get the index of the clicked adorner
        var indexOfClickedRect = IndexHitTestRect(mousePosition);
        if (indexOfClickedRect != -1)
        {
            lastMousePosition = mousePosition;
            CaptureMouse();
        }
    }

    // When an Adorner is Moving 
    private void ButtonCustomAdorner_MouseMove(object sender, MouseEventArgs e)
    {
        //Check if the leftbutton is pressed
        if (e.LeftButton == MouseButtonState.Pressed)
        {
            //Get the position of the mouse
            Point mousePosition = e.GetPosition(this);
            //Calculate the offset
            Vector offset = mousePosition - lastMousePosition;

            //Resize the Button
            ResizeButton(offset);
            //Update the last known position
            lastMousePosition = mousePosition;
        }
    }

    private void ButtonCustomAdorner_MouseUp(object sender, MouseButtonEventArgs e)
    {
        ReleaseMouseCapture();
    }

    //Gets the index of the Adorner
    private int IndexHitTestRect(Point hitPoint)
    {
        foreach (var rect in rectangles)
        {
            if (rect.Contains(hitPoint))
            {
                return Array.IndexOf(rectangles, rect);
            }
        }
        return -1;
    }

    //Resizes the Button
    private void ResizeButton(Vector offset)
    {
        double newWidth = adornedButton.Width + offset.X;
        double newHeight = adornedButton.Height + offset.Y;
        if (newWidth <= 0 || newHeight <= 0) return;
        adornedButton.Width = newWidth;
        adornedButton.Height = newHeight;
    }

    //When the Mouse Hovers over the Adorner
    protected override void OnMouseEnter(MouseEventArgs e)
    {
        base.OnMouseEnter(e);
        Point mousePosition = e.GetPosition(this);

        int IndexOfClickedRect = IndexHitTestRect(mousePosition);
        if (IndexOfClickedRect != -1)
        {//Change the Cursor
            Cursor = cornerCursors[IndexOfClickedRect];
        }
    }

    //When the mouse no longer is over the Adorner
    protected override void OnMouseLeave(MouseEventArgs e)
    {//Change the Cursor to Arrow
        base.OnMouseLeave(e);
        Cursor = Cursors.Arrow;
    }

}