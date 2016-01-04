using System;
using System.Collections.Generic;
using System.Threading;
using System.Reflection;
using System.Windows.Forms;
using System.Collections;
using System.Reflection.Emit;

namespace lim
{
	public class LimCLI {

        static void Main()
        {
            LimState state = new LimState();
            state.prompt(state);

        }

    }
}
