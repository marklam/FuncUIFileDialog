namespace FuncUIFileDialog

open System
open Elmish
open Avalonia
open Avalonia.Themes.Fluent
open Avalonia.FuncUI.Hosts
open Avalonia.Controls.ApplicationLifetimes

open Avalonia.Controls
open Avalonia.FuncUI
open Avalonia.FuncUI.DSL
open Avalonia.FuncUI.Elmish.ElmishHook

type Model = { Value : int }

type Msg = | IncrementValue

type Readable<'t> = | Readable of IReadable<'t>
module Readable =
    let subscribe (ctx : IComponentContext) (Readable r) = ctx.usePassedRead r

module MainView =
    let handler msg model =
        match msg with
        | IncrementValue ->
            { model with Value = model.Value + 1}, Cmd.none

    let myComponent key (rmodel : Readable<Model>) dispatch =
        Component.create (key,
            fun ctx ->
                let model = rmodel |> Readable.subscribe ctx
                let flag = ctx.useState false
                let value = model |> State.readMap (fun m -> m.Value)

                StackPanel.create [
                    StackPanel.children [
                        CheckBox.create [
                            CheckBox.content "Checkbox with local state"
                            CheckBox.isChecked flag.Current
                            CheckBox.onChecked (fun _ -> flag.Set true)
                            CheckBox.onUnchecked (fun _ -> flag.Set false)
                        ]
                        Button.create [
                            Button.content "Send increment message"
                            Button.onClick (fun _ -> dispatch IncrementValue)
                        ]
                        TextBlock.create [
                            TextBlock.text $"Value: {value.Current}"
                        ]
                    ]
                ]
        )

    let create () =
        let initial = { Value = 0 }

        Component (
            fun ctx ->
                let model = ctx.useState(initial)
                let _, dispatch = ctx.useElmish (model, handler)

                Grid.create [
                    Grid.rowDefinitions "*"
                    Grid.children [
                        ContentControl.create [
                            Grid.row 0
                            ContentControl.content (
                                myComponent "c1" (Readable model) dispatch
                            )
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