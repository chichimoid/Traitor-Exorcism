﻿using System.Collections;
using TMPro;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class TextDisplayer : NetworkBehaviour
{
    [SerializeField] private TMP_Text textField;
    private string _prevText;
    private Coroutine _hideTextCoroutine;
        
    public void TempDisplayText32Chars(string text, float duration = 1f)
    {
        TempDisplayText32Rpc((FixedString64Bytes)text, duration);
    }

    [Rpc(SendTo.Everyone)]
    private void TempDisplayText32Rpc(FixedString64Bytes text, float duration)
    {
        if (_hideTextCoroutine != null)
        {
            StopCoroutine(_hideTextCoroutine);
        }
        else
        {
            _prevText = textField.text;
        }
        
        textField.text = text.ToString();
        _hideTextCoroutine = StartCoroutine(HideTextAfterDelay(duration));
    }

    private IEnumerator HideTextAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        textField.text = _prevText;
        _hideTextCoroutine = null;
    }
}