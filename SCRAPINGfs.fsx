module ScrapingFs
open System.IO
open System.Text
open System.Text.RegularExpressions
open FSharp.Data



(*
    Elements
*)

let GetElements nodeName attrName attrValue (node:HtmlNode) =
    HtmlNode.descendants false ( fun n ->
        Regex.IsMatch ( HtmlNode.name n, nodeName )
        && Regex.IsMatch ( HtmlNode.attributeValue attrName n, attrValue ) ) node

let GetElementsWithAttributeValue targetSelector judgeSelector (node:HtmlNode) =
    let cssSelect selector (n:HtmlNode) = n.CssSelect selector
    let search n = n |> cssSelect targetSelector |> List.filter (cssSelect judgeSelector >> List.isEmpty >> not) |> List.distinct
    search node

let GetElementsWithString targetNodeName str (node:HtmlNode) =
    HtmlNode.descendants false ( fun n ->
        HtmlNode.name n = targetNodeName
        && Regex.IsMatch ( HtmlNode.innerText n, str ) ) node


(*
    Attribute Value
*)

let GetAttributeValue (attrName:string) (cssSelector:string) (node:HtmlNode) =
    let value = node |> fun n -> n.CssSelect cssSelector
    if Seq.isEmpty value then "" else
    value
    |> Seq.exactlyOne
    |> HtmlNode.attributeValue attrName

let GetAttributeValue2 attrName targetSelector judgeSelector (node:HtmlNode) =
    let value = node |> GetElementsWithAttributeValue targetSelector judgeSelector
    if Seq.isEmpty value then "" else
    value
    |> Seq.exactlyOne
    |> HtmlNode.attributeValue attrName


(*
    HTML
*)

let FetchDynamicHtml (url:string) =
    let req    = System.Net.WebRequest.Create(url)
    let rep    = req.GetResponse ()
    use stream = rep.GetResponseStream ()
    use reader =
        match Regex.Match(rep.ContentType, @"charset=(.*)") with
        | m when m.Success -> new StreamReader(stream, Encoding.GetEncoding(m.Groups.[1].Value))
        | _ -> new System.IO.StreamReader(stream)
    reader.ReadToEnd ()

let FetchHtmls (urls:list<string>) =
    let rec loop acc lst =
        match lst with
        | [] -> acc
        | _  -> loop ( ( FetchDynamicHtml (List.head lst) ) :: acc ) ( List.tail lst )
    loop [] urls

let FetchHtmlsByStaticLinks attrName cssSelector url =
    let rec pages (acc:list<string>) u =
        match u with
        | "" -> acc
        | _  -> if acc.Length > 100 then acc
                else
                let html = FetchDynamicHtml u
                let node = html |> HtmlDocument.Parse |> HtmlDocument.body
                let link = GetAttributeValue attrName cssSelector node
                           |> fun lk ->
                                match lk with
                                | _  when lk.StartsWith "http://" || lk.StartsWith "https://" -> lk
                                | "" -> ""
                                | _  -> lk |> fun query -> System.UriBuilder( u, Query = query ).ToString()
                pages ( html :: acc ) link

    pages [] url

let FetchHtmlsByDynamicLinks attrName targetSelector judgeSelector pattern url =
    let rec pages (acc:list<string>) u =
        match u with
        | "" -> acc
        | _  -> if acc.Length > 100 then acc
                else
                let html = FetchDynamicHtml u
                let node = html |> HtmlDocument.Parse |> HtmlDocument.body
                let link = GetAttributeValue2 attrName targetSelector judgeSelector node
                           |> fun lk ->
                                match lk with
                                | _  when lk.StartsWith "http://" || lk.StartsWith "https://" -> lk
                                | "" -> ""
                                | _  -> lk |> fun s -> Regex.Match( s, pattern ).Value
                                           |> fun query -> System.UriBuilder( u, Query = query ).ToString()
                pages ( html :: acc ) link

    pages [] url


(*
    others
*)

let GoogleSearch keyword =

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


(*
    utility
*)

let SafeSubString (n:int) (str:string) =
    if str.Length < n then str else string ( str.[0..(n - 1)] )


let Justify lst =
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


