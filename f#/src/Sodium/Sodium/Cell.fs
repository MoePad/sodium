﻿namespace Sodium

module Cell =

    let sink initialValue = new CellSink<_>(initialValue)

    let sinkWithCoalesce initialValue coalesce = new CellSink<_>(initialValue, coalesce)

    let send a (cellSink : 'T CellSink) = cellSink.Send a

    let loop (f : 'T Cell -> ('T Cell * 'a)) =
        Transaction.Run (fun () ->
            let l = new CellLoop<'T>()
            let (c, r) = f l
            l.Loop(c)
            (c, r))

    let loopWithNoCaptures (f : 'T Cell -> 'T Cell) =
        let (l, _) = loop (fun c -> (f c, ()))
        l

    let constant value = new Cell<_>(value = value)

    let constantLazy value = Stream.never () |> Stream.holdLazy value

    let sample (cell : 'T Cell) = Transaction.Apply (fun _ -> cell.SampleNoTransaction ())

    let sampleLazy (cell : 'T Cell) = Transaction.Apply cell.SampleLazy

    let internal valueInternal (transaction : Transaction) (cell : 'T Cell) =
        let spark = Stream.never ()
        transaction.Prioritized spark.Node (fun transaction -> spark.Send(transaction, ()))
        let initial = spark |> Stream.snapshotAndTakeCell cell
        initial |> Stream.merge (fun _ r -> r) (cell.Updates transaction)

    let listenWeak handler (cell : 'T Cell) = Transaction.Apply (fun transaction -> cell |> valueInternal transaction |> Stream.listenWeak handler)

    let listen handler (cell : 'T Cell) = Transaction.Apply (fun transaction -> cell |> valueInternal transaction |> Stream.listen handler)

    let map f (cell : 'T Cell) = Transaction.Apply(fun transaction -> cell.Updates transaction |> Stream.map f |> Stream.holdLazyInternal transaction (cell.SampleLazy transaction |> Lazy.map f))

    let apply (f : ('T -> 'a) Cell) (cell : 'T Cell) =
        Transaction.Apply (fun transaction ->
            let out = Stream.sink ()
            let outTarget = out.Node
            let inTarget = Node<unit>(0L)
            let (_, nodeTarget) = inTarget.Link (fun _ _ -> ()) outTarget
            let mutable fo = Option.None
            let mutable ao = Option.None
            let h = (fun (transaction : Transaction) (f : 'T -> 'a) (a : 'T) -> transaction.Prioritized out.Node (fun transaction -> out.Send(transaction, f a)))
            let listener1 = (f |> valueInternal transaction).ListenWithTransaction inTarget (fun transaction f ->
                fo <- Option.Some f
                match ao with
                    | None -> ()
                    | Some a -> h transaction f a)
            let listener2 = (cell |> valueInternal transaction).ListenWithTransaction inTarget (fun transaction a ->
                ao <- Option.Some a
                match fo with
                    | None -> ()
                    | Some f -> h transaction f a)
            ((((out.LastFiringOnly transaction).AddCleanup listener1).AddCleanup listener2).AddCleanup
                (Listener.fromAction (fun () -> inTarget.Unlink nodeTarget))) |>
                    Stream.holdLazy (lazy (f.SampleNoTransaction () (cell.SampleNoTransaction ()))))

    let lift2 f (cell1 : 'T Cell) (cell2 : 'T2 Cell) =
        apply (cell1 |> map f) cell2

    let lift3 f (cell1 : 'T Cell) (cell2 : 'T2 Cell) (cell3 : 'T3 Cell) =
        apply (apply (cell1 |> map f) cell2) cell3

    let lift4 f (cell1 : 'T Cell) (cell2 : 'T2 Cell) (cell3 : 'T3 Cell) (cell4 : 'T4 Cell) =
        apply (apply (apply (cell1 |> map f) cell2) cell3) cell4
        
    let lift5 f (cell1 : 'T Cell) (cell2 : 'T2 Cell) (cell3 : 'T3 Cell) (cell4 : 'T4 Cell) (cell5 : 'T5 Cell) =
        apply (apply (apply (apply (cell1 |> map f) cell2) cell3) cell4) cell5
                
    let lift6 f (cell1 : 'T Cell) (cell2 : 'T2 Cell) (cell3 : 'T3 Cell) (cell4 : 'T4 Cell) (cell5 : 'T5 Cell) (cell6 : 'T6 Cell) =
        apply (apply (apply (apply (apply (cell1 |> map f) cell2) cell3) cell4) cell5) cell6
                
    let lift7 f (cell1 : 'T Cell) (cell2 : 'T2 Cell) (cell3 : 'T3 Cell) (cell4 : 'T4 Cell) (cell5 : 'T5 Cell) (cell6 : 'T6 Cell) (cell7 : 'T7 Cell) =
        apply (apply (apply (apply (apply (apply (cell1 |> map f) cell2) cell3) cell4) cell5) cell6) cell7
                
    let lift8 f (cell1 : 'T Cell) (cell2 : 'T2 Cell) (cell3 : 'T3 Cell) (cell4 : 'T4 Cell) (cell5 : 'T5 Cell) (cell6 : 'T6 Cell) (cell7 : 'T7 Cell) (cell8 : 'T8 Cell) =
        apply (apply (apply (apply (apply (apply (apply (cell1 |> map f) cell2) cell3) cell4) cell5) cell6) cell7) cell8

    let liftAll f (cells : 'T Cell seq) =
        Transaction.Apply (fun transaction ->
            let c = List.ofSeq cells
            let values = c |> Seq.map (fun c -> c.SampleNoTransaction ()) |> Array.ofSeq
            let out = new Stream<'a>()
            let initialValue = lazy (f (List.ofSeq values))
            let listeners = cells |> Seq.mapi (fun i cell ->
                (cell.Updates transaction).ListenInternal out.Node transaction (fun transaction v ->
                    values.[i] <- v
                    out.Send(transaction, f (List.ofArray values))
                    ) false)
            out.AddCleanup (Listener.fromSeq listeners) |> Stream.holdLazy initialValue)

    let calm (cell : 'T Cell when 'T : equality) =
        let initialValue = cell |> sampleLazy
        let initialValueOption = Lazy.map Option.Some initialValue
        Transaction.Apply (fun transaction -> cell.Updates transaction |> Stream.calmInternal initialValueOption |> Stream.holdLazy initialValue)

    let switchC (cell : 'T Cell Cell) =
        Transaction.Apply (fun transaction ->
            let za = cell |> sampleLazy |> Lazy.map sample
            let out = new Stream<'T>()
            let mutable currentListener = Option<IListener>.None
            let h = (fun (transaction : Transaction) (c : 'T Cell) ->
                match currentListener with
                    | None -> ()
                    | Some l -> l.Unlisten()
                currentListener <- Option.Some ((c |> valueInternal transaction).ListenInternal out.Node transaction (fun t a -> out.Send(t, a)) false))
            let listener = (cell |> valueInternal transaction).ListenInternal out.Node transaction h false
            out.AddCleanup listener |> Stream.holdLazy za)

    let switchS (cell : 'T Stream Cell) =
        Transaction.Apply (fun transaction ->
            let out = new Stream<'T>()
            let mutable currentListener = (cell.SampleNoTransaction ()).ListenInternal out.Node transaction (fun t a -> out.Send(t, a)) false
            let h = (fun (transaction : Transaction) (s : 'T Stream) ->
                transaction.Last (fun () ->
                    currentListener.Unlisten()
                    currentListener <- s.ListenInternal out.Node transaction (fun t a -> out.Send(t, a)) true))
            let listener = (cell.Updates transaction).ListenInternal out.Node transaction h false
            out.AddCleanup listener)