namespace BatteryMonitor.Components;

public partial class ThresholdComponent : ContentView
{
    public String Text 
    {
        get => (string)GetValue(TextProperty); 
        set => SetValue(TextProperty, value); 
    }

    public Color LabelColor 
    {
        get => (Color)GetValue(LabelColorProperty); 
        set => SetValue(LabelColorProperty, value); 
    }

    public String ImageName 
    {
        get => (string)GetValue(ImageNameProperty); 
        set => SetValue(ImageNameProperty, value); 
    }

    public int Value
    {
        get => (int)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public int StepperMinimumValue
    {
        get => (int)GetValue(StepperMinimumValueProperty);
        set => SetValue(StepperMinimumValueProperty, value);
    }

    public int StepperMaximumValue
    {
        get => (int)GetValue(StepperMaximumValueProperty);
        set => SetValue(StepperMaximumValueProperty, value);
    }



    public ThresholdComponent()
	{
		InitializeComponent();
	}


    public static readonly BindableProperty TextProperty = 
        BindableProperty.Create(nameof(Text), typeof(string), typeof(ThresholdComponent), String.Empty);

    public static readonly BindableProperty LabelColorProperty =
        BindableProperty.Create(nameof(LabelColor), typeof(Color), typeof(ThresholdComponent));

    public static readonly BindableProperty ImageNameProperty =
        BindableProperty.Create(nameof(ImageName), typeof(string), typeof(ThresholdComponent), String.Empty);

    public static readonly BindableProperty ValueProperty =
        BindableProperty.Create(nameof(Value), typeof(int), typeof(ThresholdComponent), 0);

    public static readonly BindableProperty StepperMinimumValueProperty =
        BindableProperty.Create(nameof(StepperMinimumValue), typeof(int), typeof(ThresholdComponent), 0);

    public static readonly BindableProperty StepperMaximumValueProperty =
        BindableProperty.Create(nameof(StepperMaximumValue), typeof(int), typeof(ThresholdComponent), 0);
}