using UnityEngine;
using System.IO;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

[System.Serializable]

public struct SaveData
{
    public static SaveData Instance;

    //Mappa
    public HashSet<string> sceneNames;

    //Panchine
    public string benchSceneName;
    public Vector2 benchPos;

    public void Initialize()
    {
        if (!File.Exists(Application.persistentDataPath + "/save.bench.data"))
        {
            using (BinaryWriter writer = new BinaryWriter(File.Create(Application.persistentDataPath + "/save.bench.data")))
            {
                writer.Write(""); // benchSceneName vuoto
                writer.Write(0f); // benchPos.x = 0
                writer.Write(0f); // benchPos.y = 0
            }
        }

        if (sceneNames == null)
        {
            sceneNames = new HashSet<string>();
        }
    }

    public void SaveBench()
    {
        using (BinaryWriter writer = new BinaryWriter(File.OpenWrite(Application.persistentDataPath + "/save.bench.data")))
        {
            writer.Write(benchSceneName ?? "");
            writer.Write(benchPos.x);
            writer.Write(benchPos.y);
        }

    }

    public void LoadBench()
    {
        if (File.Exists(Application.persistentDataPath + "/save.bench.data"))
        {
            try
            {
                using (BinaryReader reader = new BinaryReader(File.OpenRead(Application.persistentDataPath + "/save.bench.data")))
                {
                    benchSceneName = reader.ReadString();
                    benchPos.x = reader.ReadSingle();
                    benchPos.y = reader.ReadSingle();
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError("Errore nel caricamento dei dati della bench: " + e.Message);
                // In caso di errore, inizializza con valori di default
                benchSceneName = "";
                benchPos = Vector2.zero;
            }
        }
    }

    public void ClearSaveData()
    {
        string filePath = Application.persistentDataPath + "/save.bench.data";
        
        try
        {
            // Elimina il file se esiste
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                Debug.Log("File di salvataggio eliminato: " + filePath);
            }
            
            // Reset dei valori in memoria
            benchSceneName = "";
            benchPos = Vector2.zero;
            
            // Reset della collezione delle scene (se necessario)
            if (sceneNames != null)
            {
                sceneNames.Clear();
            }
            
            Debug.Log("Dati di salvataggio resettati");
        }
        catch (System.Exception e)
        {
            Debug.LogError("Errore nell'eliminazione del file di salvataggio: " + e.Message);
        }
    }

    public void ResetToDefault()
    {
        // Cancella i dati esistenti
        ClearSaveData();
        
        // Ricrea il file con valori di default
        using (BinaryWriter writer = new BinaryWriter(File.Create(Application.persistentDataPath + "/save.bench.data")))
        {
            writer.Write(""); // benchSceneName vuoto
            writer.Write(0f); // benchPos.x = 0
            writer.Write(0f); // benchPos.y = 0
        }
        
        // Reinizializza la collezione delle scene
        if (sceneNames == null)
        {
            sceneNames = new HashSet<string>();
        }
        
        Debug.Log("File di salvataggio resettato ai valori di default");
    }
}
