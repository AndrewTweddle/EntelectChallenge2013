using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AndrewTweddle.BattleCity.Core;
using AndrewTweddle.BattleCity.Core.States;
using AndrewTweddle.BattleCity.Core.Collections;
using AndrewTweddle.BattleCity.Core.Elements;

namespace AndrewTweddle.BattleCity.VisualUtils
{
    public class ImageGenerator
    {
        #region Constants

        private const int CELL_WIDTH_IN_PIXELS = 4;
        private const int PEN_WIDTH = 1;

        #endregion

        public int Magnification { get; set; }
        public bool IsBackgroundChequered { get; set; }

        private int MagnificationFactor
        {
            get
            {
                if (Magnification > 0)
                {
                    return Magnification * CELL_WIDTH_IN_PIXELS;
                }
                else
                {
                    return CELL_WIDTH_IN_PIXELS;
                }
            }
        }

        public ImageGenerator()
        {
            Magnification = 1;
        }

        public Bitmap GetBitmapByName(string resourceName)
        {
            System.Reflection.Assembly asm = System.Reflection.Assembly.GetExecutingAssembly();
            string fullResourceName = string.Format("{0}.Resources.{1}", asm.GetName().Name, resourceName);
            Stream s = asm.GetManifestResourceStream(fullResourceName);
            if (s != null)
            {
                Bitmap bmp = new Bitmap(s);
                s.Close();
                return bmp;
            }
            return null;
        }

        public Bitmap GenerateBlankBoardImage(GameState gameState)
        {
            return GenerateBlankBoardImage(gameState.Walls);
        }

        public Bitmap GenerateBlankBoardImage(BitMatrix board)
        {
            Bitmap boardImage = new Bitmap(MagnificationFactor * board.Width, MagnificationFactor * board.Height);
            return boardImage;
        }

        public Bitmap GenerateGameStateImage(GameState gameState, bool fancy = false)
        {
            Bitmap boardImage = GenerateBlankBoardImage(gameState);
            using (Graphics boardGraphics = Graphics.FromImage(boardImage))
            {
                boardGraphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
                boardGraphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.None;
                DrawBoardBackground(boardGraphics, gameState.Walls);
                DrawBases(boardGraphics);
                DrawTanks(boardGraphics, gameState);
                DrawWalls(boardGraphics, gameState.Walls, fancy); // Draw walls after tanks to help detect overlap errors
                DrawBullets(boardGraphics, gameState);
            }
            return boardImage;
        }

        public Bitmap GenerateBoardImage(BitMatrix walls, bool fancy = false)
        {
            Bitmap boardImage = GenerateBlankBoardImage(walls);
            using (Graphics boardGraphics = Graphics.FromImage(boardImage))
            {
                boardGraphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
                boardGraphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.None;
                DrawBoardBackground(boardGraphics, walls);
                DrawBases(boardGraphics);
                DrawWalls(boardGraphics, walls, fancy);
            }
            return boardImage;
        }

        public void DrawBoardBackground(Graphics boardGraphics, BitMatrix board)
        {
            // Load backgrounds:
            Bitmap backgroundEven = GetBitmapByName("Background_Even.bmp");
            Bitmap backgroundOdd = GetBitmapByName("Background_Odd.bmp");
            Bitmap[] backgroundImages = new Bitmap[2];
            backgroundImages[0] = backgroundEven;
            backgroundImages[1] = IsBackgroundChequered ? backgroundOdd : backgroundEven;
            /* was:
            backgroundImages[0] = IsBackgroundChequered ? backgroundEven : backgroundOdd;
            backgroundImages[1] = backgroundOdd;
             */

            for (int x = 0; x < board.Width; x++)
            {
                for (int y = 0; y < board.Height; y++)
                {
                    bool isEvenParity = (x + y) % 2 == 0;
                    Bitmap backgroundImage = backgroundImages[isEvenParity ? 1 : 0];
                    boardGraphics.DrawImage(
                        backgroundImage,
                        MagnificationFactor * x,
                        MagnificationFactor * y,
                        MagnificationFactor,
                        MagnificationFactor);
                }
            }
        }

        public void DrawWalls(Graphics boardGraphics, BitMatrix board, bool fancy)
        {
            // Load brick images
            Bitmap[,] brickImages = new Bitmap[2, 2];
            string[] oddEvenNames = { "Even", "Odd" };
            for (int xParity = 0; xParity <= 1; xParity++)
            {
                for (int yParity = 0; yParity <= 1; yParity++)
                {
                    string oddEvenName = fancy 
                        ? String.Format("Brick_{0}X_{1}Y.bmp", oddEvenNames[xParity], oddEvenNames[yParity])
                        : "Brick_Uniform.bmp";
                    Bitmap brickBitmap = GetBitmapByName(oddEvenName);
                    brickImages[xParity, yParity] = brickBitmap;
                }
            }

            for (short x = 0; x < board.Width; x++)
            {
                for (short y = 0; y < board.Height; y++)
                {
                    if (board[x, y])
                    {
                        int isXEven = x % 2 == 0 ? 1 : 0;
                        int isYEven = y % 2 == 0 ? 1 : 0;
                        Bitmap brickImage = brickImages[isXEven, isYEven];
                        boardGraphics.DrawImage(
                            brickImage,
                            MagnificationFactor * x,
                            MagnificationFactor * y,
                            MagnificationFactor,
                            MagnificationFactor);
                    }
                }
            }
        }

