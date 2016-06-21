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

How to use
---
#####googleSearch
```fsharp
open scrapingFs.scrapingFs

googleSearch "callmekohei"
|> justify
|> List.iter ( fun [a;b] -> printfn "%s\t%s" a b )
```
result
```text
kohei (@callmekohei) - Twitter                                   	https://twitter.com/callmekohei                            
callmekohei (callmekohei) · GitHub                               	https://github.com/callmekohei                             
GitHub - callmekohei/koffeeVBA: koffeeVBA is free VBA Library ...	https://github.com/callmekohei/koffeeVBA                   
...
```
#####dynamicLink
```fsharp
open scrapingFs.scrapingFs
open FSharp.Data
open System.Text.RegularExpressions

let s =
    """
    <body>
        <div class="prevNext f_right">
        <!-- AJAX link -->
            <span>1</span>
            <a href="#" onclick="ExecuteAjaxRequest('./articleList', 'account=12345', 'DispListArticle'); return false;">
                <span class="last">2</span>
            </a>
            <a href="#" onclick="ExecuteAjaxRequest('./articleList', 'account=callmekohei', 'DispListArticle'); return false;">
                <span class="next last">Next</span>
            </a>
        </div>
    </body>
    """
HtmlDocument.Parse s
|> HtmlDocument.body
|> dynamicLink "onclick" "div > a" "span.next.last"
|> fun s -> Regex.Match ( s, "account.*(?=',)" )
|> printfn "%A"
```
result
```
account=callmekohei
```
Thanks
---
Japanese F#er.  
visit: https://gitter.im/fsugjp/public  

License
---
This software is released under the MIT License.

