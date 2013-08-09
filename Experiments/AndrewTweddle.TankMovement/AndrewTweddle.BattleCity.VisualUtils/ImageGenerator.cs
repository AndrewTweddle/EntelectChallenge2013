using AndrewTweddle.TankMovement.Core;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AndrewTweddle.BattleCity.VisualUtils
{
    public class ImageGenerator
    {
        #region Constants

        private const int CELL_WIDTH_IN_PIXELS = 4;

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

        public Bitmap GenerateBoardImage(ref bool[,] isWall, ref Unit[] units, ref Bullet[] bullets, ref Base[] bases)
        {
            Bitmap boardImage = new Bitmap(MagnificationFactor * isWall.GetLength(0), MagnificationFactor * isWall.GetLength(1));
            try
            {
                using (Graphics boardGraphics = Graphics.FromImage(boardImage))
                {
                    boardGraphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
                    boardGraphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.None;

                    // Load all the images required up front:
                    int dirCount = Enum.GetValues(typeof(Direction)).Length;

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

                    // Load base images:
                    Bitmap[] baseImages = new Bitmap[2];
                    baseImages[0] = GetBitmapByName("Base_GreenBlue");
                    baseImages[1] = GetBitmapByName("Base_BrownPurple");

                    // Load tank images:
                    string[] tankColours = { "Blue", "Green", "Purple", "Yellow" };
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

                    // Load bullets:
                    Bitmap[] bulletImages = new Bitmap[dirCount];
                    for (int d = 0; d < dirCount; d++)
                    {
                        Direction dir = (Direction)d;
                        string bulletImageName = String.Format("Bullet_{0}.bmp", dir);
                        bulletImages[d] = GetBitmapByName(bulletImageName);
                    }

                    // Load bricks
                    Bitmap[,] brickImages = new Bitmap[2, 2];
                    string[] oddEvenNames = { "Even", "Odd" };
                    for (int xParity = 0; xParity <= 1; xParity++)
                    {
                        for (int yParity = 0; yParity <= 1; yParity++)
                        {
                            string oddEvenName = String.Format("Brick_{0}X_{1}Y.bmp", oddEvenNames[xParity], oddEvenNames[yParity]);
                            Bitmap brickBitmap = GetBitmapByName(oddEvenName);
                            brickImages[xParity, yParity] = brickBitmap;
                        }
                    }

                    // Draw the board background:
                    for (int x = 0; x < isWall.GetLength(0); x++)
                    {
                        for (int y = 0; y < isWall.GetLength(1); y++)
                        {
                            bool isEvenParity = (x + y) % 2 == 0;
                            Bitmap backgroundImage = backgroundImages[isEvenParity ? 1 : 0];
                            boardGraphics.DrawImage(backgroundImage, MagnificationFactor * x, MagnificationFactor * y, MagnificationFactor, MagnificationFactor);
                        }
                    }

                    // Add bases to the map
                    if (bases != null)
                    {
                        for (int i = 0; i < bases.Length; i++)
                        {
                            Base @base = bases[i];
                            if (@base != null)
                            {
                                Bitmap baseImage = baseImages[i % 2];
                                boardGraphics.DrawImage(baseImage, 
                                    MagnificationFactor * @base.X, 
                                    MagnificationFactor * @base.Y, 
                                    MagnificationFactor, 
                                    MagnificationFactor);
                            }
                        }
                    }

                    // Add tanks to the map:
                    for (int i = 0; i < Constants.TANK_COUNT; i++)
                    {
                        Unit tank = units[i];
                        if (tank != null)
                        {
                            // TODO: Add a configuration option to specify whether to choose direction based on the Direction or Action property

                            // Choose the image with the correct color and rotation:
                            Bitmap tankImage = tankImages[i, (int)tank.Direction];
                            boardGraphics.DrawImage(tankImage,
                                MagnificationFactor * (tank.X - 2),
                                MagnificationFactor * (tank.Y - 2),
                                MagnificationFactor * Constants.TANK_WIDTH,
                                MagnificationFactor * Constants.TANK_WIDTH);

                            // TODO: Provide a configuration option to display the tank's action as a character above the tank
                        }
                    }

                    // Mark wall spaces. Do after tanks, to detect errors with walls overlapping with tanks:
                    for (int x = 0; x < isWall.GetLength(0); x++)
                    {
                        for (int y = 0; y < isWall.GetLength(1); y++)
                        {
                            if (isWall[x, y])
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

                    // Mark bullets:
                    if (bullets != null)
                    {
                        foreach (Bullet bullet in bullets)
                        {
                            if (bullet != null && bullet.Direction != Direction.NONE)
                            {
                                Image bulletImage = bulletImages[(int) bullet.Direction];
                                boardGraphics.DrawImage(
                                    bulletImage,
                                    MagnificationFactor * bullet.X,
                                    MagnificationFactor * bullet.Y,
                                    MagnificationFactor,
                                    MagnificationFactor);
                            }
                        }
                    }
                }
            }
            catch (Exception exc)
            {
                throw;
            }
            return boardImage;
        }
    }
}
