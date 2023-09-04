namespace FuncUIFileDialog

open System
open Avalonia
open Avalonia.FuncUI.Hosts
open Avalonia.Controls.ApplicationLifetimes
open Avalonia.Controls
open Elmish
open Avalonia.FuncUI
open Avalonia.FuncUI.DSL
open Avalonia.FuncUI.Types
open Avalonia.FuncUI.Builder
open Avalonia.FuncUI.Elmish
open Material.Styles.Themes
open Material.Colors
open Avalonia.Layout
open Material.Styles.Controls

[<AutoOpen>]
module Card =
    let create (attrs: IAttr<Card> list): IView<Card> =
        ViewBuilder.Create<Card>(attrs)

    type Card with
        static member insideClipping<'t when 't :> Card>(value: bool) : IAttr<'t> =
            AttrBuilder<'t>.CreateProperty<_>(Card.InsideClippingProperty, value, ValueNone)

module Counter =
    type ItemProgress = { Name : string; ProgressStatus : float }
    type State = { Item : ItemProgress }
    let init() =
        { Item = { Name = "One"; ProgressStatus = 0.0 } }, Cmd.none

    type Msg =
    | ProgressItem of int

    let background _model dispatch =
        backgroundTask {
            while true do
                do! Async.Sleep 1000
                dispatch (ProgressItem 1)
        } :> IDisposable

    let updates model : Sub<Msg> =
        [
            (["updates"], background model)
        ]

    let update (msg: Msg) (state: State) : State*Cmd<Msg> =
        match msg with
        | ProgressItem n ->
            let item = state.Item
            let newItem =
                let p = item.ProgressStatus + 0.1
                if p > 1.0 then
                    { item with ProgressStatus = 0.0 }
                else
                    { item with ProgressStatus = p }

            { state with Item = newItem }, Cmd.none


    let progressBar (item: ItemProgress) =
        ProgressBar.create [
            Grid.column 1
            ProgressBar.minimum 0.0
            ProgressBar.maximum 1.0
            ProgressBar.value item.ProgressStatus
            ProgressBar.horizontalAlignment HorizontalAlignment.Stretch
            ProgressBar.showProgressText true
        ]

    let progressLoop (item: ItemProgress) =
        ProgressBar.create [
            Grid.column 2
            ProgressBar.width 100.0
            ProgressBar.height 100.0
            ProgressBar.minimum 0.0
            ProgressBar.maximum 1.0
            ProgressBar.value item.ProgressStatus
            ProgressBar.horizontalAlignment HorizontalAlignment.Stretch
            ProgressBar.showProgressText true
            ProgressBar.classes ["circular"]
        ]

    let view (state: State) (dispatch) =
        Grid.create [
            Grid.children [
                Grid.create [
                    let item = state.Item
                    Grid.row 2
                    Grid.columnDefinitions "Auto,*,*"
                    Grid.children [
                        TextBlock.create [
                            Grid.column 0
                            TextBlock.text item.Name
                        ]
                        progressBar item
                        progressLoop item
                    ]
                ]
            ]
        ]

type MainWindow() as this =
    inherit HostWindow()
    do
        base.Title <- "Counter Example"
        base.Height <- 400.0
        base.Width <- 400.0

        Elmish.Program.mkProgram Counter.init Counter.update Counter.view
        |> Program.withSubscription Counter.updates
        |> Program.withHost this
        |> Program.withConsoleTrace
        |> Program.runWithAvaloniaSyncDispatch () // This line is important for the UI to work with updates from the background thread

type App() =
    inherit Application()

    override this.Initialize() =
        let material = new MaterialTheme(null, BaseTheme = Base.BaseThemeMode.Light, PrimaryColor = PrimaryColor.Blue, SecondaryColor = SecondaryColor.LightBlue)
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