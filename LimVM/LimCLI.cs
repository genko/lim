namespace lim
{
	public static class LimCLI
	{
	   public static string[] m_args;

        static int Main(string[] args)
        {
            LimState state = new LimState();
            m_args = args;
            state.prompt(state);
           return 0;
        }
    }
}
