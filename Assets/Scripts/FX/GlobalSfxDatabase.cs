using System.Collections.Generic;
using UnityEngine;
using Sound;

[CreateAssetMenu(fileName = "newGlobalAudioDatabase", menuName = "Data/Audio/Global Audio Database")]
public class GlobalSfxDatabase : ScriptableObject
{
    [SerializeField] private List<GlobalSfx> _sfxList = new List<GlobalSfx>();
    private Dictionary<string, AudioClip> _sfxDictionary;

    private void OnEnable() => InitializeDictionary();    

    private void InitializeDictionary()
    {
        _sfxDictionary = new Dictionary<string, AudioClip>();
        foreach (var item in _sfxList)
        {
            if (!_sfxDictionary.ContainsKey(item.name.ToLower()))            
                _sfxDictionary.Add(item.name.ToLower(), item.sfx);            
            else            
                Debug.LogWarning($"Duplicate SFX name found: {item.name}. Only the first occurrence will be used.");            
        }
    }

    public AudioClip GetAudioClip(string name)
    {
        _sfxDictionary.TryGetValue(name.ToLower(), out var clip);

        if (clip == null)        
            Debug.Log($"SFX {name} not found.");        

        return clip;
    }

    public void AddSfx(GlobalSfx newSfx)
    {
        if (!_sfxDictionary.ContainsKey(newSfx.name.ToLower()))
        {
            _sfxList.Add(newSfx);
            _sfxDictionary.Add(newSfx.name.ToLower(), newSfx.sfx);
        }
        else        
            Debug.LogWarning($"SFX {newSfx.name} already exists in the database.");        
    }

    public void RemoveSfx(string name)
    {
        var key = name.ToLower();
        if (_sfxDictionary.ContainsKey(key))
        {
            var sfxToRemove = _sfxList.Find(s => s.name.ToLower() == key);
            _sfxList.Remove(sfxToRemove);
            _sfxDictionary.Remove(key);
        }
        else        
            Debug.Log($"SFX {name} not found.");        
    }

    public void Clear()
    {
        _sfxList.Clear();
        _sfxDictionary.Clear();
    }
}
