using UnityEngine;
using TMPro;
using System.Collections;

public class WindowBattleLog : WindowBase
{
    private const int TOTAL_LINES = 7;
    private const int CHARS_PER_LINE = 22;

    private string _textToAdd;
    private float _textSpeed;
    private float _scrollSpeed;
    private Coroutine _typingCoroutine;
    public bool isActive => _textToAdd.Length > 0;
        

    public override void Open()
    {
        base.Open();
        _textSpeed = 0.03f;
        _scrollSpeed = 0.1f;
    }

    public void Breakline()
    {
        _textToAdd += "\n";
    }
    public void ClearText()
    {
        if (_typingCoroutine != null)
        {
            StopCoroutine(_typingCoroutine);
            _typingCoroutine = null;
        }
        _text.text = "";
        _textToAdd = "";
    }

    public void AddText(string text, bool breakLine = true)
    {
        string enter = breakLine ? "\n" : "";
        _textToAdd += enter + text;

        if (_typingCoroutine == null)
            _typingCoroutine = StartCoroutine(AddToDisplay());
    }

    private IEnumerator AddToDisplay()
    {
        while (isActive)
        {
            // Añadir un carácter al texto            
            _text.text += _textToAdd.Substring(0, 1);
            _textToAdd = _textToAdd.Substring(1);

            // Desplazar texto si está lleno
            if (Full())
            {
                ScrollTextUp();
                yield return new WaitForSeconds(_scrollSpeed);
            }

            // Esperar antes de agregar el siguiente carácter
            yield return new WaitForSeconds(_textSpeed);
        }

        _typingCoroutine = null;
    }

    // Método para forzar la escritura del texto restante
    public void ForceComplete()
    {
        if (_typingCoroutine != null)
        {
            StopCoroutine(_typingCoroutine);
            _typingCoroutine = null;
        }

        // Añadir todo el texto restante de inmediato
        _text.text += _textToAdd;

        // Si está lleno, desplazamos el texto
        while (Full())
        {
            ScrollTextUp();
        }

        // Limpiar el buffer de texto pendiente
        _textToAdd = "";
    }

    private bool Full()
    {
        string[] lines = _text.text.Split('\n');
        int totalLines = 0;

        foreach (var line in lines)
        {
            totalLines += Mathf.CeilToInt((float)line.Length / CHARS_PER_LINE);
        }

        return totalLines >= TOTAL_LINES;
    }

    private void ScrollTextUp()
    {
        int firstLineEnd = _text.text.IndexOf("\n") + 1;
        if (firstLineEnd > 0)
        {
            _text.text = _text.text.Substring(firstLineEnd);
        }
    }
}
