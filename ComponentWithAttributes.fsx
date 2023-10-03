#r "nuget:Avalonia.Desktop"
#r "nuget:Avalonia.FuncUI"
#r "nuget:Avalonia.Themes.Fluent"

open Avalonia
open Avalonia.FuncUI.Hosts
open Avalonia.Controls.ApplicationLifetimes
open Avalonia.Controls
open Avalonia.FuncUI
open Avalonia.FuncUI.DSL
open Avalonia.FuncUI.Types
open Avalonia.Layout
open Avalonia.Themes.Fluent

module Counter =
    let countComponent (count : IReadable<int>) =
        Component.create ("count",
            fun ctx ->
                let count = ctx.usePassedRead count
                TextBlock.create [
                    TextBlock.fontSize 48.0
                    TextBlock.verticalAlignment VerticalAlignment.Center
                    TextBlock.horizontalAlignment HorizontalAlignment.Center
                    TextBlock.text (string count.Current)
                ]
        )

    let view () =
        Component (fun ctx ->
            let state = ctx.useState 0

            Grid.create [
                Grid.rowDefinitions "Auto,*"
                Grid.columnDefinitions "*,*"

                Grid.children [
                    Button.create [
                        Grid.row 0
                        Grid.column 0
                        Button.onClick (fun _ -> state.Current - 1 |> state.Set)
                        Button.content "-"
                    ]
                    Button.create [
                        Grid.row 0
                        Grid.column 1
                        Button.onClick (fun _ -> state.Current + 1 |> state.Set)
                        Button.content "+"
                    ]
                    countComponent state
                    |> View.withAttrs [
                        Grid.row 1
                        Grid.column 0
                    ]
                    countComponent state
                    |> View.withAttrs [
                        Grid.row 1
                        Grid.column 1
                    ]
                ]
            ]
        )

type MainWindow() =
    inherit HostWindow()
    do
        base.Title <- "Counter Example"
        base.Height <- 400.0
        base.Width <- 400.0
        base.Content <- Counter.view ()

type App() =
    inherit Application()

    override this.Initialize() =
        this.Styles.Add (FluentTheme())

    override this.OnFrameworkInitializationCompleted() =
        this.Name <- "Counter Grid"
        match this.ApplicationLifetime with
        | :? IClassicDesktopStyleApplicationLifetime as desktopLifetime ->
            let mainWindow = MainWindow()
            mainWindow.MinWidth <- 850.0
            mainWindow.MinHeight <- 600.0

            desktopLifetime.MainWindow <- mainWindow
        | _ -> ()

module Program =
    let main(args: string[]) =
        AppBuilder
            .Configure<App>()
            .UsePlatformDetect()
            .UseSkia()
            .StartWithClassicDesktopLifetime(args)

Program.main [| |]