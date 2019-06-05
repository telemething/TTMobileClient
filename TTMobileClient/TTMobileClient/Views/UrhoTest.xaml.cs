using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Globalization;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

using Urho;
using Urho.Forms;
using Urho;
using Urho.Actions;
using Urho.Gui;
using Urho.Shapes;

namespace TTMobileClient.Views
{
    public static class RandomHelper
    {
        static readonly Random random = new Random();

        /// <summary>
        /// Return a random float between 0.0 (inclusive) and 1.0 (exclusive.)
        /// </summary>
        public static float NextRandom() { return (float)random.NextDouble(); }

        /// <summary>
        /// Return a random float between 0.0 and range, inclusive from both ends.
        /// </summary>
        public static float NextRandom(float range) { return (float)random.NextDouble() * range; }

        /// <summary>
        /// Return a random float between min and max, inclusive from both ends.
        /// </summary>
        public static float NextRandom(float min, float max) { return (float)((random.NextDouble() * (max - min)) + min); }

        /// <summary>
        /// Return a random integer between min and max - 1.
        /// </summary>
        public static int NextRandom(int min, int max) { return random.Next(min, max); }
    }

    public class Charts : Urho.Application
    {
        bool movementsEnabled;
        Scene scene;
        Node plotNode;
        Camera camera;
        Octree octree;
        List<Bar> bars;

        public Bar SelectedBar { get; private set; }

        public IEnumerable<Bar> Bars => bars;

        [Preserve]
        public Charts(ApplicationOptions options = null) : base(options) { }

        static Charts()
        {
            UnhandledException += (s, e) =>
            {
                if (Debugger.IsAttached)
                    Debugger.Break();
                e.Handled = true;
            };
        }

        protected override void Start()
        {
            base.Start();
            CreateScene();
            SetupViewport();
        }

        async void CreateScene()
        {
            Input.SubscribeToTouchEnd(OnTouched);

            scene = new Scene();
            octree = scene.CreateComponent<Octree>();

            plotNode = scene.CreateChild();
            var baseNode = plotNode.CreateChild().CreateChild();
            var plane = baseNode.CreateComponent<StaticModel>();
            plane.Model = CoreAssets.Models.Plane;

            var cameraNode = scene.CreateChild();
            camera = cameraNode.CreateComponent<Camera>();
            cameraNode.Position = new Vector3(10, 15, 10) / 1.75f;
            cameraNode.Rotation = new Quaternion(-0.121f, 0.878f, -0.305f, -0.35f);

            Node lightNode = cameraNode.CreateChild();
            var light = lightNode.CreateComponent<Light>();
            light.LightType = LightType.Point;
            light.Range = 100;
            light.Brightness = 1.3f;

            int size = 3;
            baseNode.Scale = new Vector3(size * 1.5f, 1, size * 1.5f);
            bars = new List<Bar>(size * size);
            for (var i = 0f; i < size * 1.5f; i += 1.5f)
            {
                for (var j = 0f; j < size * 1.5f; j += 1.5f)
                {
                    var boxNode = plotNode.CreateChild();
                    boxNode.Position = new Vector3(size / 2f - i, 0, size / 2f - j);
                    var box = new Bar(new Urho.Color(RandomHelper.NextRandom(), RandomHelper.NextRandom(), RandomHelper.NextRandom(), 0.9f));
                    boxNode.AddComponent(box);
                    box.SetValueWithAnimation((Math.Abs(i) + Math.Abs(j) + 1) / 2f);
                    bars.Add(box);
                }
            }

            SelectedBar = bars.First();
            SelectedBar.Select();



            try
            {
                await plotNode.RunActionsAsync(new EaseBackOut(new RotateBy(2f, 0, 360, 0)));
            }
            catch (OperationCanceledException) { }
            movementsEnabled = true;
        }

