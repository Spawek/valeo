using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using SteeringCarFromAbove;
using AForge.Video.DirectShow;
using AForge.Video;
using System.Runtime.InteropServices;

namespace SteeringCarFromAboveWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        const int POSITION_STEP = 30;
        TrackPlanner trackPlanner = null;
        GlyphRecognitionStudio.MainForm glyphRecogniser;
        IVideoSource videoSource = null;
        bool waitingForNextBaseImage = false;
        System.Drawing.Bitmap baseImage = null;
        Map map = null;
        MapBuilder mapBuilder = null;
        Image plannerBackGround = new Image();
        List<PositionAndOrientation> calculateTrack = null;
        CarDriver carDriver = null;
        bool carUnderDriving = false;

        CarController.DefaultCarController carController;
        CarController.MainWindow carControllerWindow;
        
        public MainWindow()
        {
            glyphRecogniser = new GlyphRecognitionStudio.MainForm();
            glyphRecogniser.frameProcessed += glyphRecogniser_frameProcessed;
            glyphRecogniser.Show();

            carController = new CarController.DefaultCarController();
            carControllerWindow = new CarController.MainWindow(carController);
            carControllerWindow.Show();

            InitializeComponent();

            Canvas_trackPlanner.Children.Add(plannerBackGround); //REMOVE IT FROM HERE

            MarkerFinder markerFinder = new MarkerFinder();
            ObstaclesFinder obstaclesFinder = new ObstaclesFinder();
            ObjectsToTrace objectsToTrace = new ObjectsToTrace(new List<string>() { "s1", "s2" }, "car", "parking");

            mapBuilder = new MapBuilder(markerFinder, obstaclesFinder, objectsToTrace);
        }

        // http://stackoverflow.com/questions/1118496/using-image-control-in-wpf-to-display-system-drawing-bitmap
        [DllImport("gdi32")]
        static extern int DeleteObject(IntPtr o);

        public static BitmapSource loadBitmap(System.Drawing.Bitmap source)
        {
            IntPtr ip = source.GetHbitmap();
            BitmapSource bs = null;
            try
            {
                bs = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(ip,
                   IntPtr.Zero, Int32Rect.Empty,
                   System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());
            }
            finally
            {
                DeleteObject(ip);
            }

            return bs;
        }

        void glyphRecogniser_frameProcessed(object sender, GlyphRecognitionStudio.MainForm.FrameData frameData)
        {
            if (waitingForNextBaseImage)
            {
                baseImage = frameData.getImage();
                this.Dispatcher.Invoke(new Action(() => image_baseImagePicker.Source = loadBitmap(baseImage)));

                map = mapBuilder.BuildMap(baseImage, frameData.getGlyphs());

                if (map != null)
                {
                    this.Dispatcher.Invoke(new Action(() => TextBlock_marksInfo.Text = map.ToString()));
                    this.Dispatcher.Invoke(new Action(() => DrawMap(map)));
                    this.Dispatcher.Invoke(new Action(() => plannerBackGround.Source = loadBitmap(baseImage)));
                    this.Dispatcher.Invoke(new Action(() => Canvas_trackPlanner.UpdateLayout()));
                }
                else
                {
                    this.Dispatcher.Invoke(new Action(() => TextBlock_marksInfo.Text = "Map couldn't be build basing on current image!"));
                }

                waitingForNextBaseImage = false;
                Console.WriteLine("New base frame acquired!");
            }
            else
            {
                if (map != null)
                {
                    mapBuilder.UpdateCarPosition(map, frameData.getGlyphs());
                    this.Dispatcher.Invoke(new Action(() => DrawMap(map)));
                    Console.WriteLine("Car position updated!");

                    if (carUnderDriving)
                    {
                        CarSteering carSteering = carDriver.CalculateCarSteering(map);
                        this.Dispatcher.Invoke(new Action(() => TextBlock_CarSteeringInformations.Text = carSteering.ToString()));

                        carController.SetTargetSpeed(carSteering.speed);
                        carController.SetTargetWheelAngle(carSteering.angle);
                        carController.OverrideTargetBrakeSetting(carSteering.brake);
                    }
                }
            }
            Console.WriteLine("Frame processed");
        }

        List<Line> lastTrack = new List<Line>();
        private void DrawTrack(List<PositionAndOrientation> track)
        {
            foreach (var item in lastTrack)
	        {
                Canvas_trackPlanner.Children.Remove(item);
	        }
            
            lastTrack.Clear();
            foreach (PositionAndOrientation item in track)
            {
                Line l = new Line();

                const double LENGTH = POSITION_STEP;
                l.Stroke = Brushes.OrangeRed;
                l.StrokeThickness = 1;
                l.X1 = item.x;
                l.X2 = item.x - Math.Cos(item.angle / 180.0d * Math.PI) * LENGTH;
                l.Y1 = item.y;
                l.Y2 = item.y - Math.Sin(item.angle / 180.0d * Math.PI) * LENGTH;

                Canvas_trackPlanner.Children.Add(l);

                lastTrack.Add(l);
            }
        }

        private void DrawMap(Map map)
        {
            DrawBorder(map);
            DrawCar(map);
            DrawParking(map);
            DrawObstacles(map);
            Canvas_trackPlanner.UpdateLayout();
        }

        private List<System.Windows.Shapes.Rectangle> lastObstacles = new List<Rectangle>();
        private void DrawObstacles(Map map)
        {
            foreach (var obstacle in lastObstacles)
            {
                Canvas_trackPlanner.Children.Remove(obstacle);
            }
            lastObstacles.Clear();

            foreach (var obstacle in map.obstacles)
            {
                System.Windows.Shapes.Rectangle rect = new System.Windows.Shapes.Rectangle();
                rect.Stroke = new SolidColorBrush(Colors.Red);
                rect.Width = obstacle.Width;
                rect.Height = obstacle.Height;
                Canvas.SetLeft(rect, obstacle.X);
                Canvas.SetTop(rect, obstacle.Y);
                rect.StrokeThickness = 3;

                Canvas_trackPlanner.Children.Add(rect);
                lastObstacles.Add(rect);
            }
        }

        private void DrawParking(Map map)
        {
            if (map.parking != null)
            {
                const double parkingSizeX = 38;
                const double parkingSizeY = 70;

                System.Windows.Shapes.Rectangle parking = new System.Windows.Shapes.Rectangle();
                parking.Stroke = new SolidColorBrush(Colors.Red);
                parking.Width = parkingSizeX;
                parking.Height = parkingSizeY;
                Canvas.SetLeft(parking, map.parking.x - parkingSizeX / 2);
                Canvas.SetTop(parking, map.parking.y - parkingSizeY / 2);
                parking.StrokeThickness = 7;

                Canvas_trackPlanner.Children.Add(parking);
            }
        }

        private System.Windows.Shapes.Polyline lastCar = null;
        private void DrawCar(Map map)
        {
            if (lastCar != null)
                Canvas_trackPlanner.Children.Remove(lastCar);

            const double CAR_WIDTH = 55;
            const double CAR_HEIGHT = 25;
            const double FRONT_SHOWER_LENTH = 10;

            System.Windows.Shapes.Polyline car = new Polyline();
            car.StrokeThickness = 7;
            car.Stroke = new SolidColorBrush(Colors.Red);
            car.Points = new PointCollection() { 
                new Point(map.car.x + CAR_WIDTH / 2, map.car.y + CAR_HEIGHT / 2),
                new Point(map.car.x - CAR_WIDTH / 2, map.car.y + CAR_HEIGHT / 2),
                new Point(map.car.x - CAR_WIDTH / 2, map.car.y - CAR_HEIGHT / 2),
                new Point(map.car.x + CAR_WIDTH / 2, map.car.y - CAR_HEIGHT / 2),
                new Point(map.car.x + CAR_WIDTH / 2, map.car.y),
                new Point(map.car.x + CAR_WIDTH + FRONT_SHOWER_LENTH / 2, map.car.y),
                new Point(map.car.x + CAR_WIDTH / 2, map.car.y),
                new Point(map.car.x + CAR_WIDTH / 2, map.car.y + CAR_HEIGHT / 2)};

            RotateTransform rt = new RotateTransform(map.car.angle, map.car.x, map.car.y);

            car.Points = new PointCollection(car.Points.Select(x => rt.Transform(x)));
            Canvas_trackPlanner.Children.Add(car);

            lastCar = car;
        }

        private System.Windows.Shapes.Rectangle lastBorder = null;
        private void DrawBorder(Map map)
        {
            if (lastBorder != null)
                Canvas_trackPlanner.Children.Remove(lastBorder);

            System.Windows.Shapes.Rectangle border = new System.Windows.Shapes.Rectangle();
            border.Stroke = new SolidColorBrush(Colors.Black);
            border.Width = map.mapWidth;
            border.Height = map.mapHeight;
            Canvas.SetLeft(border, 0);
            Canvas.SetTop(border, 0);
            border.StrokeThickness = 5;

            Canvas_trackPlanner.Children.Add(border);
            lastBorder = border;
        }

        //http://stackoverflow.com/questions/1335426/is-there-a-built-in-c-net-system-api-for-hsv-to-rgb
        public static Color ColorFromHSV(double hue, double saturation, double value)
        {
            int hi = Convert.ToInt32(Math.Floor(hue / 60)) % 6;
            double f = hue / 60 - Math.Floor(hue / 60);

            value = value * 255;
            byte v = Convert.ToByte(value);
            byte p = Convert.ToByte(value * (1 - saturation));
            byte q = Convert.ToByte(value * (1 - f * saturation));
            byte t = Convert.ToByte(value * (1 - (1 - f) * saturation));

            if (hi == 0)
                return Color.FromArgb(255, v, t, p);
            else if (hi == 1)
                return Color.FromArgb(255, q, v, p);
            else if (hi == 2)
                return Color.FromArgb(255, p, v, t);
            else if (hi == 3)
                return Color.FromArgb(255, p, q, v);
            else if (hi == 4)
                return Color.FromArgb(255, t, p, v);
            else
                return Color.FromArgb(255, v, p, q);
        }

        private List<Line> lastSeenNodesLines = new List<Line>();
        void DrawTrackPlannerSeenNodes(List<TrackPlanner.BFSNode> nodes)
        {
            foreach (Line l in lastSeenNodesLines)
            {
                Canvas_trackPlanner.Children.Remove(l);
            }
            lastSeenNodesLines.Clear();

            int counter = 0;
            foreach (TrackPlanner.BFSNode node in nodes)
            {
                if (counter++ % 5 == 0)
                {
                    int predecessorsCount = 0;
                    SteeringCarFromAbove.TrackPlanner.BFSNode curr = node;
                    while ((curr = curr.predecessor) != null) predecessorsCount++;

                    Line l = new Line();

                    const double LENGTH = POSITION_STEP;
                    l.Stroke = new SolidColorBrush(ColorFromHSV((25.0d * predecessorsCount) % 360.0d, 0.3d, 0.8d));
                    l.StrokeThickness = 1;
                    l.X1 = node.position.x;
                    l.X2 = node.position.x - Math.Cos(node.position.angle / 180.0d * Math.PI) * LENGTH;
                    l.Y1 = node.position.y;
                    l.Y2 = node.position.y - Math.Sin(node.position.angle / 180.0d * Math.PI) * LENGTH;
                    l.Opacity = 0.8;

                    Canvas_trackPlanner.Children.Add(l);
                    lastSeenNodesLines.Add(l);
                }
            }
        }

        private Point lastMouseDown_;
        private void Canvas_trackPlanner_MouseDown(object sender, MouseButtonEventArgs e)
        {
            lastMouseDown_ = e.GetPosition(Canvas_trackPlanner);
        }

        private void Canvas_trackPlanner_MouseUp(object sender, MouseButtonEventArgs e)
        {
            switch (trackPlannerMode)
            {
                case TrackPlannerMode.SETTING_PARKING_PLACE:
                    {
                        Point mouseUp = e.GetPosition(Canvas_trackPlanner);

                        double deltaY = mouseUp.Y - lastMouseDown_.Y;
                        double deltaX = mouseUp.X - lastMouseDown_.X;

                        double angleInDegrees = Math.Atan2(deltaY, deltaX) * 180 / Math.PI;

                        calculateTrack = trackPlanner.GetTrackFromPreparedPlanner(
                            new PositionAndOrientation(lastMouseDown_.X, lastMouseDown_.Y, angleInDegrees));

                        DrawTrack(calculateTrack);
                    }
                    break;
                case TrackPlannerMode.ADDING_OBSTACLES:
                    {
                        Point mouseUp = e.GetPosition(Canvas_trackPlanner);

                        int deltaY = (int)Math.Abs(mouseUp.Y - lastMouseDown_.Y);
                        int deltaX = (int)Math.Abs(mouseUp.X - lastMouseDown_.X);

                        int minY = (int)Math.Min(mouseUp.Y, lastMouseDown_.Y);
                        int minX = (int)Math.Min(mouseUp.X, lastMouseDown_.X);

                        map.obstacles.Add(new System.Drawing.Rectangle(minX, minY, deltaX, deltaY));

                        DrawMap(map);
                    }
                    break;
                case TrackPlannerMode.REMOVING_OBSTACLES:
                    {
                        Point mouseUp = e.GetPosition(Canvas_trackPlanner);

                        map.obstacles.RemoveAll(x => x.Contains((int)mouseUp.X, (int)mouseUp.Y));
                    }
                    break;
                case TrackPlannerMode.NONE:
                    break;
                default:
                    break;
            }

        }

        private void button_ChangeVideoSource_Click(object sender, RoutedEventArgs e)
        {
            VideoCaptureDeviceForm form = new VideoCaptureDeviceForm();
            if (form.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                videoSource = form.VideoDevice;
                glyphRecogniser.InjectVideoSource(videoSource);
            }
            else
            {
                Console.WriteLine("Couldn't open video source");
            }
        }


        System.Threading.Mutex m = new System.Threading.Mutex();
        private void Button_FileVideoSource_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new System.Windows.Forms.OpenFileDialog();
            System.Windows.Forms.DialogResult result = dialog.ShowDialog();

            if (result == System.Windows.Forms.DialogResult.OK)
            {
                videoSource = new FileVideoSource(dialog.InitialDirectory + dialog.FileName);
                videoSource.NewFrame += videoSource_Lock;
                glyphRecogniser.InjectVideoSource(videoSource, false);
            }
            else
            {
                Console.WriteLine("Couldn't open video source");
            }
        }


        int c = 0;
        void videoSource_Lock(object sender, NewFrameEventArgs eventArgs)
        {
            Console.WriteLine(String.Format("F: {0}", c++));
        }

        void async_VideoSourceError(object sender, VideoSourceErrorEventArgs eventArgs)
        {
            System.Windows.Forms.MessageBox.Show(eventArgs.Description);
        }

        void async_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            System.Windows.Forms.MessageBox.Show("FRAME!");
        }

        private void button_GetNextImage_Click(object sender, RoutedEventArgs e)
        {
            waitingForNextBaseImage = true;
        }

        private bool trackPrepared = false;
        private void button_PrepareTrack_Click(object sender, RoutedEventArgs e)
        {
            if (map != null)
            {
                trackPlanner = new TrackPlanner(
                    locationTolerance: POSITION_STEP - 1, angleTolerance: 9.0d,
                    positionStep: (int)POSITION_STEP, angleStep: 10.0d,
                    mapSizeX: map.mapWidth, mapSizeY: map.mapHeight);

                trackPlanner.PrepareTracks(map);
                DrawTrackPlannerSeenNodes(trackPlanner.GetSeen());
                trackPrepared = true;
            }
            else
            {
                System.Windows.Forms.MessageBox.Show("Get base map first!");
            }
        }

        private enum TrackPlannerMode { SETTING_PARKING_PLACE, ADDING_OBSTACLES, REMOVING_OBSTACLES, NONE }
        private TrackPlannerMode trackPlannerMode = TrackPlannerMode.NONE;

        private void button_AddObstacle_Click(object sender, RoutedEventArgs e)
        {
            if (map != null)
            {
                trackPlannerMode = TrackPlannerMode.ADDING_OBSTACLES;
                button_AddObstacle.IsEnabled = false;
                button_RemoveObstacle.IsEnabled = true;
                button_SetParkingPlace.IsEnabled = true;
            }
            else
            {
                System.Windows.Forms.MessageBox.Show("Prepare track first!");
            }
        }

        private void button_RemoveObstacle_Click(object sender, RoutedEventArgs e)
        {
            if (map != null)
            {
                trackPlannerMode = TrackPlannerMode.REMOVING_OBSTACLES;
                button_AddObstacle.IsEnabled = true;
                button_RemoveObstacle.IsEnabled = false;
                button_SetParkingPlace.IsEnabled = true;
            }
            else
            {
                System.Windows.Forms.MessageBox.Show("Prepare track first!");
            }
        }

        private void button_SetParkingPlace_Click(object sender, RoutedEventArgs e)
        {
            if (trackPrepared)
            {
                trackPlannerMode = TrackPlannerMode.SETTING_PARKING_PLACE;
                button_AddObstacle.IsEnabled = true;
                button_RemoveObstacle.IsEnabled = true;
                button_SetParkingPlace.IsEnabled = false;
            }
            else
            {
                System.Windows.Forms.MessageBox.Show("Prepare track first!");
            }

        }

        private void Button_ParkACar_Click(object sender, RoutedEventArgs e)
        {
            if (calculateTrack != null)
            {
                carDriver = new CarDriver(map, calculateTrack);
                carUnderDriving = true;
            }
            else
            {
                System.Windows.Forms.MessageBox.Show("Choose your track first!");
            }
        }

        private void Button_ResumeFeeding_Click(object sender, RoutedEventArgs e)
        {
            if (videoSource != null)
            {
                videoSource.Start();
            }
        }

        private void Button_PauseFeeding_Click(object sender, RoutedEventArgs e)
        {
            if (videoSource != null)
            {
                videoSource.Stop();
            }
        }

    }
}
