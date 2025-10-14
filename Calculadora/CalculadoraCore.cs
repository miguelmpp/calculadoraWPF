using System;
using System.Collections.Generic;
using System.Globalization;

namespace Calculadora
{
    // Núcleo minimalista: guarda tokens (números e + - * /) e calcula com precedência
    public class CalculadoraCore
    {
        private readonly List<string> _tokens = new();
        private readonly CultureInfo _pt = CultureInfo.CurrentCulture;

        public string Expressao => string.Join(" ", _tokens);

        public void Limpar() => _tokens.Clear();

        public void DefinirResultadoComoNovoEstado(double resultado)
        {
            _tokens.Clear();
            _tokens.Add(resultado.ToString(_pt));
        }

        public void AdicionarNumero(string digito)
        {
            if (_tokens.Count == 0 || EhOperador(_tokens[^1]))
                _tokens.Add(digito);
            else
                _tokens[^1] += digito;
        }

        public void AdicionarPonto()
        {
            if (_tokens.Count == 0 || EhOperador(_tokens[^1]))
            {
                _tokens.Add("0" + _pt.NumberFormat.NumberDecimalSeparator);
                return;
            }

            if (!_tokens[^1].Contains(_pt.NumberFormat.NumberDecimalSeparator))
                _tokens[^1] += _pt.NumberFormat.NumberDecimalSeparator;
        }

        public void AdicionarOperador(char op)
        {
            if (!"+-*/".Contains(op)) return;

            if (_tokens.Count == 0)
            {
                if (op == '-') _tokens.Add("-"); // permite começar negativo
                return;
            }

            if (EhOperador(_tokens[^1]))
                _tokens[^1] = op.ToString(); // troca operador
            else
                _tokens.Add(op.ToString());
        }

        public bool TentarCalcular(out double resultado, out string erro)
        {
            resultado = 0;
            erro = string.Empty;

            if (_tokens.Count == 0) { erro = "Nada para calcular."; return false; }
            if (EhOperador(_tokens[^1])) { erro = "Expressão incompleta."; return false; }

            try
            {
                var rpn = ParaRpn(_tokens);
                resultado = AvaliarRpn(rpn);
                return true;
            }
            catch (DivideByZeroException)
            {
                erro = "Divisão por zero.";
                return false;
            }
            catch (Exception ex)
            {
                erro = "Erro ao calcular: " + ex.Message;
                return false;
            }
        }

        // ------ internos ------

        private static bool EhOperador(string t) => t is "+" or "-" or "*" or "/";

        private static int Prec(string op) => op is "*" or "/" ? 2 : 1;

        private static bool ENumero(string t)
        {
            var norm = t.Replace(',', '.');
            return double.TryParse(norm, NumberStyles.Float, CultureInfo.InvariantCulture, out _);
        }

        private static List<string> ParaRpn(List<string> tokens)
        {
            var outp = new List<string>();
            var ops = new Stack<string>();

            foreach (var t in tokens)
            {
                if (ENumero(t))
                {
                    outp.Add(t.Replace(',', '.'));
                }
                else if (t is "+" or "-" or "*" or "/")
                {
                    while (ops.Count > 0 && Prec(ops.Peek()) >= Prec(t))
                        outp.Add(ops.Pop());
                    ops.Push(t);
                }
                else
                {
                    throw new Exception($"Token inválido: {t}");
                }
            }

            while (ops.Count > 0) outp.Add(ops.Pop());
            return outp;
        }

        private static double AvaliarRpn(List<string> rpn)
        {
            var st = new Stack<double>();
            foreach (var t in rpn)
            {
                if (double.TryParse(t, NumberStyles.Float, CultureInfo.InvariantCulture, out var n))
                {
                    st.Push(n);
                }
                else
                {
                    if (st.Count < 2) throw new Exception("Expressão inválida.");
                    var b = st.Pop();
                    var a = st.Pop();
                    double r = t switch
                    {
                        "+" => a + b,
                        "-" => a - b,
                        "*" => a * b,
                        "/" => b == 0 ? throw new DivideByZeroException() : a / b,
                        _ => throw new Exception($"Operador inválido: {t}")
                    };
                    st.Push(r);
                }
            }
            if (st.Count != 1) throw new Exception("Expressão inválida.");
            return st.Pop();
        }
    }
}
