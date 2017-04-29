﻿module Chapter6Tests

(*
    Testing examples from chapter 6: http://www.yaml.org/spec/1.2/spec.html#Basic
*)

open NUnit.Framework
open FsUnit
open TestUtils
open YamlParser


[<Test(Description="http://www.yaml.org/spec/1.2/spec.html#id2777865")>]
let ``Example 6.1. Indentation Spaces``() =
    let yml = YamlParseList "
  # Leading comment line spaces are
   # neither content nor indentation.
    
Not indented:
 By one space: |
    By four
      spaces
 Flow style: [    # Leading spaces
   By two,        # in flow style
  Also by two,    # are neither
    Still by two   # content nor
    ]             # indentation.
"
    yml.Length |> should equal 1
    let yml = yml.Head

    let pth = YamlPath.Create (sprintf "//{#'Not indented'}?/{#'By one space'}?")
    yml |> pth.Select |> ToScalar |> should equal "By four\n  spaces\n"


[<Test(Description="http://www.yaml.org/spec/1.2/spec.html#id2778101")>]
let ``Example 6.2. Indentation Indicators``() =
    let yml = YamlParseList "
? a
: - b
  -  -  c
     - d"
    yml.Length |> should equal 1
    let yml = yml.Head

    let pth = YamlPath.Create (sprintf "//{#'a'}?/[]/[]/#'d'")
    yml |> pth.Select |> ToScalar |> should equal "d"


[<Test(Description="http://www.yaml.org/spec/1.2/spec.html#id2778394")>]
let ``Example 6.3. Separation Spaces``() =
    let yml = YamlParseList "
- foo:   bar
- - baz
  - baz
"
    yml.Length |> should equal 1
    let yml = yml.Head

    let pth = YamlPath.Create (sprintf "//[]/{#'foo'}?")
    yml |> pth.Select |> ToScalar |> should equal "bar"


[<Test(Description="http://www.yaml.org/spec/1.2/spec.html#id2778720")>]
let ``Example 6.4. Line Prefixes``() =
    let yml = YamlParseList "
plain: text
  lines
quoted: \"text
  \tlines\"
block: |
  text
   \tlines
"
    yml.Length |> should equal 1
    let yml = yml.Head

    [
        ("plain", "text lines")
        ("quoted", "text lines")
        ("block", "text\n \tlines\n")
    ]
    |> List.iter(fun (k,v) ->
        let p = YamlPath.Create (sprintf "//{#'%s'}?" k)
        yml |> p.Select |> ToScalar |> should equal v
    )


[<Test(Description="http://www.yaml.org/spec/1.2/spec.html#id2778971")>]
let ``Example 6.5. Empty Lines``() =
    let yml = YamlParseList "
Folding:
  \"Empty line
   \t
  as a line feed\"
Chomping: |
  Clipped empty lines
 
"
    yml.Length |> should equal 1
    let yml = yml.Head

    [
        ("Folding", "Empty line\nas a line feed")
        ("Chomping", "Clipped empty lines\n")
    ]
    |> List.iter(fun (k,v) ->
        let p = YamlPath.Create (sprintf "//{#'%s'}?" k)
        yml |> p.Select |> ToScalar |> should equal v
    )


[<Test(Description="http://www.yaml.org/spec/1.2/spec.html#id2779289")>]
let ``Example 6.6. Line Folding``() =
    let yml = YamlParseList "
>-
  trimmed
  
 

  as
  space
"
    yml.Length |> should equal 1

    yml |> Some |> ToScalar |> should equal "trimmed\n\n\nas space"


[<Test(Description="http://www.yaml.org/spec/1.2/spec.html#id2779603")>]
let ``Example 6.7. Block Folding``() =
    let yml = YamlParseList ">\n  foo \n \n  \t bar\n\n  baz\n"
    yml.Length |> should equal 1

    yml |> Some |> ToScalar |> should equal "foo \n\n\t bar\n\nbaz\n"


[<Test(Description="http://www.yaml.org/spec/1.2/spec.html#id2779950")>]
let ``Example 6.8. Flow Folding``() =
    let yml = YamlParseList "\"\n  foo \n \n  \t bar\n\n  baz\n\""
    yml.Length |> should equal 1

    yml |> Some |> ToScalar |> should equal " foo\nbar\nbaz "


[<Test(Description="http://www.yaml.org/spec/1.2/spec.html#id2780342")>]
let ``Example 6.9. Separated Comment``() =
    let yml = YamlParseList "
key:    # Comment
  value"
    yml.Length |> should equal 1
    let yml = yml.Head

    [("key", "value")]
    |> List.iter(fun (k,v) ->
        let p = YamlPath.Create (sprintf "//{#'%s'}?" k)
        yml |> p.Select |> ToScalar |> should equal v
    )


[<Test(Description="http://www.yaml.org/spec/1.2/spec.html#id2780544")>]
let ``Example 6.10. Comment Lines``() =
    let yml = YamlParseList "
  # Comment
   

