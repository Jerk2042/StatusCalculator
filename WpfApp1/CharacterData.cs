using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Serialization;

[Serializable]
public class CharacterData
{
    public string Name { get; set; }
    public int HP { get; set; }
    public int ATK { get; set; }
    public int DEF { get; set; }
    public int DMGTakenThisTurn { get; set; }
    public int DMGTakenPerTurn { get; set; }
    public int TurnCount { get; set; }
}

[Serializable]
[XmlRoot("ApplicationState")]
public class ApplicationState
{
    [XmlArray("Characters")]
    [XmlArrayItem("CharacterData")]
    public List<CharacterData> Characters { get; set; }

    public int GlobalMechanicValue { get; set; }
    public string GameMode { get; set; }

    public ApplicationState()
    {
        // Initialize the Characters list to avoid null reference issues
        Characters = new List<CharacterData>();
    }
}
