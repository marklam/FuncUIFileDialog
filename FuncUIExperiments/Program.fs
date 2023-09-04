﻿namespace FuncUIFileDialog

open System
open Avalonia
open Avalonia.Themes.Fluent
open Avalonia.FuncUI.Hosts
open Avalonia.Controls.ApplicationLifetimes

open Avalonia.Controls
open Avalonia.FuncUI
open Avalonia.FuncUI.DSL
open Avalonia.Controls.Shapes
open OxyPlot
open OxyPlot.Avalonia
open Avalonia.FuncUI.Types
open Avalonia.FuncUI.Builder
open Avalonia.Markup.Xaml.Styling

[<AutoOpen>]
module PlotView  =

    let create (attrs: IAttr<PlotView> list): IView<PlotView> =
        ViewBuilder.Create<PlotView>(attrs)

    type PlotView with

        static member model<'t when 't :> PlotView>(value) : IAttr<'t> =
            AttrBuilder<'t>.CreateProperty<_>(PlotView.ModelProperty, value, ValueNone)

module MainView =
    let plotModel (clip : IWritable<_>) (rect : IWritable<_>) =

        let model = OxyPlot.PlotModel(Title = Random.Shared.Next().ToString())
        Diagnostics.Debug.WriteLine $"Created a plot model {model.Title}"
        let horizontalAxis = OxyPlot.Axes.LinearAxis(Position = OxyPlot.Axes.AxisPosition.Bottom)
        let verticalAxis   = OxyPlot.Axes.LinearAxis(Position = OxyPlot.Axes.AxisPosition.Left)

        model.Axes.Add(horizontalAxis)
        model.Axes.Add(verticalAxis)

        let handleTransformChanged _ =
            // Transform to screen coordinates
            let p1 = horizontalAxis.Transform(0, 0, verticalAxis)
            let p2 = horizontalAxis.Transform(100, 100, verticalAxis)

            // Change the position and size of the Avalonia shape
            rect.Set (Avalonia.Rect((min p1.X p2.X), (min p1.Y p2.Y), abs (p2.X - p1.X), abs (p2.Y - p1.Y)))
            clip.Set (Avalonia.Rect(model.PlotArea.Left, model.PlotArea.Top, model.PlotArea.Width, model.PlotArea.Height))

        horizontalAxis.TransformChanged.Add(handleTransformChanged)
        verticalAxis.TransformChanged.Add(handleTransformChanged)
        model

    let updateAxes (plotModel : OxyPlot.PlotModel) (maxVal : int) =
        Diagnostics.Debug.WriteLine $"Updating axes on {plotModel.Title}"
        plotModel.Axes[0].Zoom(0, maxVal)
        plotModel.Axes[1].Zoom(0, maxVal)
        plotModel.InvalidatePlot(false)

    let create () =
        let scales = [ 50; 100; 150; 200 ]
        let emptyPlotModel = PlotModel()
        Component (
            fun ctx ->
                let clip = ctx.useState (Avalonia.Rect())
                let rect = ctx.useState (Avalonia.Rect())
                let scale = ctx.useState(0)
                let pmodel = ctx.useState emptyPlotModel
                ctx.useEffect ((fun () -> pmodel.Set (plotModel clip rect)), [EffectTrigger.AfterInit])
                //ctx.useEffect ((
                //    fun () ->
                //        Diagnostics.Debug.WriteLine $"Hooking up a subscription"
                //        scale.Observable
                //        |> Observable.subscribe (
                //                fun i ->
                //                    updateAxes plotModel.Current i
                //            )
                //), [EffectTrigger.AfterInit])
                ctx.useEffect ((fun () -> updateAxes pmodel.Current scale.Current), [EffectTrigger.AfterChange scale])

                Grid.create [
                    Grid.columnDefinitions "Auto, *"
                    Grid.children [
                        ListBox.create [
                            Grid.column 0
                            ListBox.dataItems scales
                            ListBox.selectedItem scale.Current
                            ListBox.onSelectedItemChanged (
                                function
                                | :? int as i -> scale.Set i
                                | _ -> ()
                            )
                        ]

                        Grid.create [
                            Grid.column 1
                            Grid.children [
                                PlotView.create [
                                    PlotView.model pmodel.Current
                                ]
                                Canvas.create [
                                    Visual.clip (Media.RectangleGeometry(clip.Current) :> Media.Geometry)

                                    Canvas.children [
                                        Rectangle.create [
                                            Rectangle.isHitTestVisible false

                                            Canvas.left      rect.Current.Left
                                            Canvas.top       rect.Current.Top
                                            Rectangle.width  rect.Current.Width
                                            Rectangle.height rect.Current.Height
                                            Rectangle.fill   Avalonia.Media.Brushes.Blue
                                        ]
                                    ]
                                ]
                            ]
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
        this.Styles.Add (FluentTheme())
        let oxyplot = StyleInclude(baseUri=null, Source = Uri "avares://OxyPlot.Avalonia/Themes/Default.axaml") // TODO - check that OxyPlot.Avalonia.Themes.Default() has gone in real release
        this.Styles.Add oxyplot

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