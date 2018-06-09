module DWLiterals

// DATAWEDGE INTENT PLUGIN

[<Literal>] 
let ACTION_INTENT_PLUGIN = "com.zebra.fsharp.ACTION"
[<Literal>]
let EXTRA_BARCODE_SOURCE = "com.symbol.datawedge.source"
[<Literal>]
let EXTRA_BARCODE_STRING = "com.symbol.datawedge.data_string"
[<Literal>]
let EXTRA_LABEL_TYPE = "com.symbol.datawedge.label_type"

//  6.2 API and up Actions sent to DataWedge
[<Literal>] 
let ACTION_DATAWEDGE = "com.symbol.datawedge.api.ACTION"

//  6.2 API and up Extras sent to DataWedge
[<Literal>] 
let EXTRA_GET_ACTIVE_PROFILE = "com.symbol.datawedge.api.GET_ACTIVE_PROFILE"

// 6.2 API and up Actions received from DataWedge
[<Literal>] 
let ACTION_RESULT_DATAWEDGE = "com.symbol.datawedge.api.RESULT_ACTION"

//  6.3 API and up Extras sent to DataWedge
[<Literal>] 
let EXTRA_GET_VERSION_INFO = "com.symbol.datawedge.api.GET_VERSION_INFO";

// 6.3 API and up Extras received from DataWedge

[<Literal>]
let EXTRA_RESULT_GET_ACTIVE_PROFILE  = "com.symbol.datawedge.api.RESULT_GET_ACTIVE_PROFILE"
[<Literal>]
let EXTRA_RESULT_GET_VERSION_INFO = "com.symbol.datawedge.api.RESULT_GET_VERSION_INFO"


