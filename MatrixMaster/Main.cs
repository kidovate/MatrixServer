using System;

namespace MatrixMaster
{
  class MainClass
  {
    static NodeManager manager;
    public static void Main (string[] args)
    {
      Console.WriteLine("Launching Matrix Master server.");
      manager = new NodeManager("CompiledNodes");
    }
  }
}
