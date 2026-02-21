using System;
using System.Collections.Generic;

namespace DesignPatternChallenge
{
    // ============================
    // 1) FLYWEIGHT (estado intrínseco)
    // ============================
    public class CharacterStyle
    {
        public char Symbol { get; }
        public string FontFamily { get; }
        public int FontSize { get; }
        public string Color { get; }
        public bool IsBold { get; }
        public bool IsItalic { get; }
        public bool IsUnderline { get; }

        public CharacterStyle(
            char symbol,
            string fontFamily,
            int fontSize,
            string color,
            bool isBold,
            bool isItalic,
            bool isUnderline)
        {
            Symbol = symbol;
            FontFamily = fontFamily;
            FontSize = fontSize;
            Color = color;
            IsBold = isBold;
            IsItalic = isItalic;
            IsUnderline = isUnderline;
        }

        // Recebe estado extrínseco por parâmetro
        public void Render(int row, int column)
        {
            var style = "";
            if (IsBold) style += "B";
            if (IsItalic) style += "I";
            if (IsUnderline) style += "U";

            Console.WriteLine($"[{row},{column}] '{Symbol}' {FontFamily} {FontSize}pt {Color} {style}");
        }

        // Aproximação de memória do flyweight (um por estilo)
        public int GetMemorySizeEstimate()
        {
            // Aqui é só demonstrativo — strings dominam o custo
            return sizeof(char) +
                   32 +                 // FontFamily aprox
                   sizeof(int) +
                   32 +                 // Color aprox
                   3 * sizeof(bool);
        }
    }

    // ============================
    // 2) FLYWEIGHT FACTORY
    // ============================
    public class CharacterStyleFactory
    {
        private readonly Dictionary<string, CharacterStyle> _cache = new();

        public CharacterStyle GetStyle(
            char symbol,
            string fontFamily,
            int fontSize,
            string color,
            bool isBold,
            bool isItalic,
            bool isUnderline)
        {
            var key = BuildKey(symbol, fontFamily, fontSize, color, isBold, isItalic, isUnderline);

            if (_cache.TryGetValue(key, out var existing))
                return existing;

            var created = new CharacterStyle(symbol, fontFamily, fontSize, color, isBold, isItalic, isUnderline);
            _cache[key] = created;
            return created;
        }

        public int CachedStylesCount => _cache.Count;

        private static string BuildKey(char symbol, string font, int size, string color, bool b, bool i, bool u)
            => $"{symbol}|{font}|{size}|{color}|{b}|{i}|{u}";
    }

    // ============================
    // 3) CONTEXT (estado extrínseco)
    // ============================
    public class DocumentCharacter
    {
        public int Row { get; }
        public int Column { get; }
        public CharacterStyle Style { get; } // Flyweight compartilhado

        public DocumentCharacter(int row, int column, CharacterStyle style)
        {
            Row = row;
            Column = column;
            Style = style;
        }

        public void Render() => Style.Render(Row, Column);

        // Agora cada caractere armazena só extrínseco + referência
        public int GetMemorySizeEstimate()
        {
            // 2 ints (8 bytes) + referência (depende do runtime; aqui só ilustrativo)
            return 2 * sizeof(int) + IntPtr.Size;
        }
    }

    // ============================
    // 4) DOCUMENTO usando Flyweight
    // ============================
    public class Document
    {
        private readonly List<DocumentCharacter> _characters = new();
        private readonly CharacterStyleFactory _factory = new();

        public void AddCharacter(
            char symbol,
            string fontFamily,
            int fontSize,
            string color,
            bool isBold,
            bool isItalic,
            bool isUnderline,
            int row,
            int column)
        {
            var style = _factory.GetStyle(symbol, fontFamily, fontSize, color, isBold, isItalic, isUnderline);
            _characters.Add(new DocumentCharacter(row, column, style));
        }

        public void RenderFirst(int n)
        {
            Console.WriteLine($"Renderizando primeiros {n} caracteres:\n");
            for (int i = 0; i < Math.Min(n, _characters.Count); i++)
                _characters[i].Render();
        }

        public void PrintMemoryUsageEstimate()
        {
            long extrinsicMemory = 0;
            foreach (var c in _characters)
                extrinsicMemory += c.GetMemorySizeEstimate();

            // Memória dos estilos cacheados
            long intrinsicMemory = 0;
            // Não temos acesso direto aos valores do cache aqui sem expor, então vamos mostrar o número de estilos.
            Console.WriteLine($"\n=== Uso de Memória (Estimativa) ===");
            Console.WriteLine($"Total de caracteres: {_characters.Count}");
            Console.WriteLine($"Estilos (flyweights) criados: {_factory.CachedStylesCount}");
            Console.WriteLine($"Memória (extrínseco por caractere): ~{extrinsicMemory:N0} bytes (~{extrinsicMemory / 1024.0:N2} KB)");
            Console.WriteLine("Obs: o ganho real vem de NÃO repetir strings (fonte/cor) em cada caractere.");
        }

        public int TotalCharacters => _characters.Count;
        public int TotalStyles => _factory.CachedStylesCount;
    }

    // ============================
    // 5) DEMO
    // ============================
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== Editor de Texto (Flyweight) ===\n");

            var document = new Document();

            // Linha 1: "Hello World" em Arial 12pt preto
            string text1 = "Hello World";
            for (int i = 0; i < text1.Length; i++)
            {
                document.AddCharacter(
                    text1[i],
                    "Arial",
                    12,
                    "Black",
                    false,
                    false,
                    false,
                    1,
                    i + 1
                );
            }

            // Linha 2: "IMPORTANT" em Arial 12pt vermelho, negrito
            string text2 = "IMPORTANT";
            for (int i = 0; i < text2.Length; i++)
            {
                document.AddCharacter(
                    text2[i],
                    "Arial",
                    12,
                    "Red",
                    true,
                    false,
                    false,
                    2,
                    i + 1
                );
            }

            // Linha 3: "This is a sample text" em Arial 12pt preto
            string text3 = "This is a sample text";
            for (int i = 0; i < text3.Length; i++)
            {
                document.AddCharacter(
                    text3[i],
                    "Arial",
                    12,
                    "Black",
                    false,
                    false,
                    false,
                    3,
                    i + 1
                );
            }

            document.RenderFirst(8);

            document.PrintMemoryUsageEstimate();

            Console.WriteLine("\n=== RESULTADO ===");
            Console.WriteLine("✅ Estado intrínseco (fonte/tamanho/cor/estilo/símbolo) é compartilhado");
            Console.WriteLine("✅ Cada caractere guarda só posição + referência para o flyweight");
            Console.WriteLine("✅ Redução drástica de strings e objetos repetidos");
            Console.WriteLine($"📌 Estilos criados: {document.TotalStyles} para {document.TotalCharacters} caracteres");
        }
    }
}