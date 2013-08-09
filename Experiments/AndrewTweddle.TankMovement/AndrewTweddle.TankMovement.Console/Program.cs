using AndrewTweddle.TankMovement.Core;
using AndrewTweddle.TankMovement.Utils;
using System;
using System.IO;

namespace AndrewTweddle.TankMovement.Console
{
    public class Program
    {
        public static string ImageFolderPath { get; private set; }
        public static bool ShouldGenerateImages
        {
            get
            {
                return !String.IsNullOrEmpty(ImageFolderPath);
            }
        }

        static void Main(string[] args)
        {
            if (args.Length > 0)
            {
                string imageFolderPath = args[0];
                if (Directory.Exists(imageFolderPath))
                {
                    ImageFolderPath = imageFolderPath;
                }
                else
                {
                    System.Console.WriteLine("No images will be generated as the argument is for a folder that does not exist: {0}", imageFolderPath);
                }
            }

            System.Console.SetWindowSize(100, System.Console.LargestWindowHeight * 8 / 10);
            System.Console.SetWindowPosition(0, 0);

            TestFourTanksTryingToMoveOffTheBoard();
            TestFourTanksTryingToMoveIntoAWall();
            TestFourTanksTryingToMoveIntoTheSameSpace();
            TestFourTanksMovingAwayFromTheCommonSpace();

            TestTwoTanksInCorridorBetweenOtherTwoTanks();
            TestTwoTanksInCorridorBetweenOtherTwoTanksAndABulletWithDestroyedTankStillBlockingOtherTank();
            TestTwoTanksInCorridorBetweenOtherTwoTanksAndABulletWithDestroyedTankNoLongBlockingOtherTank();

            TestASmallCircleOfTanks();
            TestALargerCircleOfTanks();

            TestTanksFollowingOneAnotherWithNoBarrier();
            for (int i = 0; i < Constants.TANK_COUNT; i++)
            {
                TestTanksFollowingOneAnotherWithAWallInFrontOfOneOfThem(i);
            }

            System.Console.WriteLine("Press ENTER to exit");
            System.Console.ReadLine();
        }

        private static void TestFourTanksTryingToMoveOffTheBoard()
        {
            TestHelper.DisplayTestTitle("Test 4 tanks each trying to move off a different edge of the board");

            bool[,] isAWall = new bool[20, 20];
            Unit[] tanks = new Unit[Constants.TANK_COUNT];
            Unit topUnit = new Unit
            {
                X = 10,
                Y = 2,
                Direction = Direction.UP,
                Action = Core.Action.UP
            };
            Unit rightUnit = new Unit
            {
                X = 17,
                Y = 10,
                Direction = Direction.RIGHT,
                Action = Core.Action.RIGHT
            };
            Unit bottomUnit = new Unit
            {
                X = 10,
                Y = 17,
                Direction = Direction.DOWN,
                Action = Core.Action.DOWN
            };
            Unit leftUnit = new Unit
            {
                X = 2,
                Y = 10,
                Direction = Direction.LEFT,
                Action = Core.Action.LEFT
            };
            tanks[0] = topUnit;
            tanks[1] = rightUnit;
            tanks[2] = bottomUnit;
            tanks[3] = leftUnit;

            MoveStatus[] expectedStatuses = { MoveStatus.Cancelled, MoveStatus.Cancelled, MoveStatus.Cancelled, MoveStatus.Cancelled };
            TestHelper.DisplayBoardResolveAndCheckExpectations("FourTanksMovingOffTheBoard", ref isAWall, expectedStatuses, tanks, imageFolderPath: ImageFolderPath);
        }

        private static void TestFourTanksTryingToMoveIntoAWall()
        {
            TestHelper.DisplayTestTitle("Test 4 tanks each trying to move into a wall");

            bool[,] isAWall = new bool[20, 20];
            Rectangle walls = new Rectangle(8, 8, 12, 12);
            BoardHelper.AddWallsToBoard(ref isAWall, walls);

            Unit[] tanks = new Unit[Constants.TANK_COUNT];
            Unit topUnit = new Unit
            {
                X = 10,
                Y = 5,
                Direction = Direction.DOWN,
                Action = Core.Action.DOWN
            };
            Unit rightUnit = new Unit
            {
                X = 15,
                Y = 10,
                Direction = Direction.LEFT,
                Action = Core.Action.LEFT
            };
            Unit bottomUnit = new Unit
            {
                X = 10,
                Y = 15,
                Direction = Direction.UP,
                Action = Core.Action.UP
            };
            Unit leftUnit = new Unit
            {
                X = 5,
                Y = 10,
                Direction = Direction.RIGHT,
                Action = Core.Action.RIGHT
            };
            tanks[0] = topUnit;
            tanks[1] = rightUnit;
            tanks[2] = bottomUnit;
            tanks[3] = leftUnit;

            MoveStatus[] expectedStatuses = { MoveStatus.Cancelled, MoveStatus.Cancelled, MoveStatus.Cancelled, MoveStatus.Cancelled };
            TestHelper.DisplayBoardResolveAndCheckExpectations("FourTanksMovingIntoAWall", ref isAWall, expectedStatuses, tanks, imageFolderPath: ImageFolderPath);
        }

