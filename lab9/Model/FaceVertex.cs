using System;
using System.Collections.Generic;
using System.Text;

namespace lab6.Model
{
    public struct FaceVertex
    {
        public int V;  
        public int N; 
        public int T; 
        public FaceVertex(int v, int n = -1, int t = -1) 
        { 
            V = v; 
            N = n; 
            T = t; 
        }
    }
}
