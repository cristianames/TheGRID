﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AlumnoEjemplos.THE_GRID.Explosiones
{
    abstract class Explosion
    {
        internal float vida;
        internal float escudo;
        internal Dibujable duenio;

        abstract public void daniateEn(float danio);
    }
}
