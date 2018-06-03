namespace FsXamarinForms.Droid
open System

open Android.App
open Android.Content
open Android.Content.PM
open Android.OS
open Xamarin.Forms
open Xamarin.Forms.Platform.Android

type Resources = FsXamarinForms.Droid.Resource

type bReceiver (func1: String -> String -> String -> Unit, func2: String-> Unit) = 
   inherit BroadcastReceiver()               
   override this.OnReceive (context, intent) =
      let action = intent.Action
      let b = intent.Extras
      match action with 
      | "com.zebra.fsharp.ACTION" ->
            do 
                let decodedSource = b.GetString "com.symbol.datawedge.source"
                let decodedData = b.GetString "com.symbol.datawedge.data_string"
                let decodedLabelType = b.GetString "com.symbol.datawedge.label_type"
                func1 decodedSource decodedData decodedLabelType
      | "com.symbol.datawedge.api.RESULT_ACTION" ->
            do 
                let activeProfile = b.GetString "com.symbol.datawedge.api.RESULT_GET_ACTIVE_PROFILE"
                func2 activeProfile
      | _ ->
            do ()


[<Activity (Label = "F#Inventory", Icon = "@drawable/icon", Theme = "@style/MyTheme", MainLauncher = true, 
 ConfigurationChanges = (ConfigChanges.ScreenSize ||| ConfigChanges.Orientation))>]
type MainActivity() =
    inherit FormsAppCompatActivity()
    let mutable barcodeBroadcastReceiver = Unchecked.defaultof<bReceiver>

    member this.sendBarcodeNotification a b c  = 
        this.RunOnUiThread (fun () -> MessagingCenter.Send<FsXamarinForms.InventoryApp, string*string>(
                                        Xamarin.Forms.Application.Current :?> FsXamarinForms.InventoryApp, 
                                        "DataWedgeOutput", (b,c)))
    
    override this.OnCreate (bundle: Bundle) =
        FormsAppCompatActivity.TabLayoutResource <- Resources.Layout.Tabbar
        FormsAppCompatActivity.ToolbarResource <- Resources.Layout.Toolbar

        base.OnCreate (bundle)        
        Xamarin.Forms.Forms.Init (this, bundle)
        this.LoadApplication (new FsXamarinForms.InventoryApp ())
        do barcodeBroadcastReceiver <- new bReceiver( this.sendBarcodeNotification, fun _ -> ())

    override this.OnStart () =
        base.OnStart ()
        let filter = new IntentFilter "com.zebra.fsharp.ACTION"
        do filter.AddAction "com.symbol.datawedge.api.RESULT_ACTION"
        do filter.AddCategory "android.intent.category.DEFAULT"
        do this.RegisterReceiver (barcodeBroadcastReceiver, filter) |> ignore

    override this.OnStop () =
        base.OnStop ()        
        do this.UnregisterReceiver (barcodeBroadcastReceiver)        
