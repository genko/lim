public static class LimCLI
{
    static int Main(string[] args)
    {
        LimState state = new LimState(args);
        state.prompt(state);
        return 0;
    }
}
