using ReplaceDirectoryRecursive.Libs;
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
        bool? isCreateCopy = true;
        bool? isReplaceDirectories = false;
        bool? isReplaceFiles = false;
        string txtFindText = "";
        string txtReplaceTo = "";
        long sizeFiles = 0;

        public delegate void CaminhoCopyHandler(string valor, int ContPasta, bool sizerMaxLengthValid);
        public event CaminhoCopyHandler PathCopyEvent;

        public delegate void ContagemItensHandler(int qtd);
        public delegate void ContagemTamanhoHandler(long bytes);
        public event ContagemItensHandler ContagemPastasEvent;
        public event ContagemItensHandler ContagemArquivosEvent;
        public event ContagemItensHandler ContagemTotalEvent;
        public event ContagemTamanhoHandler ContagemTamanhoEvent;

        public MainWindow()
        {
            InitializeComponent();
            ContagemPastasEvent += OnContagemPastas;
            ContagemArquivosEvent += OnContagemArquivos;
            ContagemTotalEvent += OnContagemTotal;
            ContagemTamanhoEvent += OnContagemTamanho;
            PathCopyEvent += OnPathCopy;
            createModelListViewResult();
        }

        private void createModelListViewResult()
        {
            var gridView = new GridView();
            this.listViewResultReplace.View = gridView;
            gridView.Columns.Add(new GridViewColumn
            {
                Header = "Caminho",
                DisplayMemberBinding = new Binding("Caminho"),
                Width = (listViewResultReplace.Width - 100)
            });
            gridView.Columns.Add(new GridViewColumn
            {
                Header = "Status",
                DisplayMemberBinding = new Binding("Status")
            });
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

        private void OnContagemTamanho(long bytes)
        {
            var info = FilesInfo.InfoSize(bytes);
            Dispatcher.BeginInvoke((Action)(() =>
                sizeFilesExibe.Content = string.Format("{0} ({1})", info.Size, info.Format)
            ));
        }

        private void OnPathCopy(string valor, int ContPasta, bool sizerMaxLengthValid)
        {
            int multi = Convert.ToInt32(ContPasta * 100);
            float div = (float)(multi / contaTotalItens);
            int porcent = (int)Math.Round(div, 0);

            string status = "Ok";
            if (!sizerMaxLengthValid)
            {
                status = "caminho maior que o permitido!";
            }
            
            Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.ApplicationIdle, new Action(() => {
                andamentoReplace.Value = porcent;
                listViewResultReplace.Items.Add(new ListViewReplaceResult()
                {
                    Caminho = valor + " - " + ContPasta,
                    Status = status
                });

                listViewResultReplace.Items.MoveCurrentTo(listViewResultReplace.Items[listViewResultReplace.Items.Count - 1]);
                listViewResultReplace.ScrollIntoView(listViewResultReplace.Items[listViewResultReplace.Items.Count - 1]);

                if (porcent == 100)
                {
                    MessageBox.Show("Processo Finalizado");
                    buttonReset.Visibility = Visibility.Visible;
                }
            }));
        }

        private void replace_Click(object sender, RoutedEventArgs e)
        {
            this.isCreateCopy = createCopy.IsChecked;
            this.txtFindText = findText.Text;
            this.txtReplaceTo = replaceTo.Text;
            this.isReplaceDirectories = replaceDirectories.IsChecked;
            this.isReplaceFiles = replaceFiles.IsChecked;

            
            if (formIsvalid())
            {
                var caminhoDiretorio = DiretorioFonte.Text;
                var caminhoDestino = DiretorioDestino.Text;
                andamentoReplace.IsIndeterminate = true;
                buttonReplace.Visibility = Visibility.Hidden;
                DiretorioFonte.IsReadOnly = true;
                DiretorioDestino.IsReadOnly = true;
                buttonSelOrigem.IsEnabled = false;
                buttonSelDestino.IsEnabled = false;
                Task.Run(() => Inicia(caminhoDiretorio, caminhoDestino));
            }          
        }

        private bool formIsvalid()
        {
            bool resultValid = true;
            if (DiretorioFonte.Text == "")
            {
                resultValid = false;
                MessageBox.Show("Preencha o campo Origem!");
            }

            if (resultValid == true && DiretorioDestino.Text == "" && isCreateCopy  == true)
            {
                resultValid = false;
                MessageBox.Show("Preencha o campo Destino!");
            }

            return resultValid;
        }

        private void button_ClickReset(object sender, RoutedEventArgs e)
        {
            this.execReset();
        }

        private void execReset()
        {
            MainWindow reload = new MainWindow();
            reload.Show();
            this.Close();
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

        private void createCopy_Click(object sender, RoutedEventArgs e)
        {
            if (createCopy.IsChecked == false)
            {
                labelDestino.Visibility = Visibility.Hidden;
                DiretorioDestino.Visibility = Visibility.Hidden;
                buttonSelDestino.Visibility = Visibility.Hidden;
            }
            else
            {
                labelDestino.Visibility = Visibility.Visible;
                DiretorioDestino.Visibility = Visibility.Visible;
                buttonSelDestino.Visibility = Visibility.Visible;
            }
        }

        private async void Inicia(string caminhoDiretorio, string caminhoDestino)
        {
            contaPastaTotal = 1;
            contaArquivosTotal = 0;
            contaTotalItens = 0;
            contaPastaReplace = 0;

            await this.IniciaContagenItens(caminhoDiretorio);

            ContagemPastasEvent(contaPastaTotal);
            ContagemArquivosEvent(contaArquivosTotal);
            contaTotalItens = (contaPastaTotal + contaArquivosTotal);
            ContagemTotalEvent(contaTotalItens);
            ContagemTamanhoEvent(sizeFiles);

            await Dispatcher.BeginInvoke((Action)(() =>
               andamentoReplace.IsIndeterminate = false
            ));
            if (contaTotalItens > 0)
            {
                await Task.Run(() => replaceDiretorioRecursivo(caminhoDiretorio, caminhoDestino));
            }
            else
            {
                MessageBox.Show("O diretório de Origem não foi encontrado!");
            }
        }

        public async Task IniciaContagenItens(string caminhoDiretorio)
        {
            await Task.Run(() => ContaDiretorioRecursivo(caminhoDiretorio));
            await Task.Run(() => ContaArquivosRecursivo(caminhoDiretorio));
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
            else
            {
                contaPastaTotal = 0;
            }
        }

        private void ContaArquivosRecursivo(string caminhoDiretorio)
        {
            if (Directory.Exists(caminhoDiretorio))
            {
                DirectoryInfo dirAtua = new DirectoryInfo(caminhoDiretorio);
                DirectoryInfo[] subDir = dirAtua.GetDirectories();

                contaArquivosTotal += dirAtua.GetFiles().Count();

                foreach(var files in dirAtua.GetFiles())
                {
                    sizeFiles += files.Length;
                }

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
                string novoCaminho = "";
                bool sizeFolderMaxLengthValid = false;
                DirectoryInfo dirAtua = new DirectoryInfo(caminhoDiretorio);
                Thread.Sleep(100);
                if (this.isCreateCopy == false)
                {
                    for (int i = 0; i < vetCaminho.Length; i++)
                    {
                        if (string.IsNullOrEmpty(novoCaminho))
                        {
                            novoCaminho = vetCaminho[i];
                        }
                        else
                        {
                            if (i == (vetCaminho.Length - 1))
                            {
                                novoCaminho = novoCaminho + "\\" + vetCaminho[i].ToString();
                                if (this.isReplaceDirectories == true && this.txtFindText != "")
                                {
                                    novoCaminho = novoCaminho + "\\" + vetCaminho[i].ToString().Replace(this.txtFindText, this.txtReplaceTo);
                                }
                            }
                            else
                            {
                                novoCaminho = novoCaminho + "\\" + vetCaminho[i].ToString();
                            }
                        }
                    }

                    sizeFolderMaxLengthValid = (novoCaminho.Length < 260);
                    if (caminhoDiretorio != novoCaminho && sizeFolderMaxLengthValid)
                    {
                        if (this.isReplaceDirectories == true)
                            Directory.Move(caminhoDiretorio, novoCaminho);

                        dirAtua = new DirectoryInfo(novoCaminho);
                    }
                }
                else
                {
                    novoCaminho = caminhoDestino + "\\" + vetCaminho[vetCaminho.Length - 1].ToString();
                    if (this.isReplaceDirectories == true && this.txtFindText != "")
                    {
                        novoCaminho = caminhoDestino + "\\" + vetCaminho[vetCaminho.Length - 1].ToString().Replace(this.txtFindText, this.txtReplaceTo);
                    }
                    sizeFolderMaxLengthValid = (novoCaminho.Length < 260);
                    if (sizeFolderMaxLengthValid)
                        Directory.CreateDirectory(novoCaminho);
                }

                contaPastaReplace++;
                PathCopyEvent(novoCaminho, contaPastaReplace, sizeFolderMaxLengthValid);

                DirectoryInfo[] subDir = dirAtua.GetDirectories();

                FileInfo[] arquivos = dirAtua.GetFiles();
                foreach (var fileItem in arquivos)
                {
                    Thread.Sleep(100);
                    string fileNewName = fileItem.Name;
                    if (this.isReplaceFiles == true && this.txtFindText != "")
                    {
                        fileNewName = fileNewName.Replace(this.txtFindText, this.txtReplaceTo);
                    }

                    string caminhoFile = System.IO.Path.Combine(novoCaminho, fileNewName);
                    contaPastaReplace++;

                    bool sizeFileMaxLengthValid = (caminhoFile.Length < 260);

                    PathCopyEvent(caminhoFile, contaPastaReplace, sizeFileMaxLengthValid);

                    if (sizeFileMaxLengthValid)
                    {
                        if (this.isCreateCopy == false)
                        {
                            if (this.isReplaceFiles == true && fileItem.Name != fileNewName)
                            {
                                fileItem.MoveTo(caminhoFile);
                            }
                        }
                        else
                        {
                            fileItem.CopyTo(caminhoFile, true);
                        }
                    }
                }

                foreach (var item in subDir)
                {
                    this.replaceDiretorioRecursivo(item.FullName, novoCaminho);
                }
            }
        }

        public class ListViewReplaceResult
        {
            public string Caminho { get; set; }
            public string Status { get; set; }
        }
    }
}
