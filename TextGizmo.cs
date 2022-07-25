/*
*   TextGizmo.cs
*   August 2009
*   Carl Emil Carlsen
*   sixthsensor.dk
*   
*   Was updated by Jalui
*   July 2022
*/

using UnityEngine;
using System.Collections.Generic;

public class TextGizmo
{
    private static TextGizmo textGizmo = null;
    private Dictionary<char,string> texturePathLookup;
    private Camera editorCamera = null;

    public enum TextAlignment
    {
        LeftTop = 0b_100_100,
        CenterTop = 0b_010_100,
        RightTop = 0b_001_100,
        LeftMiddle = 0b_100_010,
        CenterMiddle = 0b_010_010,
        RightMiddle = 0b_001_010,
        LeftBottom = 0b_100_001,
        CenterBottom = 0b_010_001,
        RightBottom = 0b_001_001,
    }

    private static TextAlignment DefaultTextAligment { get => TextAlignment.LeftMiddle; }
    private static Color DefaultColor { get => default; }
    private static int CharTextureWidth { get => 8; }
    private static int CharTextureHeight { get => 11; }

    private static readonly string availableCharacters = " !#%'()+,-.0123456789;=abcdefghijklmnopqrstuvwxyz_{}~\\?\":/*";
    private static string AvailableCharacters { get => availableCharacters; }

    private static readonly string[] newLineCharacterCombinations = { "\n", "\r\n" };
    private static string[] NewLineCharacterCombinations { get => newLineCharacterCombinations; }

    private TextGizmo()
    {
        editorCamera = Camera.current;
        texturePathLookup = new Dictionary<char,string>();

        for (int i = 0; i < AvailableCharacters.Length; i++)
            texturePathLookup.Add(AvailableCharacters[i], "TextGizmo/CharacterImages/text_" + AvailableCharacters[i] + ".png");
    }

    private static void Initialize()
        => textGizmo = new TextGizmo();

    /// <summary>
    /// Draw single- or multiline text gizmo with left-middle alignment.<br/>Use into OnGizmos() method.
    /// </summary>
    /// <param name="position">Text pivot position.</param>
    /// <param name="text">Single- or multiline text.</param>
    public static void DrawExtended(Vector3 position, string text)
        => DrawExtended(position, text, DefaultTextAligment, DefaultColor);

    /// <summary>
    /// Draw single- or multiline text gizmo with left-middle alignment.<br/>Use into OnGizmos() method.
    /// </summary>
    /// <param name="position">Text pivot position.</param>
    /// <param name="text">Single- or multiline text.</param>
    /// <param name="color">Text color.</param>
    public static void DrawExtended(Vector3 position, string text, Color color)
        => DrawExtended(position, text, DefaultTextAligment, color);

    /// <summary>
    /// Draw single- or multiline text gizmo with an alignment.<br/>Use into OnGizmos() method.
    /// </summary>
    /// <param name="position">Text pivot position.</param>
    /// <param name="text">Single- or multiline text.</param>
    /// <param name="alignment">Text alignment.</param>
    public static void DrawExtended(Vector3 position, string text, TextAlignment alignment)
        => DrawExtended(position, text, alignment, DefaultColor);

