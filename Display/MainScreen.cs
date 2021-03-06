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
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input.Touch;

namespace Frogger.Display
{
    class MainScreen : Screen
    {
        public MainScreen(ContentManager theContent, EventHandler theScreenEvent) : base(theScreenEvent)
        {
            FroggerGame.audioManager.coin.Play();
        }

        public override void Update(GameTime theThime)
        {
            var touchCol = TouchPanel.GetState();

            foreach (var touch in touchCol)
            {
                if (touch.State == TouchLocationState.Pressed)
                {
                    ScreenEvent.Invoke(this, new EventArgs());
                }
            }
        }

        public override void Draw(SpriteBatch theBatch)
        {
            theBatch.DrawString(FroggerGame.eightBitFont, "TAP", new Vector2((FroggerGame.WIDTH / 2) - (1.5F * 28), 4 * 52), Color.White);
            theBatch.DrawString(FroggerGame.eightBitFont, "SCREEN", new Vector2((FroggerGame.WIDTH / 2) - (3 * 28), 6.5f * 52), new Color(255, 99, 255));
            theBatch.DrawString(FroggerGame.eightBitFont, "ONE PLAYER ONLY", new Vector2((FroggerGame.WIDTH / 2) - (7.5F * 28), 8.5f * 52), Color.White);
            theBatch.DrawString(FroggerGame.eightBitFont, "ONE EXTRA FROG 20000 PTS", new Vector2((FroggerGame.WIDTH / 2) - (12 * 28), 10 * 52), Color.Red);
        }
    }
}