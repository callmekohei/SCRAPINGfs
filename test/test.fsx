#if INTERACTIVE
#I @"./packages/FSharp.Data/lib/net40/"
#r @"FSharp.Data.DesignTime.dll"
#r @"FSharp.Data.dll"
#I @"/Users/kohei/Documents/myProgramming/myFSharp/SCRAPINGfs/"
#r @"SCRAPINGfs.dll"
#r @"./packages/Persimmon.Console/tools/Persimmon.dll"
#endif

module ScrapingFs_Test =
    open FSharp.Data
    open ScrapingFs
    open Persimmon
    open UseTestNameByReflection

    let localServerUrl = @"http://localhost/~kohei/testHtml/"

    let ``Test FetchHtml`` = test{

        let innerText url =
            url
            |> Sc.FetchHtml
            |> List.collect ( HtmlDocument.Parse >> HtmlDocument.elements )
            |> List.collect ( fun n -> n.CssSelect "h1" )
            |> List.map     ( fun n -> n.InnerText() )
            |> List.exactlyOne

        do! assertEquals "It works!" ( innerText @"http://localhost/" )
        do! assertEquals "test0"     ( innerText ( localServerUrl + "test0.html" ))
        do! assertEquals "test1"     ( innerText ( localServerUrl + "test1.html" ))
        do! assertEquals "test2"     ( innerText ( localServerUrl + "test2.html" ))
        do! assertEquals "test3"     ( innerText ( localServerUrl + "test3.html" ))

    }

    let ``Test FetchHtmls`` = test{

        let results =
            [0..3] |> List.map ( sprintf "test%d.html")
            |> List.map ( fun s -> localServerUrl + s )
            |> Sc.FetchHtmls
            |> List.collect (  HtmlDocument.Parse >> HtmlDocument.elements )
            |> List.collect ( fun n -> n.CssSelect "h1" )
            |> List.map     ( fun n -> n.InnerText() )

        do! assertEquals ["test3"; "test2"; "test1"; "test0"] results
    }

    let ``Test FetchHtmlsByNextLink`` = test{

        let results =
            Sc.FetchHtmlsByNextLink "href" "body > a" ( localServerUrl + "test0.html" )
            |> List.collect (  HtmlDocument.Parse >> HtmlDocument.elements )
            |> List.collect ( fun n -> n.CssSelect "a[href]" )
            |> List.map     ( fun n -> n.InnerText() )

        do! assertEquals ["Next3"; "Next2"; "Next1"] results
    }

    let ``Test GetBaseUrl`` = test {

        let node s = s |> HtmlDocument.Parse |> HtmlDocument.elements |> List.exactlyOne

        do! assertEquals "abc" ( Sc.GetBaseUrl("abc", node """<a    href="zzz" />""" ) )
        do! assertEquals "zzz" ( Sc.GetBaseUrl("abc", node """<base href="zzz" />""" ) )

    }

    let ``Test GetAbsoluteLink`` = test {

        let baseUrl = @"http://www.abc.com/images/"

        do! assertEquals @"http://www.abc.com"                     (  Sc.GetAbsoluteLink baseUrl @"http://www.abc.com" )
        do! assertEquals @"http://www.abc.com/images/picture/cats" (  Sc.GetAbsoluteLink baseUrl @"picture/cats" )
        do! assertEquals @"http://www.abc.com/picture/cats"        (  Sc.GetAbsoluteLink baseUrl @"/picture/cats" )
        do! assertEquals @"http://www.abc.com/images/#top"         (  Sc.GetAbsoluteLink baseUrl @"#top" )
        do! assertEquals ""                                        (  Sc.GetAbsoluteLink baseUrl @"mailto:someone@example.com" )
        do! assertEquals ""                                        (  Sc.GetAbsoluteLink baseUrl @"javascript:alert('Hello World!');" )

    }



