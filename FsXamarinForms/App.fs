namespace FsXamarinForms

open Elmish.XamarinForms
open Elmish.XamarinForms.DynamicViews
open Xamarin.Forms


module App = 
    open Xamarin.Forms
    open Xamarin.Forms

    type Model = 
      { Barcode : string option
        Symbology: string option
        Count: int
        ReaderInfo: string * string * string
      }

    type Msg = 
        | BarcodeUpdate of string * string
        | Reset
        | ReaderInfoUpdate of int * string

    let initModel = { Barcode = None; Symbology = None; Count = 0; ReaderInfo = ("","","") }

    let init () = initModel

    let update msg model =
        match msg with
        | BarcodeUpdate (bc, sym) -> { model with  Barcode = Some bc; Symbology = Some sym; Count = model.Count+1}
        | Reset -> { model with Count = 0}
        | ReaderInfoUpdate (pos, str) -> let (i1, i2, i3) = model.ReaderInfo
                                         { model with ReaderInfo = match pos with 
                                                                   |1 -> (str, i2, i3)
                                                                   |2 -> (i1, str, i3)
                                                                   |3 -> (i1, i2, str)
                                                                   |_ -> (i1, i2, i3)}

    let view (model: Model) dispatch =
        let fst3 (a, _, _) = a
        let snd3 (_, b, _) = b
        let thd3 (_, _, c) = c
        View.ContentPage(
          content=View.StackLayout(padding=20.0, spacing = 5.0,
                  children=[
                    View.Label(text= "Scanned Barcode:", fontSize = "Large")
                    View.Entry(text= match model.Barcode with 
                                            | None -> "<NONE>"
                                            | Some str -> str
                        ,fontSize = "Large" )
                    View.Label(text= "Symbology:", fontSize = "Large")
                    View.Entry(text= match model.Symbology with 
                                            | None -> "<NONE>"
                                            | Some str -> str
                        , fontSize = "Large" )
                    View.Label(text= "Count:", fontSize = "Large")
                    View.Entry(text= string model.Count, fontSize = "Large" )
                    View.Button(text="Reset Count", command=fixf(fun () -> dispatch Reset))
                    View.Label(text= fst3 model.ReaderInfo )
                    View.Label(text= snd3 model.ReaderInfo )
                    View.Label(text= thd3 model.ReaderInfo )
                  ]))



    let program = Program.mkSimple init update view

open App

type InventoryApp () as app = 
    inherit Application ()

    let dwOutput dispatch = 
        let newBarcodeAction dispatch = new System.Action<InventoryApp,string*string>(fun app arg -> dispatch (BarcodeUpdate arg) )
        MessagingCenter.Subscribe<InventoryApp, string*string> (Xamarin.Forms.Application.Current, "DataWedgeOutput", newBarcodeAction dispatch)

    let bcReaderInfo dispatch =
        for i in [1..3] do
            let newInfoAction dispatch = new System.Action<InventoryApp, string>(fun app arg -> dispatch (ReaderInfoUpdate (i, arg)) )
            MessagingCenter.Subscribe<InventoryApp, string> (Xamarin.Forms.Application.Current, "BCReaderInfo" + string i, newInfoAction dispatch)

    let runner = 
        program
        |> Program.withSubscription (fun _ -> Cmd.ofSub dwOutput)
        |> Program.withSubscription (fun _ -> Cmd.ofSub bcReaderInfo)
        |> Program.withConsoleTrace
        |> Program.runWithDynamicView app

    // https://fsprojects.github.io/Elmish.XamarinForms/tools.html
    do runner.EnableLiveUpdate()