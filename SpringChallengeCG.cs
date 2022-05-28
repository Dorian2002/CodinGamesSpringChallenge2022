using System;
using System.Collections.Generic;

/**
 * Auto-generated code below aims at helping you parse
 * the standard input according to the problem statement.
 **/
class Player
{
    public const int TYPE_MONSTER = 0;
    public const int TYPE_MY_HERO = 1;
    public const int TYPE_OP_HERO = 2;

    public class Entity
    {
        public int Id;
        public int Type;
        public int X, Y;
        public int ShieldLife;
        public int IsControlled;
        public int Health;
        public int Vx, Vy;
        public int NearBase;
        public int ThreatFor;
        public float ThreatLevel;

        public Entity(int id, int type, int x, int y, int shieldLife, int isControlled, int health, int vx, int vy, int nearBase, int threatFor, float threatLevel)
        {
            this.Id = id;
            this.Type = type;
            this.X = x;
            this.Y = y;
            this.ShieldLife = shieldLife;
            this.IsControlled = isControlled;
            this.Health = health;
            this.Vx = vx;
            this.Vy = vy;
            this.NearBase = nearBase;
            this.ThreatFor = threatFor;
            this.ThreatLevel = 0;
        }
    }

    static void Main(string[] args)
    {
        string[] inputs;
        inputs = Console.ReadLine().Split(' ');

        // base_x,base_y: The corner of the map representing your base
        int baseX = int.Parse(inputs[0]);
        int baseY = int.Parse(inputs[1]);
        bool isBaseTopLeft = (baseX, baseY) == (0, 0);
        int opBaseX = 17630 - baseX;
        int opBaseY = 9000 - baseY;

        int[][] defaultPositions = new int[3][];
        if (isBaseTopLeft)
        {
            defaultPositions.SetValue(new int[2] { 15200, 6200 }, 0);
            defaultPositions.SetValue(new int[2] { 2000, 4700 }, 1);
            defaultPositions.SetValue(new int[2] { 4700, 2000 }, 2);
        }
        else
        {
            defaultPositions.SetValue(new int[2] { 17630 - 15200, 9000 - 6200 }, 0);
            defaultPositions.SetValue(new int[2] { 17630 - 2000, 9000 - 4700 }, 1);
            defaultPositions.SetValue(new int[2] { 17630 - 4700, 9000 - 2000 }, 2);
        }

        // heroesPerPlayer: Always 3
        int heroesPerPlayer = int.Parse(Console.ReadLine());

        // game loop
        while (true)
        {
            inputs = Console.ReadLine().Split(' ');
            int myHealth = int.Parse(inputs[0]); // Your base health
            int myMana = int.Parse(inputs[1]); // Ignore in the first league; Spend ten mana to cast a spell

            inputs = Console.ReadLine().Split(' ');
            int oppHealth = int.Parse(inputs[0]);
            int oppMana = int.Parse(inputs[1]);

            int entityCount = int.Parse(Console.ReadLine()); // Amount of heros and monsters you can see

            List<Entity> myHeroes = new List<Entity>(entityCount);
            List<Entity> oppHeroes = new List<Entity>(entityCount);
            List<Entity> monsters = new List<Entity>(entityCount);

            for (int i = 0; i < entityCount; i++)
            {
                inputs = Console.ReadLine().Split(' ');
                int id = int.Parse(inputs[0]); // Unique identifier
                int type = int.Parse(inputs[1]); // 0=monster, 1=your hero, 2=opponent hero
                int x = int.Parse(inputs[2]); // Position of this entity
                int y = int.Parse(inputs[3]);
                int shieldLife = int.Parse(inputs[4]); // Ignore for this league; Count down until shield spell fades
                int isControlled = int.Parse(inputs[5]); // Ignore for this league; Equals 1 when this entity is under a control spell
                int health = int.Parse(inputs[6]); // Remaining health of this monster
                int vx = int.Parse(inputs[7]); // Trajectory of this monster
                int vy = int.Parse(inputs[8]);
                int nearBase = int.Parse(inputs[9]); // 0=monster with no target yet, 1=monster targeting a base
                int threatFor = int.Parse(inputs[10]); // Given this monster's trajectory, is it a threat to 1=your base, 2=your opponent's base, 0=neither
                float threatLevel = 0; // Represents the threat of a monster

                Entity entity = new Entity(
                    id, type, x, y, shieldLife, isControlled, health, vx, vy, nearBase, threatFor, threatLevel
                );

                switch (type)
                {
                    case TYPE_MONSTER:
                        monsters.Add(entity);
                        break;
                    case TYPE_MY_HERO:
                        myHeroes.Add(entity);
                        break;
                    case TYPE_OP_HERO:
                        oppHeroes.Add(entity);
                        break;
                }
            }

            bool nearMonster = IsThereNearMonster(monsters, myHeroes[0]);

            // Checks if there is a near monster and if we have enough mana to use wind spell and push monster into opponent base
            if (myMana >= 10 && nearMonster)
            {
                Console.WriteLine($"SPELL WIND {opBaseX} {opBaseY}");
            }
            else
            {
                int targetX = defaultPositions[0][0];
                int targetY = defaultPositions[1][1];

                foreach (Entity m in monsters)
                {
                    int distance = (int)Math.Sqrt((m.X - opBaseX) * (m.X - opBaseX) + (m.Y - opBaseY) * (m.Y - opBaseY));
                    if (distance < 6000)
                    {
                        targetX = m.X;
                        targetY = m.Y;
                    }
                }

                Console.WriteLine($"MOVE {targetX} {targetY}");
            }

            Entity target = BestTarget(monsters);

            // Checks for the 2 last heroes, the defensers, what's the best to do
            for (int i = 1; i < heroesPerPlayer; i++)
            {
                // Checks if there is a target else defensers take default position
                if (target != null)
                {
                    // Checks if the target monster is near base, we have enough mana and the target is not shielded then use wind spell to push it away
                    float tDistance = (int)Math.Sqrt((target.X - baseX) * (target.X - baseX) + (target.Y - baseY) * (target.Y - baseY));
                    if (tDistance <= 800 && myMana >= 10 && target.ShieldLife <= 0)
                    {
                        Console.WriteLine($"SPELL WIND {opBaseX} {opBaseY}");
                    }
                    else
                    {
                        Console.WriteLine($"MOVE {target.X} {target.Y}");
                    }
                }
                else if (monsters.Count <= 0)
                {
                    Console.WriteLine($"MOVE {defaultPositions[i][0]} {defaultPositions[i][1]}");
                }
                else
                {
                    Console.WriteLine($"WAIT");
                }
            }

            // Write an action using Console.WriteLine()
            // To debug: Console.Error.WriteLine("Debug messages...");
        }

        // Returns true if for all monsters there is one without shield and close enough from attacker
        bool IsThereNearMonster(List<Entity> monsters, Entity hero)
        {
            foreach (Entity m in monsters)
            {
                if (m.ShieldLife > 0)
                {
                    continue;
                }
                int distance = (int)Math.Sqrt((m.X - hero.X) * (m.X - hero.X) + (m.Y - hero.Y) * (m.Y - hero.Y));
                if (distance <= 1280)
                {
                    return true;
                }
            }
            return false;
        }

        // Returns the best target using entity ThreatFor and NearBase parameters
        Entity BestTarget(List<Entity> monsters)
        {
            Entity bTarget = null;
            foreach (Entity m in monsters)
            {
                float distance = (int)Math.Sqrt((m.X - baseX) * (m.X - baseX) + (m.Y - baseY) * (m.Y - baseY));
                if (m.ThreatFor == 1 && distance <= 6000)
                {
                    m.ThreatLevel = 10;
                }
                if (m.NearBase == 1 && m.ThreatFor == 1 && distance <= 6000)
                {
                    m.ThreatLevel = 20;
                }
                //Calculates distance to base and add proportionaly to threat level
                m.ThreatLevel += 1 / (distance + 1);
                //Checks if there is 
                if (m.ThreatLevel >= 10)
                {
                    if (bTarget == null)
                    {
                        bTarget = m;
                    }
                    else if (bTarget.ThreatLevel < m.ThreatLevel)
                    {
                        bTarget = m;
                    }
                }
            }
            return bTarget;
        }
    }
}
