using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Windows.Input;
using System.Windows.Shapes;
using System.Windows.Media;
using System.Collections.Generic;
using System.ComponentModel;

namespace ThreadingComponent {
    public partial class Spinner {
        #region Data
        private readonly DispatcherTimer animationTimer;
        private List<Shape> Shapes;
        [Description("If true the spinner nodes will be rectangles instead of ellipses."), Category("Appearance")]
        public bool UseRectangles { get; set; } = false;
        [Description("The number of nodes in the spinner."),Category("Appearance")]
        public int NumNodes { get; set; } = 10;
        [Description("The delay in milliseconds between each animation tick."),Category("Appearance")]
        public int Delay { get; set; } = 75;
        [Description("The height of each node in the spinner."), Category("Appearance")]
        public int NodeHeight { get; set; } = 20;
        [Description("The width of each node in the spinner."), Category("Appearance")]
        public int NodeWidth { get; set; } = 20;
        [Description("The color of each node in the spinner."), Category("Appearance")]
        public Brush NodeColor { get; set; } = new SolidColorBrush(Colors.White);
        #endregion

        #region Constructor
        public Spinner() {
            InitializeComponent();
            Shapes = new List<Shape>();
            animationTimer = new DispatcherTimer(DispatcherPriority.ContextIdle, Dispatcher);
        }
        #endregion

        #region Private Methods
        private void Start() {
            AdjustCanvasSize();
            Mouse.OverrideCursor = Cursors.Wait;
            animationTimer.Interval = new TimeSpan(0, 0, 0, 0, Delay);
            animationTimer.Tick += HandleAnimationTick;
            animationTimer.Start();
        }

        private void Stop() {
            animationTimer.Stop();
            Mouse.OverrideCursor = Cursors.Arrow;
            animationTimer.Tick -= HandleAnimationTick;
        }

        // adjusts canvas size to prevent nodes from overflowing
        private void AdjustCanvasSize() {
            canvas.Height = 100 + NodeHeight;
            canvas.Width = 100 + NodeWidth;
        }

        private void HandleAnimationTick(object sender, EventArgs e) {
            double opacityDiff = 1.0 / NumNodes;
            foreach (Shape s in Shapes) {
                if (s.Opacity / opacityDiff >= NumNodes) {
                    s.Opacity = 0.0;
                } else {
                    s.Opacity += opacityDiff;
                }
            }
        }

        private void HandleLoaded(object sender, RoutedEventArgs e) {
            double step = Math.PI * (2.0 / NumNodes);
            for (double i = 0.0; i < NumNodes; i += 1.0) {
                LoadShape(i, step);
            }
        }

        private void LoadShape(double offset, double step) {
            Shape shape = (UseRectangles ? (Shape) new Rectangle() : (Shape) new Ellipse());
            SetShapeProperties(shape, offset);
            SetShapePosition(shape, offset, step);
            Shapes.Add(shape);
            canvas.Children.Add(shape);
        }
        
        private void SetShapeProperties(Shape shape, double offset) {
            shape.Name = "S" + (int) offset;
            shape.Width = NodeWidth;
            shape.Height = NodeHeight;
            shape.Fill = NodeColor;
            shape.Opacity = offset / NumNodes;
        }

        private double RadianToDegree(double angle) {
            return angle * (180.0 / Math.PI);
        }
        
        private void SetShapePosition(Shape shape, double posOffSet, double step) {
            double rotation = posOffSet * step;
            shape.SetValue(Canvas.LeftProperty, 50.0 - Math.Cos(rotation) * 50.0);
            shape.SetValue(Canvas.TopProperty, 50.0 + Math.Sin(rotation) * 50.0);
            shape.RenderTransform = new RotateTransform() {
                Angle = - RadianToDegree(rotation),
                CenterX = NodeWidth / 2,
                CenterY = NodeHeight / 2
            };
        }

        private void HandleUnloaded(object sender, RoutedEventArgs e) {
            Stop();
        }

        private void HandleVisibleChanged(object sender, DependencyPropertyChangedEventArgs e) {
            bool isVisible = (bool)e.NewValue;

            if (isVisible)
                Start();
            else
                Stop();
        }
        #endregion
    }
}