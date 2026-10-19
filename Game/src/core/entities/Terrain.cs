namespace Game.src.core.entities.Terrain {


    public enum CellType {
        Plains,
        Water,
        Mountains,
    }

    public class Cell {
        private CellType cellType;
        public Cell(CellType type) {
            cellType = type;
        }
        public CellType Type => cellType;
    };
}