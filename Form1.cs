using System;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace Urban_Tool
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            MaximizeBox = false;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //Botão extrair juntamente com os filtros

            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Filter = "Arquivo dcm|*.dcm|Todos os arquivos (*.*)|*.*";
            openFileDialog1.Title = "Escolha o(s) arquivo(s) dcm";
            openFileDialog1.Multiselect = true;

            // aqui é a condição da caixa

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                foreach (String file in openFileDialog1.FileNames)
                {
                    using (FileStream stream = File.Open(file, FileMode.Open))
                    {

                        // conversão já que o c# nao lê binário
                        BinaryReader br = new BinaryReader(stream);
                        BinaryWriter bw = new BinaryWriter(stream);

                        int primeiroponteiro = br.ReadInt32(); // Lê o primeiro ponteiro

                        // A quantidade de texto é o primeiro ponteiro dividido por 4...
                        // Você pode pegar a quantidade de textos e multiplicar por 4 (se o ponteiro for de 4 bytes) caso queira conferir se vai dar certo em cada arquivo 
                        int quantidadedeponteiros = primeiroponteiro / 4;

                        // Você não precisa calcular diferença, já que os ponteiros são sempre relativos ao começo do arquivo.
                        // E como o primeiro ponteiro está em 0, vc não precisa da variavel offset ponteiro.

                        string todosOsTextos = "";
                        // fim do bloco

                        for (int loop = 0; loop < quantidadedeponteiros; loop++)
                        {
                            // Aqui loop é igual a 0, e 0 multiplicado por 4, é igual a 0. Então ele vai pro primeiro ponteiro de qualquer jeito
                            // Por isso tirei o seek q vinha antes
                            br.BaseStream.Seek(loop * 4, SeekOrigin.Begin); //Como offsetponteiro é igual a 0, vc pode tirar

                            int ponteiro = br.ReadInt32();

                            br.BaseStream.Seek(ponteiro, SeekOrigin.Begin); //Tirei a variavel diferença pq não fazia sentido ficar subtraindo 0.

                            bool acabouotexto = false;

                            int tamanhotexto = 0; // Inicia a variável que vai guardar o tamanho do texto a ser convertido

                            while (acabouotexto == false)
                            {
                                byte comparador = br.ReadByte(); // Cria a variavel comparador e lê um byte do texto

                                tamanhotexto++; // Vai computando +1 sempre que o comparador ler um byte 

                                if (comparador == 0)
                                {
                                    acabouotexto = true;
                                    br.BaseStream.Seek(-tamanhotexto, SeekOrigin.Current); // Volta para o primeiro byte lido pelo comparador
                                }
                            }

                            byte[] bytes = new byte[tamanhotexto]; // Cria um array que vai guardar os bytes do texto

                            for (int j = 0; j < tamanhotexto; j++)
                            {
                                bytes[j] = br.ReadByte(); // Vai lendo e guardando um byte por vez
                            }

                            string convertido = System.Text.Encoding.Default.GetString(bytes); // A variável convertido recebe o texto convertido

                            todosOsTextos += convertido.Replace("\0", String.Empty).Replace("\n", "<0A>").Replace("\x1A", "<1A>") + "\r\n";
                        }
                        File.WriteAllText(Path.Combine(Path.GetDirectoryName(file), Path.GetFileNameWithoutExtension(file)) + ".txt", todosOsTextos);
                    }
                }
                MessageBox.Show("Texto extraído com sucesso.", "Sucesso!");
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            //Botão inserir
            //MessageBox.Show("Você clicou no botão inserir");

            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Filter = "Arquivo dcm|*.txt|Todos os arquivos (*.*)|*.*";
            openFileDialog1.Title = "Escolha o(s) arquivo(s) dcm";
            openFileDialog1.Multiselect = true;


            // aqui é a condição da caixa

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                foreach (String nomeArquivoTXT in openFileDialog1.FileNames)
                {
                    //Aqui, damos a variavel nomeArquivonovo o nome do arquivo (nomeArquivo) e mandamos ele colocar a extensão DCM
                    string nomeArquivoDCM = Path.ChangeExtension(nomeArquivoTXT, "dcm");

                    int numerolinhas = 0;

                    using (StreamReader reader = new StreamReader(nomeArquivoTXT)) // Abrimos o TXT para fazer a leitura dos dados
                    {
                        string[] linhasdoTXT = File.ReadAllLines(nomeArquivoTXT); // Guarda todas as linhas na variavel

                        numerolinhas = linhasdoTXT.Length; // Verifica o número de linhas

                        int loop = 0; // A variavel loop vai ajudar a ir pro ponteiro correto

                        // Agora criamos o arquivo e preparamos para escrever nele

                        using (BinaryWriter bw = new BinaryWriter(new FileStream(nomeArquivoDCM, FileMode.Create)))
                        {
                            int ponteiro = numerolinhas * 4; // O primeiro ponteiro é sempre o número de textos multiplicado por 4, cada texto ficou em uma linha então é assim

                            foreach (var linha in linhasdoTXT) // Para cada linha do TXT, ou seja, pra cada texto ele vai fazer
                            {
                                bw.BaseStream.Seek(ponteiro, SeekOrigin.Begin); // Vai pro endereço do primeiro texto

                                string texto = linha.Replace("<0A>", "\n").Replace("<1A>", "\x1A"); //Lê a linha do TXT, substitui o q precisa e guarda em texto

                                byte[] bytes = Encoding.Default.GetBytes(texto); //Criamos um array pra armazenar os bytes convertidos do texto e colocamos os bytes nele

                                bw.Write(bytes); // Escreve o texto em bytes no arquivo
                                bw.Write((byte)0); // Escreve o endstring

                                int tamanhotexto = bytes.Length + 1; // Tamanho do texto recebe o tamanho do texto + a endstring                                

                                bw.BaseStream.Seek(loop * 4, SeekOrigin.Begin); // Vamos pro local correto escrever os ponteiros

                                bw.Write(ponteiro); // Escreve o ponteiro

                                ponteiro = ponteiro + tamanhotexto; // Acrescenta na variavel ponteiro o tamando do texto + endstring sem perder o valor que já tava nela

                                loop++; // Loop recebe + 1
                            }
                        }
                    }
                }
                MessageBox.Show("Arquivo recriado com sucesso.", "Sucesso!");
            }
        }


        private void button3_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Filter = "Arquivo ppk|*.ppk|Todos os arquivos (*.*)|*.*";
            openFileDialog1.Title = "Escolha o(s) arquivo(s) ppk";
            openFileDialog1.Multiselect = true;

            // Aqui é a condição da caixa de diálogo para selecionar o arquivo

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                string nomeArquivo = openFileDialog1.FileName;


                // Lê todo o conteúdo do arquivo em bytes
                byte[] arquivoBytes = File.ReadAllBytes(nomeArquivo);

                // Verifica se o arquivo tem pelo menos 0x80 bytes (128 em decimal)
                if (arquivoBytes.Length >= 0x80)
                {
                    // Remove os primeiros 0x80 bytes do arquivo
                    byte[] novoArquivoBytes = new byte[arquivoBytes.Length - 0x80];
                    Array.Copy(arquivoBytes, 0x80, novoArquivoBytes, 0, novoArquivoBytes.Length);

                    // Salva o novo arquivo sem os primeiros 0x80 bytes
                    string novoNomeArquivo = Path.GetDirectoryName(nomeArquivo) + "\\" + Path.GetFileNameWithoutExtension(nomeArquivo) + "_new" + Path.GetExtension(nomeArquivo);
                    File.WriteAllBytes(novoNomeArquivo, novoArquivoBytes);

                    Console.WriteLine("Arquivo processado com sucesso. Os primeiros 0x80 bytes foram removidos.");
                    Console.WriteLine("Novo arquivo salvo como: " + novoNomeArquivo);


                    foreach (String file in openFileDialog1.FileNames)
                    {
                        using (FileStream stream = File.Open(novoNomeArquivo, FileMode.Open))
                        {

                            // conversão já que o c# nao lê binário
                            BinaryReader br = new BinaryReader(stream);
                            BinaryWriter bw = new BinaryWriter(stream);

                            int primeiroponteiro = br.ReadInt32(); // Lê o primeiro ponteiro

                            // A quantidade de texto é o primeiro ponteiro dividido por 4...
                            // Você pode pegar a quantidade de textos e multiplicar por 4 (se o ponteiro for de 4 bytes) caso queira conferir se vai dar certo em cada arquivo 
                            int quantidadedeponteiros = primeiroponteiro / 4;

                            // Você não precisa calcular diferença, já que os ponteiros são sempre relativos ao começo do arquivo.
                            // E como o primeiro ponteiro está em 0, vc não precisa da variavel offset ponteiro.

                            string todosOsTextos = "";
                            // fim do bloco

                            for (int loop = 0; loop < quantidadedeponteiros; loop++)
                            {
                                // Aqui loop é igual a 0, e 0 multiplicado por 4, é igual a 0. Então ele vai pro primeiro ponteiro de qualquer jeito
                                // Por isso tirei o seek q vinha antes
                                br.BaseStream.Seek(loop * 4, SeekOrigin.Begin); //Como offsetponteiro é igual a 0, vc pode tirar

                                int ponteiro = br.ReadInt32();

                                br.BaseStream.Seek(ponteiro, SeekOrigin.Begin); //Tirei a variavel diferença pq não fazia sentido ficar subtraindo 0.

                                bool acabouotexto = false;

                                int tamanhotexto = 0; // Inicia a variável que vai guardar o tamanho do texto a ser convertido

                                while (acabouotexto == false)
                                {
                                    byte comparador = br.ReadByte(); // Cria a variavel comparador e lê um byte do texto

                                    tamanhotexto++; // Vai computando +1 sempre que o comparador ler um byte 

                                    if (comparador == 0)
                                    {
                                        acabouotexto = true;
                                        br.BaseStream.Seek(-tamanhotexto, SeekOrigin.Current); // Volta para o primeiro byte lido pelo comparador
                                    }
                                }

                                byte[] bytes = new byte[tamanhotexto]; // Cria um array que vai guardar os bytes do texto

                                for (int j = 0; j < tamanhotexto; j++)
                                {
                                    bytes[j] = br.ReadByte(); // Vai lendo e guardando um byte por vez
                                }

                                string convertido = System.Text.Encoding.Default.GetString(bytes); // A variável convertido recebe o texto convertido

                                todosOsTextos += convertido.Replace("\0", String.Empty).Replace("\n", "<0A>").Replace("\x1A", "<1A>") + "\r\n";
                            }
                            File.WriteAllText(Path.Combine(Path.GetDirectoryName(file), Path.GetFileNameWithoutExtension(file)) + ".txt", todosOsTextos);

                        }
                    }
                    File.Delete(novoNomeArquivo);
                    MessageBox.Show("Texto extraído com sucesso.", "Sucesso!");
                }
            }
        }


        private void button4_Click_1(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Filter = "Arquivo ppk|*.ppk|Todos os arquivos (*.*)|*.*";
            openFileDialog1.Title = "Escolha o(s) arquivo(s) ppk";
            openFileDialog1.Multiselect = false;

            // Aqui é a condição da caixa de diálogo para selecionar o arquivo

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                string nomeArquivo = openFileDialog1.FileName;


                // Lê todo o conteúdo do arquivo em bytes
                byte[] arquivoBytes = File.ReadAllBytes(nomeArquivo);

                // Verifica se o arquivo tem pelo menos 0x80 bytes (128 em decimal)
                if (arquivoBytes.Length >= 0x80)
                {
                    // Remove os primeiros 0x80 bytes do arquivo
                    byte[] novoArquivoBytes = new byte[arquivoBytes.Length - 0x80];
                    Array.Copy(arquivoBytes, 0x80, novoArquivoBytes, 0, novoArquivoBytes.Length);

                    // Salva o novo arquivo sem os primeiros 0x80 bytes
                    string novoNomeArquivo = Path.GetDirectoryName(nomeArquivo) + "\\" + Path.GetFileNameWithoutExtension(nomeArquivo) + "_new" + Path.GetExtension(nomeArquivo);
                    File.WriteAllBytes(novoNomeArquivo, novoArquivoBytes);

                    Console.WriteLine("Arquivo processado com sucesso. Os primeiros 0x80 bytes foram removidos.");
                    Console.WriteLine("Novo arquivo salvo como: " + novoNomeArquivo);


                    // Aqui irá mover o PPK original para uma pasta temporária, ou seja, apenas para a inserção ser efetuada com sucesso.
                    // Posteriormente o PPK manipulado será renomeado para o segundo passo da inserção ser concluído

                    // Obtém o nome da pasta a partir do nome do arquivo sem extensão
                    string nomePasta = Path.GetFileNameWithoutExtension(nomeArquivo);

                    // Obtém o diretório pai do arquivo selecionado
                    string diretorioPai = Path.GetDirectoryName(nomeArquivo);

                    // Cria o caminho completo para a nova pasta
                    string caminhoPasta = Path.Combine(diretorioPai, nomePasta);

                    // Cria a pasta
                    Directory.CreateDirectory(caminhoPasta);

                    // Cria o objeto DirectoryInfo para a pasta
                    DirectoryInfo pastaInfo = new DirectoryInfo(caminhoPasta);

                    // Define o atributo Hidden como true para tornar a pasta oculta
                    pastaInfo.Attributes |= FileAttributes.Hidden;


                    // Move o arquivo para a nova pasta
                    string novoCaminhoArquivo = Path.Combine(caminhoPasta, Path.GetFileName(nomeArquivo));
                    File.Move(nomeArquivo, novoCaminhoArquivo);

                    // Faz uma cópia do arquivo novo mas sem o _new e depois deleta

                    string novoNomeArquivo2 = Path.GetDirectoryName(nomeArquivo) + "\\" + Path.GetFileNameWithoutExtension(nomeArquivo) + "" + Path.GetExtension(nomeArquivo);
                    File.WriteAllBytes(novoNomeArquivo2, novoArquivoBytes);
                    File.Delete(novoNomeArquivo);

                    // Insersor

                    OpenFileDialog openFileDialog2 = new OpenFileDialog();
                    openFileDialog2.Filter = "Arquivo ppk|*.txt|All files (*.*)|*.*";
                    openFileDialog2.Title = "Selecione o arquivo select_en.txt";
                    openFileDialog2.Multiselect = true;

                    // aqui é a condição da caixa

                    if (openFileDialog2.ShowDialog() == DialogResult.OK)
                    {
                        foreach (String nomeArquivoTXT in openFileDialog2.FileNames)
                        {
                            //Aqui, damos a variavel nomeArquivonovo o nome do arquivo (dcm) e mandamos ele colocar a extensão PPK
                            string nomeArquivoPPK = Path.ChangeExtension(nomeArquivoTXT, "ppk");

                            int numerolinhas = 0;

                            using (StreamReader reader = new StreamReader(nomeArquivoTXT)) // Abrimos o TXT para fazer a leitura dos dados
                            {
                                string[] linhasdoTXT = File.ReadAllLines(nomeArquivoTXT); // Guarda todas as linhas na variavel

                                numerolinhas = linhasdoTXT.Length; // Verifica o número de linhas

                                int loop = 0; // A variavel loop vai ajudar a ir pro ponteiro correto

                                // Agora criamos o arquivo e preparamos para escrever nele

                                using (BinaryWriter bw = new BinaryWriter(new FileStream(nomeArquivoPPK, FileMode.Create)))
                                {
                                    int ponteiro = numerolinhas * 4; // O primeiro ponteiro é sempre o número de textos multiplicado por 4, cada texto ficou em uma linha então é assim

                                    foreach (var linha in linhasdoTXT) // Para cada linha do TXT, ou seja, pra cada texto ele vai fazer
                                    {
                                        bw.BaseStream.Seek(ponteiro, SeekOrigin.Begin); // Vai pro endereço do primeiro texto

                                        string texto = linha.Replace("<0A>", "\n").Replace("<1A>", "\x1A"); //Lê a linha do TXT, substitui o q precisa e guarda em texto

                                        byte[] bytes = Encoding.Default.GetBytes(texto); //Criamos um array pra armazenar os bytes convertidos do texto e colocamos os bytes nele

                                        bw.Write(bytes); // Escreve o texto em bytes no arquivo
                                        bw.Write((byte)0); // Escreve o endstring

                                        int tamanhotexto = bytes.Length + 1; // Tamanho do texto recebe o tamanho do texto + a endstring                                

                                        bw.BaseStream.Seek(loop * 4, SeekOrigin.Begin); // Vamos pro local correto escrever os ponteiros

                                        bw.Write(ponteiro); // Escreve o ponteiro

                                        ponteiro = ponteiro + tamanhotexto; // Acrescenta na variavel ponteiro o tamando do texto + endstring sem perder o valor que já tava nela

                                        loop++; // Loop recebe + 1
                                    }
                                }
                            }
                        }
                        MessageBox.Show("Arquivo recriado com sucesso.", "Sucesso!");
                    }


                    // Aqui faz o preechimento com o HEADER do PPK da pasta temporária

                    // Obtém o nome do arquivo original dentro da pasta temporária
                    string nomeArquivoTemp = Path.GetFileName(caminhoPasta);

                    // Obtém o caminho completo do arquivo original dentro da pasta temporária
                    string caminhoArquivoTemp = Path.Combine(caminhoPasta, nomeArquivoTemp);

                    // Lê os primeiros 0x80 bytes do arquivo original
                    byte[] primeirosBytes = new byte[0x80];
                    Array.Copy(arquivoBytes, primeirosBytes, 0x80);

                    // Lê todo o conteúdo do arquivo novo em bytes
                    byte[] arquivoNovoBytes = File.ReadAllBytes(novoNomeArquivo2);

                    // Cria um novo array de bytes com o tamanho do arquivo novo mais o tamanho dos primeiros bytes
                    byte[] novoArquivoCompletoBytes = new byte[primeirosBytes.Length + arquivoNovoBytes.Length];

                    // Copia os primeiros bytes para o início do novo arquivo
                    Array.Copy(primeirosBytes, novoArquivoCompletoBytes, primeirosBytes.Length);

                    // Copia o conteúdo do arquivo novo para depois dos primeiros bytes
                    Array.Copy(arquivoNovoBytes, 0, novoArquivoCompletoBytes, primeirosBytes.Length, arquivoNovoBytes.Length);

                    // Salva o novo arquivo completo
                    File.WriteAllBytes(novoNomeArquivo2, novoArquivoCompletoBytes);

                    Console.WriteLine("Arquivo processado com sucesso. Os primeiros 0x80 bytes foram copiados para o novo arquivo.");
                    Console.WriteLine("Novo arquivo salvo como: " + novoNomeArquivo2);


                    // Deleta a pasta temporária
                    Directory.Delete(caminhoPasta, true);
                }
            }
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("http://oaleextraducoes.blogspot.com");
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void button5_Click(object sender, EventArgs e)
        {

        }

        // Botão sobre
        private void button5_Click_1(object sender, EventArgs e)
        {
            string nomeAplicativo = "Urban Reign (PS2) Text Tool";
            string versao = "1.0";
            string desenvolvedor = "Alex 'OAleex' Félix";
            string mensagem = $"{nomeAplicativo} Versão {versao}\nDesenvolvido por {desenvolvedor}\nCom alguns códigos de base do Angel333119";

            MessageBox.Show(mensagem, "Sobre", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