    /// <summary>
    /// Draw single- or multiline text gizmo with an alignment.<br/>Use into OnGizmos() method.
    /// </summary>
    /// <param name="position">Text pivot position.</param>
    /// <param name="text">Single- or multiline text.</param>
    /// <param name="alignment">Text alignment.</param>
    /// <param name="color">Text color.</param>
    public static void DrawExtended(Vector3 position, string text, TextAlignment alignment, Color color)
    {
        if (textGizmo == null)
            Initialize();

        Vector3 screenPoint = textGizmo.editorCamera.WorldToScreenPoint(position);

        List<List<string>> availableSymbolIconPaths = GetAvailableSymbolIconPaths(text.ToLower());
        int numberOfLines = availableSymbolIconPaths.Count;
        int lineMaxLength = GetLineMaxLength(availableSymbolIconPaths);
        float textVerticalOffset = GetTextVerticalOffset(numberOfLines, alignment);

        for (int i = 0; i < availableSymbolIconPaths.Count; i++)
        {
            float lineHorizontalOffset = GetLineHorizontalOffset(availableSymbolIconPaths[i].Count, alignment);

            for (int j = 0; j < availableSymbolIconPaths[i].Count; j++)
            {
                float characterXOffset = lineHorizontalOffset + j * CharTextureWidth;
                float characterYOffset = textVerticalOffset - i * CharTextureHeight;
                Vector3 worldPoint = textGizmo.editorCamera.ScreenToWorldPoint(
                    new Vector3(screenPoint.x + characterXOffset, screenPoint.y + characterYOffset, screenPoint.z));
                Gizmos.DrawIcon(worldPoint, availableSymbolIconPaths[i][j], false, color);
            }
        }

        List<List<string>> GetAvailableSymbolIconPaths(string text)
        {
            List<List<string>> availableSymbolIconPaths = new List<List<string>>();
            availableSymbolIconPaths.Add(new List<string>());

            for (int i = 0; i < text.Length; i++)
            {
                bool isNewLine = false;

                foreach (string newLineCharacterCombination in NewLineCharacterCombinations)
                    if (text.Length - (i + 1) >= newLineCharacterCombination.Length
                                && text[i..(i + 2)] == newLineCharacterCombination)
                    {
                        availableSymbolIconPaths.Add(new List<string>());
                        i += newLineCharacterCombination.Length - 1;
                        isNewLine = true;
                        break;
                    }

                if (!isNewLine && textGizmo.texturePathLookup.ContainsKey(text[i]))
                    availableSymbolIconPaths[^1].Add(textGizmo.texturePathLookup[text[i]]);
            }

            return availableSymbolIconPaths;
        }

        int GetLineMaxLength(List<List<string>> availableSymbolIconPaths)
        {
            int maxLength = 0;

            foreach (List<string> line in availableSymbolIconPaths)
                if (line.Count > maxLength)
                    maxLength = line.Count;

            return maxLength;
        }

        float GetTextVerticalOffset(int numberOfLines, TextAlignment alignment)
        {
            if (((int)alignment & 0b_000_100) != 0)
                return -CharTextureHeight / 2f;
            else if (((int)alignment & 0b_000_010) != 0)
                return CharTextureHeight * (numberOfLines - 1) / 2f;
            else if (((int)alignment & 0b_000_001) != 0)
                return CharTextureHeight * (numberOfLines - 0.5f);
            else
                return 0;
        }

        float GetLineHorizontalOffset(int lineLength, TextAlignment alignment)
        {
            if (((int)alignment & 0b_100_000) != 0)
                return CharTextureWidth / 2f;
            else if (((int)alignment & 0b_010_000) != 0)
                return -CharTextureWidth * lineLength / 2f;
            else if (((int)alignment & 0b_001_000) != 0)
                return -CharTextureWidth * (lineLength - 0.5f);
            else
                return 0;
        }
    }

    /// <summary>
    /// Draw singleline text gizmo with left alignment.<br/>Use into OnGizmos() method.
    /// </summary>
    /// <param name="position">Text pivot position.</param>
    /// <param name="text">Singleline text.</param>
    public static void DrawBasic(Vector3 position, string text)
        => DrawBasic(position, text, DefaultColor);

    /// <summary>
    /// Draw singleline text gizmo with left alignment.<br/>Use into OnGizmos() method.
    /// </summary>
    /// <param name="position">Text pivot position.</param>
    /// <param name="text">Singleline text.</param>
    /// <param name="color">Text color.</param>
    public static void DrawBasic(Vector3 position, string text, Color color)
    {
        if (textGizmo == null)
            Initialize();

        string lowerText = text.ToLower();
        Vector3 screenPoint = textGizmo.editorCamera.WorldToScreenPoint(position);

        for (int i = 0; i < lowerText.Length; i++)
            if (textGizmo.texturePathLookup.ContainsKey(lowerText[i]))
            {
                float characterXOffset = CharTextureWidth * (0.5f + i);
                Vector3 worldPoint = textGizmo.editorCamera.ScreenToWorldPoint(
                    new Vector3(screenPoint.x + characterXOffset, screenPoint.y, screenPoint.z));
                Gizmos.DrawIcon(worldPoint, textGizmo.texturePathLookup[lowerText[i]], false, color);
            }
    }
}