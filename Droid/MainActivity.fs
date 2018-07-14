namespace FsXamarinForms.Droid

open System
open System.IO

open Android.App
open Android.Content
open Android.Content.PM
open Android.OS
open Xamarin.Forms
open Xamarin.Forms.Platform.Android

open DWLiterals

type Resources = FsXamarinForms.Droid.Resource


type DWBReceiver (notifyDecoding, notifyDWVer, notifyDWProfileName, notifyDWScannerStatus) = 
   inherit BroadcastReceiver()               
   override this.OnReceive (context, intent) =
      let action = intent.Action
      let b = intent.Extras
      match action with 
      | ACTION_INTENT_PLUGIN ->
            let source = b.GetString EXTRA_BARCODE_SOURCE
            let data = b.GetString EXTRA_BARCODE_STRING
            let symbology = b.GetString EXTRA_LABEL_TYPE
            notifyDecoding (data, symbology)
      | ACTION_RESULT_DATAWEDGE ->
            if intent.HasExtra EXTRA_RESULT_GET_VERSION_INFO then
                let dwVersionInfo = b.GetBundle EXTRA_RESULT_GET_VERSION_INFO
                let dwVersion = dwVersionInfo.GetString ("DATAWEDGE")
                let barcodeScanning = dwVersionInfo.GetString ("BARCODE_SCANNING")
                let decoderLibrary = dwVersionInfo.GetString ("DECODER_LIBRARY")
                notifyDWVer (sprintf "SF: %s  DW: %s" barcodeScanning dwVersion)
            else
                ()
            if intent.HasExtra EXTRA_RESULT_GET_ACTIVE_PROFILE then
                let profileName = b.GetString EXTRA_RESULT_GET_ACTIVE_PROFILE
                notifyDWProfileName (sprintf "Active DW Profile: %s" profileName)
            else
                ()
      | ACTION_NOTIFICATION ->
            if intent.HasExtra EXTRA_RESULT_GET_NOTIFICATION then
                let b = b.GetBundle EXTRA_RESULT_GET_NOTIFICATION
                let profileName =  b.GetString("PROFILE_NAME")
                notifyDWProfileName (sprintf "Active DW Profile: %s" profileName)
                let scannerStatus =  b.GetString("STATUS")
                notifyDWScannerStatus (sprintf "Scanner Status: %s" scannerStatus)
            else
                ()
      | _ ->
            do ()


[<Activity (Label = "F#Inventory", Icon = "@drawable/icon", Theme = "@style/MyTheme", MainLauncher = true, 
 ConfigurationChanges = (ConfigChanges.ScreenSize ||| ConfigChanges.Orientation), ScreenOrientation = ScreenOrientation.Portrait)>]
type MainActivity() =
    inherit FormsAppCompatActivity()

    let mutable barcodeBroadcastReceiver = Unchecked.defaultof<DWBReceiver>

    member this.DWApiAction (actionType) =
        use dw = new Intent ()
        do  dw.SetAction ACTION_DATAWEDGE |> ignore
        do  dw.PutExtra (actionType, "") |> ignore
        do  this.SendBroadcast dw

    member this.DWNotification (notType, actionType) =
        use b = new Bundle()
        do b.PutString("com.symbol.datawedge.api.APPLICATION_NAME","com.mastracu.FsXamarinForms")
        do b.PutString("com.symbol.datawedge.api.NOTIFICATION_TYPE", notType)
        use dw = new Intent()
        do dw.SetAction ACTION_DATAWEDGE |> ignore
        do dw.PutExtra(actionType, b) |> ignore
        this.SendBroadcast dw

    member this.send2App<'t> msg arg = 
        this.RunOnUiThread (fun () -> MessagingCenter.Send<FsXamarinForms.InventoryApp, 't>(
                                        Xamarin.Forms.Application.Current :?> FsXamarinForms.InventoryApp, 
                                        msg, arg))
    
    override this.OnCreate (bundle: Bundle) =
        FormsAppCompatActivity.TabLayoutResource <- Resources.Layout.Tabbar
        FormsAppCompatActivity.ToolbarResource <- Resources.Layout.Toolbar
        let Asset2DWAutoImport filename =
            let path = "/enterprise/device/settings/datawedge/autoimport/"
            let assets = this.Assets
            let fromStream = assets.Open filename
            // I create the file - RW for owner only, not visibile to DW
            let toFileStream = File.Create (path + filename)
            do fromStream.CopyTo toFileStream
            do toFileStream.Close ()
            do fromStream.Close ()
            // once it is copied, I give RW access for DW to process it and then remove it.  
            let javaFile =  new Java.IO.File (path + filename)
            do javaFile.SetWritable (true,false) |> ignore
            do javaFile.SetReadable (true,false) |> ignore

        base.OnCreate (bundle)        
        Xamarin.Forms.Forms.Init (this, bundle)
        this.LoadApplication (new FsXamarinForms.InventoryApp ())
        do barcodeBroadcastReceiver <- new DWBReceiver (this.send2App "DataWedgeOutput", 
                                                        this.send2App "BCReaderInfo1", 
                                                        this.send2App "BCReaderInfo2",
                                                        this.send2App "BCReaderInfo3")
        this.send2App "BCReaderInfo3" "No Scanner Status update"
        do Asset2DWAutoImport "dwprofile_F#Inventory.db"

    override this.OnStart () =
        base.OnStart ()
        let filter = new IntentFilter()
        do filter.AddAction ACTION_INTENT_PLUGIN
        do filter.AddAction ACTION_RESULT_DATAWEDGE
        do filter.AddAction ACTION_NOTIFICATION
        do filter.AddCategory "android.intent.category.DEFAULT"
        do this.RegisterReceiver (barcodeBroadcastReceiver, filter) |> ignore
        
        do this.DWNotification ("SCANNER_STATUS", EXTRA_REGISTER_FOR_NOTIFICATION)
        do this.DWApiAction(EXTRA_GET_VERSION_INFO)
        do this.DWApiAction(EXTRA_GET_ACTIVE_PROFILE)

    override this.OnStop () =
        base.OnStop ()        

        do this.DWNotification ("SCANNER_STATUS", EXTRA_UNREGISTER_FOR_NOTIFICATION)
        do this.UnregisterReceiver (barcodeBroadcastReceiver)        

