module Legivel.Tests

open NUnit.Framework
open FsUnit
open Legivel.Attributes
open Legivel.Serialization
open System


type SimpleRecord = {
    Name   : string
    Age    : int
}


let DeserializeSuccess<'tp> yml = 
    let r = Deserialize<'tp> yml
    r
    |> List.head
    |>  function
        |   Succes s -> s.Data
        |   Error e -> failwith "Unexpected error"
        
let DeserializeError<'tp> yml = 
    Deserialize<'tp> yml
    |> List.head
    |>  function
        |   Succes _ -> failwith "Unexpected success"
        |   Error e -> e

[<Test>]
let ``Deserialize - check docs - time to string - Sunny Day`` () =
    let yml = "20:03:20"
    let res = DeserializeSuccess<string> yml 
    res |> should equal "20:03:20"



[<Test>]
let ``Deserialize - int - Sunny Day`` () =
    let yml = "43"
    let res = DeserializeSuccess<int> yml 
    res |> should equal 43

[<Test>]
let ``Deserialize - float - Sunny Day`` () =
    let yml = "43.5"
    let res = DeserializeSuccess<float> yml 
    res |> should equal 43.5

[<Test>]
let ``Deserialize - bool - Sunny Day`` () =
    let yml = "true"
    let res = DeserializeSuccess<bool> yml 
    res |> should equal true


[<Test>]
let ``Deserialize - DateTime - Sunny Day`` () =
    let yml = "2014-09-12"
    let res = DeserializeSuccess<DateTime> yml 
    res |> should equal (DateTime(2014, 09, 12))


[<Test>]
let ``Deserialize - Naked Record Fields - Sunny Day`` () =
    let yml = "{ Name: 'Frank', Age: 43 }"
    let res = DeserializeSuccess<SimpleRecord> yml 
    res.Name    |> should equal "Frank"
    res.Age     |> should equal 43


[<Ignore("Acceptance of default values must be a configurable option")>]
[<Test>]
let ``Deserialize - Naked Record Fields - default values - Sunny Day`` () =
    let yml = "{ Name: 'Frank' }"
    let res = DeserializeSuccess<SimpleRecord> yml 
    res.Name    |> should equal "Frank"
    res.Age     |> should equal 0


type SimpleAnnotatedRecord = {
    [<YamlField(Name = "name")>] Name   : string
    [<YamlField(Name = "age")>] Age     : int
}

[<Test>]
let ``Deserialize - Annotated Record Fields - Sunny Day`` () =
    let yml = "{ name: 'Frank', age: 43 }"
    let res = DeserializeSuccess<SimpleAnnotatedRecord> yml 
    res.Name    |> should equal "Frank"
    res.Age     |> should equal 43


[<Ignore("Acceptance of default values must be a configurable option")>]
[<Test>]
let ``Deserialize - Annotated Record Fields - default values - Sunny Day`` () =
    let yml = "{ name: 'Frank' }"
    let res = DeserializeSuccess<SimpleAnnotatedRecord> yml 
    res.Name    |> should equal "Frank"
    res.Age     |> should equal 0


type OptionalField = {
    Name   : string
    Age    : int option
}

[<Test>]
let ``Deserialize - Optional Record Field - present - Sunny Day`` () =
    let yml = "{ Name: 'Frank', Age: 43 }"
    let res = DeserializeSuccess<OptionalField> yml 
    res.Name    |> should equal "Frank"
    res.Age     |> should equal (Some 43)


[<Test>]
let ``Deserialize - Optional Record Fields - missing - Sunny Day`` () =
    let yml = "{ Name: 'Frank' }"
    let res = DeserializeSuccess<OptionalField> yml 
    res.Name    |> should equal "Frank"
    res.Age     |> should equal None


[<Test>]
let ``Deserialize - Optional Record Fields - null value - Sunny Day`` () =
    let yml = "{ Name: 'Frank', Age: null }"
    let res = DeserializeSuccess<OptionalField> yml 
    res.Name    |> should equal "Frank"
    res.Age     |> should equal None


type NestedRecord = {
        Street      : string
        HouseNumber : int
    }

type ContainingNested = {
        Name    : string
        Address : NestedRecord
    }