        void OnTouched(TouchEndEventArgs e)
        {
            Ray cameraRay = camera.GetScreenRay((float)e.X / Graphics.Width, (float)e.Y / Graphics.Height);
            var results = octree.RaycastSingle(cameraRay, RayQueryLevel.Triangle, 100, DrawableFlags.Geometry);
            if (results != null)
            {
                var bar = results.Value.Node?.Parent?.GetComponent<Bar>();
                if (SelectedBar != bar)
                {
                    SelectedBar?.Deselect();
                    SelectedBar = bar;
                    SelectedBar?.Select();
                }
            }
        }

        protected override void OnUpdate(float timeStep)
        {
            if (Input.NumTouches >= 1 && movementsEnabled)
            {
                var touch = Input.GetTouch(0);
                plotNode.Rotate(new Quaternion(0, -touch.Delta.X, 0), TransformSpace.Local);
            }
            base.OnUpdate(timeStep);
        }

        public void Rotate(float toValue)
        {
            plotNode.Rotate(new Quaternion(0, toValue, 0), TransformSpace.Local);
        }

        void SetupViewport()
        {
            var renderer = Renderer;
            var vp = new Viewport(Context, scene, camera, null);
            renderer.SetViewport(0, vp);
        }
    }

    public class Bar : Component
    {
        Node barNode;
        Node textNode;
        Text3D text3D;
        Urho.Color color;
        float lastUpdateValue;

        public float Value
        {
            get { return barNode.Scale.Y; }
            set { barNode.Scale = new Vector3(1, value < 0.3f ? 0.3f : value, 1); }
        }

        public void SetValueWithAnimation(float value) => barNode.RunActionsAsync(new EaseBackOut(new ScaleTo(3f, 1, value, 1)));

        public Bar(Urho.Color color)
        {
            this.color = color;
            ReceiveSceneUpdates = true;
        }

        public override void OnAttachedToNode(Node node)
        {
            barNode = node.CreateChild();
            barNode.Scale = new Vector3(1, 0, 1); //means zero height
            var box = barNode.CreateComponent<Box>();
            box.Color = color;

            textNode = node.CreateChild();
            textNode.Rotate(new Quaternion(0, 180, 0), TransformSpace.World);
            textNode.Position = new Vector3(0, 10, 0);
            text3D = textNode.CreateComponent<Text3D>();
            text3D.SetFont(CoreAssets.Fonts.AnonymousPro, 60);
            text3D.TextEffect = TextEffect.Stroke;

            base.OnAttachedToNode(node);
        }

        protected override void OnUpdate(float timeStep)
        {
            var pos = barNode.Position;
            var scale = barNode.Scale;
            barNode.Position = new Vector3(pos.X, scale.Y / 2f, pos.Z);
            textNode.Position = new Vector3(0.5f, scale.Y + 0.2f, 0);
            var newValue = (float)Math.Round(scale.Y, 1);
            if (lastUpdateValue != newValue)
                text3D.Text = newValue.ToString("F01", CultureInfo.InvariantCulture);
            lastUpdateValue = newValue;
        }

        public void Deselect()
        {
            barNode.RemoveAllActions();//TODO: remove only "selection" action
            barNode.RunActions(new EaseBackOut(new TintTo(1f, color.R, color.G, color.B)));
        }

        public void Select()
        {
            Selected?.Invoke(this);
            // "blinking" animation
            barNode.RunActions(new RepeatForever(new TintTo(0.3f, 1f, 1f, 1f), new TintTo(0.3f, color.R, color.G, color.B)));
        }

