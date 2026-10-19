using System.Collections.Generic;
using Game.src.core.entities.Building;
using Game.src.core.entities.Terrain;
using Game.src.core.utils.Factory;

namespace Game.src.core.utils {
    public class MapMatrix<T> where T : class {
        private T[,] matrix;
        private int rows;
        private int cols;

        public MapMatrix(int rows, int cols) {
            matrix = new T[rows, cols];
            this.rows = rows;
            this.cols = cols;
        }
        public MapMatrix(int[,] matr, Func<int, T> converter) {
            rows = matr.GetLength(0);
            cols = matr.GetLength(1);
            matrix = new T[rows, cols];
            for (int i = 0; i < rows; i++) {
                for (int j = 0; j < cols; j++) {
                    matrix[i, j] = converter(matr[i, j]);
                }
            }
        }
        public int Rows => rows;
        public int Cols => cols;
        public void Resize(int nrows, int ncols) {
            T[,] new_matr = new T[nrows, ncols];
            int r = Math.Min(nrows, rows);
            int c = Math.Min(ncols, cols);
            for (int y = 0; y < r; y++) {
                for (int x = 0; x < c; x++) {
                    new_matr[y, x] = matrix[y, x];
                }
            }
            matrix = new_matr;
            rows = nrows;
            cols = ncols;
        }
        public T this[int row, int col] {
            get {
                if (!(row >= 0 && row < rows && col >= 0 && col < cols)) {
                    throw new IndexOutOfRangeException($"Индекс [{row}, {col}] за пределами матрицы.");
                }
                return matrix[row, col];
            }
            set {
                if (!(row >= 0 && row < rows && col >= 0 && col < cols)) {
                    throw new IndexOutOfRangeException($"Индекс [{row}, {col}] за пределами матрицы.");
                }
                matrix[row, col] = value;
            }
        }
        public IEnumerator<T> GetEnumerator() {
            for (int i = 0; i < rows; i++) {
                for (int j = 0; j < cols; j++) {
                    yield return matrix[i, j];
                }
            }
        }
        public IEnumerable<(int row, int col)> GetNeighbours(int row, int col) {
            foreach (var (dr, dc) in new[] { (-1, 0), (1, 0), (0, -1), (0, 1) }) {
                int r = row + dr;
                int c = col + dc;
                if (r >= 0 && r < rows && c >= 0 && c < cols) {
                    yield return (r, c);
                }
            }
        }
        public void Delete(int row, int col) {
            if (row >= 0 && row < rows && col >= 0 && col < cols) {
                matrix[row, col] = null!;
                return;
            }
            throw new IndexOutOfRangeException("Неверный индекс");
        }
    }

    public static class Converter {
        public static Cell IntToCell(int n) {
            return n switch {
                1 => new Cell(CellType.Plains),
                2 => new Cell(CellType.Water),
                3 => new Cell(CellType.Mountains),
                _ => throw new IndexOutOfRangeException("неверное значение"),
            };
        }
        public static int CellToInt(Cell c) {
            return c.Type switch {
                CellType.Plains => 1,
                CellType.Water => 2,
                CellType.Mountains => 3,
                _ => throw new IndexOutOfRangeException("неверное значение"),
            };
        }
        public static int BuildingToInt(Building b) {
            return b.Type switch {
                BuildingType.Tower => 1,
                BuildingType.Wall => 2,
                BuildingType.Castle => 3,
                _ => throw new IndexOutOfRangeException("неверное значение"),
            };
        }
        public static Building IntToBuilding(int n) {
            return n switch {
                1 => BuildingFactory.InitBuilding(BuildingType.Tower),
                2 => BuildingFactory.InitBuilding(BuildingType.Wall),
                3 => BuildingFactory.InitBuilding(BuildingType.Castle),
                _ => throw new IndexOutOfRangeException("неверное значение"),
            };
        }
    }
}
