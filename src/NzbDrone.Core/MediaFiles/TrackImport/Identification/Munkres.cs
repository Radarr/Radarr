/*
    The MIT License (MIT)

    Copyright (c) 2000 Robert A. Pilgrim
                       Murray State University
                       Dept. of Computer Science & Information Systems
                       Murray,Kentucky

    Permission is hereby granted, free of charge, to any person obtaining a copy
    of this software and associated documentation files (the "Software"), to deal
    in the Software without restriction, including without limitation the rights
    to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
    copies of the Software, and to permit persons to whom the Software is
    furnished to do so, subject to the following conditions:

    The above copyright notice and this permission notice shall be included in all
    copies or substantial portions of the Software.

    THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
    IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
    FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
    AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
    LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
    OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
   SOFTWARE.

 */

using System;
using System.Linq;
using System.Collections.Generic;

namespace NzbDrone.Core.MediaFiles.TrackImport.Identification
{
    public class Munkres
    {
        private double[,] C;
        private readonly double[,] C_orig;
        private int[,] M;
        private int[,] path;
        private int[] RowCover;
        private int[] ColCover;
        private readonly int n;
        private readonly int nrow_orig;
        private readonly int ncol_orig;
        private int path_count;
        private int path_row_0;
        private int path_col_0;
        private int step;

        public Munkres(double[,] costMatrix)
        {
            C = PadMatrix(costMatrix);
            n = C.GetLength(0);
            nrow_orig = costMatrix.GetLength(0);
            ncol_orig = costMatrix.GetLength(1);
            
            C_orig = C.Clone() as double[,];
            RowCover = new int[n];
            ColCover = new int[n];
            M = new int[n, n];
            path = new int[2*n + 1, 2];

            step = 1;
        }

        public int[,] Marked
        {
            get
            {
                return M;
            }
        }

        public List<Tuple<int, int>> Solution
        {
            get
            {
                var value = new List<Tuple<int, int>>();
                for (int row = 0; row < nrow_orig; row++)
                {
                    for (int col = 0; col < ncol_orig; col++)
                    {
                        if (M[row, col] == 1)
                        {
                            value.Add(Tuple.Create(row, col));
                        }
                    }
                }
                return value;
            }
        }

        public double Cost
        {
            get
            {
                var soln = Solution;
                return soln.Select(x => C_orig[x.Item1, x.Item2]).Sum();
            }
        }

        private double[,] PadMatrix(double[,] matrix)
        {
            var newdim = Math.Max(matrix.GetLength(0), matrix.GetLength(1));
            var outp = new double[newdim, newdim];
            for (int row = 0; row < matrix.GetLength(0); row++)
            {
                for (int col = 0; col < matrix.GetLength(1); col++)
                {
                    outp[row, col] = matrix[row, col];
                }
            }
            return outp;
        }

        //For each row of the cost matrix, find the smallest element and subtract
        //it from every element in its row.  When finished, Go to Step 2.
        private void step_one(ref int step)
        {
            double min_in_row;

            for (int r = 0; r < n; r++)
            {
                min_in_row = C[r, 0];
                for (int c = 0; c < n; c++)
                {
                    if (C[r, c] < min_in_row)
                    {
                        min_in_row = C[r, c];
                    }
                }
                for (int c = 0; c < n; c++)
                {
                    C[r, c] -= min_in_row;
                }
            }
            step = 2;
        }

        //Find a zero (Z) in the resulting matrix.  If there is no starred 
        //zero in its row or column, star Z. Repeat for each element in the 
        //matrix. Go to Step 3.
        private void step_two(ref int step)
        {
            for (int r = 0; r < n; r++)
            {
                for (int c = 0; c < n; c++)
                {
                    if (C[r, c] == 0 && RowCover[r] == 0 && ColCover[c] == 0)
                    {
                        M[r, c] = 1;
                        RowCover[r] = 1;
                        ColCover[c] = 1;
                    }
                }
            }
            for (int r = 0; r < n; r++)
            {
                RowCover[r] = 0;
            }
            for (int c = 0; c < n; c++)
            {
                ColCover[c] = 0;
            }
            step = 3;
        }

        //Cover each column containing a starred zero.  If K columns are covered, 
        //the starred zeros describe a complete set of unique assignments.  In this 
        //case, Go to DONE, otherwise, Go to Step 4.
        private void step_three(ref int step)
        {
            int colcount;
            for (int r = 0; r < n; r++)
            {
                for (int c = 0; c < n; c++)
                {
                    if (M[r, c] == 1)
                    {
                        ColCover[c] = 1;
                    }
                }
            }

            colcount = 0;
            for (int c = 0; c < n; c++)
            {
                if (ColCover[c] == 1)
                {
                    colcount += 1;
                }
            }
            if (colcount >= n)
            {
                step = 7;
            }
            else
            {
                step = 4;
            }
        }

        //methods to support step 4
        private void find_a_zero(ref int row, ref int col)
        {
            int r = 0;
            int c;
            bool done;
            row = -1;
            col = -1;
            done = false;
            while (!done)
            {
                c = 0;
                while (true)
                {
                    if (C[r, c] == 0 && RowCover[r] == 0 && ColCover[c] == 0)
                    {
                        row = r;
                        col = c;
                        done = true;
                    }
                    c += 1;
                    if (c >= n || done)
                    {
                        break;
                    }
                }
                r += 1;
                if (r >= n)
                {
                    done = true;
                }
            }
        }