        public event Action<Bar> Selected;
    }


    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class UrhoTest : ContentPage
    {
        UrhoSurface urhoSurface;
        Charts urhoApp;
        Xamarin.Forms.Slider selectedBarSlider;

        public UrhoTest()
        {
            InitializeComponent();

            var restartBtn = new Xamarin.Forms.Button { Text = "Restart" };
            restartBtn.Clicked += (sender, e) => StartUrhoApp();

            urhoSurface = new UrhoSurface();
            urhoSurface.VerticalOptions = LayoutOptions.FillAndExpand;

            Xamarin.Forms.Slider rotationSlider = new Xamarin.Forms.Slider(0, 500, 250);
            rotationSlider.ValueChanged += (s, e) => urhoApp?.Rotate((float)(e.NewValue - e.OldValue));

            selectedBarSlider = new Xamarin.Forms.Slider(0, 5, 2.5);
            selectedBarSlider.ValueChanged += OnValuesSliderValueChanged;

            Title = " UrhoSharp + Xamarin.Forms";
            Content = new StackLayout
            {
                Padding = new Thickness(12, 12, 12, 40),
                VerticalOptions = LayoutOptions.FillAndExpand,
                Children = {
                    urhoSurface,
                    restartBtn,
                    new Label { Text = "ROTATION::" },
                    rotationSlider,
                    new Label { Text = "SELECTED VALUE:" },
                    selectedBarSlider,
                }
            };
        }

        protected override void OnDisappearing()
        {
            UrhoSurface.OnDestroy();
            base.OnDisappearing();
        }

        void OnValuesSliderValueChanged(object sender, ValueChangedEventArgs e)
        {
            if (urhoApp?.SelectedBar != null)
                urhoApp.SelectedBar.Value = (float)e.NewValue;
        }

        private void OnBarSelection(Bar bar)
        {
            //reset value
            selectedBarSlider.ValueChanged -= OnValuesSliderValueChanged;
            selectedBarSlider.Value = bar.Value;
            selectedBarSlider.ValueChanged += OnValuesSliderValueChanged;
        }

        protected override async void OnAppearing()
        {
            StartUrhoApp();
        }

        async void StartUrhoApp()
        {
            urhoApp = await urhoSurface.Show<Charts>(new ApplicationOptions(assetsFolder: null) { Orientation = ApplicationOptions.OrientationType.LandscapeAndPortrait });
        }
    }












    //******************************************************************


    /*public partial class UrhoTest : ContentPage
    {
        Xamarin.Forms.Slider selectedBarSlider, rotationSlider;
        UrhoSurface urhoSurface;
        Charts urhoApp;

        public UrhoTest()
        {
            InitializeComponent();


            urhoSurface = new UrhoSurface();
            urhoSurface.VerticalOptions = LayoutOptions.FillAndExpand;

            rotationSlider = new Xamarin.Forms.Slider(0, 500, 250);

            selectedBarSlider = new Xamarin.Forms.Slider(0, 5, 2.5);

            Title = " UrhoSharp + Xamarin.Forms";
            Content = new StackLayout
            {
                Padding = new Thickness(12, 12, 12, 40),
                VerticalOptions = LayoutOptions.FillAndExpand,
                Children = {
                    urhoSurface,  // added
                    new Label { Text = "ROTATION:" },
                    rotationSlider,
                    new Label { Text = "SELECTED VALUE:" },
                    selectedBarSlider,
                }
            };

        }

        protected override async void OnAppearing()
        {
            urhoApp = await urhoSurface.Show<Charts>(new ApplicationOptions(assetsFolder: null)
                { Orientation = ApplicationOptions.OrientationType.Portrait });

            foreach (var bar in urhoApp.Bars)
            {
                bar.Selected += OnBarSelection;
            }
        }

        private void OnBarSelection(Bar bar)
        {
            //reset value
            selectedBarSlider.ValueChanged -= OnValuesSliderValueChanged;
            selectedBarSlider.Value = bar.Value;
            selectedBarSlider.ValueChanged += OnValuesSliderValueChanged;
        }

        void OnValuesSliderValueChanged(object sender, ValueChangedEventArgs e)
        {  // C# 6
            if (urhoApp?.SelectedBar != null)
            {
                urhoApp.SelectedBar.Value = (float)e.NewValue;
            }
        }
    }*/
}