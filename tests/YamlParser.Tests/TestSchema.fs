﻿module TestSchema

open NUnit.Framework
open FsUnit
open TagResolution
open RepresentationGraph
open YamlParser
open YamlParser.Internals
open TestUtils

let TagResolveScalar schema s  =
    let nst = TagResolution.NonSpecific.NonSpecificTagQM
    let makeScalar s =
        let dl = DocumentLocation.Create 0 0
        ScalarNode(NodeData<string>.Create nst s (ParseInfo.Create dl dl))
    let scalar = makeScalar s
    TagResolutionInfo.Create ("?") ([]) (scalar) (scalar.Kind)
    |> schema 

[<Test>]
let ``Test JSON Schema Tags``() =
    JSON.NullGlobalTag.canonFn "null" |> should equal "null"

    JSON.BooleanGlobalTag.canonFn "true" |> should equal "true"
    JSON.BooleanGlobalTag.canonFn "false" |> should equal "false"

    [("1234","+1234"); ("-1234", "-1234"); ("0","+0"); ("-0" ,"+0")]
    |> List.iter(fun (i,e) -> JSON.IntegerGlobalTag.canonFn i |> should equal e)

    //  http://www.yaml.org/spec/1.2/spec.html#id2804318
    [
        ("0.", "+0.0e+001");("0.0", "+0.0e+001");("1.0", "+0.1e+001");("20.0","+0.2e+002");
        ("-0.0", "+0.0e+001");
        ("3.141500","+0.31415e+001");("100.01500","+0.100015e+003");("-1.","-0.1e+001")
        ("2.3e+4", "+0.23e+005")
    ]
    |> List.iter(fun (i,e) -> JSON.FloatGlobalTag.canonFn i |> should equal e)


[<Test>]
let ``Test JSON TagResolution``() =
    let tagResolveScalar = TagResolveScalar JSONSchema.TagResolution 

    tagResolveScalar "null" 
        |> Option.map(fun e -> (e.Uri |> should equal (JSON.NullGlobalTag.Uri)); e) |> Option.isSome |> should equal true

    ["false" ; "true"]
    |> List.iter(fun s ->
        tagResolveScalar s |> Option.map(fun e -> (e.Uri |> should equal (JSON.BooleanGlobalTag.Uri)); e) |> Option.isSome |> should equal true
    )

    ["0" ; "-0" ; "3" ;"-19" ]
    |> List.iter(fun s ->
        tagResolveScalar s |> Option.map(fun e -> (e.Uri |> should equal (JSON.IntegerGlobalTag.Uri)); e) |> Option.isSome |> should equal true
    )

    ["0." ; "-0.0" ;"12e+03";"-2E+05" ]
    |> List.iter(fun s ->
        tagResolveScalar s |> Option.map(fun e -> (e.Uri |> should equal (JSON.FloatGlobalTag.Uri)); e) |> Option.isSome |> should equal true
    )

    ["True"; "Null"; "0o7"; "0x3A"; "+12.3"]
    |> List.iter(fun s ->
        (fun () -> tagResolveScalar s |> ignore) |> should throw typeof<TagResolutionException>
    )



[<Test>]    //  http://www.yaml.org/spec/1.2/spec.html#id2805712
let ``Test Yaml Core Schema Tags``() =
    ["null"; "NULL"; "Null"; ""; "~" ]
    |> List.iter(fun i -> YamlCore.NullGlobalTag.canonFn i |> should equal "~")

    ["true";"True";"TRUE"]
    |> List.iter(fun i -> YamlCore.BooleanGlobalTag.canonFn i |> should equal "true")

    ["false";"False";"FALSE"]
    |> List.iter(fun i -> YamlCore.BooleanGlobalTag.canonFn i |> should equal "false")


    [("0","+0"); ("-0","+0"); ("0o7", "+7"); ("0x3A","+58"); ("-19" ,"-19")]
    |> List.iter(fun (i,e) -> YamlCore.IntegerGlobalTag.canonFn i |> should equal e)

    //  http://www.yaml.org/spec/1.2/spec.html#id2804318
    [
        ("0.", "+0.0e+001");("-0.0", "+0.0e+001");(".5", "+0.5e+000");("+12e+03","+0.12e+005");
        ("-2E+05", "-0.2e+006")
        (".inf","+.inf");("-.Inf","-.inf");("+.INF","+.inf")
        (".NAN", ".nan")
    ]
    |> List.iter(fun (i,e) -> YamlCore.FloatGlobalTag.canonFn i |> should equal e)


