using UnityEngine;

public class Stairs : InteractableBase {

    [SerializeField] private Transform _topTransform;
    [SerializeField] private Transform _bottomTransform;
    [SerializeField] private int _topFloorIndex;
    [SerializeField] private int _bottomFloorIndex;

    public override Transform InteractionPoint {
        get {
            // Retorna o ponto de interação dependendo do andar atual
            int currentFloorIndex = FloorManager.Instance.CurrentFloorIndex;
            if (currentFloorIndex == _topFloorIndex) {
                return _topTransform;
            } else if (currentFloorIndex == _bottomFloorIndex) {
                return _bottomTransform;
            } else {
                return base.InteractionPoint; // Ponto padrão se não estiver em nenhum dos andares
            }
        }
    } 
    public override void Interact(GameObject interactor) {
        // Verifica se o interactor está no andar de cima ou de baixo
        int currentFloorIndex = FloorManager.Instance.CurrentFloorIndex;

        if (currentFloorIndex == _topFloorIndex) {
            // Se estiver no andar de cima, desce para o andar de baixo
            FloorManager.Instance.ShowFloor(_bottomFloorIndex);
            interactor.transform.position = _bottomTransform.position;
        } else if (currentFloorIndex == _bottomFloorIndex) {
            // Se estiver no andar de baixo, sobe para o andar de cima
            FloorManager.Instance.ShowFloor(_topFloorIndex);
            interactor.transform.position = _topTransform.position;
        }
    }
}
