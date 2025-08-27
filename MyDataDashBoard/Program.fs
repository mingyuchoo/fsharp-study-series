module DataDashboard.Program

open Avalonia
open Avalonia.Controls.ApplicationLifetimes
open Avalonia.FuncUI
open Avalonia.FuncUI.Hosts
open Avalonia.Themes.Fluent
open DataDashboard.MainView

type MainWindow () as this =
    inherit HostWindow ()

    do
        printfn "MainWindow created"
        base.Title <- "Data Analytics Dashboard"
        base.Width <- 1200.0
        base.Height <- 800.0

        Elmish.Program.mkProgram init update view
        |> Avalonia.FuncUI.Elmish.Program.withHost this
        |> Elmish.Program.run

type App () =
    inherit Application ()

    override this.Initialize () =
        printfn "Initializing app"
        this.Styles.Add (new FluentTheme ())

    override this.OnFrameworkInitializationCompleted () =
        printfn "Framework initialized"

        match this.ApplicationLifetime with
        | :? IClassicDesktopStyleApplicationLifetime as desktopLifetime -> desktopLifetime.MainWindow <- MainWindow ()
        | _ -> ()

[<EntryPoint>]
let main args =
    try
        printfn "Starting Data Analytics Dashboard"
        AppBuilder.Configure<App>().UsePlatformDetect().UseSkia().StartWithClassicDesktopLifetime (args)
    with ex ->
        printfn "Error starting application: %s" ex.Message
        printfn "StackTrace: %s" ex.StackTrace
        1