[<Test>]
let ``Test YamlCore TagResolution``() =
    let tagResolveScalar = TagResolveScalar YamlCoreSchema.TagResolution 

    ["null"; "NULL"; "Null"; ""; "~" ]
    |> List.iter(fun s ->
        tagResolveScalar s |> Option.map(fun e -> (e.Uri |> should equal (YamlCore.NullGlobalTag.Uri)); e) |> Option.isSome |> should equal true
    )

    ["true";"True";"TRUE";"false";"False";"FALSE"]
    |> List.iter(fun s ->
        tagResolveScalar s |> Option.map(fun e -> (e.Uri |> should equal (YamlCore.BooleanGlobalTag.Uri)); e) |> Option.isSome |> should equal true
    )

    ["0" ; "-0" ; "3" ;"-19"; "0o7"; "0x3A" ]
    |> List.iter(fun s ->
        tagResolveScalar s |> Option.map(fun e -> (e.Uri |> should equal (YamlCore.IntegerGlobalTag.Uri)); e) |> Option.isSome |> should equal true
    )

    [
        "0."; "-0.0"; ".5"; "+12e+03";
        "-2E+05"
        ".inf"; "-.Inf"; "+.INF"
        ".NAN"
    ]
    |> List.iter(fun s ->
        tagResolveScalar s |> Option.map(fun e -> (e.Uri |> should equal (YamlCore.FloatGlobalTag.Uri)); e) |> Option.isSome |> should equal true
    )

    ["not matched"; "2017-03-18"; "plain string"]
    |> List.iter(fun s ->
        tagResolveScalar s |> Option.map(fun e -> (e.Uri |> should equal (Failsafe.StringGlobalTag.Uri)); e) |> Option.isSome |> should equal true
    )


[<Test>]
let ``Test YamlExtended Canonical Integers - Simple``() =
    YamlExtended.IntegerGlobalTag.Canonical "5" |> should equal "+5"

[<Test>]
let ``Test YamlExtended Canonical Integers - Binary``() =
    YamlExtended.IntegerGlobalTag.Canonical "0b101" |> should equal "+5"

[<Test>]
let ``Test YamlExtended Canonical Integers - Octal``() =
    YamlExtended.IntegerGlobalTag.Canonical "017" |> should equal "+15"

[<Test>]
let ``Test YamlExtended Canonical Integers - Hexadecimal``() =
    YamlExtended.IntegerGlobalTag.Canonical "0x12" |> should equal "+18"

[<Test>]
let ``Test YamlExtended Canonical Integers - Sexagesimal``() =
    YamlExtended.IntegerGlobalTag.Canonical "190:20:30" |> should equal "+685230"


[<Test>]
let ``Test YamlExtended Canonical Floats - Simple``() =
    YamlExtended.FloatGlobalTag.Canonical "81.23" |> should equal "+0.8123e+002"
    float(YamlExtended.FloatGlobalTag.Canonical "81.23") |> should equal (float "81.23")

[<Test>]
let ``Test YamlExtended Canonical Floats - Shifted decimal``() =
    YamlExtended.FloatGlobalTag.Canonical "0.008123" |> should equal "+0.8123e-002"
    float(YamlExtended.FloatGlobalTag.Canonical "0.008123") |> should equal (float "0.008123")

[<Test>]
let ``Test YamlExtended Canonical Floats - Normalized decimal``() =
    YamlExtended.FloatGlobalTag.Canonical "1.008123" |> should equal "+0.1008123e+001"
    float(YamlExtended.FloatGlobalTag.Canonical "1.008123") |> should equal (float "1.008123")  

    YamlExtended.FloatGlobalTag.Canonical "0.8123" |> should equal "+0.8123e+000"
    float(YamlExtended.FloatGlobalTag.Canonical "0.8123") |> should equal (float "+0.8123e+000")

[<Test>]
let ``Test YamlExtended Canonical Floats - Sexagesimal``() =
    YamlExtended.FloatGlobalTag.Canonical "190:20:30.15" |> should equal "+0.68523015e+006"
    float(YamlExtended.FloatGlobalTag.Canonical "190:20:30.15") |> should equal (float "685230.15")

