namespace FsXamarinForms.Droid

open System

open Android.App
open Android.Content
open Android.Content.PM
open Android.OS
open Xamarin.Forms
open Xamarin.Forms.Platform.Android

open DWLiterals

type Resources = FsXamarinForms.Droid.Resource

type bReceiver (func1: String -> String -> String -> Unit, func2: String-> Unit) = 
   inherit BroadcastReceiver()               
   override this.OnReceive (context, intent) =
      let action = intent.Action
      let b = intent.Extras
      match action with 
      | ACTION_INTENT_PLUGIN ->
            let decodedSource = b.GetString EXTRA_BARCODE_SOURCE
            let decodedData = b.GetString EXTRA_BARCODE_STRING
            let decodedLabelType = b.GetString EXTRA_LABEL_TYPE
            func1 decodedSource decodedData decodedLabelType
      | ACTION_RESULT_DATAWEDGE ->
            let activeProfile = b.GetString EXTRA_RESULT_GET_ACTIVE_PROFILE
            func2 activeProfile
      | _ ->
            do ()


[<Activity (Label = "F#Inventory", Icon = "@drawable/icon", Theme = "@style/MyTheme", MainLauncher = true, 
 ConfigurationChanges = (ConfigChanges.ScreenSize ||| ConfigChanges.Orientation))>]
type MainActivity() =
    inherit FormsAppCompatActivity()
    let mutable barcodeBroadcastReceiver = Unchecked.defaultof<bReceiver>

    member this.getActiveProfileIntent () =
            let dw = new Intent ()
            do  dw.SetAction ACTION_DATAWEDGE |> ignore
            do  dw.PutExtra (EXTRA_GET_ACTIVE_PROFILE, "") |> ignore
            do  this.SendBroadcast dw

    member this.showActiveProfile (a:String) = 
       this.RunOnUiThread( fun() -> 
          let barcodeToast = (Android.Widget.Toast.MakeText(this, "Active DW Profile: " + a, Android.Widget.ToastLength.Long))
          do barcodeToast.Show() )


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
        do barcodeBroadcastReceiver <- new bReceiver( this.sendBarcodeNotification, this.showActiveProfile )
        let dwApi = DependencyService.Get<IDwApi.IDwApi>()
        dwApi.ApiEvent.Add (this.getActiveProfileIntent)

    override this.OnStart () =
        base.OnStart ()
        let filter = (new IntentFilter(ACTION_INTENT_PLUGIN))
        do filter.AddAction ACTION_RESULT_DATAWEDGE
        do filter.AddCategory "android.intent.category.DEFAULT"
        do this.RegisterReceiver (barcodeBroadcastReceiver, filter) |> ignore

    override this.OnStop () =
        base.OnStop ()        
        do this.UnregisterReceiver (barcodeBroadcastReceiver)        
