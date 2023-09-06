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
open Avalonia.FuncUI.Elmish.ElmishHook
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

    type Augmentations = { Version : int; Notes : Map<int, string> }

    type State = { ItemsByIndex : ImmutableList<Item>; Augmentations : Augmentations; SelectedIndex : int }

    type Msg =
    | SetNote of int*string

    let initial =
        let items = ImmutableList.CreateRange (seq { for i = 0 to 9_999_999 do yield { Index = i } })
        { ItemsByIndex = items; Augmentations = { Version = 0; Notes = Map.empty }; SelectedIndex = 0 }

    let update (msg: Msg) (state: State) : State*Cmd<Msg> =
        match msg with
        | SetNote (n,note) ->
            let augmentations = state.Augmentations
            let newVersion = augmentations.Version + 1
            let newNotes = augmentations.Notes.Add(n, note)
            if n = 0 then printfn "Setting note for %d to '%s', version %d" n note newVersion
            { state with Augmentations = { Version = newVersion; Notes = newNotes } }, Cmd.none

    let virtualizingStackPanel =
        FuncTemplate<Panel>(
            fun () -> VirtualizingStackPanel(AreVerticalSnapPointsRegular=true)
        )

    let itemTemplate (annotations : IReadable<_>) dispatch =
        printfn "itemTemplate constructed with annotations version %d" annotations.Current.Version
        fun (item : Item) ->
            Component.create ("item", (
                fun ctx -> 
                    let annotations = ctx.usePassedRead annotations
                    if item.Index = 0 then printfn "itemTemplate invoked for item %d with annotations version %d" item.Index annotations.Current.Version
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
                                let note =
                                    Map.tryFind item.Index annotations.Current.Notes
                                    |> Option.defaultValue ""
                                Grid.row 1
                                Grid.column 1
                                TextBox.background Brushes.Yellow
                                TextBox.text note
                                TextBox.onTextChanged (fun note -> SetNote (item.Index, note) |> dispatch)
                                TextBox.verticalAlignment VerticalAlignment.Center
                            ]
                        ]
                    ]
            ))

    let annotatedList dispatch (selPeakGroup : IWritable<Item option>) (peaksList : ImmutableList<Item>) (annotations : IReadable<Augmentations>) =
        printfn "List rebuilt with annotations %d" annotations.Current.Version
        ListBox.create [
            ListBox.itemsPanel virtualizingStackPanel

            ListBox.dataItems peaksList
            ListBox.itemTemplate (DataTemplateView.create<IView<_>,Item> (itemTemplate annotations dispatch))
            ListBox.onSelectedItemChanged (
                function
                | :? Item as t ->
                    selPeakGroup.Set (Some t)
                | _ ->
                    selPeakGroup.Set None
            )
            ListBox.selectedItem (selPeakGroup.Current |> Option.toRef)
        ]

    let view () =
        Component (
            fun ctx ->
                let state = ctx.useState initial
                let _, dispatch = ctx.useElmish(state, update)

                let selectedItem = ctx.useState None

                let augmentations =
                    state
                    |> State.readMap (
                        fun m ->
                            let note0 = Map.tryFind 0 m.Augmentations.Notes |> Option.defaultValue "(not found)"
                            printfn "augmentations updated with version %d, index 0 note %s"
                                m.Augmentations.Version
                                note0
                            m.Augmentations
                    )

                Grid.create [
                    Grid.columnDefinitions "*,*"
                    Grid.children [

                        annotatedList dispatch selectedItem state.Current.ItemsByIndex augmentations

                        StackPanel.create [
                            Grid.column 1
                            StackPanel.orientation Orientation.Vertical
                            StackPanel.verticalAlignment VerticalAlignment.Stretch
                            StackPanel.horizontalAlignment HorizontalAlignment.Stretch
                            StackPanel.background Brushes.LightGray
                            StackPanel.children [
                                TextBlock.create [
                                    TextBlock.text "Note for item 0:"
                                ]
                                TextBox.create [
                                    TextBox.text (Map.tryFind 0 augmentations.Current.Notes |> Option.defaultValue "")
                                    TextBox.onTextChanged (fun s -> dispatch (SetNote(0, s)))
                                    TextBox.background Brushes.Yellow
                                ]
                                TextBlock.create [
                                    TextBlock.text $"(from version %d{augmentations.Current.Version})"
                                ]
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

        base.Content <- Counter.view ()

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