"
    yml.Length |> should equal 0


[<Test(Description="http://www.yaml.org/spec/1.2/spec.html#id2780696")>]
let ``Example 6.11. Multi-Line Comments``() =
    let yml = YamlParseList "
key:    # Comment
        # lines
  value

"
    yml.Length |> should equal 1
    let yml = yml.Head

    [("key", "value")]
    |> List.iter(fun (k,v) ->
        let p = YamlPath.Create (sprintf "//{#'%s'}?" k)
        yml |> p.Select |> ToScalar |> should equal v
    )


[<Test(Description="http://www.yaml.org/spec/1.2/spec.html#id2780696")>]
let ``Example 6.12. Separation Spaces``() =
    let yml = YamlParseList "{ first: Sammy, last: Sosa }:\n# Statistics:\n  hr:  # Home runs\n     65\n  avg: # Average\n   0.278"

    yml.Length |> should equal 1
    let yml = yml.Head

    [("first", "Sammy"); ("last", "Sosa")]
    |> List.iter(fun (k,v) ->
        let p = YamlPath.Create (sprintf "//{}/{#'%s'}?" k)
        yml |> p.Select |> ToScalar |> should equal v
    )

    [("hr", "65");("avg", "0.278")]
    |> List.iter(fun (k,v) ->
        let p = YamlPath.Create (sprintf "//{}?/{#'%s'}?" k)
        yml |> p.Select |> ToScalar |> should equal v
    )

[<Ignore "Check warning">]
[<Test(Description="http://www.yaml.org/spec/1.2/spec.html#id2781445")>]
let ``Example 6.13. Reserved Directives``() =
    let yml = YamlParseList "
%FOO  bar baz # Should be ignored
              # with a warning.
--- \"foo\"
"
    yml.Length |> should equal 1

    yml |> Some |> ToScalar |> should equal "foo"


[<Ignore "Check warning">]
[<Test(Description="http://www.yaml.org/spec/1.2/spec.html#id2781929")>]
let ``Example 6.14. “YAML” directive``() =
    let yml = YamlParseList "
%YAML 1.3 # Attempt parsing
           # with a warning
---
\"foo\""
    yml.Length |> should equal 1

    yml |> Some |> ToScalar |> should equal "foo"


//[<Ignore "Check warning">]
[<Test(Description="http://www.yaml.org/spec/1.2/spec.html#id2782032")>]
let ``Example 6.15. Invalid Repeated YAML directive``() =
    let yml = YamlParseList "
%YAML 1.2
%YAML 1.1
foo"
    yml.Length |> should equal 0
    // check error


[<Test(Description="http://www.yaml.org/spec/1.2/spec.html#id2782252")>]
let ``Example 6.16. “TAG” directive``() =
    let yml = YamlParseList "
%TAG !yaml! tag:yaml.org,2002:
---
!yaml!str \"foo\"
"
    yml.Length |> should equal 1
    yml |> Some |> ToScalar |> should equal "foo"


[<Ignore "Check error">]
[<Test(Description="http://www.yaml.org/spec/1.2/spec.html#id2782400")>]
let ``Example 6.17. Invalid Repeated TAG directive``() =
    let yml = YamlParseList "
%TAG ! !foo
%TAG ! !foo
bar"
    yml.Length |> should equal 0
    // check error


[<Test(Description="http://www.yaml.org/spec/1.2/spec.html#id2782728")>]
let ``Example 6.18. Primary Tag Handle``() =
    let yml = YamlParseList "
# Private
!foo \"bar\"
...
# Global
%TAG ! tag:example.com,2000:app/
---
!foo \"bar\"
"
    yml.Length |> should equal 2
    let yml = yml |> List.last 

    let p = YamlPath.Create (sprintf "//#'bar'")
    yml |> p.Select |> ToScalar |> should equal "bar"


//[<Ignore "Should support local tags">]
[<Test(Description="http://www.yaml.org/spec/1.2/spec.html#id2782940")>]
let ``Example 6.19. Secondary Tag Handle``() =
    let yml = YamlParseList "
%TAG !! tag:example.com,2000:app/
---
!!int 1 - 3 # Interval, not integer
"
    yml.Length |> should equal 1
    yml |> Some |> ToScalar |> should equal "1 - 3"
    yml |> Some |> ToScalarTag |> should equal "tag:example.com,2000:app/int"


[<Ignore "Should support local tags">]
[<Test(Description="http://www.yaml.org/spec/1.2/spec.html#id2783195")>]
let ``Example 6.20. Tag Handles``() =
    let yml = YamlParseList "
%TAG !e! tag:example.com,2000:app/
---
!e!foo \"bar\"
"
    yml.Length |> should equal 1
    yml |> Some |> ToScalar |> should equal "bar"
    yml |> Some |> ToScalarTag |> should equal "tag:example.com,2000:app/foo"