[<Test>]
let ``Test YamlExtended Canonical Floats - Specials``() =
    YamlExtended.FloatGlobalTag.Canonical ".inf"  |> should equal "+.inf"
    YamlExtended.FloatGlobalTag.Canonical "+.inf" |> should equal "+.inf"
    YamlExtended.FloatGlobalTag.Canonical "-.inf" |> should equal "-.inf"
    YamlExtended.FloatGlobalTag.Canonical ".nan"  |> should equal ".nan"


[<Test>]
let ``Test YamlExtended Null - Sunny Day``() =
    ["null"; "NULL"; "Null"]
    |> List.iter(fun input ->
        let node = YamlParseForSchema YamlExtendedSchema input
        Some [node] |> ToScalar      |> should equal input
        Some [node] |> ExtractTag |> should equal YamlExtended.NullGlobalTag.Uri
    )

[<Test>]
let ``Test YamlExtended Null - Various types - Sunny Day``() =
    let yml = YamlParseList "
# A document may be null.
---
---
# This mapping has four keys,
# one has a value.
empty:
canonical: ~
english: null
~: null key
---
# This sequence has five
# entries, two have values.
sparse:
  - ~
  - 2nd entry
  -
  - 4th entry
  - Null 
"
    yml.Length |> should equal 3

    let [y1;y2;y3]  = yml

    Some [y1] |> ExtractTag |> should equal  YamlExtended.NullGlobalTag.Uri

    ["empty"; "canonical"; "english"]
    |>  List.iter(fun k ->
        let ypath = sprintf "//{#'%s'}?" k
        let pth = YamlPath.Create ypath
        let root = y2 |> pth.Select 
        root |> Option.get |> List.length |> should equal 1
        root |> ExtractTag |> should equal  YamlExtended.NullGlobalTag.Uri
    )


[<Test>]
let ``Test YamlExtended Null - Rainy Day``() =
    [
        "\"null and more text\""
        "\"null\nand more text\""
    ]
    |> List.iter(fun input ->
        let node = YamlParseForSchema YamlExtendedSchema  input
        Some [node] |> ToScalar   |> should equal (input.Replace("\"", "").Replace("\n", " "))
        Some [node] |> ExtractTag |> should equal Failsafe.StringGlobalTag.Uri
    )

[<Test>]
let ``Test YamlExtended Bool - Sunny Day``() =
    [
        "yes";"no";"y";"n";"Y";"N";"Yes";"No";"YES";"NO";
        "true";"false";"True";"False";"TRUE";"FALSE";
        "on";"off";"On";"Off";"ON";"OFF"
    ]
    |> List.iter(fun input ->
        let node = YamlParseForSchema YamlExtendedSchema  input
        Some [node] |> ToScalar      |> should equal input
        Some [node] |> ExtractTag |> should equal YamlExtended.BooleanGlobalTag.Uri
    )

[<Test>]
let ``Test YamlExtended Bool - Rainy Day``() =
    [
        "\"true and more text\""
        "\"true\nand more text\""
    ]
    |> List.iter(fun input ->
        let node = YamlParseForSchema YamlExtendedSchema  input
        Some [node] |> ToScalar   |> should equal (input.Replace("\"", "").Replace("\n", " "))
        Some [node] |> ExtractTag |> should equal Failsafe.StringGlobalTag.Uri
    )

[<Test>]
let ``Test YamlExtended Intgeger - Sunny Day``() =
    [
        "1"; "100"; "1000"
        "-1"; "-100"; "-1000"
        "+1"; "+100"; "+1000"
        "0b1"; "0b100"; "0b1000"
        "-0b1"; "-0b100"; "-0b1000"
        "+0b1"; "+0b100"; "+0b1000"
        "0x1"; "0x100"; "1000"
        "-0x1"; "-0x100"; "-0x1000"
        "+0x1"; "+0x100"; "+0x1000"
        "190:20:30"
    ]
    |> List.iter(fun input ->
        let node = YamlParseForSchema YamlExtendedSchema  input
        Some [node] |> ToScalar   |> should equal input
        Some [node] |> ExtractTag |> should equal YamlExtended.IntegerGlobalTag.Uri
    )

[<Test>]
let ``Test YamlExtended Integer - Rainy Day``() =
    [
        "\"100 and more text\""
        "\"100\nand more text\""
    ]
    |> List.iter(fun input ->
        let node = YamlParseForSchema YamlExtendedSchema  input
        Some [node] |> ToScalar   |> should equal (input.Replace("\"", "").Replace("\n", " "))
        Some [node] |> ExtractTag |> should equal Failsafe.StringGlobalTag.Uri
    )


