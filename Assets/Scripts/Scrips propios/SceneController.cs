using UnityEngine.SceneManagement;
using UnityEngine;
using System.Collections;

public class SceneController : MonoBehaviour
{
    public void ChangeScene(string sceneName)
    {
        CarProperties.SaveProperties();
        SceneManager.LoadScene(sceneName);
        StartCoroutine(LoadPropertiesInNewScene());
    }

    private IEnumerator LoadPropertiesInNewScene()
    {
        yield return new WaitForSeconds(1f);  // Espera un momento para que la escena cargue
        CarProperties.LoadProperties();
    }
}