//[<Ignore "Should support local tags">]
[<Test(Description="http://www.yaml.org/spec/1.2/spec.html#id2783499")>]
let ``Example 6.21. Local Tag Prefix``() =
    let yml = YamlParseList "
%TAG !m! !my-
--- # Bulb here
!m!light fluorescent
...
%TAG !m! !my-
--- # Color here
!m!light green
"
    yml.Length |> should equal 2

    let [y1;y2] = yml

    [y1] |> Some |> ToScalar |> should equal "fluorescent"
    [y1] |> Some |> ToScalarTag |> should equal "my-light"

    [y2] |> Some |> ToScalar |> should equal "green"
    [y2] |> Some |> ToScalarTag |> should equal "my-light"


//[<Ignore "Should support local tags">]
[<Test(Description="http://www.yaml.org/spec/1.2/spec.html#id2783726")>]
let ``Example 6.22. Global Tag Prefix``() =
    let yml = YamlParseList "

%TAG !e! tag:example.com,2000:app/
---
- !e!foo \"bar\"
"
    yml.Length |> should equal 1
    yml |> Some |> ToScalar |> should equal "bar"
    yml |> Some |> ToScalarTag |> should equal "tag:example.com,2000:app/foo"



[<Test(Description="http://www.yaml.org/spec/1.2/spec.html#id2783940")>]
let ``Example 6.23. Node Properties``() =
    let yml = YamlParseList "
!!str &a1 \"foo\":
  !!str bar
&a2 baz : *a1
"
    yml.Length |> should equal 1
    let yml = yml.Head

    [("foo", "bar"); ("baz", "foo")]
    |> List.iter(fun (k,v) ->
        let p = YamlPath.Create (sprintf "//{#'%s'}?" k)
        yml |> p.Select |> ToScalar |> should equal v
    )

[<Ignore "Check tags">]
[<Test(Description="http://www.yaml.org/spec/1.2/spec.html#id2784370")>]
let ``Example 6.24. Verbatim Tags``() =
    let yml = YamlParseList "
!<tag:yaml.org,2002:str> foo :
  !<!bar> baz
"
    yml.Length |> should equal 1
    let yml = yml.Head

    [("foo", "baz")]
    |> List.iter(fun (k,v) ->
        let p = YamlPath.Create (sprintf "//{#'%s'}?" k)
        yml |> p.Select |> ToScalar |> should equal v
    )

[<Ignore "Check error message">]
[<Test(Description="http://www.yaml.org/spec/1.2/spec.html#id2784444")>]
let ``Example 6.25. Invalid Verbatim Tags``() =
    let yml = YamlParseList "
- !<!> foo
- !<$:?> bar
"
    yml.Length |> should equal 0


[<Ignore "Check tags">]
[<Test(Description="http://www.yaml.org/spec/1.2/spec.html#id2785009")>]
let ``Example 6.26. Tag Shorthands``() =
    let yml = YamlParseList "
%TAG !e! tag:example.com,2000:app/
---
- !local foo
- !!str bar
- !e!tag%21 baz
"
    yml.Length |> should equal 1
    let yml = yml.Head

    ["foo"; "bar"; "baz"]
    |> List.iter(fun v ->
        let p = YamlPath.Create (sprintf "//[]/#'%s'" v)
        yml |> p.Select |> ToScalar |> should equal v
    )


//[<Ignore "Check error message">]
[<Test(Description="http://www.yaml.org/spec/1.2/spec.html#id2785092")>]
let ``Example 6.27. Invalid Tag Shorthands``() =
    let yml = YamlParseList "
%TAG !e! tag:example,2000:app/
---
- !e! foo
- !h!bar baz
"
    yml.Length |> should equal 0


[<Test(Description="http://www.yaml.org/spec/1.2/spec.html#id2785512")>]
let ``Example 6.28. Non-Specific Tags``() =
    // values have been changed for test-evaluation purposes
    let yml = YamlParseList "
# Assuming conventional resolution:
- \"1\"
- 12
- ! 123
"
    yml.Length |> should equal 1
    let yml = yml.Head

    [
        ("tag:yaml.org,2002:str", "1")
        ("tag:yaml.org,2002:int", "12")
        ("tag:yaml.org,2002:str", "123")
    ]
    |> List.iter(fun (t,v) ->
        let p = YamlPath.Create (sprintf "//[]/#'%s'" v)
        yml |> p.Select |> ToScalarTag |> should equal t
        yml |> p.Select |> ToScalar |> should equal v
    )


[<Test(Description="http://www.yaml.org/spec/1.2/spec.html#id2785977")>]
let ``Example 6.29. Node Anchors``() =
    let yml = YamlParseList "
First occurrence: &anchor Value
Second occurrence: *anchor
"
    yml.Length |> should equal 1
    let yml = yml.Head

    [("First occurrence", "Value"); ("Second occurrence", "Value")]
    |> List.iter(fun (k,v) ->
        let p = YamlPath.Create (sprintf "//{#'%s'}?" k)
        yml |> p.Select |> ToScalar |> should equal v
    )