[<Test>]
let ``Test YamlExtended Timestamp - Sunny Day``() =
    [
        ("2001-12-15 2:59:43.10",           "2001-12-15T02:59:43.1000000Z")
        ("2002-12-14",                      "2002-12-14T00:00:00.0000000Z")
        ("2001-12-14t21:59:43.10-05:00",    "2001-12-15T02:59:43.1000000Z")
        ("2001-12-14 21:59:43.10 -5",       "2001-12-15T02:59:43.1000000Z")
    ]
    |> List.iter(fun (input, expect) ->
        YamlExtended.TimestampGlobalTag.Canonical input |> should equal expect
        let node = YamlParseForSchema YamlExtendedSchema input
        Some [node] |> ExtractTag |> should equal YamlExtended.TimestampGlobalTag.Uri
    )


[<Test>]
let ``Test YamlExtended value sunny day - value detected``() =
    let yml = YamlParseForSchema TagResolution.YamlExtendedSchema "
link with:
  - = : library1.dll
    version: 1.2"
    
    let ypath = "//{#'link with'}?/[]/{#'='}"
    let pth = YamlPath.Create ypath
    yml |> pth.Select |> ExtractTag |> should equal TagResolution.YamlExtended.ValueGlobalTag.Uri


[<Test>]
let ``Test YamlExtended merge sunny day``() =
    let yml = YamlParseForSchema TagResolution.YamlExtendedSchema "
---
- &CENTER { x: 1, ! y: 2 }
- &LEFT { x: 0, ! y: 2 }
- &BIG { r: 10 }
- &SMALL { r: 1 }

# All the following maps are equal:

- first: # Explicit keys
      x: 1
      ! y: 2
      r: 10
      label: center/big

  second: # Merge one map
      << : *CENTER
      r: 10
      label: center/big

  third: # Merge multiple maps
      << : [ *CENTER, *BIG ]
      label: center/big

  fourth: # Override
      << : [  *BIG, *LEFT, *SMALL ]  #  
      x: 1
      label: center/big
"
    let x = YamlPath.Create "//{#'x'}?"
    let y = YamlPath.Create "//{#'y'}?"
    let r = YamlPath.Create "//{#'r'}?"
    let label = YamlPath.Create "//{#'label'}?"

    ["first"; "second"; "third"; "fourth"]
    |>  List.iter(fun k ->
        let ypath = sprintf "//[]/{#'%s'}?" k
        let pth = YamlPath.Create ypath
        let root = yml |> pth.Select |> Option.get
        root |> List.length |> should equal 1
        
        root.Head |> x.Select |> ToScalar |> should equal "1"
        root.Head |> y.Select |> ToScalar |> should equal "2"
        root.Head |> r.Select |> ToScalar |> should equal "10"
        root.Head |> label.Select |> ToScalar |> should equal "center/big"
    )
[<Test>]
let ``Test YamlExtended merge rainy day - cannot use << in seq``() =
    let err = YamlParseForSchemaWithErrors TagResolution.YamlExtendedSchema "[1,2, << ]"

    err.Error.Length |> should be (greaterThan 0)
    err.Error |> List.filter(fun m -> m.Message.StartsWith("Merge tag or << cannot be used in the sequence")) |> List.length |> should equal 1

[<Test>]
let ``Test YamlExtended merge sunny day - can use << in seq with !``() =
    YamlParseForSchema TagResolution.YamlExtendedSchema "[1,2, ! << ]" |> ignore
    // no exception..

[<Test>]
let ``Test YamlExtended merge rainy day - cannot merge non-mappings in seq``() =
    let err = YamlParseForSchemaWithErrors TagResolution.YamlExtendedSchema "
---
- &CENTER { x: 1, ! y: 2 }
- &LEFT { x: 0, ! y: 2 }
- &BIG { r: 10 }
- &SMALL { r: 1 }

# All the following maps are equal:

- # Sequence contains scalar
    << : [ *CENTER, *BIG, invalid value ]
    label: center/big    
"

    err.Error.Length |> should be (greaterThan 0)
    err.Error |> List.filter(fun m -> m.Message.StartsWith("Incorrect Node type")) |> List.length |> should equal 1

