using System;
using UnityEngine;

[Serializable]
public class SaveRoot
{
    public string version = "1.0.0";
    public MetadataDto metadata;
    public GameDto game;

    // opcionális (a PDF-ben szerepel), de most nem számoljuk:
    public string checksum_sha1;
}

[Serializable]
public class MetadataDto
{
    public string created_at_utc;
    public int seed;
}

[Serializable]
public class GameDto
{
    public string state;          // "Running" | "GameOver"
    public int turn;
    public string currentPlayer;  // "RED" | "BLUE"

    public MapDto map;
    public PlayerDto[] players;
    public UnitDto[] units;

    public CastlesDto castles;
}

[Serializable]
public class CastlesDto
{
    public PosDto RED;
    public PosDto BLUE;
}

[Serializable]
public class MapDto
{
    public int width, height;

    // Unity JsonUtility nem szereti a 2D tömböt, ezért flat lista tile-okkal (x,y)
    public TileDto[] tiles;
}

[Serializable]
public class TileDto
{
    public int x, y;
    public string type;   // "PLAINS" | ...
    public string owner;  // "NONE" | "RED" | "BLUE"
}

[Serializable]
public class PlayerDto
{
    public string id;     // "RED" | "BLUE"
    public string name;
    public int resources;

    public LimitsDto limits;
}

[Serializable]
public class LimitsDto
{
    public int soldier = RecruitmentService.MaxSoldier;
    public int knight = RecruitmentService.MaxKnight;
    public int archer = RecruitmentService.MaxArcher;
    public int total = RecruitmentService.MaxTotal;
}

[Serializable]
public class UnitDto
{
    public string id;
    public string owner;   // "RED" | "BLUE"
    public string type;    // "Soldier" | "Knight" | "Archer"
    public PosDto pos;
    public int hp;         // most 1-nek használjuk (nincs HP rendszered)
}

[Serializable]
public class PosDto
{
    public int x, y;
}
