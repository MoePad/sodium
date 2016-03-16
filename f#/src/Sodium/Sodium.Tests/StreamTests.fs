﻿namespace Sodium.Tests

open NUnit.Framework
open Sodium
open System
open System.Collections.Generic

[<TestFixture>]
type StreamTests() =
    
    let flip f x y = f y x

    [<Test>]
    member __.``Test Stream Send``() =
        use s = Stream.sink ()
        let out = List<_>()
        (
            use _l = (s |> Stream.listen out.Add)
            s.Send 5
        )
        CollectionAssert.AreEqual([5], out)
        s.Send 6
        CollectionAssert.AreEqual([5], out)

    [<Test>]
    member __.``Test Map``() =
        use s = Stream.sink ()
        use m = s |> Stream.map ((+) 2 >> string)
        let out = List<_>()
        (
            use _l = (m |> Stream.listen out.Add)
            s.Send 5
            s.Send 3
        )
        CollectionAssert.AreEqual(["7"; "5"], out)

    [<Test>]
    member __.``Test OrElse Non-Simultaneous``() =
        use s1 = Stream.sink ()
        use s2 = Stream.sink ()
        let out = List<_>()
        (
            use _l = (s1 |> Stream.orElse s2 |> Stream.listen out.Add)
            s1.Send 7
            s2.Send 9
            s1.Send 8
        )
        CollectionAssert.AreEqual([7;9;8], out)

    [<Test>]
    member __.``Test OrElse Simultaneous 1``() =
        use s1 = Stream.sinkWithCoalesce (fun _ r -> r)
        use s2 = Stream.sinkWithCoalesce (fun _ r -> r)
        let out = List<_>()
        (
            use _l = (s2 |> Stream.orElse s1 |> Stream.listen out.Add)
            Transaction.Run (fun () -> s1.Send 7; s2.Send 60)
            Transaction.Run (fun () -> s1.Send 9)
            Transaction.Run (fun () -> s1.Send 7; s1.Send 60; s2.Send 8; s2.Send 90)
            Transaction.Run (fun () -> s2.Send 8; s2.Send 90; s1.Send 7; s1.Send 60)
            Transaction.Run (fun () -> s2.Send 8; s1.Send 7; s2.Send 90; s1.Send 60)
        )
        CollectionAssert.AreEqual([60;9;90;90;90], out)

    [<Test>]
    member __.``Test OrElse Simultaneous 2``() =
        use s = Stream.sink ()
        use s2 = s |> Stream.map ((*) 2)
        let out = List<_>()
        (
            use _l = (s |> Stream.orElse s2 |> Stream.listen out.Add)
            s.Send 7
            s.Send 9
        )
        CollectionAssert.AreEqual([7;9], out)

    [<Test>]
    member __.``Test OrElse Left Bias``() =
        use s = Stream.sink ()
        use s2 = s |> Stream.map ((*) 2)
        let out = List<_>()
        (
            use _l = (s2 |> Stream.orElse s |> Stream.listen out.Add)
            s.Send 7
            s.Send 9
        )
        CollectionAssert.AreEqual([14;18], out)

    [<Test>]
    member __.``Test Merge Non-Simultaneous``() =
        use s1 = Stream.sink ()
        use s2 = Stream.sink ()
        let out = List<_>()
        (
            use _l = (s1 |> Stream.merge (+) s2 |> Stream.listen out.Add)
            s1.Send 7
            s2.Send 9
            s1.Send 8
        )
        CollectionAssert.AreEqual([7;9;8], out)

    [<Test>]
    member __.``Test Merge Simultaneous``() =
        use s = Stream.sink ()
        use s2 = s |> Stream.map ((*) 2)
        let out = List<_>()
        (
            use _l = (s |> Stream.merge (+) s2 |> Stream.listen out.Add)
            s.Send 7
            s.Send 9
        )
        CollectionAssert.AreEqual([21;27], out)

    [<Test>]
    member __.``Test Coalesce 1``() =
        use s = Stream.sinkWithCoalesce (+)
        let out = List<_>()
        (
            use _l = (s |> Stream.listen out.Add)
            Transaction.Run (fun () -> s.Send 2)
            Transaction.Run (fun () -> s.Send 8; s.Send 40)
        )
        CollectionAssert.AreEqual([2;48], out)

    [<Test>]
    member __.``Test Coalesce 2``() =
        use s = Stream.sinkWithCoalesce (+)
        let out = List<_>()
        (
            use _l = (s |> Stream.listen out.Add)
            Transaction.Run (fun () -> for i = 1 to 5 do s.Send i)
            Transaction.Run (fun () -> for i = 6 to 10 do s.Send i)
        )
        CollectionAssert.AreEqual([15;40], out)

    [<Test>]
    member __.``Test Filter``() =
        use s = Stream.sink ()
        let out = List<_>()
        (
            use _l = (s |> Stream.filter Char.IsUpper |> Stream.listen out.Add)
            s.Send 'H'
            s.Send 'o'
            s.Send 'I'
        )
        CollectionAssert.AreEqual(['H';'I'], out)

    [<Test>]
    member __.``Test Filter Option``() =
        use s = Stream.sink ()
        let out = List<_>()
        (
            use _l = (s |> Stream.filterOption |> Stream.listen out.Add)
            s.Send (Option.Some "tomato")
            s.Send Option.None
            s.Send (Option.Some "peach")
            s.Send Option.None
            s.Send (Option.Some "pear")
        )
        CollectionAssert.AreEqual(["tomato";"peach";"pear"], out)

    [<Test>]
    member __.``Test Loop Stream``() =
        use sa = Stream.sink ()
        let (_, sc) = Stream.loop (fun sb ->
            let scLocal = sa |> Stream.map (flip (%) 10) |> Stream.merge (*) sb
            let sbOut = sa |> Stream.map (flip (/) 10) |> Stream.filter ((<>) 0)
            (sbOut, scLocal))
        let out = List<_>()
        (
            use _l = (sc |> Stream.listen out.Add)
            sa.Send 2
            sa.Send 52
        )
        CollectionAssert.AreEqual([2;10], out)

    [<Test>]
    member __.``Test Gate``() =
        use sc = Stream.sink ()
        use cGate = Cell.sink true
        let out = List<_>()
        (
            use _l = (sc |> Stream.gate cGate |> Stream.listen out.Add)
            sc.Send 'H'
            cGate.Send false
            sc.Send 'O'
            cGate.Send true
            sc.Send 'I'
        )
        CollectionAssert.AreEqual(['H';'I'], out)

    [<Test>]
    member __.``Test Calm``() =
        use s = Stream.sink ()
        let out = List<_>()
        (
            use _l = (s |> Stream.calm |> Stream.listen out.Add)
            s.Send 2
            s.Send 2
            s.Send 2
            s.Send 4
            s.Send 2
            s.Send 4
            s.Send 4
            s.Send 2
            s.Send 2
            s.Send 2
            s.Send 2
            s.Send 2
            s.Send 4
            s.Send 2
            s.Send 4
            s.Send 4
            s.Send 2
            s.Send 2
            s.Send 2
            s.Send 2
            s.Send 2
            s.Send 4
            s.Send 2
            s.Send 4
            s.Send 4
            s.Send 2
            s.Send 2
            s.Send 2
            s.Send 2
            s.Send 2
            s.Send 4
            s.Send 2
            s.Send 4
            s.Send 4
            s.Send 2
            s.Send 2
            s.Send 2
            s.Send 2
            s.Send 2
            s.Send 4
            s.Send 2
            s.Send 4
            s.Send 4
            s.Send 2
            s.Send 2
        )
        CollectionAssert.AreEqual([2;4;2;4;2;4;2;4;2;4;2;4;2;4;2;4;2;4;2;4;2], out)