        private bool star_in_row(int row)
        {
            bool tmp = false;
            for (int c = 0; c < n; c++)
            {
                if (M[row, c] == 1)
                {
                    tmp = true;
                }
            }
            return tmp;
        }

        private void find_star_in_row(int row, ref int col)
        {
            col = -1;
            for (int c = 0; c < n; c++)
            {
                if (M[row, c] == 1)
                {
                    col = c;
                }
            }
        }

        //Find a noncovered zero and prime it.  If there is no starred zero 
        //in the row containing this primed zero, Go to Step 5.  Otherwise, 
        //cover this row and uncover the column containing the starred zero. 
        //Continue in this manner until there are no uncovered zeros left. 
        //Save the smallest uncovered value and Go to Step 6.
        private void step_four(ref int step)
        {
            int row = -1;
            int col = -1;
            bool done;

            done = false;
            while (!done)
            {
                find_a_zero(ref row, ref col);
                if (row == -1)
                {
                    done = true;
                    step = 6;
                }
                else
                {
                    M[row, col] = 2;
                    if (star_in_row(row))
                    {
                        find_star_in_row(row, ref col);
                        RowCover[row] = 1;
                        ColCover[col] = 0;
                    }
                    else
                    {
                        done = true;
                        step = 5;
                        path_row_0 = row;
                        path_col_0 = col;
                    }
                }
            }
        }

        // methods to support step 5
        private void find_star_in_col(int c, ref int r)
        {
            r = -1;
            for (int i = 0; i < n; i++)
            {
                if (M[i, c] == 1)
                {
                    r = i;
                }
            }
        }

        private void find_prime_in_row(int r, ref int c)
        {
            for (int j = 0; j < n; j++)
            {
                if (M[r, j] == 2)
                {
                    c = j;
                }
            }
        }

        private void augment_path()
        {
            for (int p = 0; p < path_count; p++)
            {
                if (M[path[p, 0], path[p, 1]] == 1)
                {
                    M[path[p, 0], path[p, 1]] = 0;
                }
                else
                {
                    M[path[p, 0], path[p, 1]] = 1;
                }
            }
        }

        private void clear_covers()
        {
            for (int r = 0; r < n; r++)
            {
                RowCover[r] = 0;
            }
            for (int c = 0; c < n; c++)
            {
                ColCover[c] = 0;
            }
        }

        private void erase_primes()
        {
            for (int r = 0; r < n; r++)
            {
                for (int c = 0; c < n; c++)
                {
                    if (M[r, c] == 2)
                    {
                        M[r, c] = 0;
                    }
                }
            }
        }


        //Construct a series of alternating primed and starred zeros as follows.  
        //Let Z0 represent the uncovered primed zero found in Step 4.  Let Z1 denote 
        //the starred zero in the column of Z0 (if any). Let Z2 denote the primed zero 
        //in the row of Z1 (there will always be one).  Continue until the series 
        //terminates at a primed zero that has no starred zero in its column.  
        //Unstar each starred zero of the series, star each primed zero of the series, 
        //erase all primes and uncover every line in the matrix.  Return to Step 3.
        private void step_five(ref int step)
        {
            bool done;
            int r = -1;
            int c = -1;

            path_count = 1;
            path[path_count - 1, 0] = path_row_0;
            path[path_count - 1, 1] = path_col_0;
            done = false;
            while (!done)
            {
                find_star_in_col(path[path_count - 1, 1], ref r);
                if (r > -1)
                {
                    path_count += 1;
                    path[path_count - 1, 0] = r;
                    path[path_count - 1, 1] = path[path_count - 2, 1];
                }
                else
                {
                    done = true;
                }
                if (!done)
                {
                    find_prime_in_row(path[path_count - 1, 0], ref c);
                    path_count += 1;
                    path[path_count - 1, 0] = path[path_count - 2, 0];
                    path[path_count - 1, 1] = c;
                }
            }
            augment_path();
            clear_covers();
            erase_primes();
            step = 3;
        }

        //methods to support step 6
        private void find_smallest(ref double minval)
        {
            for (int r = 0; r < n; r++)
            {
                for (int c = 0; c < n; c++)
                {
                    if (RowCover[r] == 0 && ColCover[c] == 0)
                    {
                        if (minval > C[r, c])
                        {
                            minval = C[r, c];
                        }
                    }
                }
            }
        }

        //Add the value found in Step 4 to every element of each covered row, and subtract 
        //it from every element of each uncovered column.  Return to Step 4 without 
        //altering any stars, primes, or covered lines.
        private void step_six(ref int step)
        {
            double minval = double.MaxValue;
            find_smallest(ref minval);
            for (int r = 0; r < n; r++)
            {
                for (int c = 0; c < n; c++)
                {
                    if (RowCover[r] == 1)
                    {
                        C[r, c] += minval;
                    }
                    if (ColCover[c] == 0)
                    {
                        C[r, c] -= minval;
                    }
                }
            }
            step = 4;
        }

        public void Run()
        {
            bool done = false;
            while (!done)
            {
                switch (step)
                {
                    case 1:
                        step_one(ref step);
                        break;
                    case 2:
                        step_two(ref step);
                        break;
                    case 3:
                        step_three(ref step);
                        break;
                    case 4:
                        step_four(ref step);
                        break;
                    case 5:
                        step_five(ref step);
                        break;
                    case 6:
                        step_six(ref step);
                        break;
                    case 7:
                        done = true;
                        break;
                }
            }
        }
    }
}
