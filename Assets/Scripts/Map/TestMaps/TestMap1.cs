using System.Collections.Generic;

public enum MT
{
    G, //Ground
    W, //Wall
    N, //NoBuild
    S, //Spawn
    C  //Castle(Goal)
}

public class TestMap1 {
    public static List<List<MT>> testMap = new List<List<MT>>() {
            new List<MT>() {MT.N, MT.N, MT.N, MT.G, MT.G, MT.G, MT.G, MT.G, MT.G, MT.G, MT.G, MT.G, MT.G, MT.G, MT.G, MT.G, MT.G, MT.G, MT.G, MT.G},
            new List<MT>() {MT.C, MT.N, MT.N, MT.G, MT.G, MT.G, MT.G, MT.G, MT.G, MT.G, MT.G, MT.G, MT.G, MT.G, MT.G, MT.G, MT.G, MT.G, MT.G, MT.G},
            new List<MT>() {MT.N, MT.N, MT.N, MT.G, MT.G, MT.G, MT.G, MT.G, MT.G, MT.G, MT.G, MT.G, MT.G, MT.G, MT.G, MT.G, MT.G, MT.G, MT.G, MT.G},
            new List<MT>() {MT.W, MT.W, MT.W, MT.W, MT.G, MT.G, MT.G, MT.G, MT.G, MT.G, MT.G, MT.G, MT.G, MT.G, MT.W, MT.G, MT.G, MT.G, MT.G, MT.G},
            new List<MT>() {MT.W, MT.W, MT.W, MT.W, MT.G, MT.G, MT.G, MT.G, MT.G, MT.G, MT.G, MT.G, MT.G, MT.G, MT.W, MT.G, MT.G, MT.G, MT.G, MT.G},
            new List<MT>() {MT.G, MT.G, MT.G, MT.G, MT.G, MT.G, MT.G, MT.G, MT.G, MT.G, MT.G, MT.G, MT.G, MT.G, MT.W, MT.G, MT.G, MT.G, MT.G, MT.G},
            new List<MT>() {MT.G, MT.G, MT.G, MT.G, MT.G, MT.G, MT.G, MT.G, MT.G, MT.G, MT.G, MT.G, MT.G, MT.G, MT.W, MT.G, MT.G, MT.G, MT.G, MT.G},
            new List<MT>() {MT.G, MT.G, MT.G, MT.G, MT.G, MT.G, MT.G, MT.W, MT.W, MT.G, MT.G, MT.G, MT.G, MT.G, MT.W, MT.G, MT.G, MT.G, MT.G, MT.G},
            new List<MT>() {MT.G, MT.G, MT.G, MT.G, MT.G, MT.G, MT.G, MT.G, MT.W, MT.G, MT.G, MT.G, MT.G, MT.W, MT.W, MT.G, MT.G, MT.G, MT.G, MT.G},
            new List<MT>() {MT.G, MT.G, MT.G, MT.G, MT.G, MT.G, MT.G, MT.G, MT.G, MT.G, MT.G, MT.G, MT.G, MT.G, MT.W, MT.G, MT.G, MT.G, MT.G, MT.G},
            new List<MT>() {MT.G, MT.G, MT.G, MT.G, MT.G, MT.G, MT.G, MT.G, MT.G, MT.G, MT.G, MT.G, MT.G, MT.G, MT.W, MT.G, MT.G, MT.G, MT.G, MT.G},
            new List<MT>() {MT.G, MT.G, MT.G, MT.G, MT.G, MT.G, MT.G, MT.G, MT.G, MT.G, MT.G, MT.G, MT.G, MT.G, MT.W, MT.G, MT.G, MT.G, MT.G, MT.G},
            new List<MT>() {MT.G, MT.G, MT.G, MT.G, MT.G, MT.G, MT.G, MT.G, MT.G, MT.G, MT.G, MT.G, MT.G, MT.G, MT.W, MT.G, MT.G, MT.G, MT.G, MT.G},
            new List<MT>() {MT.G, MT.G, MT.G, MT.G, MT.G, MT.G, MT.G, MT.G, MT.G, MT.G, MT.G, MT.G, MT.G, MT.G, MT.W, MT.G, MT.G, MT.G, MT.G, MT.G},
            new List<MT>() {MT.G, MT.G, MT.G, MT.G, MT.G, MT.G, MT.G, MT.G, MT.G, MT.G, MT.G, MT.G, MT.G, MT.G, MT.W, MT.G, MT.G, MT.G, MT.G, MT.G},
            new List<MT>() {MT.W, MT.W, MT.W, MT.W, MT.G, MT.G, MT.G, MT.G, MT.G, MT.G, MT.G, MT.G, MT.G, MT.G, MT.W, MT.G, MT.G, MT.G, MT.G, MT.G},
            new List<MT>() {MT.W, MT.W, MT.W, MT.W, MT.G, MT.G, MT.G, MT.G, MT.G, MT.G, MT.G, MT.G, MT.G, MT.G, MT.G, MT.G, MT.G, MT.G, MT.G, MT.G},
            new List<MT>() {MT.N, MT.N, MT.N, MT.N, MT.G, MT.G, MT.G, MT.G, MT.G, MT.G, MT.G, MT.G, MT.G, MT.G, MT.W, MT.G, MT.G, MT.G, MT.G, MT.G},
            new List<MT>() {MT.S, MT.N, MT.N, MT.N, MT.G, MT.G, MT.G, MT.G, MT.G, MT.G, MT.G, MT.G, MT.G, MT.G, MT.W, MT.G, MT.G, MT.G, MT.G, MT.G},
            new List<MT>() {MT.N, MT.N, MT.N, MT.N, MT.G, MT.G, MT.G, MT.G, MT.G, MT.G, MT.G, MT.G, MT.G, MT.G, MT.W, MT.G, MT.G, MT.G, MT.G, MT.G},
            };
}