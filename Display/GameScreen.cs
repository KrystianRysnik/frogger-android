﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Frogger.GameObjects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Frogger.Display
{
    class GameScreen : Screen
    {
        HUD hud;
        Frog player;
        List<Wall> walls = new List<Wall>();
        List<Car> cars = new List<Car>();
        List<Turtle[]> groupOfTurtles = new List<Turtle[]>();
        List<Log> logs = new List<Log>();
        List<Meta> meta = new List<Meta>();

        int Level { set; get; } // Show max 15
        int Life { set; get; }
        int MetaReach = 0;
        int rewardForJump = 0;

        bool isExtraSound = false;
                
        public GameScreen(ContentManager theContent, EventHandler theScreenEvent) : base(theScreenEvent)
        {
            hud = new HUD();
            player = new Frog("green", new Vector2(8 * (FroggerGame.textureManager.frogGreen.Width/6), 14 * 52));
            rewardForJump = player.Location.Y - FroggerGame.textureManager.frogGreen.Height;

            for (int i = 0; i < 5; i++)
            {
                meta.Add(new Meta(new Vector2(23 + (FroggerGame.textureManager.meta.Width/2 * i + 98 * i), 52+49)));
            }

            for (int x = 0; x < 14; x++)
            {
                walls.Add(new Wall(new Vector2(x * FroggerGame.textureManager.wall.Width, 8 * 52)));
            }

            for (int x = 0; x < 14; x++)
            {
                walls.Add(new Wall(new Vector2(x * FroggerGame.textureManager.wall.Width, 14 * 52)));
            }

            SpawnTurtles(7, 3, 52, 52, -52);
            SpawnTurtles(4, 2, 78, 104, -52);
            SpawnLogs(6, 3, 104, 104, FroggerGame.WIDTH + 52*3);
            SpawnLogs(3, 4, 52, 0, FroggerGame.WIDTH + 52*4);
            SpawnLogs(5, 6, 104, 104, FroggerGame.WIDTH + 52 * 3);

            SpawnCars(9, 2, (int)(5.5 * 52), FroggerGame.WIDTH / 2, -104);
            SpawnCars(10, 1, 4 * 52, -108, FroggerGame.WIDTH);
            SpawnCars(11, 3, 4 * 52, FroggerGame.WIDTH / 2 - 52, -78);
            SpawnCars(12, 2, 4 * 52, FroggerGame.WIDTH / 2 - 52, FroggerGame.WIDTH + 52);
            SpawnCars(13, 3, (int)(4.5 * 52), FroggerGame.WIDTH / 2 - 52, -78);
        }

        public override void Update(GameTime theTime)
        {
            if (hud.isGameOver && hud.slidePosition.X == 0)
            {
                FroggerGame.scoreManager.SaveScore(hud.Score);
                userScore = hud.Score;            
                ScreenEvent.Invoke(this, new EventArgs());
                return;
            }

            if (FroggerGame.audioManager.squashInstance.State != SoundState.Playing
                && FroggerGame.audioManager.plunkInstance.State != SoundState.Playing
                && FroggerGame.audioManager.themeInstance.State == SoundState.Paused)
            {
                if (isExtraSound == true && FroggerGame.audioManager.extraInstance.State != SoundState.Playing)
                {
                    FroggerGame.audioManager.themeInstance.Resume();
                    isExtraSound = false;
                }
                else if (isExtraSound == false)
                {
                    FroggerGame.audioManager.extraInstance.Play();
                    isExtraSound = true;
                }
            }
         

            if (!player.IsDead)
            {
                hud.Update(theTime);
                if (!hud.isGameOver)
                {
                    player.Update(theTime);
                }

                if (hud.isTimeEnd)
                {
                    RestartPlayerLocation(true, false);            
                    hud.isTimeEnd = false;
                    hud.Life--;
                }

                foreach (Car car in cars)
                {
                    car.Update(theTime);
                    if (car.Location.Intersects(player.Location))
                    {
                        player.IsHit = true;
                    }
                }
                foreach (Turtle[] turtles in groupOfTurtles)
                {
                    foreach (Turtle turtle in turtles)
                    {
                        turtle.Update(theTime);
                        if (turtle.Location.Intersects(player.Location))
                        {
                            player.IsCollision = true;
                        }
                    }
                    if (player.IsCollision)
                    {
                        if (turtles[0].Location.X < player.Location.X + 15 && turtles[0].Location.Y == player.Location.Y
                            && player.Location.X - 15 < turtles[turtles.Length - 1].Location.X)
                        {
                            player.IsStick = true;
                            player.StickMove(turtles[0].Position);
                            player.IsCollision = false;
                        }
                    }
                }
                foreach (Log log in logs)
                {
                    log.Update(theTime);
                    if (log.Location.Intersects(player.Location))
                    {
                        if (player.ShouldIStickToThisObject(log))
                        {
                            player.IsStick = true;
                            player.StickMove(log.Position);
                        }
                    }
                }
                foreach (Meta m in meta)
                {
                    if (m.Location.Intersects(player.Location) && !m.IsShow && player.Location.Y < 3 * 52)
                    {
                        m.IsShow = true;
                        MetaReach++;
                        hud.isReachMeta = true;
                        hud.Score += 200;
                        RestartPlayerLocation(false, false);
                    }
                  
                    if (MetaReach == 5)
                    {
                        NewStage();
                    }
                }

                if (player.Location.Y < 3 * 52 && hud.isReachMeta == false)
                {
                    player.IsHit = true;
                }

                if (player.IsHit == true)
                {
                    RestartPlayerLocation(true, false);
                    player.IsHit = false;
                }

                if (player.IsStick == true)
                {
                    player.Position += new Vector2((int)player.Move.X, player.Move.Y);
                    player.moveVector += new Vector2((int)player.Move.X, player.Move.Y);
                    player.IsStick = false;
                }
                else if (player.Location.Y >= 3 * 52 && player.Location.Y <= 7 * 52 && !player.IsStick)
                {
                    RestartPlayerLocation(true, true);
                }
                CheckRewardForJump();
            }
            else
            {
                player.Update(theTime);
                logs.ForEach(log => log.Update(theTime));
                cars.ForEach(car => car.Update(theTime));

                foreach (Turtle[] turtles in groupOfTurtles)
                {
                    foreach (Turtle turtle in turtles)
                    {
                        turtle.Update(theTime);
                    }
                }
            }
        }

        public override void Draw(SpriteBatch theBatch)
        {
            walls.ForEach(wall => wall.Draw(theBatch));
            cars.ForEach(car => car.Draw(theBatch));
            logs.ForEach(log => log.Draw(theBatch));
            meta.ForEach(meta => meta.Draw(theBatch));

            foreach (Turtle[] turtles in groupOfTurtles)
            {
                foreach (Turtle turtle in turtles)
                {
                    turtle.Draw(theBatch);
                }
            }

            for (int i = 0; i < 5; i++)
            {
                theBatch.Draw(FroggerGame.textureManager.forest, new Vector2(i * FroggerGame.textureManager.forest.Width, 52), Color.White);
            }
          
            player.Draw(theBatch);
            hud.Draw(theBatch);

            base.Draw(theBatch);
        }

        public void StartGame()
        {
            isGameStarted = true;
            isGameOver = false;
            RestartPlayerLocation(false, false);
        }

        private void NewStage()
        {            
            hud.Level++;
            RestartPlayerLocation(false, false);
            meta.ForEach(m => m.IsShow = false);
            MetaReach = 0;
        }

        private void RestartPlayerLocation(bool isDead, bool isDrown)
        {
            if (isDead && isDrown)
            {
                player.IsDrown = true;
                player.IsDead = true;
                hud.Life--;
                hud.Time = 60f;
                FroggerGame.audioManager.themeInstance.Pause();
                FroggerGame.audioManager.plunkInstance.Play();
            }
            else if (isDead && !isDrown)
            {
                player.IsDead = true;
                hud.Life--;
                hud.Time = 60f;
                FroggerGame.audioManager.themeInstance.Pause();
                FroggerGame.audioManager.squashInstance.Play();
            }
            else
            { 
               player.RestartLocation();
            }
            rewardForJump = player.Location.Y - FroggerGame.textureManager.frogGreen.Height;
        }

        private void CheckRewardForJump()
        {
            if (rewardForJump >= player.Location.Y)
            {
                hud.Score += 10;
                rewardForJump -= FroggerGame.textureManager.frogGreen.Height;
            }
        }

        private void SpawnLogs(int row, int length, int spaceBetween, int startFrom, int restart)
        {
            if (length == 6)
            {
                for (int i = 0; i < 3; i++)
                {
                    logs.Add(new Log(length, new Vector2(startFrom, row * 52), restart));
                    startFrom += (6*52) + spaceBetween;
                }
            }
            else if (length == 4)
            {
                for (int i = 0; i < 4; i++)
                {
                    logs.Add(new Log(length, new Vector2(startFrom, row * 52), restart));
                    startFrom += (4 * 52) + spaceBetween;
                }
            }
            else
            {
                for (int i = 0; i < 4; i++)
                {
                    logs.Add(new Log(length, new Vector2(startFrom, row * 52), restart));
                    startFrom += (3 * 52) + spaceBetween;
                }
            }
        }

        private void SpawnCars(int row, int length, int spaceBetween, int startFrom, int restart)
        {
            for (int i = 0; i < length; i++)
            {
                cars.Add(new Car(row, new Vector2(startFrom, row * 52), restart));
                startFrom += spaceBetween;
            }
         }       
        
        private void SpawnTurtles(int row, int length, int spaceBetween, int startFrom, int restart)
        {
            Turtle[] turtles = new Turtle[length];
            for (int i = 0; i < length; i++)
            {
                turtles[i] = new Turtle("diver", new Vector2(startFrom, row * 52), restart);
                startFrom += 52;
            }
            groupOfTurtles.Add(turtles);
            turtles = new Turtle[length];
            startFrom += spaceBetween;
            for (int i = 0; i < length; i++)
            {
                turtles[i] = new Turtle("normal", new Vector2(startFrom, row * 52), restart);
                startFrom += 52;
            }
            groupOfTurtles.Add(turtles);
            turtles = new Turtle[length];
            startFrom += spaceBetween;
            for (int i = 0; i < length; i++)
            {
                turtles[i] = new Turtle("normal", new Vector2(startFrom, row * 52), restart);
                startFrom += 52;
            }
            groupOfTurtles.Add(turtles);
            turtles = new Turtle[length];
            startFrom += spaceBetween;
            for (int i = 0; i < length; i++)
            {
                turtles[i] = new Turtle("normal", new Vector2(startFrom, row * 52), restart);
                startFrom += 52;
            }
        }    
    }
}