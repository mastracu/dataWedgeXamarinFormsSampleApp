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


type bReceiver (func1, func2) = 
   inherit BroadcastReceiver()               
   override this.OnReceive (context, intent) =
      let action = intent.Action
      let b = intent.Extras
      match action with 
      | ACTION_INTENT_PLUGIN ->
            let decodedSource = b.GetString EXTRA_BARCODE_SOURCE
            let decodedData = b.GetString EXTRA_BARCODE_STRING
            let decodedLabelType = b.GetString EXTRA_LABEL_TYPE
            func1 (decodedSource, decodedData, decodedLabelType)
      | ACTION_RESULT_DATAWEDGE ->
            if intent.HasExtra EXTRA_RESULT_GET_VERSION_INFO then
                let dwVersionInfo = b.GetBundle EXTRA_RESULT_GET_VERSION_INFO
                let dwVersion = dwVersionInfo.GetString ("DATAWEDGE")
                func2 dwVersion
            else
                ()
      | _ ->
            do ()


[<Activity (Label = "F#Inventory", Icon = "@drawable/icon", Theme = "@style/MyTheme", MainLauncher = true, 
 ConfigurationChanges = (ConfigChanges.ScreenSize ||| ConfigChanges.Orientation), ScreenOrientation = ScreenOrientation.Portrait)>]
type MainActivity() =
    inherit FormsAppCompatActivity()
    let mutable barcodeBroadcastReceiver = Unchecked.defaultof<bReceiver>

    member this.getBCReaderInfo () =
            let dw = new Intent ()
            do  dw.SetAction ACTION_DATAWEDGE |> ignore
            do  dw.PutExtra (EXTRA_GET_VERSION_INFO, "") |> ignore
            do  this.SendBroadcast dw

    member this.sendBCReaderInfo ver = 
        this.RunOnUiThread (fun () -> MessagingCenter.Send<FsXamarinForms.InventoryApp, string>(
                                        Xamarin.Forms.Application.Current :?> FsXamarinForms.InventoryApp, 
                                        "BCReaderInfo", ver))

    member this.sendBarcodeNotification (_, b, c)  = 
        this.RunOnUiThread (fun () -> MessagingCenter.Send<FsXamarinForms.InventoryApp, string*string>(
                                        Xamarin.Forms.Application.Current :?> FsXamarinForms.InventoryApp, 
                                        "DataWedgeOutput", (b,c)))
    
    override this.OnCreate (bundle: Bundle) =
        FormsAppCompatActivity.TabLayoutResource <- Resources.Layout.Tabbar
        FormsAppCompatActivity.ToolbarResource <- Resources.Layout.Toolbar

        base.OnCreate (bundle)        
        Xamarin.Forms.Forms.Init (this, bundle)
        this.LoadApplication (new FsXamarinForms.InventoryApp ())
        do barcodeBroadcastReceiver <- new bReceiver( this.sendBarcodeNotification, this.sendBCReaderInfo )
        let dwApi = DependencyService.Get<IDwApi.IDwApi>()
        dwApi.ApiEvent.Add (this.getBCReaderInfo)

    override this.OnStart () =
        base.OnStart ()
        let filter = new IntentFilter()
        do filter.AddAction ACTION_INTENT_PLUGIN
        do filter.AddAction ACTION_RESULT_DATAWEDGE
        do filter.AddCategory "android.intent.category.DEFAULT"
        do this.RegisterReceiver (barcodeBroadcastReceiver, filter) |> ignore
        do this.getBCReaderInfo()

    override this.OnStop () =
        base.OnStop ()        
        do this.UnregisterReceiver (barcodeBroadcastReceiver)        
