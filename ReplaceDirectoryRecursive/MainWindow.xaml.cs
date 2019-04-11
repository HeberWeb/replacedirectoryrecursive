using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ReplaceDirectoryRecursive
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 
    public partial class MainWindow : Window
    {
        int contaPastaTotal = 0;
        int contaArquivosTotal = 0;
        int contaTotalItens = 0;
        int contaPastaReplace = 0;

        public delegate void CaminhoCopyHandler(string valor, int ContPasta);
        public event CaminhoCopyHandler PathCopyEvent;

        public delegate void ContagemItensHandler(int qtd);
        public event ContagemItensHandler ContagemPastasEvent;
        public event ContagemItensHandler ContagemArquivosEvent;
        public event ContagemItensHandler ContagemTotalEvent;

        public MainWindow()
        {
            InitializeComponent();
            ContagemPastasEvent += OnContagemPastas;
            ContagemArquivosEvent += OnContagemArquivos;
            ContagemTotalEvent += OnContagemTotal;
            PathCopyEvent += OnPathCopy;
        }

        private void OnContagemPastas(int qtd)
        {
            Dispatcher.BeginInvoke((Action)(() =>
                qtdPastasExibe.Content = qtd
            ));
        }

        private void OnContagemArquivos(int qtd)
        {
            Dispatcher.BeginInvoke((Action)(() =>
                qtdArquivosExibe.Content = qtd
            ));
        }

        private void OnContagemTotal(int qtd)
        {
            Dispatcher.BeginInvoke((Action)(() =>
                qdtTotalExibe.Content = qtd
            ));
        }

        private void OnPathCopy(string valor, int ContPasta)
        {
            int multi = Convert.ToInt32(ContPasta * 100);
            float div = (float)(multi / contaTotalItens);
            int porcent = (int)Math.Round(div, 0);
            Dispatcher.BeginInvoke((Action)(() =>
                textBoxResultCopy.AppendText(valor+" - " + ContPasta + "\n")
            ));

            Dispatcher.BeginInvoke((Action)(() =>
                textBoxResultCopy.ScrollToEnd()
            ));

            Dispatcher.BeginInvoke((Action)(() => 
                andamentoReplace.Value = porcent
            ));

            if (porcent == 100)
            {
                MessageBox.Show("Processo Finalizado");
                Dispatcher.BeginInvoke((Action)(() =>
                    buttonReset.Visibility = Visibility.Visible
                ));
            }
        }

        private void replace_Click(object sender, RoutedEventArgs e)
        {
            if (DiretorioFonte.Text != "" && DiretorioDestino.Text != "")
            {
                var caminhoDiretorio = DiretorioFonte.Text;
                var caminhoDestino = DiretorioDestino.Text;
                andamentoReplace.IsIndeterminate = true;
                buttonReplace.Visibility = Visibility.Hidden;
                textBoxResultCopy.AppendText("Realizando contagem dos itens...\n");
                DiretorioFonte.IsReadOnly = true;
                DiretorioDestino.IsReadOnly = true;
                buttonSelOrigem.IsEnabled = false;
                buttonSelDestino.IsEnabled = false;
                Task.Run(() => Inicia(caminhoDiretorio, caminhoDestino));
            }
            else
            {
                MessageBox.Show("Preencha origem e destino");
            }
           
        }

        private void button_ClickReset(object sender, RoutedEventArgs e)
        {
            ContagemPastasEvent(0);
            ContagemArquivosEvent(0);
            ContagemTotalEvent(0);
            contaPastaTotal = 1;
            contaArquivosTotal = 0;
            contaTotalItens = 0;
            contaPastaReplace = 0;
            buttonReset.Visibility = Visibility.Hidden;
            buttonSelOrigem.IsEnabled = true;
            buttonSelDestino.IsEnabled = true;
            buttonReplace.Visibility = Visibility.Visible;
            andamentoReplace.Value = 0;
            textBoxResultCopy.Text = "";
            DiretorioFonte.IsReadOnly = false;
            DiretorioDestino.IsReadOnly = false;
            DiretorioFonte.Text = "";
            DiretorioDestino.Text = "";
        }

        private void button_ClickSelFolders(object sender, RoutedEventArgs e)
        {
            using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                Button clickButton = (Button)sender;
                System.Windows.Forms.DialogResult result = dialog.ShowDialog();
                var PathSel = dialog.SelectedPath;
                if (clickButton.Name == "buttonSelOrigem")
                {
                    DiretorioFonte.Text = PathSel;
                }
                else if (clickButton.Name == "buttonSelDestino")
                {
                    DiretorioDestino.Text = PathSel;
                }



            }
        }

        private void Inicia(string caminhoDiretorio, string caminhoDestino)
        {
            contaPastaTotal = 1;
            contaArquivosTotal = 0;
            contaTotalItens = 0;
            contaPastaReplace = 0;

            List<Task> tasks = new List<Task>();
            tasks.Add(Task.Run(() => ContaDiretorioRecursivo(caminhoDiretorio)));
            tasks.Add(Task.Run(() => ContaArquivosRecursivo(caminhoDiretorio)));
            Task.WaitAll(tasks.ToArray());

            ContagemPastasEvent(contaPastaTotal);
            ContagemArquivosEvent(contaArquivosTotal);
            contaTotalItens = (contaPastaTotal + contaArquivosTotal);
            ContagemTotalEvent(contaTotalItens);

            Dispatcher.BeginInvoke((Action)(() =>
               andamentoReplace.IsIndeterminate = false
            ));

            Task.Run(() => replaceDiretorioRecursivo(caminhoDiretorio, caminhoDestino));
        }

        private void ContaDiretorioRecursivo(string caminhoDiretorio)
        {
            if (Directory.Exists(caminhoDiretorio))
            {

                DirectoryInfo dirAtua = new DirectoryInfo(caminhoDiretorio);
                DirectoryInfo[] subDir = dirAtua.GetDirectories();

                contaPastaTotal += dirAtua.GetDirectories().Count();
                foreach (var item in subDir)
                {
                    this.ContaDiretorioRecursivo(item.FullName);
                }
            }
        }

        private void ContaArquivosRecursivo(string caminhoDiretorio)
        {
            if (Directory.Exists(caminhoDiretorio))
            {
                DirectoryInfo dirAtua = new DirectoryInfo(caminhoDiretorio);
                DirectoryInfo[] subDir = dirAtua.GetDirectories();

                contaArquivosTotal += dirAtua.GetFiles().Count();
                //ContagemArquivos(contaArquivosTotal);

                foreach (var item in subDir)
                {
                    this.ContaArquivosRecursivo(item.FullName);
                }
            }
        }

        private void replaceDiretorioRecursivo(string caminhoDiretorio, string caminhoDestino)
        {
            if (Directory.Exists(caminhoDiretorio))
            {
                var vetCaminho = caminhoDiretorio.Split('\\');
                string novoCaminho = caminhoDestino + "\\" + vetCaminho[vetCaminho.Length - 1].ToString().Replace(" ", "_");

                Directory.CreateDirectory(novoCaminho);
                contaPastaReplace++;
                PathCopyEvent(novoCaminho, contaPastaReplace);

                DirectoryInfo dirAtua = new DirectoryInfo(caminhoDiretorio);
                DirectoryInfo[] subDir = dirAtua.GetDirectories();

                FileInfo[] arquivos = dirAtua.GetFiles();
                foreach (var fileItem in arquivos)
                {
                    string caminhoFile = System.IO.Path.Combine(novoCaminho, fileItem.Name);
                    fileItem.CopyTo(caminhoFile, true);
                    contaPastaReplace++;
                    PathCopyEvent(caminhoFile, contaPastaReplace);
                }

                foreach (var item in subDir)
                {
                    this.replaceDiretorioRecursivo(item.FullName, novoCaminho);
                }
            }
        }
    }
}