        public void DrawBases(Graphics boardGraphics)
        {
            // Load base images:
            Bitmap[] baseImages = new Bitmap[2];
            baseImages[0] = GetBitmapByName("Base_GreenBlue.bmp");
            baseImages[1] = GetBitmapByName("Base_BrownPurple.bmp");

            for (int i = 0; i < Game.Current.Players.Length; i++)
            {
                Player player = Game.Current.Players[i];
                Base @base = player.Base;
                if (@base != null)
                {
                    Bitmap baseImage = baseImages[i % 2];
                    boardGraphics.DrawImage(baseImage,
                        MagnificationFactor * @base.Pos.X,
                        MagnificationFactor * @base.Pos.Y,
                        MagnificationFactor,
                        MagnificationFactor);
                }
            }
        }

        public void DrawTanks(Graphics boardGraphics, GameState gameState)
        {
            int dirCount = Enum.GetValues(typeof(Direction)).Length;

            // Load tank images:
            string[] tankColours = { "Blue", "Purple", "Green", "Yellow" };
            Bitmap[,] tankImages = new Bitmap[Constants.TANK_COUNT, dirCount];
            for (int t = 0; t < Constants.TANK_COUNT; t++)
            {
                for (int d = 0; d < dirCount; d++)
                {
                    Direction dir = (Direction)d;
                    string tankImageName = String.Format("Tank_{0}_{1}.bmp", tankColours[t], dir);
                    tankImages[t, d] = GetBitmapByName(tankImageName);
                }
            }

            for (int i = 0; i < Constants.TANK_COUNT; i++)
            {
                MobileState tankState = gameState.MobileStates[i];
                if (tankState.IsActive)
                {
                    // TODO: Add a configuration option to specify whether to choose direction based on the Direction or Action property

                    // Choose the image with the correct color and rotation:
                    Bitmap tankImage = tankImages[i, (int)tankState.Dir];
                    boardGraphics.DrawImage(tankImage,
                        MagnificationFactor * (tankState.Pos.X - Constants.TANK_EXTENT_OFFSET),
                        MagnificationFactor * (tankState.Pos.Y - Constants.TANK_EXTENT_OFFSET),
                        MagnificationFactor * Constants.SEGMENT_SIZE,
                        MagnificationFactor * Constants.SEGMENT_SIZE);

                    // TODO: Provide a configuration option to display the tank's action as a character above the tank
                }
            }
        }

        public void DrawBullets(Graphics boardGraphics, GameState gameState)
        {
            int dirCount = Enum.GetValues(typeof(Direction)).Length;

            // Load bullets:
            Bitmap[] bulletImages = new Bitmap[dirCount];
            for (int d = 0; d < dirCount; d++)
            {
                Direction dir = (Direction)d;
                string bulletImageName = String.Format("Bullet_{0}.bmp", dir);
                bulletImages[d] = GetBitmapByName(bulletImageName);
            }

            for (int i = Constants.MIN_BULLET_INDEX; i <= Constants.MAX_BULLET_INDEX; i++)
            {
                MobileState bulletState = gameState.MobileStates[i];
                if (bulletState.IsActive && bulletState.Dir != Direction.NONE)
                {
                    Image bulletImage = bulletImages[(byte)bulletState.Dir];
                    boardGraphics.DrawImage(
                        bulletImage,
                        MagnificationFactor * bulletState.Pos.X,
                        MagnificationFactor * bulletState.Pos.Y,
                        MagnificationFactor,
                        MagnificationFactor);
                }
            }
        }

        public void DrawSegmentMatrixOverlay(Bitmap boardImage, BitMatrix board, Matrix<SegmentState> segStateMatrix, Axis axisOfMovement)
        {
            using (Graphics boardGraphics = Graphics.FromImage(boardImage))
            {
                DrawSegmentMatrixOverlay(boardGraphics, board, segStateMatrix, axisOfMovement);
            }
        }

        public void DrawSegmentMatrixOverlay(Graphics boardGraphics, BitMatrix board, Matrix<SegmentState> segStateMatrix, Axis axisOfMovement)
        {
            int[] xOffset = new int[2];
            int[] yOffset = new int[2];

            switch (axisOfMovement)
            {
                case Axis.Horizontal:
                    xOffset[0] = (MagnificationFactor - 1) / 2;
                    xOffset[1] = (MagnificationFactor - 1) / 2;
                    yOffset[0] = 0;
                    yOffset[1] = MagnificationFactor - 1;
                    break;
                case Axis.Vertical:
                    xOffset[0] = 0;
                    xOffset[1] = MagnificationFactor - 1;
                    yOffset[0] = (MagnificationFactor - 1) / 2;
                    yOffset[1] = (MagnificationFactor - 1) / 2;
                    break;
            }

            using (Pen clearPen = new Pen(Color.White, PEN_WIDTH))
            using (Pen shootableWallPen = new Pen(Color.Yellow, PEN_WIDTH))
            using (Pen unshootableWallPen = new Pen(Color.Magenta, PEN_WIDTH))
            using (Pen outOfBoundsPen = new Pen(Color.Blue))
            {
                Pen penToUse;

                for (short x = 0; x < segStateMatrix.Width; x++)
                {
                    int xPos = x * MagnificationFactor;
                    for (short y = 0; y < segStateMatrix.Height; y++)
                    {
                        int yPos = y * MagnificationFactor;
                        switch (segStateMatrix[x,y])
                        {
                            case SegmentState.Clear:
                                penToUse = clearPen;
                                break;
                            case SegmentState.ShootableWall:
                                penToUse = shootableWallPen;
                                break;
                            case SegmentState.UnshootablePartialWall:
                                penToUse = unshootableWallPen;
                                break;
                            default: 
                                // case SegmentState.OutOfBounds:
                                penToUse = outOfBoundsPen;
                                break;
                        }
                        boardGraphics.DrawLine(penToUse,
                            xPos + xOffset[0], yPos + yOffset[0],
                            xPos + xOffset[1], yPos + yOffset[1]);
                    }
                }
            }
        }
    }
}
