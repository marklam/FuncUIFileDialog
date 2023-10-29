namespace ComponentsWithProgress

open System
open System.Collections.Generic
open Avalonia
open Avalonia.Controls
open Avalonia.Controls.ApplicationLifetimes
open Avalonia.Layout
open Avalonia.Media
open Elmish
open Avalonia.FuncUI
open Avalonia.FuncUI.Elmish.ElmishHook
open Avalonia.FuncUI.Hosts
open Avalonia.FuncUI.Types
open Avalonia.FuncUI.DSL
open Material.Colors
open Material.Styles.Themes

[<AutoOpen>]
module ControlStuff =
    type ListBox with
        static member viewItemsWithTemplateAndSelChange (items : #IReadOnlyList<'t>) template (getSel : unit -> 't option) (setSel : 't option -> unit) =
            [
                ListBox.viewItems (
                    items |> Seq.map template |> List.ofSeq
                )
                ListBox.onSelectedIndexChanged (
                    (fun i ->
                        if i >= 0 && i < items.Count then
                            setSel (Some (items[i]))
                        else
                            setSel None
                    ), subPatchOptions = SubPatchOptions.OnChangeOf items
                )
                let selIndex = getSel() |> Option.bind (fun s -> items |> Seq.tryFindIndex ((=)s)) |> Option.defaultValue -1
                ListBox.selectedIndex (selIndex)
            ]

type InProgress = | Running | Done
type WorkflowStage = | Stage1 | Stage2 | Stage3
type Model = { Stages : WorkflowStage list; CurrentStage : WorkflowStage; Counter : int}
type Msg = | Tick | SelectStage of WorkflowStage

module Views =
    let dispatchTicks _model dispatch =
        backgroundTask {
            while true do
                do! Async.Sleep 1000
                dispatch Tick
        } :> IDisposable

    let tickSubscription model : Sub<Msg> =
        [
            (["updates"], dispatchTicks model)
        ]

    let update msg model =
        match msg with
        | Tick ->
            { model with Counter = model.Counter + 1}, Cmd.none
        | SelectStage stage ->
            { model with CurrentStage = stage }, Cmd.none

    let flightStatus (model : Model) (stage : WorkflowStage) =
        match stage with
        | Stage1 -> InProgress.Running
        | Stage2 -> InProgress.Done
        | Stage3 -> InProgress.Running

    let stageHeader (model : IReadable<Model>) (stage : WorkflowStage) =
        printfn $"stageHeader created for {stage}"
        Component.create (stage.ToString(),
            fun ctx ->
                printfn $"stageHeader rendered for {stage}"
                let model = ctx.usePassedRead model
                let flightStatus = model |> State.readMap (fun m -> flightStatus m stage)
                let indicator =
                    match flightStatus.Current with
                    | Done ->
                        None
                    | Running ->
                        ProgressBar.create [
                            ProgressBar.verticalAlignment VerticalAlignment.Stretch
                            ProgressBar.horizontalAlignment HorizontalAlignment.Stretch
                            ProgressBar.classes ["circular"; "no-transitions"] // no-transitions avoids a flicker in the circular control
                            ProgressBar.isIndeterminate true
                        ] :> IView |> Some

                Grid.create [
                    Grid.height 30
                    Grid.columnDefinitions "*,30"
                    Grid.children [
                        TextBlock.create [
                            Grid.column 0
                            TextBlock.classes ["Body2"]
                            TextBlock.text (stage.ToString())
                            TextBlock.margin 5.0
                        ]
                        Viewbox.create [
                            Grid.column 1
                            Viewbox.margin 5.0
                            Viewbox.stretch Stretch.Fill
                            Viewbox.horizontalAlignment HorizontalAlignment.Stretch
                            Viewbox.verticalAlignment VerticalAlignment.Stretch
                            Viewbox.child indicator
                        ]
                    ]
                ]

        ) :> IView

    let stageList (model : IReadable<Model>) dispatch =
        printfn "stageList called"
        Component.create ("stageList",
            fun ctx ->
                printfn "stageList rendered"
                let model = ctx.usePassedRead model
                let currentStage = model |> State.readMap (fun m -> m.CurrentStage)

                ListBox.create [
                    Grid.column 0
                    ListBox.classes ["NoScroll"]

                    yield! ListBox.viewItemsWithTemplateAndSelChange model.Current.Stages
                        (stageHeader model)
                        (fun () -> currentStage.Current |> Some)
                        (Option.iter (SelectStage >> dispatch))
                ]
        ) :> IView

    let view () =

        Component (
            fun ctx ->
                let initial = { Stages = [ Stage1; Stage2; Stage3 ]; CurrentStage = Stage1; Counter = 0 }
                let state = ctx.useState initial
                let mapProgram =
                    Program.withSubscription tickSubscription

                let _, dispatch = ctx.useElmish(state, update, mapProgram)

                Grid.create [
                    Grid.columnDefinitions "*,3*"
                    Grid.children [
                        stageList state dispatch
                    ]
                ]

        )

type MainWindow() =
    inherit HostWindow()
    do
        base.Title <- "ComponentsWithProgress"
        base.Height <- 400.0
        base.Width <- 400.0

        base.Content <-Views.view ()

type App() =
    inherit Application()

    override this.Initialize() =
        let material = new MaterialTheme(null, BaseTheme = Base.BaseThemeMode.Light, PrimaryColor = PrimaryColor.Blue, SecondaryColor = SecondaryColor.LightBlue)
        this.Styles.Add (material)

    override this.OnFrameworkInitializationCompleted() =
        this.Name <- "ComponentsWithProgress"
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