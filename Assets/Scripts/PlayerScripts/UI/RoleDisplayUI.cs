using System.Collections;
using TMPro;
using UnityEngine;

namespace PlayerScripts.UI
{
    public class RoleDisplayUI : MonoBehaviour
    {
        [SerializeField] private GameObject uiObject;
        [SerializeField] private TMP_Text roleField;
        [SerializeField] private float delay;
        
        private void Start()
        {
            NetworkPlayer.GetLocalInstance().OnPlayerRoleSet += Show;
            
            uiObject.SetActive(false);
        }
        
        private void Show(PlayerRole role)
        {
            uiObject.SetActive(true);
            
            roleField.text = role.ToString();
            StartCoroutine(HideAfterDelay());
        }

        private IEnumerator HideAfterDelay()
        {
            yield return new WaitForSeconds(delay);
            
            roleField.text = "";
            
            uiObject.SetActive(false);
        }
    }
}