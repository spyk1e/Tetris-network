using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace Client
{
    class Platform
    {
        #region Argument
        byte[,] board; // Board of the game
        int delay; // Delay for the falling blocks
        Player player; // Player object
        Block block; // Current block
        bool win = false; // If user win the game
        bool gameover = false; // If game is over
        List<Player> players; // List of players with their id and score
        bool editing = false; // Avoid clean/show block when already remaning cause of thread
        #endregion

        #region GetSet
        public byte[,] Board { get => board; set => board = value; }
        public int Delay { get => delay; set => delay = value; }
        internal Player Player { get => player; set => player = value; }
        internal Block Block { get => block; set => block = value; }
        public bool Win { get => win; set => win = value; }
        public bool Gameover { get => gameover; set => gameover = value; }
        internal List<Player> Players { get => players; set => players = value; }
        public bool Editing { get => editing; set => editing = value; }
        #endregion

        #region Constructor
        public Platform(int col, int row, int delay, Player player)
        {
            this.Delay = delay;
            this.Player = player;
            this.Block = null;
            Board = new byte[col, row];
            // Initializing the board
            for (int c = 0; c < col; c++)
            {
                for (int r = 0; r < row; r++)
                {
                    Board[c, r] = 0;
                }
            }
        }
        #endregion

        #region Function
        // Create block with his ID
        public void AddBlock(int blockid)
        {
            this.Block = new Block(blockid, (Board.GetLength(0) / 2) - 1, 0);
        }

        // Start the game : 2 thread start
        // Timer for block go down each delay
        // Listen key press of user
        public bool StartGame()
        {
            // Ask block to the server
            string[] ServerResult = AskServerBlock();
            // Add the block the server just send
            AddBlock(Int16.Parse(ServerResult[0]));

            // Show the board on screen
            DisplayBoard();

            // Thread to make the block fall
            Thread fallingBlockThread;
            fallingBlockThread = new Thread(new ThreadStart(DownBlock));
            fallingBlockThread.Start();

            // Thread to listen to keys
            Thread ThreadKey;
            ThreadKey = new Thread(new ThreadStart(ListenKey));
            ThreadKey.Start();

            // Player play until both thread have stopped (i.e. the block is on the bottom and can't move
            while (fallingBlockThread.IsAlive && ThreadKey.IsAlive)
            {
                // wait
            }
            return true;
        }

        // Check if game is lost (block on the first row of the board)
        public bool GameOver()
        {
            for (int c = 0; c < Board.GetLength(0); c++)
            {
                if (Board[c, 0] != 0)
                {
                    return true;
                }
            }
            return false;
        }

        // Listen thread for key press user
        public void ListenKey()
        {
            ConsoleKeyInfo keyinfo;
            do
            {
                keyinfo = Console.ReadKey(true);
                KeyAction(keyinfo.KeyChar);
            }
            while (!Gameover && !Win);// listen until the game is over (win or lose)
            Thread.CurrentThread.Abort();// Abort the thread
        }

        // Do the action left / right : depend of the key parameter 
        public void KeyAction(char key)
        {
            ClearCurrentBlock();
            bool check;
            if (key.ToString() == Player.Left && Block.Column > 0) // check if block is not at the last left position and key left up
            {
                check = true;
                for (int r = Block.Row; r < Block.Row + Block.Blocks.GetLength(1); r++)
                {
                    if (Board[Block.Column - 1, r] != 0)
                    {
                        check = false;
                    }
                }
                if (check)// Move the block on the left if it's possible
                {
                    Block.Column -= 1;
                }
            }
            else if (key.ToString() == Player.Right && Block.Column < Board.GetLength(0) - Block.Blocks.GetLength(0)) // check if block is not at the last right position and key right up
            {
                check = true;
                for (int r = Block.Row; r < Block.Row + Block.Blocks.GetLength(1); r++)
                {
                    if (Board[Block.Column + Block.Blocks.GetLength(0), r] != 0)
                    {
                        check = false;
                    }
                }
                if (check)// Move the block on the right if it's possible
                {
                    Block.Column += 1;
                }
            }
            else if (key.ToString() == Player.Hight && BlockCanRotate()) // check if block can rotate and key up press
            {
                Block.Rotate();
            }
            else if (key.ToString() == Player.Low && BlockCanRotate()) // check if block can rotate and key down press
            {
                Block.RotateInverse();
            }
            DisplayCurrentBlock();// Display the block
        }

        // Return if block can rotate or not => check if all pixel of the block is empty in board
        public bool BlockCanRotate()
        {
            bool canRotate = true;
            for (int r = Block.Row; r < Block.Row + Block.Blocks.GetLength(1); r++)
            {
                for (int c = Block.Column; c < Block.Column + Block.Blocks.GetLength(0); c++)
                {
                    if (Board[c, r] != 0)
                    {
                        canRotate = false;
                    }
                }
            }
            return canRotate;
        }

        // Go down the block each delay and display the new board
        // If block can be place (can't go down), it place and ask for new block to server
        public void DownBlock()
        {
            while (Thread.CurrentThread.IsAlive && !Gameover)
            {
                if (BlockCanDown())
                {
                    ClearCurrentBlock();// Remove the current block display
                    Block.DownBlock();// Make the block go down
                    DisplayCurrentBlock();// Display the new block position
                }
                else// Block on the bottom, ask the server a new one
                {
                    PlaceBlock();
                    string[] ServerResult = AskServerBlock();

                    if (ServerResult[2] == "0")
                    {
                        AddBlock(Int16.Parse(ServerResult[0]));// Add a new block
                        for (int i = 0; i < Int16.Parse(ServerResult[1]); i++)
                        {
                            RemoveLastRow();
                        }
                    }
                    else // Game over
                    {
                        Gameover = true;
                        Win = true;
                    }
                    if (!Win && !Gameover)
                    {
                        DisplayBoard();
                    }

                }
                if (GameOver())
                {
                    Gameover = true;
                    Player.SendGameOverToServer();
                }
                Thread.Sleep(Delay);// Wait the next tic to go down again
            }
            Console.Clear();
            if (Win)
            {
                Console.WriteLine("You won this game !");
            }
            else
            {
                Console.WriteLine("You lost this game !");
            }
            DisplayPlayerScore();
            Console.WriteLine("Press a key to replay a game or use ctrl + c to quit the game.");
            Console.ReadKey();
            Thread.CurrentThread.Abort();
        }

        // Function to modify the delay over time
        private void UpdateDelay()
        {
            if (Delay > Board.GetLength(1) * 2)
            {
                Delay -= Board.GetLength(1);
            }
        }

        // Remove the last row of the board => case value = 2
        public void RemoveLastRow()
        {
            for (int r = 0; r < Board.GetLength(1) - 1; r++)
            {
                for (int c = 0; c < Board.GetLength(0); c++)
                {
                    Board[c, r] = Board[c, r + 1];
                }
            }
            for (int c = 0; c < Board.GetLength(0); c++)
            {
                Board[c, Board.GetLength(1) - 1] = 2;
            }
            DisplayBoard();
        }

        // Remove and place the block on the board + call method to remove row if is full (check for all rows of the board)
        public void PlaceBlock()
        {
            for (int r = Block.Row; r < Block.Row + Block.Blocks.GetLength(1); r++)
            {
                for (int c = Block.Column; c < Block.Column + Block.Blocks.GetLength(0); c++)
                {
                    if (Block.Blocks[c - Block.Column, r - Block.Row] == 1)
                    {
                        Board[c, r] = 1;
                    }
                }
            }
            RemvoeRowIfFull();
        }

        // Remove row when it full (check for all rows of the board) and go down all blocks below
        public void RemvoeRowIfFull()
        {
            bool test = true;
            int count = 0;
            while (test)
            {
                test = false;
                for (int r = 0; r < Board.GetLength(1); r++)
                {
                    count = Board.GetLength(0);
                    for (int c = 0; c < Board.GetLength(0); c++)
                    {
                        if (Board[c, r] == 1)
                        {
                            count--;
                        }
                    }
                    if (count == 0) // remove line and down all blocks
                    {
                        for (int r1 = r; r1 > 0; r1--) // down all blocks on the line remove
                        {
                            for (int c = 0; c < Board.GetLength(0); c++)
                            {
                                Board[c, r1] = Board[c, r1 - 1];
                            }
                        }
                        test = true;
                        Player.Score += 1;
                        Player.SendNewRow();
                        DisplayBoard();
                        UpdateDelay(); // descrease delay when remove a row
                    }
                }
            }
        }

        // Send request to server to get new block information : return string[BlockID, NumberRow_ToRemove, GameOver]
        public string[] AskServerBlock()
        {
            string NewBlockResult = Player.AskServerBlock();
            return NewBlockResult.Split(';'); // Contain : [BlockID, NumberRow_ToRemove, GameOver]
        }

        // Check if the block can go down one time or not (can be place in board if not)
        public bool BlockCanDown()
        {
            bool canDown = true;

            if (Block.Row + Block.Blocks.GetLength(1) >= Board.GetLength(1)) // Block cannot down more than the tetris size
            {
                return false;
            }

            for (int c = Block.Column; c < Block.Column + Block.Blocks.GetLength(0); c++) // For each column of the block
            {
                for (int r = Block.Row; r < Block.Row + Block.Blocks.GetLength(1); r++)
                {
                    if (Board[c, r + 1] != 0 && Block.Blocks[c - Block.Column, r - Block.Row] != 0) // Block already on the place
                    {
                        return false;
                    }
                }
            }
            return canDown;
        }

        // Edit the console, remove current block on his position
        public void ClearCurrentBlock()
        {
            while (Editing)
            {

            }
            Editing = true;
            int left = Console.CursorLeft;
            int top = Console.CursorTop;
            for (int r = Block.Row; r < Block.Row + Block.Blocks.GetLength(1); r++)
            {
                for (int c = Block.Column; c < Block.Column + Block.Blocks.GetLength(0); c++)
                {
                    if (Block.Blocks[c - Block.Column, r - Block.Row] == 1)
                    {
                        Console.SetCursorPosition(c + 2, r);
                        Console.Write("\b");
                        Console.Write(" ");
                    }
                }
            }
            Console.SetCursorPosition(left, top);
            Editing = false;
        }

        // Edit the console, display current block at his position
        public void DisplayCurrentBlock()
        {
            while (Editing)// Wait until the other function stop editing the console
            {

            }
            Editing = true;
            int left = Console.CursorLeft;
            int top = Console.CursorTop;
            for (int r = Block.Row; r < Block.Row + Block.Blocks.GetLength(1); r++)// For every rows
            {
                for (int c = Block.Column; c < Block.Column + Block.Blocks.GetLength(0); c++)// For every column
                {
                    // Display the block
                    if (Block.Blocks[c - Block.Column, r - Block.Row] == 1)
                    {
                        Console.SetCursorPosition(c + 2, r);// Put the cursor on the right position
                        Console.Write("\b");// Remove the char
                        Console.BackgroundColor = ConsoleColor.DarkBlue;// Change color
                        Console.Write(' ');// Print a DarkBlue block on the console
                        Console.ResetColor();// Reset color
                    }
                }
            }
            Console.SetCursorPosition(left, top);// Put the cursor back on the standby position
            Editing = false;
        }

        // Display in console the board with the block
        public void DisplayBoard()
        {
            Console.Clear();
            for (int r = 0; r < Board.GetLength(1); r++)// For every rows
            {
                // DarkGrey contour
                Console.BackgroundColor = ConsoleColor.DarkGray;
                Console.Write(" ");
                Console.ResetColor();
                for (int c = 0; c < Board.GetLength(0); c++)// For every column
                {
                    if (Board[c, r] == 1)// DarkBlue block if it's a block
                    {
                        Console.BackgroundColor = ConsoleColor.DarkBlue;
                    }
                    else if (Board[c, r] == 2)// DarkRed block if it's a penalty row
                    {
                        Console.BackgroundColor = ConsoleColor.DarkRed;
                    }
                    Console.Write(' ');
                    Console.ResetColor();
                }
                // DarkGrey contour
                Console.BackgroundColor = ConsoleColor.DarkGray;
                Console.WriteLine(" ");
                Console.ResetColor();
            }
            // DarkGrey bottom
            Console.BackgroundColor = ConsoleColor.DarkGray;
            for (int c = 0; c < Board.GetLength(0) + 2; c++)
            {
                Console.Write(" ");
            }
            Console.ResetColor();
            // Display the score of every players
            DisplayPlayerScore();
        }

        //Function to display the score of every players
        public void DisplayPlayerScore()
        {
            Players = Player.GetScoreAllPlayers();// Get the players score
            Console.WriteLine("\nPlayers scores :");
            if (Players != null)
            {
                for (int i = 0; i < Players.Count; i++)// For every players
                {
                    if (Player.ID == Players[i].ID)// If it's the current player : score in green
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                    }
                    // Write the ID and the score
                    Console.WriteLine((i + 1) + ". " + Players[i].Score + " points for player ID : " + Players[i].ID);
                    if (Player.ID == Players[i].ID)
                    {
                        Console.ResetColor();
                    }
                }
            }
        }
    }
    #endregion
}