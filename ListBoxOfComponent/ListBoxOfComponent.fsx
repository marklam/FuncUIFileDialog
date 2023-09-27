#r "nuget:Avalonia.Desktop"
#r "nuget:Avalonia.FuncUI"
#r "nuget:Avalonia.FuncUI.Elmish"
#r "nuget:Avalonia.Themes.Fluent"

open System
open Avalonia
open Avalonia.FuncUI.Hosts
open Avalonia.Controls.ApplicationLifetimes
open Avalonia.Controls
open Avalonia.FuncUI
open Avalonia.FuncUI.DSL
open Avalonia.FuncUI.Types
open Avalonia.Layout
open System.Collections.Immutable
open Avalonia.Controls.Templates
open Avalonia.Media
open Avalonia.Themes.Fluent

module Option =
    let toRef (o : 't option) =
        match o with
        | None   -> Unchecked.defaultof<'t>
        | Some o -> o

module Counter =
    type Item = { Index : int }

    type State = { ItemsByIndex : ImmutableList<Item>; SelectedIndex : int }

    let initial =
        let items = ImmutableList.CreateRange (seq { for i = 0 to 9_999_999 do yield { Index = i } })
        { ItemsByIndex = items; SelectedIndex = 0 }

    let virtualizingStackPanel =
        FuncTemplate<Panel>(
            fun () -> VirtualizingStackPanel(AreVerticalSnapPointsRegular=true)
        )

    let grid (item : Item) =
        Grid.create [
            Grid.height 80
            Grid.columnDefinitions "Auto,*"
            Grid.rowDefinitions "*,*"
            Grid.children [
                TextBlock.create [
                    Grid.row 0
                    Grid.column 0
                    TextBlock.text "Index"
                    TextBlock.background Brushes.LightBlue
                    TextBlock.horizontalAlignment HorizontalAlignment.Stretch
                    TextBlock.verticalAlignment VerticalAlignment.Center
                ]
                TextBlock.create [
                    Grid.row 0
                    Grid.column 1
                    TextBlock.text (string item.Index)
                    TextBlock.verticalAlignment VerticalAlignment.Center
                ]
                TextBlock.create [
                    Grid.row 1
                    Grid.column 0
                    TextBlock.text "Notes"
                    TextBlock.background Brushes.LightBlue
                    TextBlock.horizontalAlignment HorizontalAlignment.Stretch
                    TextBlock.verticalAlignment VerticalAlignment.Center
                ]
                TextBox.create [
                    Grid.row 1
                    Grid.column 1
                    TextBox.background Brushes.Yellow
                    TextBox.text ""
                    TextBox.verticalAlignment VerticalAlignment.Center
                ]
            ]
        ]

    let itemTemplateComponent (item : Item) =
        Component.create ("item", (
            fun ctx ->
                grid item
        ))

    let itemTemplateGrid (item : Item) =
            grid item

    let listbox templateForView (selPeakGroup : IWritable<Item option>) (peaksList : ImmutableList<Item>) =
        Component.create ("listbox", (
            fun ctx ->
                let selPeakGroup = ctx.usePassed selPeakGroup
                ListBox.create [
                    ListBox.itemsPanel virtualizingStackPanel

                    ListBox.dataItems peaksList
                    ListBox.itemTemplate (DataTemplateView.create<IView<_>,Item> templateForView)
                    ListBox.onSelectedItemChanged (
                        function
                        | :? Item as t ->
                            selPeakGroup.Set (Some t)
                        | _ ->
                            selPeakGroup.Set None
                    )
                    ListBox.selectedItem (selPeakGroup.Current |> Option.toRef)
                ]
        ))

    let listOfGrids (selPeakGroup : IWritable<Item option>) (peaksList : ImmutableList<Item>) =
        listbox itemTemplateGrid selPeakGroup peaksList

    let listOfComponents (selPeakGroup : IWritable<Item option>) (peaksList : ImmutableList<Item>) =
        listbox itemTemplateComponent selPeakGroup peaksList

    let view () =
        Component (
            fun ctx ->
                let state = ctx.useState initial
                let selectedItem = ctx.useState None

                Grid.create [
                    Grid.columnDefinitions "*,*"
                    Grid.rowDefinitions "Auto,*"
                    Grid.children [
                        TextBlock.create [
                            Grid.row 0
                            Grid.column 0
                            TextBlock.text "List of Grids"
                        ]
                        Grid.create [
                            Grid.row 1
                            Grid.column 0
                            Grid.children [
                                listOfGrids selectedItem state.Current.ItemsByIndex
                            ]
                        ]

                        TextBlock.create [
                            Grid.row 0
                            Grid.column 1
                            TextBlock.text "List of Components"
                        ]
                        Grid.create [
                            Grid.row 1
                            Grid.column 1
                            Grid.children [
                                listOfComponents selectedItem state.Current.ItemsByIndex
                            ]
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

        base.Content <- Counter.view()

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
    let main(args: string[]) =
        AppBuilder
            .Configure<App>()
            .UsePlatformDetect()
            .UseSkia()
            .StartWithClassicDesktopLifetime(args)

Program.main [| |]