using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoogleDriveAPITransferCMD
{
    class Program
    {
        public static List<string> ArgValid = new List<string>() { "-P", "-M", "-F", "-D" };
        static void Main(string[] args)
        {
            if (args.Length <= 0)
                new CommCenter();
            else if (args.Length == 1 && args[0].ToLower().Contains("help"))
            {
                new CommCenter("Helper");
            }
            else
            {
                try
                {
                    Dictionary<string, string> ArgsDic = ArgsAsDict(args);

                    if (ArgsDic.ContainsKey("-M"))
                    {
                        switch (ArgsDic["-M"])
                        {
                            case "Up":
                                if (ArgsDic.ContainsKey("-P") && ArgsDic.ContainsKey("-F"))
                                    new CommCenter("Upload", ArgsDic["-P"], ArgsDic["-F"], ArgsDic.ContainsKey("-D") ? ArgsDic["-D"] : "");
                                else
                                    Console.WriteLine("Faltam argumentos nessa discu... Digo, execução!\nArgumentos em Falta: -P : Caminho do arquivo e -F Arquivo com formato");
                                break;
                            case "Down":
                                if (ArgsDic.ContainsKey("-P") && ArgsDic.ContainsKey("-F"))
                                    new CommCenter("Download", ArgsDic["-P"], ArgsDic["-F"], ArgsDic.ContainsKey("-D") ? ArgsDic["-D"] : "");
                                else
                                    Console.WriteLine("Faltam argumentos nessa discu... Digo, execução!\nArgumentos em Falta: -P : Caminho do arquivo e -F Arquivo com formato");
                                break;
                            case "Human":
                                new CommCenter();
                                break;
                            default:
                                Console.WriteLine("Argumentos inválidos, digite alguns argumento ou digite help para obter ajuda;");
                                Environment.Exit(-2);
                                break;
                        }
                    }
                    else
                    {
                        Console.WriteLine("Acho que faltam argumentos para continuar esse discu... digo, execução");
                        Console.WriteLine("Argumentos em falta: ", "-M : modo de operação");
                    }

                }
                catch(Exception e)
                {
                    Console.WriteLine("Erro na execução do programa: {0}", e.Message);
                    Environment.Exit(-35);
                }
            }


            //new DriveAPIAccess().DoUpload().Wait();
            //new DriveAPIAccess().ShowFiles().Wait();
            //new DriveAPIAccess().DeleteFile().Wait();

            //Console.Read();
        }

        public static Dictionary<string, string> ArgsAsDict(string[] args)
        {
            Dictionary<string, string> Dic = new Dictionary<string, string>();

            foreach(string arg in args)
            {
                if (arg[0] == '-' && char.IsLetter(arg[1]) && ArgValid.Any(x => x.Contains(arg.Substring(0, 2))))
                    Dic.Add(arg.Substring(0,2), arg.Substring(2));
            }

            return Dic;
        }
    }
}