        private static void TestFourTanksTryingToMoveIntoTheSameSpace()
        {
            TestHelper.DisplayTestTitle("Test 4 tanks each trying to move into a common space");

            bool[,] isAWall = new bool[20, 20];

            Unit[] tanks = new Unit[Constants.TANK_COUNT];
            Unit topUnit = new Unit
            {
                X = 10,
                Y = 5,
                Direction = Direction.DOWN,
                Action = Core.Action.DOWN
            };
            Unit rightUnit = new Unit
            {
                X = 15,
                Y = 10,
                Direction = Direction.LEFT,
                Action = Core.Action.LEFT
            };
            Unit bottomUnit = new Unit
            {
                X = 10,
                Y = 15,
                Direction = Direction.UP,
                Action = Core.Action.UP
            };
            Unit leftUnit = new Unit
            {
                X = 5,
                Y = 10,
                Direction = Direction.RIGHT,
                Action = Core.Action.RIGHT
            };
            tanks[0] = topUnit;
            tanks[1] = rightUnit;
            tanks[2] = bottomUnit;
            tanks[3] = leftUnit;

            MoveStatus[] expectedStatuses = { MoveStatus.Cancelled, MoveStatus.Cancelled, MoveStatus.Cancelled, MoveStatus.Cancelled };
            TestHelper.DisplayBoardResolveAndCheckExpectations("FourTanksMovingIntoTheSameSpace", ref isAWall, expectedStatuses, tanks, imageFolderPath: ImageFolderPath);
        }

        private static void TestFourTanksMovingAwayFromTheCommonSpace()
        {
            TestHelper.DisplayTestTitle("Test 4 tanks each trying to move away from a common space");

            bool[,] isAWall = new bool[20, 20];

            Unit[] tanks = new Unit[Constants.TANK_COUNT];
            Unit topUnit = new Unit
            {
                X = 10,
                Y = 5,
                Direction = Direction.UP,
                Action = Core.Action.UP
            };
            Unit rightUnit = new Unit
            {
                X = 15,
                Y = 10,
                Direction = Direction.RIGHT,
                Action = Core.Action.RIGHT
            };
            Unit bottomUnit = new Unit
            {
                X = 10,
                Y = 15,
                Direction = Direction.DOWN,
                Action = Core.Action.DOWN
            };
            Unit leftUnit = new Unit
            {
                X = 5,
                Y = 10,
                Direction = Direction.LEFT,
                Action = Core.Action.LEFT
            };
            tanks[0] = topUnit;
            tanks[1] = rightUnit;
            tanks[2] = bottomUnit;
            tanks[3] = leftUnit;

            MoveStatus[] expectedStatuses = { MoveStatus.Approved, MoveStatus.Approved, MoveStatus.Approved, MoveStatus.Approved };
            TestHelper.DisplayBoardResolveAndCheckExpectations("FourTanksMovingAwayFromACommonSpace", ref isAWall, expectedStatuses, tanks, imageFolderPath: ImageFolderPath);
        }     

        private static void TestTwoTanksInCorridorBetweenOtherTwoTanks()
        {
            TestTwoTanksInCorridorBetweenOtherTwoTanksWithOptionalBulletAndRule("TwoTanksInCorridorBetweenTwoOtherTanks",
                "Test two tanks in corridor between two other tanks", includeBullet:false);
        }

        private static void TestTwoTanksInCorridorBetweenOtherTwoTanksAndABulletWithDestroyedTankNoLongBlockingOtherTank()
        {
            TestTwoTanksInCorridorBetweenOtherTwoTanksWithOptionalBulletAndRule(
                "TwoTanksInCorridorBetweenTwoOtherTanks_TankBulletCollisionAllowsMovement",
                "Test two tanks in a corridor with a tank destroyed by a bullet allowing movement into vacated space",
                includeBullet:true, canATankMoveIntoTheSpaceLeftByATankThatJustMovedIntoABullet:true);
        }

