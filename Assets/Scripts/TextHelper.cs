using System.Collections;
using TMPro;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class TextHelper : NetworkBehaviour
{
    [SerializeField] private TMP_Text textField;
    private string _prevText;
    private Coroutine _hideTextCoroutine;
        
    public void TempDisplayText128Chars(string text, float duration = 1f)
    {
        TempDisplayText128ServerRpc((FixedString64Bytes)text, duration);
    }

    [ServerRpc(RequireOwnership = false)]
    private void TempDisplayText128ServerRpc(FixedString64Bytes text, float duration)
    {
        TempDisplayText128ClientRpc(text, duration);
    }

    [ClientRpc]
    private void TempDisplayText128ClientRpc(FixedString64Bytes text, float duration)
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