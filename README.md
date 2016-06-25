SCRAPINGfs
---
The utility library for web scraping.

Install
---
```
$ git install https://github.com/callmekohei/SCRAPINGfs
```

Prepare
---
```
$ source build.bash
```

Structure
---
```

.
└── ScrapingFs
     ├── Sc
        ├── FetchHtml
        ├── FetchHtmls
        ├── FetchHtmlsByNextLink 
        ├── GetBaseUrl
        ├── GetAbsoluteLink
        ├── GetElements
        ├── GetElementsWithString
        └── GetAttributeValues
     └── Util
        ├── GoogleSearch
        ├── SafeSubString
        └── Justify
```

How to use
---
#####GoogleSearch
```fsharp
open ScrapingFs

Util.GoogleSearch "callmekohei"
|> Util.Justify
|> List.iter ( fun [a;b] -> printfn "%s\t%s" a b )
```
result
```text
kohei (@callmekohei) - Twitter              https://twitter.com/callmekohei
callmekohei (callmekohei) · GitHub          https://github.com/callmekohei
GitHub - callmekohei/koffeeVBA: koffeeV ... https://github.com/callmekohei/koffeeVBA
...
```

.  

#####GetElements
```fsharp
open FSharp.Data
open ScrapingFs

let s =
    """
    <body>
        <div class="foo"/>
        <div class="bar"/>
        <div href ="abc/">
        <div href/>

        <a   class="foo"/>
        <a   class="bar"/>
        <a   href ="abc"/>
        <a   href/>
    </body>
    """

s
|> HtmlDocument.Parse
|> HtmlDocument.body
|> Sc.GetElements "div|a" "class" "foo|bar"
|> printfn "%A"

```
result
```
seq [<div class="foo" />; <div class="bar" />; <a class="foo" />; <a class="bar" />]
```
.  

#####GetElementsWithString
```fsharp
open FSharp.Data
open ScrapingFs

let s =
    """
    <body>
        <tr>
            <th>card</th>
            <td>
                <p>
                <strong>enable</strong> （VISA、MASTER、JCB、AMEX）
                </p>
            </td>
        </tr>

        <tr>
            <th>seats</th>
            <td>
                <p>
                    <strong>90seats</strong>
                </p>
            </td>
        </tr>
    </body>
    """

s
|> HtmlDocument.Parse
|> HtmlDocument.body
|> Sc.GetElementsWithString "tr" "card"
|> printfn "%A"
```
result
```
seq[<tr>
        <th>card</th>
        <td>
            <p>
                <strong>enable</strong> （VISA、MASTER、JCB、AMEX） 
            </p>
        </td>
    </tr>]
```

.  
#####GetAttributeValues
```fsharp
open FSharp.Data
open ScrapingFs

let s =
    """
    <frame src="callmekohei/menu.html" name="menu">
        <frame src="callmekohei/company.html" name="main" />
    </frame>
    """

s
|> HtmlDocument.Parse
|> HtmlDocument.elements
|> List.exactlyOne
|> Sc.GetAttributeValues "src" "frame[src]"
|> printfn "%A"

```
result
```
Some ["callmekohei/menu.html"; "callmekohei/company.html"]
```


.  
#####FetchHtmlsByNextLink
```fsharp
 +---------+         +---------+        +---------+
    page1      +-->     page2      +-->    page3
               |                   |
    next       |        next       |       next
    button   --+        button   --+       button
 +---------+         +---------+        +---------+
```


```fsharp
open ScrapingFs

let cssSelectorShowsNextPageLink = "div.c_pager_num > ul > li.c_pager_num-next > a"

url |> Sc.FetchHtmlsByNextLink "href" cssSelectorShowsNextPageLink
```
result ( image )
```
[
    page1
    page2
    ...
    page100
]

```


.  


Thanks
---
Japanese F#er  
visit: https://gitter.im/fsugjp/public  

License
---
This software is released under the MIT License.

