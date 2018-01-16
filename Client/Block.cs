using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    class Block
    {
        #region Argument
        private int id;// Type of the block
        private byte[,] blocks;// Representation of the block
        private int column;// Column of the block
        private int row;// Row of the block
        #endregion

        #region GetSet
        public int Id { get => id; }
        public byte[,] Blocks { get => blocks; }
        public int Column { get => column; set => column = value; }
        public int Row { get => row; set => row = value; }
        #endregion

        #region Constructor
        public Block(int id, int column, int row)
        {
            this.id = id;
            this.column = column;
            this.row = row;
            // Fill the block according to its type
            if (id == 0) // #
            {
                this.blocks = new byte[1, 1];
                FillBlock();
            }
            else if (id == 1) // ##
            {                 // ##
                this.blocks = new byte[2, 2];
                FillBlock();
            }
            else if (id == 2) // .#
            {                 // ##
                this.blocks = new byte[2, 2];
                FillBlock();
                this.blocks[0, 0] = 0;
            }
            else if (id == 3) // ..#
            {                 // ..#
                              // ###
                this.blocks = new byte[3, 3];
                FillBlock();
                this.blocks[0, 0] = 0;
                this.blocks[0, 1] = 0;
                this.blocks[1, 0] = 0;
                this.blocks[1, 1] = 0;
            }
        }
        #endregion

        #region Function
        // Fill the array block with 1 value
        private void FillBlock()
        {
            for (int c = 0; c < Blocks.GetLength(0); c++)
            {
                for (int r = 0; r < Blocks.GetLength(1); r++)
                {
                    Blocks[c, r] = 1;
                }
            }
        }

        // Make the block rotate
        public void Rotate()
        {
            // Block before rotate
            byte[,] block_mem = new byte[Blocks.GetLength(0), Blocks.GetLength(1)];

            // Rotating the block according to its position before it
            for (int c = 0; c < Blocks.GetLength(0); c++)
            {
                for (int r = 0; r < Blocks.GetLength(1); r++)
                {
                    block_mem[c, r] = Blocks[c, r];
                }
            }
            for (int c = 0; c < Blocks.GetLength(0); c++)
            {
                for (int r = 0; r < Blocks.GetLength(1); r++)
                {
                    Blocks[c, r] = block_mem[Blocks.GetLength(0) - 1 - r, c];
                }
            }
        }

        // Rotate in the other direction
        public void RotateInverse()
        {
            // Block before rotation
            byte[,] block_mem = new byte[Blocks.GetLength(0), Blocks.GetLength(1)];

            // Rotating the block according to its position before it
            for (int c = 0; c < Blocks.GetLength(0); c++)
            {
                for (int r = 0; r < Blocks.GetLength(1); r++)
                {
                    block_mem[c, r] = Blocks[c, r];
                }
            }
            for (int c = 0; c < Blocks.GetLength(0); c++)
            {
                for (int r = 0; r < Blocks.GetLength(1); r++)
                {
                    Blocks[c, r] = block_mem[r, Blocks.GetLength(1) - 1 - c];
                }
            }
        }

        // Make the block fall of one row
        public void DownBlock()
        {
            Row += 1;
        }
    }
    #endregion
}
