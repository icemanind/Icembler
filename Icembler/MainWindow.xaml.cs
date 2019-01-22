using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

namespace Icembler
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            SourceCodeTextBox.Options.ConvertTabsToSpaces = false;
            SourceCodeTextBox.Options.IndentationSize = 10;

            //var vd = new VirtualDisk();

            //vd.AddFile(System.IO.File.ReadAllBytes(@"c:\q\file1.bas"), "faa.bas", FileTypes.Basic, FileModes.Ascii);
            //vd.AddFile(@"C:\q\file1.bas", "faa.bas", FileTypes.Basic, FileModes.Ascii);
            //vd.AddFile(@"C:\q\file2.bas", "zebra.bas", FileTypes.Basic, FileModes.Ascii);
            //vd.WriteVirtualDiskToFile(@"C:\q\test.dsk");

            string program = "DOG\tEQU\t$15+2*4\nCAT\tEQU\tDOG-10\nMOUSE\tEQU\t-32\nSTART\tLDA\t#10\n\tEND\tSTART";
            program = "\tORG\t$3F00\nSTART\tLDX\t#1024\n\tLDA\t#65\n\tSTA\t$10,X\n\tRTS\n\tEND\tSTART";
            SourceCodeTextBox.AppendText(program);
           
        }

        private void AssembleButton_Click(object sender, RoutedEventArgs e)
        {
            var assembler = new Assembler();
            byte[] data = assembler.Assemble(SourceCodeTextBox.Text);

            var vd = new VirtualDisk();

            data = vd.ConvertToDecbMachineLanguageFile(data, assembler.StartingAddress, assembler.ExecutionAddress);

            vd.AddFile(data, "TEST.BIN", FileTypes.MachineLanguage, FileModes.Binary);
            vd.WriteVirtualDiskToFile(@"C:\q\test.dsk");
        }
    }
}