        private static void TestTwoTanksInCorridorBetweenOtherTwoTanksAndABulletWithDestroyedTankStillBlockingOtherTank()
        {
            TestTwoTanksInCorridorBetweenOtherTwoTanksWithOptionalBulletAndRule(
                "TwoTanksInCorridorBetweenTwoOtherTanks_TankBulletCollision_MovementBlocked",
                "Test two tanks in a corridor with one destroyed by a bullet but still blocking the other",
                includeBullet: true, canATankMoveIntoTheSpaceLeftByATankThatJustMovedIntoABullet:false);
        }

        private static void TestTwoTanksInCorridorBetweenOtherTwoTanksWithOptionalBulletAndRule(string imageFileName,
            string testTitle, bool includeBullet, bool canATankMoveIntoTheSpaceLeftByATankThatJustMovedIntoABullet = true)
        {
            TestHelper.DisplayTestTitle(testTitle);

            bool[,] isAWall = new bool[25, 25];
            Unit[] tanks = new Unit[Constants.TANK_COUNT];

            Unit leftUnit = new Unit
            {
                X = 5,
                Y = 6,
                Direction = Direction.RIGHT,
                Action = Core.Action.RIGHT
            };
            Unit topUnit = new Unit
            {
                X = 10,
                Y = 10,
                Direction = Direction.DOWN,
                Action = Core.Action.DOWN
            };
            Unit rightUnit = new Unit
            {
                X = 15,
                Y = 15,
                Direction = Direction.LEFT,
                Action = Core.Action.LEFT
            };
            Unit bottomUnit = new Unit
            {
                X = 10,
                Y = 18,
                Direction = Direction.UP,
                Action = Core.Action.UP
            };
            tanks[0] = leftUnit;
            tanks[1] = topUnit;
            tanks[2] = rightUnit;
            tanks[3] = bottomUnit;

            MoveStatus[] expectedStatuses = { MoveStatus.Approved, MoveStatus.Approved, MoveStatus.Cancelled, MoveStatus.Approved };

            Bullet[] bullets = new Bullet[Constants.TANK_COUNT];
            if (includeBullet)
            {
                expectedStatuses[3] = MoveStatus.Destroyed;

                Bullet bullet = new Bullet
                {
                    X = 10,
                    Y = 15,
                    Direction = Direction.LEFT
                };
                bullets[2] = bullet;

                if (canATankMoveIntoTheSpaceLeftByATankThatJustMovedIntoABullet)
                {
                    expectedStatuses[1] = MoveStatus.Cancelled;  // tank 2 is now conflicting with 1 not 3, but that causes 1's move to be cancelled
                    expectedStatuses[0] = MoveStatus.Cancelled;
                }
                // Otherwise there is no difference to tank 2, because it is being blocked by the destruction of tank 3, not by its presence
            }
            GameRuleConfiguration ruleConfig = new GameRuleConfiguration
            {
                CanATankMoveIntoTheSpaceLeftByATankThatJustMovedIntoABullet = canATankMoveIntoTheSpaceLeftByATankThatJustMovedIntoABullet
            };

            TestHelper.DisplayBoardResolveAndCheckExpectations(imageFileName, ref isAWall, expectedStatuses, tanks, bullets, bases:null, 
                ruleConfig:ruleConfig, imageFolderPath: ImageFolderPath);
        }

        private static void TestASmallCircleOfTanks()
        {
            TestHelper.DisplayTestTitle("Test a circle of tanks following one another (body dependencies)");

            bool[,] isAWall = new bool[25, 25];
            Unit[] tanks = new Unit[Constants.TANK_COUNT];
            Unit topUnit = new Unit
            {
                X = 10,
                Y = 10,
                Direction = Direction.RIGHT,
                Action = Core.Action.RIGHT
            };
            Unit rightUnit = new Unit
            {
                X = 15,
                Y = 13,
                Direction = Direction.DOWN,
                Action = Core.Action.DOWN
            };
            Unit bottomUnit = new Unit
            {
                X = 12,
                Y = 18,
                Direction = Direction.LEFT,
                Action = Core.Action.LEFT
            };
            Unit leftUnit = new Unit
            {
                X = 7,
                Y = 15,
                Direction = Direction.UP,
                Action = Core.Action.UP
            };
            tanks[0] = topUnit;
            tanks[1] = rightUnit;
            tanks[2] = bottomUnit;
            tanks[3] = leftUnit;

            MoveStatus[] expectedStatuses = { MoveStatus.Cancelled, MoveStatus.Cancelled, MoveStatus.Cancelled, MoveStatus.Cancelled };
            TestHelper.DisplayBoardResolveAndCheckExpectations("SmallCircleOfTanks", ref isAWall, expectedStatuses, tanks, imageFolderPath: ImageFolderPath);
        }

