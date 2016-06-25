#if INTERACTIVE
#I @"./packages/FSharp.Data/lib/net40/"
#r @"FSharp.Data.DesignTime.dll"
#r @"FSharp.Data.dll"
#endif
module ScrapingFs
open System.IO
open System.Text
open System.Text.RegularExpressions
open FSharp.Data


type Sc () =

    static member FetchDynamicHtml (url:string) =
        let req    = System.Net.WebRequest.Create(url)
        let rep    = req.GetResponse ()
        use stream = rep.GetResponseStream ()
        use reader =
            match Regex.Match(rep.ContentType, @"charset=(.*)") with
            | m when m.Success -> new StreamReader(stream, Encoding.GetEncoding(m.Groups.[1].Value))
            | _ -> new System.IO.StreamReader(stream)
        reader.ReadToEnd ()

    static member FetchHtmls (urls:list<string>) =
        let rec loop acc lst =
            match lst with
            | [] -> acc
            | _  -> loop ( ( Sc.FetchDynamicHtml (List.head lst) ) :: acc ) ( List.tail lst )
        loop [] urls

    static member FetchHtmlsByNextLink attrName cssSelector url =
        let rec pages (acc:list<string>) nextLink =
            match nextLink with
            | "" -> acc
            | _  -> if acc.Length > 100 then acc
                    else
                    let html =    nextLink  |> Sc.FetchDynamicHtml
                    let node =    html      |> HtmlDocument.Parse |> HtmlDocument.elements
                    let baseUrl = Sc.GetBaseUrl( url, node |> List.exactlyOne )
                    let link =    node      |> fun n -> n.CssSelect cssSelector
                                            |> fun l -> match Seq.isEmpty l with
                                                        | true  -> [""]
                                                        | false -> l |>  List.map ( HtmlNode.attributeValue attrName )
                                            |> List.exactlyOne
                                            |> Sc.GetAbsoluteLink baseUrl
                    pages ( html :: acc ) link
        pages [] url

    static member GetBaseUrl (baseUrl:string, ?node:HtmlNode) =

        let f (n:HtmlNode) =
            n.CssSelect "base"
            |> List.map ( HtmlNode.attributeValue "href" )
            |> List.distinct
            |> fun l -> if List.isEmpty l then baseUrl else List.exactlyOne l

        match node with
        | Some node -> f node
        | None ->
            baseUrl
            |> Sc.FetchDynamicHtml
            |> HtmlDocument.Parse
            |> HtmlDocument.elements
            |> List.exactlyOne
            |> f

    static member GetAbsoluteLink (baseUrl:string) (link:string) =
        match link with
        | _  when link.StartsWith "http://" || link.StartsWith "https://" -> link
        | "" ->   ""
        | _  -> let lk = System.Uri( System.Uri( baseUrl ), link ).AbsoluteUri.ToString()
                if lk.StartsWith "http://" || lk.StartsWith "https://" then lk else ""

    static member GetElements nodeName attrName attrValue (node:HtmlNode) =
        HtmlNode.descendants false ( fun n ->
            Regex.IsMatch ( HtmlNode.name n, nodeName )
            && Regex.IsMatch ( HtmlNode.attributeValue attrName n, attrValue ) ) node

    static member GetElementsWithString targetNodeName str (node:HtmlNode) =
        HtmlNode.descendants false ( fun n ->
            HtmlNode.name n = targetNodeName
            && Regex.IsMatch ( HtmlNode.innerText n, str ) ) node

    static member GetAttributeValues (attrName:string) (cssSelector:string) (node:HtmlNode) =
        node.CssSelect cssSelector
        |> fun l ->
            match Seq.isEmpty l with
            | true  -> None
            | false -> l |>  List.map ( HtmlNode.attributeValue attrName ) |> Some



   (*

        TODO: delete these functions when ':parent Selector' implemented

    *)

    static member GetElementsBySubject targetSelector judgeSelector (node:HtmlNode) =
        let cssSelect selector (n:HtmlNode) = n.CssSelect selector
        let search n = n |> cssSelect targetSelector |> List.filter (cssSelect judgeSelector >> List.isEmpty >> not) |> List.distinct
        search node

    static member GetAttributeValueBySubject attrName targetSelector judgeSelector (node:HtmlNode) =
        let value = node |> Sc.GetElementsBySubject targetSelector judgeSelector
        if Seq.isEmpty value then "" else
        value
        |> Seq.exactlyOne
        |> HtmlNode.attributeValue attrName

    static member FetchHtmlsByLinksBySubject attrName targetSelector judgeSelector pattern url =
 
        let rec pages (acc:list<string>) nextLink =
            match nextLink with
            | "" -> acc
            | _  -> if acc.Length > 100 then acc
                    else
                    let html = nextLink |> Sc.FetchDynamicHtml
                    let node = html     |> HtmlDocument.Parse |> HtmlDocument.elements
                    let baseUrl = Sc.GetBaseUrl( url, node |> List.exactlyOne )
                    let link = node |> List.exactlyOne     |> Sc.GetAttributeValueBySubject attrName targetSelector judgeSelector
                                        |> fun lk -> match lk with
                                                     | _  when lk.StartsWith "http://" || lk.StartsWith "https://" -> lk
                                                     | "" -> ""
                                                     | _  -> lk |> fun s -> Regex.Match( s, pattern ).Value
                                                             |> fun query -> System.UriBuilder( nextLink, Query = query ).ToString()
                    pages ( html :: acc ) link

        pages [] url

             




type Util  =

    static member GoogleSearch keyword =

        let baseStr  = "http://www.google.co.uk/search?q=" + keyword
        let fragment = "#q=" + keyword + "&start="

        baseStr :: ( [10..10..30] |> List.map    ( fun n ->  baseStr + fragment + string n ))
        |> Seq.map     ( fun url -> HtmlDocument.Load url )
        |> Seq.map     HtmlDocument.body
        |> Seq.collect ( fun n -> n.CssSelect "a[href]" )
        |> Seq.map     ( fun n -> n.InnerText(), n.AttributeValue "href" )
        |> Seq.filter  ( fun (name, url) -> name <> "Cached" && name <> "Similar" && url.StartsWith("/url?"))
        |> Seq.map     ( fun (name, url) -> name, url.Substring(0, url.IndexOf("&sa=")).Replace("/url?q=", ""))
        |> Seq.map     ( fun (a,b) -> [a;b] )
        |> Seq.toList

    static member SafeSubString (n:int) (str:string) =
        if str.Length < n then str else string ( str.[0..(n - 1)] )

    static member Justify lst =
        let swapRowColumn lst =
            lst
            |> List.collect List.indexed
            |> List.groupBy fst
            |> List.map snd
            |> List.map (List.map snd)

        let sjis = Encoding.GetEncoding "Shift_JIS"

        let justify lst =
            let lst = List.map (fun (str : string) -> str, sjis.GetByteCount str) lst
            let max =
                lst
                |> List.map snd
                |> List.max
            List.map (fun (str, len) -> str + String.replicate (max - len) " ") lst

        lst
        |> swapRowColumn
        |> List.map justify
        |> swapRowColumn
 