[<Test>]
let ``Deserialize - Nested Record - Sunny Day`` () =
    let yml = "{ Name: 'Frank', Address: { Street: 'Rosegarden', HouseNumber: 5 } }"
    let res = DeserializeSuccess<ContainingNested> yml 
    res.Name    |> should equal "Frank"
    res.Address.Street |> should equal "Rosegarden"
    res.Address.HouseNumber |> should equal 5


type ContainingOptionalNested = {
        Name    : string
        Address : NestedRecord option
    }

[<Test>]
let ``Deserialize - Nested Optional Record - present - Sunny Day`` () =
    let yml = "{ Name: 'Frank', Address: { Street: 'Rosegarden', HouseNumber: 5 } }"
    let res = DeserializeSuccess<ContainingOptionalNested> yml 
    res.Name    |> should equal "Frank"
    res.Address |> Option.get |> fun a -> a.Street |> should equal "Rosegarden"
    res.Address |> Option.get |> fun a -> a.HouseNumber |> should equal 5

[<Test>]
let ``Deserialize - Nested Optional Record - missing - Sunny Day`` () =
    let yml = "{ Name: 'Frank' }"
    let res = DeserializeSuccess<ContainingOptionalNested> yml 
    res.Name    |> should equal "Frank"
    res.Address |> should equal None

[<Test>]
let ``Deserialize - List - Sunny Day`` () =
    let yml = "[1, 1, 3, 5, 8, 9]" // not a pattern
    let res = DeserializeSuccess<int list> yml 
    res |> should equal [1; 1; 3; 5; 8; 9]

[<Test>]
let ``Deserialize - List with type mismatch scalar element - Rainy Day`` () =
    let yml = "[1, 1, a]" 
    let res = DeserializeError<int list> yml 
    res.Error.Length |> should be (greaterThan 0)
    res.Error.Head |> fun m -> m.Message.StartsWith("Type mismatch") |> should equal true

[<Test>]
let ``Deserialize - List with type mismatch map element - Rainy Day`` () =
    let yml = "[1, 1, {1 : 2}]"
    let res = DeserializeError<int list> yml 
    res.Error.Length |> should be (greaterThan 0)
    res.Error.Head |> fun m -> m.Message.StartsWith("Type mismatch") |> should equal true

[<Test>]
let ``Deserialize - List with type mismatch seq element - Rainy Day`` () =
    let yml = "[1, 1, [1, 2]]"
    let res = DeserializeError<int list> yml 
    res.Error.Length |> should be (greaterThan 0)
    res.Error.Head |> fun m -> m.Message.StartsWith("Type mismatch") |> should equal true

[<Test>]
let ``Deserialize - List with general type mismatch, given scalar for seq - Rainy Day`` () =
    let yml = "wrongtype"
    let res = DeserializeError<int list> yml 
    res.Error.Length |> should be (greaterThan 0)
    res.Error.Head |> fun m -> m.Message.StartsWith("Expecting a Sequence Node") |> should equal true

[<Test>]
let ``Deserialize - List with general type mismatch, given map for seq - Rainy Day`` () =
    let yml = "{ a : 1}"
    let res = DeserializeError<int list> yml 
    res.Error.Length |> should be (greaterThan 0)
    res.Error.Head |> fun m -> m.Message.StartsWith("Expecting a Sequence Node") |> should equal true


type ListField = {
    Name   : string
    Scores : int list
}

[<Test>]
let ``Deserialize - Record with List - Sunny Day`` () =
    let yml = "{ Name: 'Frank', Scores: [1, 1, 3, 5, 8, 9]}"
    let res = DeserializeSuccess<ListField> yml 
    res.Name    |> should equal "Frank"
    res.Scores  |> should equal [1; 1; 3; 5; 8; 9]


type UnionCaseNoData =
    |   Zero
    |   One
    |   Two
    |   Three

[<Test>]
let ``Deserialize - Discriminated Union Simple - Sunny Day`` () =
    let yml = "One"
    let res = DeserializeSuccess<UnionCaseNoData> yml 
    res |> should equal UnionCaseNoData.One


[<Test>]
let ``Deserialize - Discriminated Union - Bad value - Rainy Day`` () =
    let yml = "Four"    //  does not exist
    let res = DeserializeError<UnionCaseNoData> yml 
    res.Error.Length |> should be (greaterThan 0)
    res.Error.Head |> fun m -> m.Message.StartsWith("Union case 'Four' not availble in type") |> should equal true




