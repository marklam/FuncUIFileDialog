namespace FuncUIFileDialog

open System
open Avalonia
open Avalonia.Themes.Fluent
open Avalonia.FuncUI.Hosts
open Avalonia.Controls.ApplicationLifetimes

open Avalonia.Controls
open Avalonia.FuncUI
open Avalonia.FuncUI.DSL
open Avalonia.FuncUI.Types
open Avalonia.FuncUI.Builder
open RangeSlider.Avalonia.Controls
open RangeSlider.Avalonia.Themes.Fluent
open Material.Styles.Themes
open Material.Colors
open Avalonia.Layout

[<AutoOpen>]
module RangeSlider  =
    open Avalonia.Collections
    open RangeSlider.Avalonia.Controls.Primitives

    let create (attrs: IAttr<RangeSlider> list): IView<RangeSlider> =
        ViewBuilder.Create<RangeSlider>(attrs)

    type RangeSlider with
        static member isDirectionReversed<'t when 't :> RangeSlider>(value) : IAttr<'t> = AttrBuilder<'t>.CreateProperty<_>(RangeSlider.IsDirectionReversedProperty, value, ValueNone)
        static member isSnapToTickEnabled<'t when 't :> RangeSlider>(value) : IAttr<'t> = AttrBuilder<'t>.CreateProperty<_>(RangeSlider.IsSnapToTickEnabledProperty, value, ValueNone)
        static member isThumbOverlap<'t when 't :> RangeSlider>(value) : IAttr<'t> = AttrBuilder<'t>.CreateProperty<_>(RangeSlider.IsThumbOverlapProperty, value, ValueNone)
        static member moveWholeRange<'t when 't :> RangeSlider>(value) : IAttr<'t> = AttrBuilder<'t>.CreateProperty<_>(RangeSlider.MoveWholeRangeProperty, value, ValueNone)
        static member orientation<'t when 't :> RangeSlider>(value) : IAttr<'t> = AttrBuilder<'t>.CreateProperty<_>(RangeSlider.OrientationProperty, value, ValueNone)
        static member thumbFlyoutPlacement<'t when 't :> RangeSlider>(value) : IAttr<'t> = AttrBuilder<'t>.CreateProperty<_>(RangeSlider.ThumbFlyoutPlacementProperty, value, ValueNone)
        static member tickFrequency<'t when 't :> RangeSlider>(value) : IAttr<'t> = AttrBuilder<'t>.CreateProperty<_>(RangeSlider.TickFrequencyProperty, value, ValueNone)
        static member tickPlacement<'t when 't :> RangeSlider>(value) : IAttr<'t> = AttrBuilder<'t>.CreateProperty<_>(RangeSlider.TickPlacementProperty, value, ValueNone)
        static member ticks<'t when 't :> RangeSlider>(value) : IAttr<'t> = AttrBuilder<'t>.CreateProperty<_>(RangeSlider.TicksProperty, value, ValueNone)
        static member ticks<'t when 't :> RangeSlider>(value: seq<float>) : IAttr<'t> = value |> AvaloniaList |> RangeSlider.ticks

    type RangeBase with
        static member minimum<'t when 't :> RangeBase>(value) : IAttr<'t> = AttrBuilder<'t>.CreateProperty<_>(RangeBase.MinimumProperty, value, ValueNone)
        static member maximum<'t when 't :> RangeBase>(value) : IAttr<'t> = AttrBuilder<'t>.CreateProperty<_>(RangeBase.MaximumProperty, value, ValueNone)
        static member lowerSelectedValue<'t when 't :> RangeBase>(value) : IAttr<'t> = AttrBuilder<'t>.CreateProperty<_>(RangeBase.LowerSelectedValueProperty, value, ValueNone)
        static member upperSelectedValue<'t when 't :> RangeBase>(value) : IAttr<'t> = AttrBuilder<'t>.CreateProperty<_>(RangeBase.UpperSelectedValueProperty, value, ValueNone)
        static member smallChange<'t when 't :> RangeBase>(value) : IAttr<'t> = AttrBuilder<'t>.CreateProperty<_>(RangeBase.SmallChangeProperty, value, ValueNone)
        static member largeChange<'t when 't :> RangeBase>(value) : IAttr<'t> = AttrBuilder<'t>.CreateProperty<_>(RangeBase.LargeChangeProperty, value, ValueNone)

        static member onLowerSelectedValueChanged<'t when 't :> RangeBase>(func: double -> unit, ?subPatchOptions) : IAttr<'t> =
            AttrBuilder<'t>.CreateSubscription<double>(RangeBase.LowerSelectedValueProperty, func, ?subPatchOptions = subPatchOptions)
        static member onUpperSelectedValueChanged<'t when 't :> RangeBase>(func: double -> unit, ?subPatchOptions) : IAttr<'t> =
            AttrBuilder<'t>.CreateSubscription<double>(RangeBase.UpperSelectedValueProperty, func, ?subPatchOptions = subPatchOptions)

module MainView =
    let create () =
        Component (
            fun ctx ->
                StackPanel.create [
                    StackPanel.orientation Orientation.Vertical
                    StackPanel.children [
                        Button.create [
                            Button.content "Click me"
                        ]
                        RangeSlider.create [
                            RangeSlider.minimum 0.0
                            RangeSlider.maximum 100.0
                            RangeSlider.lowerSelectedValue 0.0
                            RangeSlider.upperSelectedValue 100.0
                            RangeSlider.onLowerSelectedValueChanged (fun v -> Diagnostics.Debug.WriteLine $"Lower: {v}")
                            RangeSlider.onUpperSelectedValueChanged (fun v -> Diagnostics.Debug.WriteLine $"Upper: {v}")
                        ]
                    ]
                ]
        )

type MainWindow() =
    inherit HostWindow()

    do
        base.Title   <- "FuncUIExperiments"
        base.Content <- MainView.create ()

type App() =
    inherit Application()

    override this.Initialize() =
        let material = new MaterialTheme(null, BaseTheme = Base.BaseThemeMode.Light, PrimaryColor = PrimaryColor.Blue, SecondaryColor = SecondaryColor.LightBlue)
        let fluent = FluentTheme()

        this.Styles.Add (fluent)
        this.Styles.Add (RangeSliderTheme())
        this.Styles.Add (material)

    override this.OnFrameworkInitializationCompleted() =
        this.Name <- "FuncUIExperiments"
        match this.ApplicationLifetime with
        | :? IClassicDesktopStyleApplicationLifetime as desktopLifetime ->
            let mainWindow = MainWindow()
            mainWindow.MinWidth <- 850.0
            mainWindow.MinHeight <- 600.0

            desktopLifetime.MainWindow <- mainWindow
        | _ -> ()

module Program =

    [<EntryPoint; STAThread>]
    let main(args: string[]) =
        AppBuilder
            .Configure<App>()
            .UsePlatformDetect()
            .UseSkia()
            .StartWithClassicDesktopLifetime(args)