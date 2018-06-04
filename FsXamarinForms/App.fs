namespace FsXamarinForms

open Elmish.XamarinForms
open Elmish.XamarinForms.DynamicViews
open Xamarin.Forms


module App = 

    type Model = 
      { Barcode : string option
        Symbology: string option
        Count: int
      }

    type Msg = 
        | BarcodeUpdate of string * string
        | Reset

    let initModel = { Barcode = None; Symbology = None; Count = 0 }

    let init () = initModel, Cmd.none

    let update msg model =
        match msg with
        | BarcodeUpdate (bc, sym) -> { model with  Barcode = Some bc; Symbology = Some sym; Count = model.Count+1}, Cmd.none
        | Reset -> initModel, Cmd.none  

    let view (model: Model) dispatch =
        Xaml.ContentPage(
          content=Xaml.StackLayout(padding=20.0, spacing = 5.0,
                  children=[
                    Xaml.Label(text= "Scanned Barcode:", fontSize = "Large")
                    Xaml.Entry(text= match model.Barcode with 
                                            | None -> ""
                                            | Some str -> str
                        ,fontSize = "Large" )
                    Xaml.Label(text= "Symbology:", fontSize = "Large")
                    Xaml.Entry(text= match model.Symbology with 
                                            | None -> ""
                                            | Some str -> str
                        , fontSize = "Large" )
                    Xaml.Label(text= "Count:", fontSize = "Large")
                    Xaml.Entry(text= string model.Count, fontSize = "Large" )
                    Xaml.Button(text="Reset", command=fixf(fun () -> dispatch Reset))
                  ]))

open App


type InventoryApp () as app = 
    inherit Application ()

    let dwOutput dispatch = 
        let newBarcodeAction dispatch = new System.Action<InventoryApp,string*string>(fun app arg -> dispatch (BarcodeUpdate arg) )
        MessagingCenter.Subscribe<InventoryApp, string*string> (Xamarin.Forms.Application.Current, "DataWedgeOutput", newBarcodeAction dispatch)

    let program = Program.mkProgram init update view
    let runner = 
        program
        |> Program.withSubscription (fun _ -> Cmd.ofSub dwOutput)
        |> Program.withConsoleTrace
        |> Program.withDynamicView app
        |> Program.run