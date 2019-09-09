using NUnit.Framework;
using NzbDrone.Core.MediaFiles.TrackImport.Identification;
using NzbDrone.Test.Common;
using FluentAssertions;

namespace NzbDrone.Core.Test.MediaFiles.TrackImport.Identification
{
    [TestFixture]
    public class MunkresFixture : TestBase
    {
        // 2d arrays don't play nicely with attributes
        public void RunTest(double[,] costMatrix, double expectedCost)
        {
            var m = new Munkres(costMatrix);
            m.Run();
            m.Cost.Should().Be(expectedCost);
        }
        
        [Test]
        public void MunkresSquareTest1()
        {
            var C = new double[,] {
                { 1, 2, 3 },
                { 2, 4, 6 },
                { 3, 6, 9 }
            };

            RunTest(C, 10);

        }

        [Test]
        public void MunkresSquareTest2()
        {
            var C = new double[,] {
                { 400, 150, 400 },
                { 400, 450, 600 },
                { 300, 225, 300 }
            };

            RunTest(C, 850);
        }

        [Test]
        public void MunkresSquareTest3()
        {
            var C = new double[,] {
                { 10, 10, 8 },
                {  9,  8, 1 },
                {  9,  7, 4 }
            };

            RunTest(C, 18);
        }

        [Test]
        public void MunkresSquareTest4()
        {
            var C = new double[,] {
                {  5,  9, 1 },
                { 10,  3, 2 },
                {  8,  7, 4 }
            };

            RunTest(C, 12);
        }

        [Test]
        public void MunkresSquareTest5()
        {
            var C = new double[,] {
                {12, 26, 17,  0, 0},
                {49, 43, 36, 10, 5},
                {97,  9, 66, 34, 0},
                {52, 42, 19, 36, 0},
                {15, 93, 55, 80, 0}
            };

            RunTest(C, 48);
        }

        [Test]
        public void Munkres5x5Test()
        {
            var C = new double[,] {
                {12, 9, 27, 10, 23},
                {7, 13, 13, 30, 19},
                {25, 18, 26, 11, 26},
                {9, 28, 26, 23, 13},
                {16, 16, 24, 6, 9}
            };

            RunTest(C, 51);
        }

        [Test]
        public void Munkres10x10Test()
        {
            var C = new double[,] {
                {37, 34, 29, 26, 19, 8, 9, 23, 19, 29},
                {9, 28, 20, 8, 18, 20, 14, 33, 23, 14},
                {15, 26, 12, 28, 6, 17, 9, 13, 21, 7},
                {2, 8, 38, 36, 39, 5, 36, 2, 38, 27},
                {30, 3, 33, 16, 21, 39, 7, 23, 28, 36},
                {7, 5, 19, 22, 36, 36, 24, 19, 30, 2},
                {34, 20, 13, 36, 12, 33, 9, 10, 23, 5},
                {7, 37, 22, 39, 33, 39, 10, 3, 13, 26},
                {21, 25, 23, 39, 31, 37, 32, 33, 38, 1},
                {17, 34, 40, 10, 29, 37, 40, 3, 25, 3}
            };

            RunTest(C, 66);
        }

        [Test]
        public void Munkres20x20Test()
        {
            var C = new double[,] {
                {5, 4, 3, 9, 8, 9, 3, 5, 6, 9, 4, 10, 3, 5, 6, 6, 1, 8, 10, 2},
                {10, 9, 9, 2, 8, 3, 9, 9, 10, 1, 7, 10, 8, 4, 2, 1, 4, 8, 4, 8},
                {10, 4, 4, 3, 1, 3, 5, 10, 6, 8, 6, 8, 4, 10, 7, 2, 4, 5, 1, 8},
                {2, 1, 4, 2, 3, 9, 3, 4, 7, 3, 4, 1, 3, 2, 9, 8, 6, 5, 7, 8},
                {3, 4, 4, 1, 4, 10, 1, 2, 6, 4, 5, 10, 2, 2, 3, 9, 10, 9, 9, 10},
                {1, 10, 1, 8, 1, 3, 1, 7, 1, 1, 2, 1, 2, 6, 3, 3, 4, 4, 8, 6},
                {1, 8, 7, 10, 10, 3, 4, 6, 1, 6, 6, 4, 9, 6, 9, 6, 4, 5, 4, 7},
                {8, 10, 3, 9, 4, 9, 3, 3, 4, 6, 4, 2, 6, 7, 7, 4, 4, 3, 4, 7},
                {1, 3, 8, 2, 6, 9, 2, 7, 4, 8, 10, 8, 10, 5, 1, 3, 10, 10, 2, 9},
                {2, 4, 1, 9, 2, 9, 7, 8, 2, 1, 4, 10, 5, 2, 7, 6, 5, 7, 2, 6},
                {4, 5, 1, 4, 2, 3, 3, 4, 1, 8, 8, 2, 6, 9, 5, 9, 6, 3, 9, 3},
                {3, 1, 1, 8, 6, 8, 8, 7, 9, 3, 2, 1, 8, 2, 4, 7, 3, 1, 2, 4},
                {5, 9, 8, 6, 10, 4, 10, 3, 4, 10, 10, 10, 1, 7, 8, 8, 7, 7, 8, 8},
                {1, 4, 6, 1, 6, 1, 2, 10, 5, 10, 2, 6, 2, 4, 5, 5, 3, 5, 1, 5},
                {5, 6, 9, 10, 6, 6, 10, 6, 4, 1, 5, 3, 9, 5, 2, 10, 9, 9, 5, 1},
                {10, 9, 4, 6, 9, 5, 3, 7, 10, 1, 6, 8, 1, 1, 10, 9, 5, 7, 7, 5},
                {2, 6, 6, 6, 6, 2, 9, 4, 7, 5, 3, 2, 10, 3, 4, 5, 10, 9, 1, 7},
                {5, 2, 4, 9, 8, 4, 8, 2, 4, 1, 3, 7, 6, 8, 1, 6, 8, 8, 10, 10},
                {9, 6, 3, 1, 8, 5, 7, 8, 7, 2, 1, 8, 2, 8, 3, 7, 4, 8, 7, 7},
                {8, 4, 4, 9, 7, 10, 6, 2, 1, 5, 8, 5, 1, 1, 1, 9, 1, 3, 5, 3}
            };

            RunTest(C, 22);
        }

        [Test]
        public void MunkresRectangularTest1()
        {
            var C = new double[,] {
                { 400, 150, 400, 1 },
                { 400, 450, 600, 2 },
                { 300, 225, 300, 3 }
            };

            RunTest(C, 452);
        }

        [Test]
        public void MunkresRectangularTest2()
        {
            var C = new double[,] {
                { 10, 10, 8, 11 },
                {  9,  8, 1, 1  },
                {  9,  7, 4, 10 }
            };

            RunTest(C, 15);
        }

        [Test]
        public void MunkresRectangularTest3()
        {
            var C = new double[,] {
                {34, 26, 17, 12},
                {43, 43, 36, 10},
                {97, 47, 66, 34},
                {52, 42, 19, 36},
                {15, 93, 55, 80}
            };

            RunTest(C, 70);
        }

    }
}