type UCData1 = {
    Name : string
    Age  : int
}


[<YamlField("TypeOf")>]
type UnionCaseWithData =
    |   One of UCData1
    |   [<YamlValue("two")>] Two of UCData1

[<Test>]
let ``Deserialize - Discriminated Union With Data - Sunny Day`` () =
    let yml = "
        Name: 'Frank'
        Age:  43
        TypeOf : One
    "
    let res = DeserializeSuccess<UnionCaseWithData> yml
    match res with
    |   One d -> d.Name |> should equal "Frank"
                 d.Age  |> should equal 43
    | _ -> failwith "Incorrect value!"


[<Test>]
let ``Deserialize - Discriminated Union With Bad Data - Rainy Day`` () =
    let yml = "
        Bad:  Value
        TypeOf : One
    "
    let res = DeserializeError<UnionCaseWithData> yml
    res.Error.Length |> should be (greaterThan 0)
    res.Error.Head |> fun m -> m.Message.StartsWith("Missing value for field") |> should equal true


[<Test>]
let ``Deserialize - Discriminated Union-Alias With Data - Sunny Day`` () =
    let yml = "
        Name: 'Frank'
        Age:  43
        TypeOf : two    # alias
    "
    let res = DeserializeSuccess<UnionCaseWithData> yml
    match res with
    |   Two d -> d.Name |> should equal "Frank"
                 d.Age  |> should equal 43
    | _ -> failwith "Incorrect value!"


type UnionCaseEnum =
    |   Zero=0      
    |   One=1
    |   [<YamlValue("two")>] Two=2
    |   Three=3

[<Test>]
let ``Deserialize - Discriminated Union Enum Simple - Sunny Day`` () =
    let yml = "One"
    let res = DeserializeSuccess<UnionCaseEnum> yml
    res |> should equal UnionCaseEnum.One

[<Test>]
let ``Deserialize - Discriminated Union Enum-Alias - Sunny Day`` () =
    let yml = "two # alias"
    let res = DeserializeSuccess<UnionCaseEnum> yml
    res |> should equal UnionCaseEnum.Two


[<Test>]
let ``Deserialize - Discriminated Union Enum - Bad value - Rainy Day`` () =
    let yml = "Four"    //  does not exist
    let res = DeserializeError<UnionCaseEnum> yml 
    res.Error.Length |> should be (greaterThan 0)
    res.Error.Head |> fun m -> m.Message.StartsWith("Union case 'Four' not availble in type") |> should equal true


[<Test>]
let ``Deserialize - Mapping blockstyle - Sunny Day`` () =
    let yml = "{a : b, c : d}"
    let res = DeserializeSuccess<Map<string,string>> yml
    res.["a"] |> should equal "b"
    res.["c"] |> should equal "d"


[<Test>]
let ``Deserialize - Mapping flowstyle - Sunny Day`` () =
    let yml = "
    a : b
    c : d"
    let res = DeserializeSuccess<Map<string,string>> yml
    res.["a"] |> should equal "b"
    res.["c"] |> should equal "d"


[<Test>]
let ``Deserialize - Mapping with DU enum - Sunny Day`` () =
    let yml = "{a : Zero, c : two}"
    let res = DeserializeSuccess<Map<string,UnionCaseEnum>> yml
    res.["a"] |> should equal UnionCaseEnum.Zero
    res.["c"] |> should equal UnionCaseEnum.Two


[<Test>]
let ``Deserialize - Mapping with DU and record - Sunny Day`` () =
    let yml = "
        Zero : { Name: 'Frank', Age:  43 }
        two  : { Name: 'Rosi', Age:  45 } "

    let res = DeserializeSuccess<Map<UnionCaseEnum,UCData1>> yml
    res.[UnionCaseEnum.Zero] |> should equal {UCData1.Name = "Frank"; Age = 43 }
    res.[UnionCaseEnum.Two] |> should equal {UCData1.Name = "Rosi"; Age = 45 }


type RecursiveType = {
        Data : int
        Next : RecursiveType option
    }

[<Test>]
let ``Deserialize - Recursive Type - Sunny Day`` () =
    let yml = "
Data: 1
Next:
    Data: 2
"
    let res = DeserializeSuccess<RecursiveType> yml
    res.Data            |> should equal 1
    res.Next.Value.Data |> should equal 2
    res.Next.Value.Next |> should equal None

