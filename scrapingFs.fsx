namespace scrapingFs
module scrapingFs =
    open System.IO
    open System.Text
    open System.Text.RegularExpressions
    open FSharp.Data

    (*
        printf utility
    *)

    let justify lst =
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

    (*
        Elements
    *)

    let getElements nodeName attrName attrValue (node:HtmlNode) =
        HtmlNode.descendants false ( fun n ->
            Regex.IsMatch ( HtmlNode.name n, nodeName )
            && Regex.IsMatch ( HtmlNode.attributeValue attrName n, attrValue ) ) node

    let getElementsWithAttributeValue targetSelector judgeSelector (node:HtmlNode) =
        let cssSelect selector (n:HtmlNode) = n.CssSelect selector
        let search n = n |> cssSelect targetSelector |> List.filter (cssSelect judgeSelector >> List.isEmpty >> not) |> List.distinct
        search node

    let getElementsWithString targetNodeName str (node:HtmlNode) =
        HtmlNode.descendants false ( fun n ->
            HtmlNode.name n = targetNodeName
            && Regex.IsMatch ( HtmlNode.innerText n, str ) ) node

    (*
        Links
    *)

    let staticLink attrName cssSelector (node:HtmlNode) =
        let value = node |> fun n -> n.CssSelect cssSelector
        if Seq.isEmpty value then "" else
        value
        |> Seq.exactlyOne
        |> HtmlNode.attributeValue attrName

    let dynamicLink attrName targetSelector judgeSelector (node:HtmlNode) =
        let value = node |> getElementsWithAttributeValue targetSelector judgeSelector
        if Seq.isEmpty value then "" else
        value
        |> Seq.exactlyOne
        |> HtmlNode.attributeValue attrName

    (*
        HTML
    *)

    let getDynamicHtml (url:string) =
        let req    = System.Net.WebRequest.Create(url)
        let rep    = req.GetResponse ()
        use stream = rep.GetResponseStream ()
        use reader =
            match Regex.Match(rep.ContentType, @"charset=(.*)") with
            | m when m.Success -> new StreamReader(stream, Encoding.GetEncoding(m.Groups.[1].Value))
            | _ -> new System.IO.StreamReader(stream)
        reader.ReadToEnd ()

    let getHtmls (urls:list<string>) =
        let rec loop acc lst =
            match lst with
            | [] -> acc
            | _  -> loop ( (getDynamicHtml (List.head lst) ) :: acc ) ( List.tail lst )
        loop [] urls

    let getHtmlsByStaticLinks attrName cssSelector url =
        let rec pages (acc:list<string>) u =
            match u with
            | "" -> acc
            | _  -> if acc.Length > 100 then acc
                    else
                    let html = getDynamicHtml u
                    let node = html |> HtmlDocument.Parse |> HtmlDocument.body
                    let link = staticLink attrName cssSelector node
                               |> fun lk ->
                                    match lk with
                                    | _  when lk.StartsWith "http://" || lk.StartsWith "https://" -> lk
                                    | "" -> ""
                                    | _  -> lk |> fun query -> System.UriBuilder( u, Query = query ).ToString()
                    pages ( html :: acc ) link

        pages [] url

    let getHtmlsByDynamicLinks attrName targetSelector judgeSelector pattern url =
        let rec pages (acc:list<string>) u =
            match u with
            | "" -> acc
            | _  -> if acc.Length > 100 then acc
                    else
                    let html = getDynamicHtml u
                    let node = html |> HtmlDocument.Parse |> HtmlDocument.body
                    let link = dynamicLink attrName targetSelector judgeSelector node
                               |> fun lk ->
                                    match lk with
                                    | _  when lk.StartsWith "http://" || lk.StartsWith "https://" -> lk
                                    | "" -> ""
                                    | _  -> lk |> fun s -> Regex.Match( s, pattern ).Value
                                               |> fun query -> System.UriBuilder( u, Query = query ).ToString()
                    pages ( html :: acc ) link

        pages [] url

