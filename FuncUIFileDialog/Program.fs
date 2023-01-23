namespace TestApp

open Avalonia
open Avalonia.Themes.Fluent
open Avalonia.FuncUI.Hosts
open Avalonia.Controls
open Avalonia.Controls.ApplicationLifetimes
open Avalonia.Layout
open Avalonia.FuncUI
open Avalonia.FuncUI.DSL
open Avalonia.FuncUI.Elmish.ElmishHook

type Model =
    {
        Value : int option
    } with
    static member Default = { Value = None }

type Msg =
    | UpdateValue

module TaskView =
    let update msg model =
        printfn $"Msg.update {msg}"
        match msg with
        | UpdateValue ->
            let v = (model.Value |> Option.defaultValue 0) + 1
            { model with Value = Some v }, Cmd.none

    let create key (_model : IWritable<Model>) =
        Component.create (key,
            fun ctx ->
                printfn "TaskView render"
                let model = ctx.usePassed(_model, renderOnChange = true)
                let _, dispatch = ctx.useElmish (model, update)
                let value = model |> State.readMap (fun m -> m.Value)

                StackPanel.create [
                    StackPanel.orientation Orientation.Horizontal
                    StackPanel.children [
                        match value.Current with
                        | None ->
                            ()
                        | Some v ->
                            TextBlock.create [
                                TextBlock.text (v.ToString())
                            ]
                        Button.create [
                            Button.content "Update"
                            Button.onClick (fun _ -> printfn "Button click"; UpdateValue |> dispatch)
                        ]
                    ]
                ]
        )

module MainView =
    let create () =
        Component (
            fun ctx ->
                printfn $"MainView render"

                let model = ctx.useState (Model.Default)

                Grid.create [
                    Grid.rowDefinitions "*"
                    Grid.children [
                        ContentControl.create [
                            Grid.row 0
                            ContentControl.content (
                                TaskView.create "taskview" model
                            )
                        ]
                    ]
                ]
        )

type MainWindow() =
    inherit HostWindow()

    do
        base.Title   <- "Test"
        base.Content <- MainView.create ()

type App() =
    inherit Application()

    override this.Initialize() =
        this.Styles.Add (FluentTheme(baseUri = null, Mode = FluentThemeMode.Dark))

    override this.OnFrameworkInitializationCompleted() =
        this.Name <- "Test"
        match this.ApplicationLifetime with
        | :? IClassicDesktopStyleApplicationLifetime as desktopLifetime ->
            let mainWindow = MainWindow()
            mainWindow.MinWidth <- 850.0
            mainWindow.MinHeight <- 600.0

            desktopLifetime.MainWindow <- mainWindow
        | _ -> ()

module Program =

    [<EntryPoint>]
    let main(args: string[]) =
        AppBuilder
            .Configure<App>()
            .UsePlatformDetect()
            .UseSkia()
            .StartWithClassicDesktopLifetime(args)