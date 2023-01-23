namespace TestApp

open Elmish
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
        Value1 : int option
        Value2 : string option
    } with
    static member Default = { Value1 = None; Value2 = None }

type Msg =
    | UpdateValue1
    | UpdateValue2

module Views =
    let update msg model =
        printfn $"Msg.update {msg}"
        match msg with
        | UpdateValue1 ->
            let v = (model.Value1 |> Option.defaultValue 0) + 1
            { model with Value1 = Some v }, Cmd.none
        | UpdateValue2 ->
            let v = (model.Value2 |> Option.defaultValue "") + "."
            { model with Value2 = Some v }, Cmd.none

    let view1 key (_model : IWritable<Model>) =
        Component.create (key,
            fun ctx ->
                printfn "view1 render"
                let model = ctx.usePassed(_model, renderOnChange = true)
                let _, dispatch = ctx.useElmish (model, update)
                let value = model |> State.readMap (fun m -> m.Value1)

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
                            Button.onClick (fun _ -> printfn "View1 Button click"; UpdateValue1 |> dispatch)
                        ]
                    ]
                ]
        )

    let view2 key (_model : IWritable<Model>) =
        Component.create (key,
            fun ctx ->
                printfn "view2 render"
                let model = ctx.usePassed(_model, renderOnChange = true)
                let _, dispatch = ctx.useElmish (model, update)
                let value = model |> State.readMap (fun m -> m.Value2)

                StackPanel.create [
                    StackPanel.orientation Orientation.Horizontal
                    StackPanel.children [
                        match value.Current with
                        | None ->
                            ()
                        | Some v ->
                            TextBlock.create [
                                TextBlock.text v
                            ]
                        Button.create [
                            Button.content "Update"
                            Button.onClick (fun _ -> printfn "View2 Button click"; UpdateValue2 |> dispatch)
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
                    Grid.rowDefinitions "*,*"
                    Grid.children [
                        ContentControl.create [
                            Grid.row 0
                            ContentControl.content (
                                Views.view1 "view1" model
                            )
                        ]
                        ContentControl.create [
                            Grid.row 1
                            ContentControl.content (
                                Views.view2 "view2" model
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