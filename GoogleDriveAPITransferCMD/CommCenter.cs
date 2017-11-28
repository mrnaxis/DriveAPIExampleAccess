using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GoogleDriveAPITransferCMD
{
    class CommCenter
    {
        public CommCenter(string Mode = "Human", string FolderPath="", string FileName="", string GoogleDir = "")
        {
            switch (Mode)
            {
                case "Helper":
                    Halplease();
                    break;
                case "Human":
                    HumanControl();
                    break;
                case "Upload":
                    UploadFileMode(FolderPath,FileName,GoogleDir);
                    break;
                case "Download":
                    DownloadFileMode(FolderPath, FileName);
                    break;
                default:
                    Console.WriteLine("Selecione Um Modo de Operação Válido, use help para te ajudar");
                    Environment.Exit(-10);
                    break;
            }
        }
        public void HumanControl()
        {
            bool ExecMode = true;
            Console.WriteLine("Bem vindo Humano, como posso te servir?");
            while (ExecMode)
            {
                Console.WriteLine("Selecione com um número uma opção abaixo");
                Console.WriteLine("1 - Listar Arquivos\n2 - Criar Diretorio \n3 - Upload de Arquivo\n4 - Download de Arquivo\n5 - Exclusão de Arquivo\n6 - Sair");

                string op = Console.ReadLine();
                int opNum = 0;
                if (int.TryParse(op, out opNum))
                {
                    DriveAPIAccess DAA;
                    bool continuity = true;
                    switch (opNum)
                    {
                        
                        case 1:
                            Console.Clear();
                            DAA = new DriveAPIAccess();
                            DAA.ShowFiles().Wait();
                            break;
                        case 2:
                            Console.Clear();
                            Console.WriteLine("Bem, dê um nome para o diretório que deseja criar, caso ele já exista ele não será criado!");
                            string newGDriveDir = Console.ReadLine();
                            DAA = new DriveAPIAccess();
                            DAA.CreateGDriveDir(newGDriveDir).Wait();
                            break;
                        case 3:
                            Console.Clear();
                            while (continuity)
                            {
                                Console.WriteLine("Digite o caminho completo do arquivo que deseja subir, incluindo o formato!\nDica: Se você estiver com preguiça, utilize ctrl+c shit+insert para inserir os dados no console");
                                string path = Console.ReadLine();
                                if (AuxFileCommCenter.Exists(path))
                                {
                                    Console.WriteLine("Muito bem Humano! Excelente escrita de caminho, Antes de continuar tenho que perguntar, deseja adicionar em algum diretório esse arquivo? Se não quiser apenas pressione Enter.");
                                    Console.WriteLine("Lembrando que caso o diretório não exista, ele será criado.");
                                    string dir = Console.ReadLine();

                                    Console.WriteLine("Ótimo! Seu Arquivo será enviado em breve");

                                    if (string.IsNullOrEmpty(dir))
                                        dir = null;

                                    Thread.Sleep(5000);
                                    Console.Clear();
                                    DAA = new DriveAPIAccess();
                                    DAA.DoUpload(path,dir).Wait();
                                    continuity = false;
                                    ExecMode = false;
                                }
                                else
                                    Console.WriteLine("É... o caminho não existe ou o nome do arquivo está incorreto, de qualquer forma, tente novamente!");
                                
                            }
                            break;
                        case 4:
                            Console.Clear();
                            while (continuity)
                            {
                                Console.WriteLine("Digite o caminho completo do arquivo que deseja baixar, incluindo o formato!\nDica: Se você estiver com preguiça, utilize ctrl+c shit+insert para inserir os dados no console");
                                Console.WriteLine("Lembre-se que para o download, você deve digitar o nome do arquivo igual ao qual você deseja do google drive!\n");
                                string path = Console.ReadLine();
                                if (!string.IsNullOrEmpty(path))
                                {
                                    Console.WriteLine("Muito bem Humano! Excelente escrita de caminho, vamos baixar o seu arquivo agora, você deve visualizar o progresso na sua tela em breve!");

                                    Thread.Sleep(5000);
                                    Console.Clear();
                                    DAA = new DriveAPIAccess();
                                    DAA.DoDownload(path).Wait();
                                    continuity = false;
                                    ExecMode = false;
                                }
                                else
                                    Console.WriteLine("É... o caminho não existe ou o nome do arquivo está incorreto, de qualquer forma, tente novamente!");

                            }
                            break;
                        case 5:
                            Console.Clear();
                            while (continuity)
                            {
                                Console.WriteLine("Digite o nome do arquivo que deseja excluir, incluindo o formato!\nDica: Se você estiver com preguiça, utilize ctrl+c shit+insert para inserir os dados no console");
                                Console.WriteLine("Lembre-se que você deve digitar o nome do arquivo igual ao qual você deseja excluir do google drive!\n");
                                string path = Console.ReadLine();
                                if (!string.IsNullOrEmpty(path))
                                {
                                    Console.WriteLine("Muito bem Humano! Vamos excluir o seu arquivo agora!");
                                    Thread.Sleep(1000);
                                    Console.Clear();
                                    DAA = new DriveAPIAccess();
                                    DAA.DeleteFile(path).Wait();
                                    continuity = false;
                                    ExecMode = false;
                                }
                                else
                                    Console.WriteLine("É... o nome do arquivo está incorreto, de qualquer forma, tente novamente!");

                            }
                            break;
                        case 6:
                            Console.WriteLine("Adeus Humano, em breve nos veremos...");
                            ExecMode = false;
                            Environment.Exit(0);
                            break;
                        default:
                            Console.WriteLine("Selecione uma opção válida!");
                            break;
                    }
                }
                else
                    Console.WriteLine("Selecione uma opção válida!");
            }
        }

        public void UploadFileMode(string Folder, string FileName, string GoogleDir = "")
        {
            DriveAPIAccess DAA = new DriveAPIAccess();
            Console.WriteLine("Iniciando Upload Automata");
            string FullPath = AuxFileCommCenter.AdjustPath(Folder, FileName);

            if (string.IsNullOrEmpty(GoogleDir))
                GoogleDir = null;

            DAA.DoUpload(FullPath, GoogleDir).Wait();
        }

        public void DownloadFileMode(string Folder, string FileName, string GoogleDir = "")
        {
            DriveAPIAccess DAA = new DriveAPIAccess();
            DAA = new DriveAPIAccess();
            Console.WriteLine("Iniciando Download Automata");
            string FullPath = AuxFileCommCenter.AdjustPath(Folder, FileName);
            DAA.DoDownload(FullPath).Wait();
        }

        public void Halplease()
        {
            Console.WriteLine("Ajuda: ");
            Console.WriteLine("Utilize os argumentos da seguinte maneira em qualquer ordem: ");
            Console.WriteLine("'nomedoexe.exe -Adados1 -Cdados2 -Bdados3'");
            Console.WriteLine("Argumentos: ");
            Console.WriteLine("-P | Indica o caminho da pasta para download ou upload do arquivo, lembre-se das aspas para casos com espaços em branco no nome.");
            Console.WriteLine("-F | Indica o arquivo com o formato para upload ou download, lembre-se das aspas para casos com espaços em branco no nome.");
            Console.WriteLine("-D | Indica a pasta que deseja armazenar o arquivo dentro do drive, evite espaços");
            Console.WriteLine("-M | Modo de operação do programa. Up para upload, Down para download, Human (padrão) para uso de uma interface no console.");
            Console.WriteLine("Obs.: No modo de download, o caminho escrito deve ser o local onde será salvo o arquivo, sendo o nome do arquivo dentro do caminho igual ao que está no drive");
            Console.WriteLine("Pressione qualquer tecla para continuar...");
            Console.Read();
            Environment.Exit(0);
        }
    }
}
