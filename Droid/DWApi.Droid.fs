namespace DwApi

open Xamarin.Forms

type DwApi() =
  let apiEvent = new Event<unit> ()
  interface IDwApi.IDwApi with
     member this.ApiEvent = apiEvent.Publish
     member this.GetDwProfile () = apiEvent.Trigger ()

[<assembly: Dependency(typedefof<DwApi>)>]
()

