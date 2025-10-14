using System.Globalization;
using System.Windows;
using System.Windows.Input;

namespace Calculadora
{
    public partial class MainWindow : Window
    {
        private CalculadoraCore core = new CalculadoraCore();
        private readonly CultureInfo _pt = CultureInfo.CurrentCulture;

        public MainWindow()
        {
            InitializeComponent();
            textoSaida.Text = "0";
        }

        private void processa_Click(object sender, RoutedEventArgs e)
        {
            if (core.TentarCalcular(out double resultado, out string erro))
            {
                textoSaida.Text = resultado.ToString(_pt);
                core.DefinirResultadoComoNovoEstado(resultado);
                conteudo.Text = core.Expressao;
            }
            else
            {
                MessageBox.Show(erro, "Atenção", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void botaoBuffer_Click(object sender, RoutedEventArgs e)
        {
            var entrada = (campoDeEntrada.Text ?? "").Trim();
            if (string.IsNullOrWhiteSpace(entrada)) return;

            if (EhOperador(entrada))
            {
                var op = NormalizaOperador(entrada);
                if (op != '\0') core.AdicionarOperador(op);
                campoDeEntrada.Clear();
                conteudo.Text = core.Expressao;
                return;
            }

            if (TentarInserirNumero(entrada))
            {
                campoDeEntrada.Clear();
                conteudo.Text = core.Expressao;
                return;
            }

            MessageBox.Show("Digite um número ou um operador (+, -, *, /).", "Entrada inválida", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void clear_Click(object sender, RoutedEventArgs e)
        {
            core = new CalculadoraCore();
            campoDeEntrada.Text = string.Empty;
            textoSaida.Text = "0";
            conteudo.Text = string.Empty;
        }

        private void campoDeEntrada_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) botaoBuffer_Click(sender, e);
        }

        private static bool EhOperador(string s)
        {
            if (s.Length == 1) return "+-*/xX÷×−".Contains(s);
            s = s.ToLowerInvariant();
            return s is "x" or "div" or "mult" or "mais" or "menos";
        }

        private static char NormalizaOperador(string s) => s switch
        {
            "+" => '+',
            "-" or "−" => '-',
            "*" or "x" or "X" or "×" => '*',
            "/" or "÷" or "div" => '/',
            "mult" => '*',
            "mais" => '+',
            "menos" => '-',
            _ => '\0'
        };

        private bool TentarInserirNumero(string texto)
        {
            var norm = texto.Replace(',', '.');
            if (!double.TryParse(norm, System.Globalization.NumberStyles.Float, CultureInfo.InvariantCulture, out _))
                return false;

            foreach (var ch in texto)
            {
                if (char.IsDigit(ch)) core.AdicionarNumero(ch.ToString());
                else if (ch is '.' or ',') core.AdicionarPonto();
                else if (ch == '-' && texto.IndexOf('-') == 0) core.AdicionarNumero("-");
            }
            return true;
        }
    }
}
