using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using Windows.UI;
using Emmellsoft.IoT.Rpi.SenseHat;
using RichardsTech.Sensors;

namespace RPi.SenseHat.Demo.Demos
{
    public class SideScroller : SenseHatDemo
    {

        public SideScroller(ISenseHat senseHat)
            : base(senseHat)
        {
        }
        public static int SCORE = 0;
        public static int SPEED = 800;
        public static int HIGH_SCORE = 0;

        public override void Run()
        {
            while(true) // Loops a game
            {
                SCORE = 0;
                SPEED = 800;

Player currentPlayer = new Player(0, 4);
                Enemy[] enemies = new Enemy[6] { new Enemy(8, 3), new Enemy(12, 5),
                new Enemy(16, 7), new Enemy(20, 3), new Enemy(24, 6), new Enemy(28,  2) };


                SenseHat.Display.Clear();

                while (true) //Loops a tick
                {
                    Vector3 piTilt = new Vector3(0, 0, 0);

                    if (SenseHat.Sensors.ImuSensor.Update())
                        if (SenseHat.Sensors.Acceleration.HasValue)
                            piTilt = SenseHat.Sensors.Acceleration.Value;

                    SenseHat.Display.Clear(); // Clear the screen.

                    currentPlayer.updatePosition(piTilt); // Move the pixel.

                    SenseHat.Display.Screen[currentPlayer.x, currentPlayer.y] = Colors.Green; // Draw the player.

                    foreach (Enemy badGuy in enemies) //Loop through enemies and draw
                    {
                        badGuy.move();
                        if (badGuy.x > 7)
                            continue;

                        SenseHat.Display.Screen[badGuy.x, badGuy.y] = Colors.Red;
                    }

                    SenseHat.Display.Update(); // Update the physical display.

                    Sleep(TimeSpan.FromMilliseconds(SPEED)); // Take a short nap.

                    if (currentPlayer.isDead(enemies))
                    {
                        SenseHat.Display.Clear();
                        SenseHat.Display.Update(); // Update the physical display.
                        Sleep(TimeSpan.FromMilliseconds(300));

                        for (int i = 4; i > 0; i--)
                        {
                            SenseHat.Display.Screen[currentPlayer.x, currentPlayer.y] = Colors.Green; // Draw the player.
                            SenseHat.Display.Update(); // Update the physical display.
                            Sleep(TimeSpan.FromMilliseconds(300));

                            SenseHat.Display.Screen[currentPlayer.x, currentPlayer.y] = Colors.Red; // Draw the player.
                            SenseHat.Display.Update(); // Update the physical display.
                            Sleep(TimeSpan.FromMilliseconds(300));
                        }

                        
                        break;
                    }
                    if (SenseHat.Joystick.Update() && (SenseHat.Joystick.EnterKey == KeyState.Pressing))
                    {
                        // The middle button is just pressed.
                        while (true)
                        {
                            if (SenseHat.Joystick.Update() && (SenseHat.Joystick.EnterKey == KeyState.Pressing))
                            {
                                // The middle button is just pressed.
                                break;
                            }
                        }

                    }
                }

                String scoreString = "Game Over! Score: " + SCORE + "!";
                if (SCORE > HIGH_SCORE)
                {
                    HIGH_SCORE = SCORE;
                    scoreString += " New High Score !!!";
                }
                BwScrollText textScroller = new BwScrollText(SenseHat, scoreString);
                textScroller.Run();
                SenseHat.Display.Clear();
                SenseHat.Display.Update();
            }
        }


        public class Player
        {
            public int x { get; set; }
            public int y { get; set; }

            public Player(int x, int y)
            {
                this.x = x;
                this.y = y;
            }

            public void updatePosition(Vector3 tilt)
            {
                //read gyro, if tilted x amount in one direction or another update Y posistion
                
                if (tilt.Y >= 0.1)
                    y++;
                else if (tilt.Y <= -0.1)
                    y--;

                if (y < 0)
                    y = 0;
                else if (y > 7)
                    y = 7;
            }
            public Boolean isDead(Enemy[] enemies)
            {
                //Loop through 'enemies' array, return true if Player x and y are equal with any enemy
                foreach (Enemy badGuy in enemies)
                {
                    if (badGuy.y == this.y && badGuy.x == this.x)
                        return true;
                }
                return false;
            }
        }

        public class Enemy
        {
            public int x { get; set; }
            public int y { get; set; }

            private static Random random = new Random();

            public Enemy(int x, int y)
            {
                this.x = x;
                this.y = y;
            }

            public void move()
            {
                x--;
                if (x < 0)
                {
                    x = 20;
                    y = random.Next(0, 8);
                    SCORE++;
                    if (SCORE % 5 == 0 && SPEED > 100) //Increase the speed of things by 100 MS every 5 points
                        SPEED -= 100;
                }
            }
        }


       }
}
