namespace FuncUIFileDialog

open System
open Elmish
open Avalonia
open Avalonia.Themes.Fluent
open Avalonia.FuncUI.Hosts
open Avalonia.Controls.ApplicationLifetimes

open Avalonia.Controls
open Avalonia.Layout
open Avalonia.FuncUI
open Avalonia.FuncUI.DSL
open Avalonia.FuncUI.Elmish.ElmishHook
open Avalonia.Threading

type Model =
    {
        ProjectFile : string option
    } with
    static member Default = { ProjectFile = None }

type WindowParams =
    { Window : Window }

type Msg =
    | PickProjectFile
    | SetProjectFile of string

module Msg =
    let handler windowParams msg model =
        match msg with
        | PickProjectFile ->
            let saveFileTask () =
                Dispatcher.UIThread.InvokeAsync<string>(
                    System.Func<System.Threading.Tasks.Task<string>>(fun () ->
                    let dialog = SaveFileDialog(Title = "Pick the project file")
                    dialog.ShowAsync windowParams.Window
                    ))

            model, Cmd.OfTask.perform saveFileTask () SetProjectFile
        | SetProjectFile f ->
            { model with ProjectFile = Some f }, Cmd.none

module TaskView =
    let create update key (model : IWritable<Model>) =
        Component.create (key,
            fun ctx ->
                let model = ctx.usePassed model
                let _, dispatch = ctx.useElmish (model, update)
                let projectFile = model |> State.readMap (fun m -> m.ProjectFile)

                StackPanel.create [
                    StackPanel.orientation Orientation.Vertical
                    StackPanel.children [
                        TextBlock.create [
                            TextBlock.text "Here are the details of your project:"
                        ]

                        TextBlock.create [
                            TextBlock.text "Project file:"
                        ]

                        match projectFile.Current with
                        | None ->
                            Button.create [
                                Button.content "Select a file"
                                Button.onClick (fun _ -> PickProjectFile |> dispatch)
                            ]
                        | Some p ->
                            TextBox.create [
                                TextBox.text p
                            ]
                    ]
                ]
        )

module MainView =
    let create windowParams =
        let update = Msg.handler windowParams

        Component (
            fun ctx ->
                let model = ctx.useState (Model.Default)

                Grid.create [
                    Grid.rowDefinitions "*"
                    Grid.children [
                        ContentControl.create [
                            Grid.row 0
                            ContentControl.content (
                                TaskView.create update "taskview" model
                            )
                        ]
                    ]
                ]
        )

type MainWindow() as this =
    inherit HostWindow()

    do
        base.Title   <- "FuncUIFileDialog"
        base.Content <- MainView.create { Window = this }

type App() =
    inherit Application()

    override this.Initialize() =
        this.Styles.Add (FluentTheme(baseUri = null, Mode = FluentThemeMode.Dark))

    override this.OnFrameworkInitializationCompleted() =
        this.Name <- "FuncUIFileDialog"
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