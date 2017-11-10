namespace AzureService

module Program = 

    open Suave                 // always open suave
    open Suave.Successful      // for OK-result
    open Suave.Web             // for config

    [<EntryPoint>]
    let main _ = 
        startWebServer { defaultConfig with bindings = [HttpBinding.createSimple HTTP "0.0.0.0" 80]} (OK "Hello World!")
        0

