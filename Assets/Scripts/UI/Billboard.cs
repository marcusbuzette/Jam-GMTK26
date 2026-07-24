using UnityEngine;

public class Billboard : MonoBehaviour {
    [Tooltip("Deixe vazio para buscar a Camera.main automaticamente")]
    public Camera targetCamera;

    private void Start() {
        // Se nenhuma câmera for atribuída no Inspector, ele busca a principal automaticamente
        if (targetCamera == null) {
            targetCamera = Camera.main;
        }
    }

    private void LateUpdate() {
        // Se ainda não houver câmera (ex: câmera foi destruída), não faz nada
        if (targetCamera == null) return;

        // Copiar a rotação da câmera mantém o Canvas perfeitamente paralelo à tela.
        // Isso evita textos invertidos e distorções de perspectiva nas bordas.
        transform.rotation = targetCamera.transform.rotation;

        // --- ALTERNATIVA ---
        // Se o seu jogo for 3D isométrico ou top-down e você quiser que o balão 
        // fique "em pé" no chão e apenas gire no eixo Y (como árvores clássicas de Doom),
        // comente a linha de cima e descomente a linha abaixo:
        //
        // transform.forward = new Vector3(targetCamera.transform.forward.x, 0f, targetCamera.transform.forward.z);
    }
}
