open System

// Define a functino to construct a message to print
let from whom = sprintf "from %s" whom

[<EntryPoint>]
let main argv =
    // Call the function
    let message = from "F#"
    printfn "Hello world %s" message
    0 // return an integer exit code