        private static void TestALargerCircleOfTanks()
        {
            TestHelper.DisplayTestTitle("Test a circle of tanks following one another (trailing edge dependencies)");

            bool[,] isAWall = new bool[25, 25];
            Unit[] tanks = new Unit[Constants.TANK_COUNT];
            Unit topUnit = new Unit
            {
                X = 10,
                Y = 10,
                Direction = Direction.RIGHT,
                Action = Core.Action.RIGHT
            };
            Unit rightUnit = new Unit
            {
                X = 15,
                Y = 14,
                Direction = Direction.DOWN,
                Action = Core.Action.DOWN
            };
            Unit bottomUnit = new Unit
            {
                X = 11,
                Y = 19,
                Direction = Direction.LEFT,
                Action = Core.Action.LEFT
            };
            Unit leftUnit = new Unit
            {
                X = 6,
                Y = 15,
                Direction = Direction.UP,
                Action = Core.Action.UP
            };
            tanks[0] = topUnit;
            tanks[1] = rightUnit;
            tanks[2] = bottomUnit;
            tanks[3] = leftUnit;

            MoveStatus[] expectedStatuses = { MoveStatus.Cancelled, MoveStatus.Cancelled, MoveStatus.Cancelled, MoveStatus.Cancelled };
            TestHelper.DisplayBoardResolveAndCheckExpectations("LargerCircleOfTanks", ref isAWall, expectedStatuses, tanks, imageFolderPath: ImageFolderPath);
        }

        private static void TestTanksFollowingOneAnotherWithAWallInFrontOfOneOfThem(int indexOfBlockedTank)
        {
            string testName = String.Format("Test tanks where each is following the next with a wall in front of tank {0}", indexOfBlockedTank);
            TestTanksFollowingOneAnother(testName, indexOfBlockedTank);
        }

        private static void TestTanksFollowingOneAnotherWithNoBarrier()
        {
            TestTanksFollowingOneAnother("Test tanks where each is following the next with no walls blocking any of them");
        }

        private static void TestTanksFollowingOneAnother(string testTitle, int indexOfBlockedTank = -1)
        {
            TestHelper.DisplayTestTitle(testTitle);

            bool[,] isAWall = new bool[35, 35];
            MoveStatus[] expectedMoveStatuses = { MoveStatus.Approved, MoveStatus.Approved, MoveStatus.Approved, MoveStatus.Approved };
            Unit[] tanks = new Unit[Constants.TANK_COUNT];

            for (int i = 0; i < tanks.Length; i++)
            {
                Unit newUnit = new Unit
                {
                    Action = Core.Action.DOWN,
                    Direction = Direction.DOWN,
                    X = 10 + 3 * i,
                    Y = 10 + 5 * i
                };
                tanks[i] = newUnit;

                // Add a wall in front of a tank (if index is within range):
                if (indexOfBlockedTank == i)
                {
                    Rectangle wall = new Rectangle(0, newUnit.Y + 3, newUnit.X, newUnit.Y + 3);
                    BoardHelper.AddWallsToBoard(ref isAWall, wall);
                    // was: isAWall[newUnit.X, newUnit.Y + 3] = true;
                }

                // Set the expectation to failure for all tanks up to and including the one with the wall in front of it
                if (i <= indexOfBlockedTank)
                {
                    expectedMoveStatuses[i] = MoveStatus.Cancelled;
                }
            }

            string imageFileName = indexOfBlockedTank >= 0 ? String.Format("TanksFollowingOneAnother_WallBeforeTank_{0}", indexOfBlockedTank) : "TanksFollowingOneAnother";
            TestHelper.DisplayBoardResolveAndCheckExpectations(imageFileName, ref isAWall, expectedMoveStatuses, tanks, imageFolderPath: ImageFolderPath);
        }
    }
}
