using System;
using System.Threading;

namespace starfighter
{
    class Program
    {
        public static byte width = (byte)Console.WindowWidth;
        public static byte height = (byte)Console.WindowHeight;
        public const byte MAX_ENEMIES = 4;
        const short ENEMY_WMIN = 1000, ENEMY_WMAX = 2000;
        const byte ENEMY_DAMAGE = 5;
        static Player pl;
        static Enemy[] nme;
        static Thread thd, thd2;
        static Random rnd = new Random();
        static int health;
        static int score;

        static void Main(string[] args)
        {
            health = 100;
            score = 0;
            pl = new Player();
            pl.DrawPlayer();
            nme = new Enemy[MAX_ENEMIES];
            thd = new Thread(new ThreadStart(DrawStuff));
            thd2 = new Thread(new ThreadStart(GenerateEnemy));
            thd.Start();
            thd2.Start();
            Console.CursorVisible = false;
            Console.Title = "Starfighter";
            ConsoleKey key;
            while (health > 0)
            {
                key = Console.ReadKey(true).Key;
                if (key == ConsoleKey.LeftArrow)
                    pl.Left();
                else if (key == ConsoleKey.RightArrow)
                    pl.Right();
                else if (key == ConsoleKey.Spacebar)
                    pl.Shoot();
            }
            thd2.Abort();
            thd.Abort();
            while (Console.ReadKey().Key != ConsoleKey.Escape) ;
        }

        static void DrawStuff()
        {
            while (true)
            {
                Console.Clear();
                DrawBackground();
                DrawScore();
                pl.DrawPlayer();
                pl.DrawProj();
                DrawEnemy();
                score += pl.CheckCollisions(nme);
                health -= pl.CheckPlayerDamage(nme);
                if (health < 1)
                {
                    health = 0;
                    DrawScore();
                    Console.SetCursorPosition(pl.RightBorder + 4, Program.height - 14);
                    Console.Write("Game Over");
                    return;
                }
                Thread.Sleep(50);
            }
        }

        static void DrawBackground()
        {
            for (int i = 0; i < Program.height; i++)
            {
                Console.SetCursorPosition(pl.LeftBorder - 1, i);
                Console.Write("|");
                Console.SetCursorPosition(pl.RightBorder + 1, i);
                Console.Write("|");
            }
        }

        static void GenerateEnemy()
        {
            while (true)
            {
                for (int i = 0; i < MAX_ENEMIES; i++)
                {
                    if (nme[i] == null)
                    {
                        nme[i] = new Enemy(pl.LeftBorder + 2, pl.RightBorder - 2);
                        Thread.Sleep(rnd.Next(ENEMY_WMIN, ENEMY_WMAX));
                    }
                }
            }
        }

        static void DrawEnemy()
        {
            for (int i = 0; i < MAX_ENEMIES; i++)
                if (nme[i] != null)
                    if (!nme[i].DrawEnemy())
                    {
                        nme[i] = null;
                        health -= ENEMY_DAMAGE;
                    }
        }

        static void DrawScore()
        {
            Console.SetCursorPosition(pl.RightBorder + 4, Program.height - 18);
            Console.Write("Health: " + health);
            Console.SetCursorPosition(pl.RightBorder + 4, Program.height - 16);
            Console.Write("Score: " + score);
        }
    }

    class Player
    {
        const byte MAX_PROJECTILES = 3;
        const byte MAX_MOVE = 20;
        const byte ENEMY_SCORE = 200;
        const byte ENEMY_DAMAGE = 10;
        byte pos;
        byte left;
        byte right;
        byte bottom;
        Projectile[] proj;

        public Player()
        {
            pos = 0;
            left = (byte)(Program.width / 2 - MAX_MOVE / 2);
            right = (byte)(left + MAX_MOVE + 3);
            bottom = (byte)Program.height;
            proj = new Projectile[MAX_PROJECTILES];
            DrawPlayer();
        }

        public byte LeftBorder { get { return left; } }
        public byte RightBorder { get { return right; } }

        public void Left()
        {
            if (pos > 1)
                pos--;
        }

        public void Right()
        {
            if (pos < MAX_MOVE)
                pos++;
        }

        public void Shoot()
        {
            for (int i = 0; i < MAX_PROJECTILES; i++)
            {
                if (proj[i] == null)
                {
                    proj[i] = new Projectile((sbyte)(left + pos + 1),
                        (sbyte)(Program.height - 3));
                    break;
                }
            }
        }

        public void DrawProj()
        {
            for (int i = 0; i < MAX_PROJECTILES; i++)
                if (proj[i] != null)
                    if (!proj[i].DoProjectile())
                        proj[i] = null;
        }

        public int CheckCollisions(Enemy[] nme)
        {
            int score = 0;
            for (int i = 0; i < Program.MAX_ENEMIES; i++)
                if (nme[i] != null)
                    for (int i2 = 0; i2 < MAX_PROJECTILES; i2++)
                        if (proj[i2] != null)
                            if (nme[i].CheckCollision(proj[i2].X, proj[i2].Y))
                            {
                                proj[i2] = null;
                                nme[i] = null;
                                score += ENEMY_SCORE;
                                break;
                            }
            return score;
        }

        public int CheckPlayerDamage(Enemy[] nme)
        {
            int health = 0;
            for (int i = 0; i < Program.MAX_ENEMIES; i++)
                if (nme[i] != null)
                    if (nme[i].CheckCollision((byte)(left + pos + 1), (byte)(bottom - 2)) ||
                    nme[i].CheckCollision((byte)(left + pos), (byte)(bottom - 1)) ||
                    nme[i].CheckCollision((byte)(left + pos + 1), (byte)(bottom - 1)) ||
                    nme[i].CheckCollision((byte)(left + pos + 2), (byte)(bottom - 1)))
                    {
                        nme[i] = null;
                        health += ENEMY_DAMAGE;
                    }
            return health;
        }

        public void DrawPlayer()
        {
            Console.SetCursorPosition(left + pos + 1, bottom - 2);
            Console.Write("X");
            Console.SetCursorPosition(left + pos, bottom - 1);
            Console.Write("XXX");
        }
    }

    class Projectile
    {
        byte x, y;
        public byte X { get { return x; } }
        public byte Y { get { return y; } }

        public Projectile(sbyte x, sbyte y)
        {
            this.x = (byte)x;
            this.y = (byte)y;
        }

        public bool DoProjectile()
        {
            Console.SetCursorPosition(x, y);
            Console.Write(" ");
            if (y < 1) return false;
            y--;
            Console.SetCursorPosition(x, y);
            Console.Write("!");
            return true;
        }
    }

    class Enemy
    {
        const byte MAX_WAIT = 5;
        byte wait = 0;
        byte x, y;
        static Random rnd = new Random();

        public Enemy(int min, int max)
        {
            x = (byte)rnd.Next(min, max);
            y = 0;
        }

        public bool DrawEnemy()
        {
            Console.SetCursorPosition(x, y);
            Console.Write(" ");
            if (y == Program.height - 1) return false;
            if (wait > 0)
                wait--;
            else
            {
                wait = MAX_WAIT;
                y++;
            }
            Console.SetCursorPosition(x, y);
            Console.Write("E");
            return true;
        }

        public bool CheckCollision(byte x, byte y)
        {
            return this.x == x && this.y > y - 2 && this.y <= y;
        }
    }
}