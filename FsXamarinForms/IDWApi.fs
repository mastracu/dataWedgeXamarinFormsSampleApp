namespace IDwApi

open System.Collections.Generic

type IDwApi =
   interface
      abstract ApiEvent : IEvent<unit>
      abstract GetDwProfile : unit -> unit
   end