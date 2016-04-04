﻿open System
open System.Linq
open System.Collections.Generic

Environment.CurrentDirectory <- Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\Downloads"

#load "3f-EulerCycle.fsx"
#load "3g-EulerPath.fsx"
#load "3d-3e-debruin.fsx"
#load "3h-3i-Genome.fsx"
#load "AllEulerian.fsx"
#load "SpanningTree.fsx"

open AllEulerian
open ``3f-EulerCycle``
open ``3g-EulerPath``
open ``3d-3e-debruin``
open ``3h-3i-Genome``
open SpanningTree

// see if we can walk the tree and so it must be a cycle
let isCycle (tree : 'a seq) (graph : 'a Euler) (revGraph : 'a Euler) =
    if tree.Except graph.Keys |> Seq.isEmpty && tree.Except revGraph.Keys |> Seq.isEmpty then
        let gr = tree |> Seq.map (fun v -> v, graph.[v]) |> fun s -> s.ToDictionary(fst, snd)
        let walked = walk id gr
        walked.Count = tree.Count() + 1 // when it's a loop it ends with the same vertice it started
    else
        false

let findIsolatedCycles (graph : 'a Euler) =
    let trees = findMaxSpanTrees graph |> Seq.toList
    let revGraph = reverseAdj graph

    // fully connected?
    if trees.Length = 1 then Seq.empty
    else
        seq {
            for tree in trees do
                if isCycle tree graph revGraph then yield tree
        }

let findMaxNonBranching (graph : 'a Euler) =
    let revGraph = reverseAdj graph

    let zeroIn = graph.Keys.Except revGraph.Keys
    let moreThanOneOut = 
            graph 
            |> Seq.filter (fun kvp -> kvp.Value.Count > 1) 
            |> Seq.map (fun kvp -> kvp.Key)
    
    let crossRoads = zeroIn.Union moreThanOneOut |> HashSet

    seq {
        for start in crossRoads do
            let next = graph.[start]
            let contig = [start].ToList()
            for v in next do
                contig.Add v
                if crossRoads.Contains v then yield contig |> Seq.toList
    }

let findContigs (arr : string seq) =
    let graph = debruijn arr
    findMaxNonBranching graph |> Seq.toList


let arr = ["ATG"; "ATG"; "TGT"; "TGG"; "CAT"; "GGA"; "GAT"; "AGA"]
findContigs arr