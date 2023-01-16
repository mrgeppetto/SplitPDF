using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Path = System.IO.Path;

namespace SplitPDF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        // My Variables
        bool split = false;
        bool combine = false;


        // My Events
        private void btn_Minimize(object sender, RoutedEventArgs e)
        {
            if (this.WindowState == WindowState.Minimized)
            {
                this.WindowState = WindowState.Normal;
            }
            else
            {
                this.WindowState = WindowState.Minimized;
            }
        }
        private void btn_Maximize(object sender, RoutedEventArgs e)
        {
            if (this.WindowState != WindowState.Normal)
            {
                this.WindowState = WindowState.Normal;
            }
            else
            {
                this.WindowState = WindowState.Maximized;
            }
        }
        private void btn_Close(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
        private void on_Move(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }
        private void ListBox_Drop(object sender, DragEventArgs e)
        {
            getFileName(e);
        }
        private void btn_Remove(object sender, RoutedEventArgs e)
        {
            if (myFilePath.SelectedItems.Count < 1)
            {
                MessageBox.Show("Error Line 76: Please Select a file to remove");
            }
            else
            {
                myFilePath.Items.Remove(myFilePath.SelectedItem);
            }

        }
        private void btn_Split(object sender, RoutedEventArgs e)
        {
            bool split = true;
            bool combine = false;

            if (myFilePath.Items.Count == 1)
            {
                createDesktopFolder(split, combine);
            }
            else if (myFilePath.Items.Count == 0)
            {
                MessageBox.Show("Error: Please add a PDF file to split");
            }
            else
            {
                MessageBox.Show("Error: You can only split 1 PDF file at a time");
            }
            
        }
        private void btn_Combine(object sender, RoutedEventArgs e)
        {
            bool split = false;
            bool combine = true;
            if (myFilePath.Items.Count > 1)
            {
                createDesktopFolder(split, combine);
            }
            else
            {
                MessageBox.Show("Error: Please add at least 2 PDF Files to Combine");
            }
            
        }


        /*
         * My Methods
         *Get the filename from the listbox
        */
        public ListBox getFileName(DragEventArgs e)
        {
            string[] myFile = (string[])e.Data.GetData(DataFormats.FileDrop, false);
            myFilePath.Items.Add(myFile[0]);
            return myFilePath;
        }
        // Create Necessary folders and call PDF Split or Combine
        public void createDesktopFolder(bool split, bool combine)
        {
            string pdfDesktopFolder = $"PDFSplit Files";
            string path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            string myFolderPath = $"{path}\\{pdfDesktopFolder}";
            string myOriginalFolderPath = $"{myFolderPath}\\Original";
            string myNewOriginalCopy = string.Empty;
            string myNewDocuments = $"{myFolderPath}\\NewDocuments";

            if (Directory.Exists(path))
            {
                // Try to create directories
                try
                {
                    System.IO.Directory.CreateDirectory(myFolderPath);
                    System.IO.Directory.CreateDirectory(myOriginalFolderPath);
                    System.IO.Directory.CreateDirectory(myNewDocuments);
                }
                catch
                {
                    MessageBox.Show("There was an error");
                }

                if (Path.Exists(myOriginalFolderPath) && myFilePath.Items.Count > 0)
                {
                    if (myFilePath.Items.Count == 1 && split == true && combine == false)
                    {
                        var originalFileName = Path.GetFileName(myFilePath.Items.GetItemAt(0).ToString());
                        if (!Path.Exists($"{myOriginalFolderPath}\\{originalFileName}"))
                        {
                            File.Copy($"{myFilePath.Items.GetItemAt(0)}", $"{myOriginalFolderPath}\\{originalFileName}");
                        }
                        myNewOriginalCopy = $"{myOriginalFolderPath}\\{originalFileName}";
                        createPDF(myNewDocuments, myNewOriginalCopy);
                        split = false; 
                    }
                    else if (myFilePath.Items.Count > 1 && split == true && combine == false)
                    {
                        MessageBox.Show("Error: Cannot Split more than one PDF at a time");
                    }
                    else if (myFilePath.Items.Count > 1 && split == true && combine == false)
                    {
                        MessageBox.Show("Error: Please Select the combine button");
                    }
                    else if (myFilePath.Items.Count > 1 && split == false && combine == true)
                    {
                        int indexCount = 0;
                        foreach (var item in myFilePath.Items)
                        {
                            var originalFileName = Path.GetFileName(myFilePath.Items.GetItemAt(indexCount).ToString());
                    
                            if (!Path.Exists($"{myOriginalFolderPath}\\{originalFileName}"))
                            {
                                if (originalFileName != null)
                                {
                                    File.Copy($"{myFilePath.Items.GetItemAt(indexCount)}", $"{myOriginalFolderPath}\\{originalFileName}");
                                }
                            }
                            indexCount++;
                        }
                        MessageBox.Show("Originals have been copied to new originals folder");
                        mergePDF(myNewDocuments, myOriginalFolderPath);
                        combine = false;
                    }
                    else if (myFilePath.Items.Count == 1 && split == false && combine == true)
                    {
                        MessageBox.Show("Please add another pdf file to combine");
                    }
                    else
                    {
                        MessageBox.Show("Error: Check file location or make sure the listbox is not empty");
                        return;
                    }

                }
            }
            else
            {
                MessageBox.Show("Error 128: Directory Does Not Exist");
            }
        }


        // PDF Split Logic
        public void createPDF(string myNewDocuments, string myNewOriginalCopy)
        {
            if (myFilePath.Items.Count > 0 && myFilePath.Items.Count < 2)
            {
                int counter = 0;
                System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);


                // Open new original pdf document
                PdfDocument origDocCopy = PdfReader.Open(myNewOriginalCopy, PdfDocumentOpenMode.Import);
                // Check the pages in the pdf file
                if (origDocCopy.Pages.Count < 2)
                {
                    MessageBox.Show("Erroe Line 160: There is only one page inside the PDF File, aborting");
                    return;
                }
                else
                {
                    for (int i = 0; i < origDocCopy.Pages.Count; i++)
                    {
                        counter++;

                        PdfDocument newDoc = new PdfDocument();

                        newDoc.AddPage(origDocCopy.Pages[i]);
                        newDoc.Save($"{myNewDocuments}\\DocumentCopy_{i}.pdf");
                    }

                    MessageBox.Show("PDF Split: Complete!");
                }

            }
            else
            {
                MessageBox.Show("Error Line 155: Please make sure the pdf list is not empty or contains more than one pdf during a split");
            }

        }

        // PDF Combine Logic
        public void mergePDF(string myNewDocuments, string myOriginalFolderPath)
        {
           if (Path.Exists(myOriginalFolderPath))
           {
                if (myFilePath.Items.Count != 0 && myFilePath.Items.Count > 1)
                {
                    // New pdf Document
                    PdfDocument newDoc = new PdfDocument();
                    
                    System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
                    
                    // Loop through each file
                    foreach (var doc in myFilePath.Items)
                    {
                        // Get pages from each file
                        int counter = 0;
                        PdfDocument origDocCopy = PdfReader.Open(doc.ToString(), PdfDocumentOpenMode.Import);
                        for (int i = 0; i < origDocCopy.Pages.Count; i++)
                        {
                            counter++;
                            newDoc.AddPage(origDocCopy.Pages[i]);
                            newDoc.Save($"{myNewDocuments}\\MergedDocument.pdf");
                        }
                    }
                    MessageBox.Show("Docment has been merged");
                }
                else
                {
                    MessageBox.Show("There was an error merging PDF Document see method mergePDF");
                }
            }
        }


    }
}
