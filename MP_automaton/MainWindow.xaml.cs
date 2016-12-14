using MP_automaton.Logic;
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

namespace MP_automaton.GUI
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Automaton automaton = new Automaton();
        private int prError = 0;
        string _text;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void richTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var text = new TextRange(richTextBox.Document.ContentStart, richTextBox.Document.ContentEnd).Text.Trim();
            var result = automaton.ProcessChain(text);
            if (result == true)
            {
                textBlock.Text = "Строка подходит.";
            }
            else if (_text != text)
            {
                _text = text;
                TextRange range2 = new TextRange(richTextBox.Document.ContentStart, richTextBox.Document.ContentEnd);
                range2.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Black);

                textBlock.Text = automaton.LastError + ".";

                TextPointer start = FindPointerAtTextOffset(
        richTextBox.Document.ContentStart, automaton.ErrorNum, seekStart: true);
                TextPointer end = FindPointerAtTextOffset(start, 1, seekStart: false);
                if (start != null && end != null)
                {
                    TextRange range = new TextRange(start, end);
                    range.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Red);
                }

                prError = automaton.ErrorNum;
            }
        }

        TextPointer FindPointerAtTextOffset(TextPointer from, int offset, bool seekStart)
        {
            if (from == null)
                return null;

            TextPointer current = from;
            TextPointer end = from.DocumentEnd;
            int charsToGo = offset;

            while (current.CompareTo(end) != 0)
            {
                Run currentRun;
                if (current.GetPointerContext(LogicalDirection.Forward) == TextPointerContext.Text &&
                    (currentRun = current.Parent as Run) != null)
                {
                    var remainingLengthInRun = current.GetOffsetToPosition(currentRun.ContentEnd);
                    if (charsToGo < remainingLengthInRun ||
                        (charsToGo == remainingLengthInRun && !seekStart))
                        return current.GetPositionAtOffset(charsToGo);
                    charsToGo -= remainingLengthInRun;
                    current = currentRun.ElementEnd;
                }
                else
                {
                    current = current.GetNextContextPosition(LogicalDirection.Forward);
                }
            }
            if (charsToGo == 0 && !seekStart)
                return end;
            return null;
        }
    }
}
