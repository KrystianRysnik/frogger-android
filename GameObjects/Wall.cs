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
using Microsoft.Xna.Framework.Graphics;

namespace Frogger.GameObjects
{
    class Wall : GameObject
    {

        public Wall(Vector2 position)
        {
            Texture = FroggerGame.textureManager.wall;
            Location = new Rectangle((int)position.X, (int)position.Y, Texture.Width, Texture.Height);
        }
    }
}